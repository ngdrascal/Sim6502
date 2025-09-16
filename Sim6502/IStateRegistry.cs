namespace Sim6502;

public interface IStateRegistry
{
    void Map(States state, Action<Context> method);
}