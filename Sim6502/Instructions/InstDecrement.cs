using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Instructions;

public class InstDecrement : InstBase
{
    public InstDecrement(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.DECacc, States.InstDECacc2);
        registry.Map(OpCodes.DECzpg, States.InstDECzpg2);
        registry.Map(OpCodes.DECzpgx, States.InstDECzpgx2);
        registry.Map(OpCodes.DECabs, States.InstDECabs2);
        registry.Map(OpCodes.DECabsx, States.InstDECabsx2);
        registry.Map(OpCodes.DEXimp, States.InstDEXimp2);
        registry.Map(OpCodes.DEYimp, States.InstDEYimp2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstDECacc2, ctx => DecAcc2(ctx));
        stateRegistry.Map(States.InstDECzpg2, ctx => DecZpg2(ctx));
        stateRegistry.Map(States.InstDECzpg3, ctx => DecZpg3(ctx));
        stateRegistry.Map(States.InstDECzpg4, ctx => DecZpg4(ctx));
        stateRegistry.Map(States.InstDECzpg5, ctx => DecZpg5(ctx));
        stateRegistry.Map(States.InstDECzpgx2, ctx => DecZpgx2(ctx));
        stateRegistry.Map(States.InstDECzpgx3, ctx => DecZpgx3(ctx));
        stateRegistry.Map(States.InstDECzpgx4, ctx => DecZpgx4(ctx));
        stateRegistry.Map(States.InstDECzpgx5, ctx => DecZpgx5(ctx));
        stateRegistry.Map(States.InstDECzpgx6, ctx => DecZpgx6(ctx));
        stateRegistry.Map(States.InstDECabs2, ctx => DecAbs2(ctx));
        stateRegistry.Map(States.InstDECabs3, ctx => DecAbs3(ctx));
        stateRegistry.Map(States.InstDECabs4, ctx => DecAbs4(ctx));
        stateRegistry.Map(States.InstDECabs5, ctx => DecAbs5(ctx));
        stateRegistry.Map(States.InstDECabs6, ctx => DecAbs6(ctx));
        stateRegistry.Map(States.InstDECabsx2, ctx => DecAbsx2(ctx));
        stateRegistry.Map(States.InstDECabsx3, ctx => DecAbsx3(ctx));
        stateRegistry.Map(States.InstDECabsx4, ctx => DecAbsx4(ctx));
        stateRegistry.Map(States.InstDECabsx5, ctx => DecAbsx5(ctx));
        stateRegistry.Map(States.InstDECabsx6, ctx => DecAbsx6(ctx));
        stateRegistry.Map(States.InstDECabsx7, ctx => DecAbsx7(ctx));
        stateRegistry.Map(States.InstDEXimp2, ctx => DexImp2(ctx));
        stateRegistry.Map(States.InstDEYimp2, ctx => DeyImp2(ctx));
    }

    private void DecTempAndUpdateFlags(Context ctx)
    {
        var temp = ctx.Regs.Temp;
        temp.Dec();
        ctx.Regs.P.Zero.UpdateValue(temp.EqualsZero());
        ctx.Regs.P.Negative.UpdateValue(temp.IsBitSet(7));
    }

    private void DecAndUpdateFlags(Context ctx, UInt8 value)
    {
        value.Dec();
        ctx.Regs.P.Zero.UpdateValue(value.EqualsZero());
        ctx.Regs.P.Negative.UpdateValue(value.IsBitSet(7));
    }

    ///////////////////////////////////////////////////////////////////////////////
    // DEC - Decrement Memory by One
    // M - 1 -> M
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // accumlator     DEC           3A      1      2
    // zeropage       DEC oper      C6      2      5
    // zeropage,X     DEC oper,X    D6      2      6
    // absolute       DEC oper      CE      3      6
    // absolute,X     DEC oper,X    DE      3      7
    ///////////////////////////////////////////////////////////////////////////////

    // [0x3A] DEC accumulator
    public void DecAcc2(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            DecAndUpdateFlags(ctx, ctx.Regs.A);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xC6] DEC zeropage
    public void DecZpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstDECzpg3);
    }
    public void DecZpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstDECzpg4);
    }
    public void DecZpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            DecTempAndUpdateFlags(ctx);
        ctx.AdvanceState(States.InstDECzpg5);
    }
    public void DecZpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xD6] DEC zeropage,X
    public void DecZpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstDECzpgx3);
    }
    public void DecZpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstDECzpgx4);
    }
    public void DecZpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstDECzpgx5);
    }
    public void DecZpgx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            DecTempAndUpdateFlags(ctx);
        ctx.AdvanceState(States.InstDECzpgx6);
    }
    public void DecZpgx6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xCE] DEC absolute
    public void DecAbs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstDECabs3);
    }
    public void DecAbs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstDECabs4);
    }
    public void DecAbs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstDECabs5);
    }
    public void DecAbs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            DecTempAndUpdateFlags(ctx);
        ctx.AdvanceState(States.InstDECabs6);
    }
    public void DecAbs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xDE] DEC absolute,X
    public void DecAbsx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstDECabsx3);
    }
    public void DecAbsx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstDECabsx4);
    }
    public void DecAbsx4(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.IncEAWithX();
        ctx.AdvanceState(States.InstDECabsx5);
    }
    public void DecAbsx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstDECabsx6);
    }
    public void DecAbsx6(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            DecTempAndUpdateFlags(ctx);
        ctx.AdvanceState(States.InstDECabsx7);
    }
    public void DecAbsx7(Context ctx)
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
    public void DexImp2(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            DecAndUpdateFlags(ctx, ctx.Regs.X);
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
    public void DeyImp2(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            DecAndUpdateFlags(ctx, ctx.Regs.Y);
        ctx.AdvanceState(States.Fetch);
    }
}
