namespace W65C02S.Engine;

internal interface IStateRegistry
{
    void Map(States state, Action<Context> method);
}