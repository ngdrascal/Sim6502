namespace Sim6502;

internal interface IStateRegistry
{
    void Map(States state, Action<Context> method);
}