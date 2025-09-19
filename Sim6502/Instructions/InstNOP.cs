// File-scoped namespace for Sim6502.Instructions
namespace Sim6502.Instructions;

public class InstNOP : InstBase2, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.NOP02, States.InstNOP2);
        registry.Map(OpCodes.NOP03, States.Fetch);
        registry.Map(OpCodes.NOP0B, States.Fetch);
        registry.Map(OpCodes.NOP13, States.Fetch);
        registry.Map(OpCodes.NOP1B, States.Fetch);
        registry.Map(OpCodes.NOP22, States.InstNOP2);
        registry.Map(OpCodes.NOP23, States.Fetch);
        registry.Map(OpCodes.NOP2B, States.Fetch);
        registry.Map(OpCodes.NOP33, States.Fetch);
        registry.Map(OpCodes.NOP3B, States.Fetch);
        registry.Map(OpCodes.NOP42, States.InstNOP2);
        registry.Map(OpCodes.NOP43, States.Fetch);
        registry.Map(OpCodes.NOP44, States.InstNOP3);
        registry.Map(OpCodes.NOP4B, States.Fetch);
        registry.Map(OpCodes.NOP53, States.Fetch);
        registry.Map(OpCodes.NOP54, States.InstNOP4);
        registry.Map(OpCodes.NOP5B, States.Fetch);
        registry.Map(OpCodes.NOP5C, States.InstNOP8);
        registry.Map(OpCodes.NOP62, States.InstNOP2);
        registry.Map(OpCodes.NOP63, States.Fetch);
        registry.Map(OpCodes.NOP6B, States.Fetch);
        registry.Map(OpCodes.NOP73, States.Fetch);
        registry.Map(OpCodes.NOP7B, States.Fetch);
        registry.Map(OpCodes.NOP82, States.InstNOP2);
        registry.Map(OpCodes.NOP83, States.Fetch);
        registry.Map(OpCodes.NOP8B, States.Fetch);
        registry.Map(OpCodes.NOP93, States.Fetch);
        registry.Map(OpCodes.NOP9B, States.Fetch);
        registry.Map(OpCodes.NOPA3, States.Fetch);
        registry.Map(OpCodes.NOPAB, States.Fetch);
        registry.Map(OpCodes.NOPB3, States.Fetch);
        registry.Map(OpCodes.NOPBB, States.Fetch);
        registry.Map(OpCodes.NOPC2, States.InstNOP2);
        registry.Map(OpCodes.NOPC3, States.Fetch);
        registry.Map(OpCodes.NOPD3, States.Fetch);
        registry.Map(OpCodes.NOPD4, States.InstNOP4);
        registry.Map(OpCodes.NOPDC, States.InstNOP4);
        registry.Map(OpCodes.NOPE3, States.Fetch);
        registry.Map(OpCodes.NOP, States.InstNOP2);
        registry.Map(OpCodes.NOPEB, States.Fetch);
        registry.Map(OpCodes.NOPF3, States.Fetch);
        registry.Map(OpCodes.NOPF4, States.InstNOP4);
        registry.Map(OpCodes.NOPFB, States.Fetch);
        registry.Map(OpCodes.NOPFC, States.InstNOP4);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstNOP2, Nop2);
        registry.Map(States.InstNOP3, Nop3);
        registry.Map(States.InstNOP4, Nop4);
        registry.Map(States.InstNOP5, Nop5);
        registry.Map(States.InstNOP6, Nop6);
        registry.Map(States.InstNOP7, Nop7);
        registry.Map(States.InstNOP8, Nop8);

        return this;
    }

    /////////////////////////////////////////////////////////////////////////////
    // NOP - No Operation
    // M -> A
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        NOP           EA      1      2  
    /////////////////////////////////////////////////////////////////////////////
    private void Nop2(Context ctx)
    {
        ctx.AdvanceState(States.Fetch);
    }

    private void Nop3(Context ctx)
    {
        ctx.AdvanceState(States.InstNOP2);
    }

    private void Nop4(Context ctx)
    {
        ctx.AdvanceState(States.InstNOP3);
    }

    private void Nop5(Context ctx)
    {
        ctx.AdvanceState(States.InstNOP4);
    }

    private void Nop6(Context ctx)
    {
        ctx.AdvanceState(States.InstNOP5);
    }

    private void Nop7(Context ctx)
    {
        ctx.AdvanceState(States.InstNOP6);
    }

    private void Nop8(Context ctx)
    {
        ctx.AdvanceState(States.InstNOP7);
    }
}
