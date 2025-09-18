namespace Sim6502.Instructions;

public interface IInstruction
{
    IInstruction RegisterT2State(IT2Registry registry);

    IInstruction RegisterStates(IStateRegistry registry);
}