namespace Sim6502;

public class StateChangingEventArgs : EventArgs
{
    public States OldState { get; }

    public States NewState { get; }

    public StateChangingEventArgs(States oldState, States newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}