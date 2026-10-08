using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine;

internal class InstDecrement : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.DECacc, States.InstDECacc2);
        registry.Map(OpCodes.DECzpg, States.InstDECzpg2);
        registry.Map(OpCodes.DECzpgx, States.InstDECzpgx2);
        registry.Map(OpCodes.DECabs, States.InstDECabs2);
        registry.Map(OpCodes.DECabsx, States.InstDECabsx2);
        registry.Map(OpCodes.DEXimp, States.InstDEXimp2);
        registry.Map(OpCodes.DEYimp, States.InstDEYimp2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstDECacc2, DecAcc2);
        stateRegistry.Map(States.InstDECzpg2, DecZpg2);
        stateRegistry.Map(States.InstDECzpg3, DecZpg3);
        stateRegistry.Map(States.InstDECzpg4, DecZpg4);
        stateRegistry.Map(States.InstDECzpg5, DecZpg5);
        stateRegistry.Map(States.InstDECzpgx2, DecZpgx2);
        stateRegistry.Map(States.InstDECzpgx3, DecZpgx3);
        stateRegistry.Map(States.InstDECzpgx4, DecZpgx4);
        stateRegistry.Map(States.InstDECzpgx5, DecZpgx5);
        stateRegistry.Map(States.InstDECzpgx6, DecZpgx6);
        stateRegistry.Map(States.InstDECabs2, DecAbs2);
        stateRegistry.Map(States.InstDECabs3, DecAbs3);
        stateRegistry.Map(States.InstDECabs4, DecAbs4);
        stateRegistry.Map(States.InstDECabs5, DecAbs5);
        stateRegistry.Map(States.InstDECabs6, DecAbs6);
        stateRegistry.Map(States.InstDECabsx2, DecAbsx2);
        stateRegistry.Map(States.InstDECabsx3, DecAbsx3);
        stateRegistry.Map(States.InstDECabsx4, DecAbsx4);
        stateRegistry.Map(States.InstDECabsx5, DecAbsx5);
        stateRegistry.Map(States.InstDECabsx6, DecAbsx6);
        stateRegistry.Map(States.InstDECabsx7, DecAbsx7);
        stateRegistry.Map(States.InstDEXimp2, DexImp2);
        stateRegistry.Map(States.InstDEYimp2, DeyImp2);

        return this;
    }

    private void DecTempAndUpdateFlags(Context ctx)
    {
        var temp = ctx.Regs.Temp;
        temp = temp.Dec();
        ctx.Regs.Temp = temp;
        ctx.Regs.P.Zero = temp.EqualsZero();
        ctx.Regs.P.Negative = temp.IsBitSet(7);
    }

    private UInt8 DecAndUpdateFlags(Context ctx, UInt8 value)
    {
        value = value.Dec();
        ctx.Regs.P.Zero = value.EqualsZero();
        ctx.Regs.P.Negative = value.IsBitSet(7);

        return value;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // DEC - Decrement Memory by One
    // M - 1 -> M
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // accumulator    DEC           3A      1      2
    // zeropage       DEC oper      C6      2      5
    // zeropage,X     DEC oper,X    D6      2      6
    // absolute       DEC oper      CE      3      6
    // absolute,X     DEC oper,X    DE      3      7
    ///////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0x3A] DEC accumulator
    // -------------------------------------------------------------------------
    private void DecAcc2(Context ctx)
    {
        ReadAndDiscard(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.A = DecAndUpdateFlags(ctx, ctx.Regs.A);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xC6] DEC zeropage
    // -------------------------------------------------------------------------
    private void DecZpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstDECzpg3);
    }
    private void DecZpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstDECzpg4);
    }
    private void DecZpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            DecTempAndUpdateFlags(ctx);

        ctx.AdvanceState(States.InstDECzpg5);
    }
    private void DecZpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xD6] DEC zeropage,X
    // -------------------------------------------------------------------------
    private void DecZpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstDECzpgx3);
    }
    private void DecZpgx3(Context ctx)
    {
        IndexZpWithX(ctx);

        ctx.AdvanceState(States.InstDECzpgx4);
    }
    private void DecZpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstDECzpgx5);
    }
    private void DecZpgx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            DecTempAndUpdateFlags(ctx);

        ctx.AdvanceState(States.InstDECzpgx6);
    }
    private void DecZpgx6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xCE] DEC absolute
    // -------------------------------------------------------------------------
    private void DecAbs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstDECabs3);
    }
    private void DecAbs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstDECabs4);
    }
    private void DecAbs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstDECabs5);
    }
    private void DecAbs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            DecTempAndUpdateFlags(ctx);

        ctx.AdvanceState(States.InstDECabs6);
    }
    private void DecAbs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xDE] DEC absolute,X
    // -------------------------------------------------------------------------
    private void DecAbsx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstDECabsx3);
    }
    private void DecAbsx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstDECabsx4);
    }
    private void DecAbsx4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEAWithX();

        ctx.AdvanceState(States.InstDECabsx5);
    }
    private void DecAbsx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstDECabsx6);
    }
    private void DecAbsx6(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            DecTempAndUpdateFlags(ctx);

        ctx.AdvanceState(States.InstDECabsx7);
    }
    private void DecAbsx7(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // DEX - Decrement Index X by One
    // X - 1 -> X
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        DEX           CA      1      2
    ///////////////////////////////////////////////////////////////////////////////
    private void DexImp2(Context ctx)
    {
        ReadAndDiscard(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.X = DecAndUpdateFlags(ctx, ctx.Regs.X);

        ctx.AdvanceState(States.Fetch);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // DEY - Decrement Index Y by One
    // Y - 1 -> Y
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        DEY           88      1      2 
    ///////////////////////////////////////////////////////////////////////////////   
    private void DeyImp2(Context ctx)
    {
        ReadAndDiscard(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Y = DecAndUpdateFlags(ctx, ctx.Regs.Y);

        ctx.AdvanceState(States.Fetch);
    }
}
