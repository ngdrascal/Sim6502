namespace Sim6502.Instructions;

public class InstWAI : InstBase
{
    public InstWAI(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.WAIimp, States.InstWAIimp2);
    }

    protected override void RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstWAIimp2, ctx => Imp2(ctx));
        registry.Map(States.InstWAIimp3, ctx => Imp3(ctx));
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

    public void Imp2(Context ctx)
    {
        ctx.AdvanceState(States.InstWAIimp3);
    }

    public void Imp3(Context ctx)
    {
        ctx.AdvanceState(States.WaitForInterrupt);
    }
}