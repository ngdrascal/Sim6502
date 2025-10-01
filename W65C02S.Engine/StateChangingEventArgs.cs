namespace W65C02S.Engine;

public class StateChangingEventArgs : EventArgs
{
    public States NewState { get; }

    public StateChangingEventArgs(States newState)
    {
        NewState = newState;
    }
}