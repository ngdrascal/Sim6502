namespace Sim6502.Instructions;

internal class InstWAI : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.WAIimp, States.InstWAIimp2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstWAIimp2, Imp2);
        registry.Map(States.InstWAIimp3, Imp3);

        return this;
    }

    /////////////////////////////////////////////////////////////////////////////
    // WAI - Wait for interrupt
    // 0 -> RDY
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        WAI           CB      1      3 
    /////////////////////////////////////////////////////////////////////////////

    private void Imp2(Context ctx)
    {
        ctx.AdvanceState(States.InstWAIimp3);
    }

    private void Imp3(Context ctx)
    {
        ctx.AdvanceState(States.WaitForInterrupt);
    }
}