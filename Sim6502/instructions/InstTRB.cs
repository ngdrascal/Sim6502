namespace Sim6502.Instructions;

public class InstTRB : InstBase
{
    public InstTRB(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.TRBzpg, States.InstTRBzpg2);
        registry.Map(OpCodes.TRBabs, States.InstTRBabs2);
    }

    protected override void RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstTRBzpg2, ctx => Zpg2(ctx));
        registry.Map(States.InstTRBzpg3, ctx => Zpg3(ctx));
        registry.Map(States.InstTRBzpg4, ctx => Zpg4(ctx));
        registry.Map(States.InstTRBzpg5, ctx => Zpg5(ctx));

        registry.Map(States.InstTRBabs2, ctx => Abs2(ctx));
        registry.Map(States.InstTRBabs3, ctx => Abs3(ctx));
        registry.Map(States.InstTRBabs4, ctx => Abs4(ctx));
        registry.Map(States.InstTRBabs4, ctx => Abs5(ctx));
        registry.Map(States.InstTRBabs4, ctx => Abs6(ctx));
    }

    protected void TestAndReset(Context ctx)
    {
        var acc = ctx.Regs.A.Copy();
        var temp = ctx.Regs.Temp;

        var result = acc.Not().And(temp);
        temp.UpdateValue(result);

        if (result.EqualsZero())
            ctx.Regs.P.SetZero();
        else
            ctx.Regs.P.ClearZero();
    }

    /////////////////////////////////////////////////////////////////////////////
    // TRB - Test and Reset Bits in Memory
    // (A & M) -> Z, (~A & M) -> M
    // N V B D I Z C
    // - - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // zeropage       TRB oper      14      2      5  
    // absolute       TRB oper      1C      3      6  
    /////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0x14] TRB zeropage
    // -------------------------------------------------------------------------
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstTRBzpg3);
    }

    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            var result = ctx.Regs.A.And(ctx.Regs.Temp);
            ctx.Regs.P.Zero.UpdateValue(result.EqualsZero());
        }
        ctx.AdvanceState(States.InstTRBzpg4);
    }

    public void Zpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            TestAndReset(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    public void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x1C] TRB absolute
    // -------------------------------------------------------------------------
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstTRBabs3);
    }

    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstTRBabs4);
    }

    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstTRBabs5);
    }


    public void Abs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            TestAndReset(ctx);

        ctx.AdvanceState(States.InstTRBabs6);
    }

    public void Abs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
