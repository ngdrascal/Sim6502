namespace Sim6502.Instructions;

/// <summary>
/// Implements STP (Stop the processor) instruction for W65c02s.
/// </summary>
public class InstSTP : InstBase2, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.STPimp, States.InstSTPimp2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstSTPimp2, Imp2);
        stateRegistry.Map(States.InstSTPimp3, Imp3);

        return this;
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