namespace Sim6502;

public class StateChangingEventArgs : EventArgs
{
    public States NewState { get; }

    public StateChangingEventArgs(States newState)
    {
        NewState = newState;
    }
}