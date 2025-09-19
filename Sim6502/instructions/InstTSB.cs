namespace Sim6502.Instructions;

public class InstTSB : InstBase2, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.TSBzpg, States.InstTSBzpg2);
        registry.Map(OpCodes.TSBabs, States.InstTSBabs2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstTSBzpg2, Zpg2);
        registry.Map(States.InstTSBzpg3, Zpg3);
        registry.Map(States.InstTSBzpg4, Zpg4);
        registry.Map(States.InstTSBzpg5, Zpg5);

        registry.Map(States.InstTSBabs2, Abs2);
        registry.Map(States.InstTSBabs3, Abs3);
        registry.Map(States.InstTSBabs4, Abs4);
        registry.Map(States.InstTSBabs5, Abs5);
        registry.Map(States.InstTSBabs6, Abs6);

        return this;
    }

    private void TestAndSet(Context ctx)
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
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstTSBzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstTSBzpg4);
    }

    private void Zpg4(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            TestAndSet(ctx);

        ctx.AdvanceState(States.InstTSBzpg5);
    }

    private void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x0C] TSB absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstTSBabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstTSBabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstTSBabs5);
    }

    private void Abs5(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            TestAndSet(ctx);

        ctx.AdvanceState(States.InstTSBabs6);
    }

    private void Abs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
