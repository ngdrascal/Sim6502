namespace Sim6502.Instructions;

public class InstTSB : InstBase
{
    public InstTSB(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.TSBzpg, States.InstTSBzpg2);
        registry.Map(OpCodes.TSBabs, States.InstTSBabs2);
    }

    protected override void RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstTSBzpg2, ctx => Zpg2(ctx));
        registry.Map(States.InstTSBzpg3, ctx => Zpg3(ctx));
        registry.Map(States.InstTSBzpg4, ctx => Zpg4(ctx));
        registry.Map(States.InstTSBzpg5, ctx => Zpg5(ctx));

        registry.Map(States.InstTSBabs2, ctx => Abs2(ctx));
        registry.Map(States.InstTSBabs3, ctx => Abs3(ctx));
        registry.Map(States.InstTSBabs4, ctx => Abs4(ctx));
        registry.Map(States.InstTSBabs5, ctx => Abs5(ctx));
        registry.Map(States.InstTSBabs6, ctx => Abs6(ctx));
    }

    protected void TestAndSet(Context ctx)
    {
        var acc = ctx.Regs.A.Copy();
        var temp = ctx.Regs.Temp;
        var result = acc.Or(temp);
        temp.UpdateValue(result);
        if (result.EqualsZero())
            ctx.Regs.P.SetZero();
        else
            ctx.Regs.P.ClearZero();
    }

    /////////////////////////////////////////////////////////////////////////////
    // TSB - Test And Reset Memory Bits With Accumulator
    // A | M -> M
    // N V B D I Z C
    // - - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // zeropage       TSB oper      04      2      5  
    // absolute       TSB oper      0C      3      6  
    ///////////////////////////////////////////////////////////////////////////// 

    // -------------------------------------------------------------------------
    // [0x04] TSB zeropage
    // -------------------------------------------------------------------------
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstTSBzpg3);
    }

    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstTSBzpg4);
    }

    public void Zpg4(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            TestAndSet(ctx);
        ctx.AdvanceState(States.InstTSBzpg5);
    }

    public void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x0C] TSB absolute
    // -------------------------------------------------------------------------
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstTSBabs3);
    }

    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstTSBabs4);
    }

    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstTSBabs5);
    }

    public void Abs5(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            TestAndSet(ctx);
        ctx.AdvanceState(States.InstTSBabs6);
    }

    public void Abs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
