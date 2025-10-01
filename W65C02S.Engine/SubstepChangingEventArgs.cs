namespace W65C02S.Engine;

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