namespace Sim6502.Instructions;

/// <summary>
/// Implements STP (Stop the processor) instruction for W65c02s.
/// </summary>
public class InstSTP : InstBase
{
    public InstSTP(IT2Registry it2Registry, IStateRegistry stateRegistry)
        : base(it2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.STPimp, States.InstSTPimp2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstSTPimp2, Imp2);
        stateRegistry.Map(States.InstSTPimp2, Imp3);
    }

    /////////////////////////////////////////////////////////////////////////////
    // STP - Stop
    // 1 -> phi2
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        STP           DB      1      3 
    /////////////////////////////////////////////////////////////////////////////

    private void Imp2(Context ctx)
    {
        ctx.AdvanceState(States.InstSTPimp3);
    }

    private void Imp3(Context ctx)
    {
        ctx.AdvanceState(States.Stop);
    }
}