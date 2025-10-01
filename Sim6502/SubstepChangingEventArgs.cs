namespace Sim6502;

public class SubstepChangingEventArgs : EventArgs
{
    public States State { get; }

    public int Substep { get; }

    public SubstepChangingEventArgs(States state, int substep)
    {
        State = state;
        Substep = substep;
    }
}