namespace W65C02S.Engine;

internal class Interrupts : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.Interrupt1, Interrupt1);

        return this;
    }

    /////////////////////////////////////////////////////////////////////////////
    // Interrupt (both NMI and IRQ)
    /////////////////////////////////////////////////////////////////////////////
    private void Interrupt1(Context ctx)
    {
        // load operand into the temp reg., then ignore it, don't advance the PC
        LoadTempFromPcDontAdvancePc(ctx);

        ctx.AdvanceState(States.InstBRKimp2);
    }
}
