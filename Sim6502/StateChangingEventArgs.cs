namespace Sim6502;

internal class StateChangingEventArgs : EventArgs
{
    public States NewState { get; }

    public StateChangingEventArgs(States newState)
    {
        NewState = newState;
    }
}