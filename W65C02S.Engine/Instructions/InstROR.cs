using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine;

internal class InstROR : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.RORacc, States.InstRORacc2);
        registry.Map(OpCodes.RORzpg, States.InstRORzpg2);
        registry.Map(OpCodes.RORzpgx, States.InstRORzpgx2);
        registry.Map(OpCodes.RORabs, States.InstRORabs2);
        registry.Map(OpCodes.RORabsx, States.InstRORabsx2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstRORacc2, Acc2);
        stateRegistry.Map(States.InstRORzpg2, Zpg2);
        stateRegistry.Map(States.InstRORzpg3, Zpg3);
        stateRegistry.Map(States.InstRORzpg4, Zpg4);
        stateRegistry.Map(States.InstRORzpg5, Zpg5);

        stateRegistry.Map(States.InstRORzpgx2, Zpgx2);
        stateRegistry.Map(States.InstRORzpgx3, Zpgx3);
        stateRegistry.Map(States.InstRORzpgx4, Zpgx4);
        stateRegistry.Map(States.InstRORzpgx5, Zpgx5);
        stateRegistry.Map(States.InstRORzpgx6, Zpgx6);

        stateRegistry.Map(States.InstRORabs2, Abs2);
        stateRegistry.Map(States.InstRORabs3, Abs3);
        stateRegistry.Map(States.InstRORabs4, Abs4);
        stateRegistry.Map(States.InstRORabs5, Abs5);
        stateRegistry.Map(States.InstRORabs6, Abs6);

        stateRegistry.Map(States.InstRORabsx2, Absx2);
        stateRegistry.Map(States.InstRORabsx3, Absx3);
        stateRegistry.Map(States.InstRORabsx4, Absx4);
        stateRegistry.Map(States.InstRORabsx5, Absx5);
        stateRegistry.Map(States.InstRORabsx6, Absx6);
        stateRegistry.Map(States.InstRORabsx7, Absx7);

        return this;
    }

    private UInt8 RorUpdateFlags(Context ctx, UInt8 value)
    {
        var oldCarry = ctx.Regs.P.Carry.Copy();
        ctx.Regs.P.Carry = value.IsBitSet(0);
        value = value.Shr(oldCarry);
        ctx.Regs.P.Zero = value.EqualsZero();
        ctx.Regs.P.Negative = value.IsBitSet(7);

        return value;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // ROR - Rotate Right
    // C -> [76543210] -> C
    // N V B D I Z C
    // + - - - - + +
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // accumulator    ROR A         6A      1      2  
    // zeropage       ROR oper      66      2      5  
    // zeropage,X     ROR oper,X    76      2      6  
    // absolute       ROR oper      6E      3      6  
    // absolute,X     ROR oper,X    7E      3      7  
    /////////////////////////////////////////////////////////////////////////////// 

    // -------------------------------------------------------------------------
    // [0x6A] ROR accumulator
    // -------------------------------------------------------------------------
    private void Acc2(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.A = RorUpdateFlags(ctx, ctx.Regs.A);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x66] ROR zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstRORzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstRORzpg4);
    }

    private void Zpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = RorUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstRORzpg5);
    }

    private void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x76] ROR zeropage,X 
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstRORzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        IndexZpWithX(ctx);

        ctx.AdvanceState(States.InstRORzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstRORzpgx5);
    }

    private void Zpgx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = RorUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstRORzpgx6);
    }

    private void Zpgx6(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x6E] ROR absolute 
    // ------------------------------------------------------------------------- 
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstRORabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstRORabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstRORabs5);
    }

    private void Abs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = RorUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstRORabs6);
    }

    private void Abs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x7E] ROR absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstRORabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHighAddX(ctx, States.InstRORabsx4, States.InstRORabsx5);
    }

    private void Absx4(Context ctx)
    {
        // page cross: dummy read, the bus stays on PC+2
        ctx.AdvanceState(States.InstRORabsx5);
    }

    private void Absx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstRORabsx6);
    }

    private void Absx6(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = RorUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstRORabsx7);
    }

    private void Absx7(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
