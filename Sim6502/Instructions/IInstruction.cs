namespace Sim6502.Instructions;

internal interface IInstruction
{
    IInstruction RegisterT2State(IT2Registry registry);

    IInstruction RegisterStates(IStateRegistry registry);
}