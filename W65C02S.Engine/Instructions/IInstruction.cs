namespace W65C02S.Engine;

internal interface IInstruction
{
    IInstruction RegisterT2State(IT2Registry registry);

    IInstruction RegisterStates(IStateRegistry registry);
}