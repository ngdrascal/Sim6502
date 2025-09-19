namespace Sim6502.Instructions;

public class InstTRB : InstBase2, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.TRBzpg, States.InstTRBzpg2);
        registry.Map(OpCodes.TRBabs, States.InstTRBabs2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstTRBzpg2, Zpg2);
        registry.Map(States.InstTRBzpg3, Zpg3);
        registry.Map(States.InstTRBzpg4, Zpg4);
        registry.Map(States.InstTRBzpg5, Zpg5);

        registry.Map(States.InstTRBabs2, Abs2);
        registry.Map(States.InstTRBabs3, Abs3);
        registry.Map(States.InstTRBabs4, Abs4);
        registry.Map(States.InstTRBabs5, Abs5);
        registry.Map(States.InstTRBabs6, Abs6);

        return this;
    }

    private void TestAndReset(Context ctx)
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
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstTRBzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            var result = ctx.Regs.A.And(ctx.Regs.Temp);
            ctx.Regs.P.Zero.UpdateValue(result.EqualsZero());
        }

        ctx.AdvanceState(States.InstTRBzpg4);
    }

    private void Zpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            TestAndReset(ctx);

        ctx.AdvanceState(States.InstTRBzpg5);
    }

    private void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x1C] TRB absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstTRBabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstTRBabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstTRBabs5);
    }


    private void Abs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            TestAndReset(ctx);

        ctx.AdvanceState(States.InstTRBabs6);
    }

    private void Abs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
