using W65C02S.Engine.Types;

namespace W65C02S.Engine;

internal class InstASL : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.ASLacc, States.InstASLacc2);
        registry.Map(OpCodes.ASLzpg, States.InstASLzpg2);
        registry.Map(OpCodes.ASLzpgx, States.InstASLzpgx2);
        registry.Map(OpCodes.ASLabs, States.InstASLabs2);
        registry.Map(OpCodes.ASLabsx, States.InstASLabsx2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstASLacc2, Acc2);

        stateRegistry.Map(States.InstASLzpg2, Zpg2);
        stateRegistry.Map(States.InstASLzpg3, Zpg3);
        stateRegistry.Map(States.InstASLzpg4, Zpg4);
        stateRegistry.Map(States.InstASLzpg5, Zpg5);

        stateRegistry.Map(States.InstASLzpgx2, Zpgx2);
        stateRegistry.Map(States.InstASLzpgx3, Zpgx3);
        stateRegistry.Map(States.InstASLzpgx4, Zpgx4);
        stateRegistry.Map(States.InstASLzpgx5, Zpgx5);
        stateRegistry.Map(States.InstASLzpgx6, Zpgx6);

        stateRegistry.Map(States.InstASLabs2, Abs2);
        stateRegistry.Map(States.InstASLabs3, Abs3);
        stateRegistry.Map(States.InstASLabs4, Abs4);
        stateRegistry.Map(States.InstASLabs5, Abs5);
        stateRegistry.Map(States.InstASLabs6, Abs6);

        stateRegistry.Map(States.InstASLabsx2, Absx2);
        stateRegistry.Map(States.InstASLabsx3, Absx3);
        stateRegistry.Map(States.InstASLabsx4, Absx4);
        stateRegistry.Map(States.InstASLabsx5, Absx5);
        stateRegistry.Map(States.InstASLabsx6, Absx6);
        stateRegistry.Map(States.InstASLabsx7, Absx7);

        return this;
    }

    private UInt8 AslUpdateFlags(Context ctx, UInt8 value)
    {
        ctx.Regs.P.Carry = value.IsBitSet(7);
        ctx.Regs.P.Negative = value.IsBitSet(6);
        value = value.Shl();
        ctx.Regs.P.Zero = value.EqualsZero();

        return value;
    }

    /////////////////////////////////////////////////////////////////////////////
    // ASL - Arithmetic Shift Left
    // C <- [76543210] <- 0
    // N V B D I Z C
    // + - - - - + +
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // accumulator    ASL A         0A      1      2  
    // zeropage       ASL oper      06      2      5  
    // zeropage,X     ASL oper,X    16      2      6  
    // absolute       ASL oper      0E      3      6  
    // absolute,X     ASL oper,X    1E      3      7 
    ///////////////////////////////////////////////////////////////////////////// 

    // -------------------------------------------------------------------------
    // [0xA0] ASL accumulator
    // -------------------------------------------------------------------------
    private void Acc2(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.A = AslUpdateFlags(ctx, ctx.Regs.A);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xA6] ASL zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstASLzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstASLzpg4);
    }

    private void Zpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = AslUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstASLzpg5);
    }

    private void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x16] ASL zeropage,X 
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstASLzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstASLzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstASLzpgx5);
    }

    private void Zpgx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = AslUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstASLzpgx6);
    }

    private void Zpgx6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x0E] ASL absolute 
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstASLabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstASLabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstASLabs5);
    }

    private void Abs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = AslUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstASLabs6);
    }

    private void Abs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x1E] ASL absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstASLabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstASLabsx4);
    }

    private void Absx4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEAWithX();

        ctx.AdvanceState(States.InstASLabsx5);
    }

    private void Absx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstASLabsx6);
    }

    private void Absx6(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = AslUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstASLabsx7);
    }

    private void Absx7(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
