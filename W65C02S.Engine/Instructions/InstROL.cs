using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine;

internal class InstROL : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.ROLacc, States.InstROLacc2);
        registry.Map(OpCodes.ROLzpg, States.InstROLzpg2);
        registry.Map(OpCodes.ROLzpgx, States.InstROLzpgx2);
        registry.Map(OpCodes.ROLabs, States.InstROLabs2);
        registry.Map(OpCodes.ROLabsx, States.InstROLabsx2);
        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstROLacc2, Acc2);
        stateRegistry.Map(States.InstROLzpg2, Zpg2);
        stateRegistry.Map(States.InstROLzpg3, Zpg3);
        stateRegistry.Map(States.InstROLzpg4, Zpg4);
        stateRegistry.Map(States.InstROLzpg5, Zpg5);
        stateRegistry.Map(States.InstROLzpgx2, Zpgx2);
        stateRegistry.Map(States.InstROLzpgx3, Zpgx3);
        stateRegistry.Map(States.InstROLzpgx4, Zpgx4);
        stateRegistry.Map(States.InstROLzpgx5, Zpgx5);
        stateRegistry.Map(States.InstROLzpgx6, Zpgx6);
        stateRegistry.Map(States.InstROLabs2, Abs2);
        stateRegistry.Map(States.InstROLabs3, Abs3);
        stateRegistry.Map(States.InstROLabs4, Abs4);
        stateRegistry.Map(States.InstROLabs5, Abs5);
        stateRegistry.Map(States.InstROLabs6, Abs6);
        stateRegistry.Map(States.InstROLabsx2, Absx2);
        stateRegistry.Map(States.InstROLabsx3, Absx3);
        stateRegistry.Map(States.InstROLabsx4, Absx4);
        stateRegistry.Map(States.InstROLabsx5, Absx5);
        stateRegistry.Map(States.InstROLabsx6, Absx6);
        stateRegistry.Map(States.InstROLabsx7, Absx7);
        return this;
    }

    private UInt8 RolUpdateFlags(Context ctx, UInt8 value)
    {
        var oldCarry = ctx.Regs.P.Carry.Copy();
        ctx.Regs.P.Carry = value.IsBitSet(7);
        value = value.Shl(oldCarry);
        ctx.Regs.P.Zero = value.EqualsZero();
        ctx.Regs.P.Negative = value.IsBitSet(7);

        return value;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // ROL - Rotate Left
    // C <- [76543210] <- C
    // N V B D I Z C
    // + - - - - + +
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // accumulator    ROL A         2A      1      2  
    // zeropage       ROL oper      26      2      5  
    // zeropage,X     ROL oper,X    36      2      6  
    // absolute       ROL oper      2E      3      6  
    // absolute,X     ROL oper,X    3E      3      7
    /////////////////////////////////////////////////////////////////////////////// 

    // -------------------------------------------------------------------------
    // [0x2A] ROL accumulator
    // -------------------------------------------------------------------------
    private void Acc2(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.A = RolUpdateFlags(ctx, ctx.Regs.A);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x26] ROL zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstROLzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstROLzpg4);
    }

    private void Zpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = RolUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstROLzpg5);
    }

    private void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x36] ROL zeropage,X 
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstROLzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstROLzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstROLzpgx5);
    }

    private void Zpgx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = RolUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstROLzpgx6);
    }
    private void Zpgx6(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x2E] ROL absolute 
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstROLabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstROLabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstROLabs5);
    }

    private void Abs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = RolUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstROLabs6);
    }

    private void Abs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x3E] ROL absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstROLabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHighAddX(ctx, States.InstROLabsx4, States.InstROLabsx5);
    }

    private void Absx4(Context ctx)
    {
        // page cross: dummy read, the bus stays on PC+2
        ctx.AdvanceState(States.InstROLabsx5);
    }

    private void Absx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstROLabsx6);
    }

    private void Absx6(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = RolUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstROLabsx7);
    }

    private void Absx7(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
