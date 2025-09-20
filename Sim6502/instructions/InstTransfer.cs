namespace Sim6502.Instructions;

public class InstTransfer : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.TAXimp, States.InstTAXimp2);
        registry.Map(OpCodes.TAYimp, States.InstTAYimp2);
        registry.Map(OpCodes.TSXimp, States.InstTSXimp2);
        registry.Map(OpCodes.TXAimp, States.InstTXAimp2);
        registry.Map(OpCodes.TXSimp, States.InstTXSimp2);
        registry.Map(OpCodes.TYAimp, States.InstTYAimp2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstTAXimp2, InstTAXimp2);
        registry.Map(States.InstTAYimp2, InstTAYimp2);
        registry.Map(States.InstTSXimp2, InstTSXimp2);
        registry.Map(States.InstTXAimp2, InstTXAimp2);
        registry.Map(States.InstTXSimp2, InstTXSimp2);
        registry.Map(States.InstTYAimp2, InstTYAimp2);

        return this;
    }

    /////////////////////////////////////////////////////////////////////////////
    // TAX - Transfer Accumulator to Index X
    // A -> X
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        TAX           AA      1      2
    /////////////////////////////////////////////////////////////////////////////
    private void InstTAXimp2(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.SetXUpdateFlags(ctx.Regs.A);

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // TAY - Transfer Accumulator to Index Y
    // A -> Y
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        TAY           A8      1      2
    /////////////////////////////////////////////////////////////////////////////
    private void InstTAYimp2(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.SetYUpdateFlags(ctx.Regs.A);

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // TSX - Transfer Stack Pointer to Index X
    // SP -> X
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        TSX           BA      1      2
    /////////////////////////////////////////////////////////////////////////////
    private void InstTSXimp2(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.SetXUpdateFlags(ctx.Regs.S);

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // TXA - Transfer Index X to Accumulator
    // X -> A
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        TXA           8A      1      2
    /////////////////////////////////////////////////////////////////////////////
    private void InstTXAimp2(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.UpdateAUpdateFlags(ctx.Regs.X);

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // TXS - Transfer Index X to Stack Register
    // X -> SP
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        TXS           9A      1      2
    /////////////////////////////////////////////////////////////////////////////
    private void InstTXSimp2(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.S.UpdateValue(ctx.Regs.X);

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // TYA - Transfer Index Y to Accumulator
    // Y -> A
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        TSX           BA      1      2
    /////////////////////////////////////////////////////////////////////////////
    private void InstTYAimp2(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.UpdateAUpdateFlags(ctx.Regs.Y);

        ctx.AdvanceState(States.Fetch);
    }
}
