namespace W65C02S.Engine;

internal class InstBitMemory : InstBase, IInstruction
{
    // one entry per opcode: the eight RMB and eight SMB variants differ only in bit and direction
    private readonly record struct Variant(
        OpCodes OpCode, int Bit, bool Set, States Zpg2, States Zpg3, States Zpg4, States Zpg5);

    private static readonly Variant[] Variants =
    [
        new(OpCodes.RMB0zpg, 0, false, States.InstRMB0zpg2, States.InstRMB0zpg3, States.InstRMB0zpg4,
            States.InstRMB0zpg5),
        new(OpCodes.RMB1zpg, 1, false, States.InstRMB1zpg2, States.InstRMB1zpg3, States.InstRMB1zpg4,
            States.InstRMB1zpg5),
        new(OpCodes.RMB2zpg, 2, false, States.InstRMB2zpg2, States.InstRMB2zpg3, States.InstRMB2zpg4,
            States.InstRMB2zpg5),
        new(OpCodes.RMB3zpg, 3, false, States.InstRMB3zpg2, States.InstRMB3zpg3, States.InstRMB3zpg4,
            States.InstRMB3zpg5),
        new(OpCodes.RMB4zpg, 4, false, States.InstRMB4zpg2, States.InstRMB4zpg3, States.InstRMB4zpg4,
            States.InstRMB4zpg5),
        new(OpCodes.RMB5zpg, 5, false, States.InstRMB5zpg2, States.InstRMB5zpg3, States.InstRMB5zpg4,
            States.InstRMB5zpg5),
        new(OpCodes.RMB6zpg, 6, false, States.InstRMB6zpg2, States.InstRMB6zpg3, States.InstRMB6zpg4,
            States.InstRMB6zpg5),
        new(OpCodes.RMB7zpg, 7, false, States.InstRMB7zpg2, States.InstRMB7zpg3, States.InstRMB7zpg4,
            States.InstRMB7zpg5),
        new(OpCodes.SMB0zpg, 0, true, States.InstSMB0zpg2, States.InstSMB0zpg3, States.InstSMB0zpg4,
            States.InstSMB0zpg5),
        new(OpCodes.SMB1zpg, 1, true, States.InstSMB1zpg2, States.InstSMB1zpg3, States.InstSMB1zpg4,
            States.InstSMB1zpg5),
        new(OpCodes.SMB2zpg, 2, true, States.InstSMB2zpg2, States.InstSMB2zpg3, States.InstSMB2zpg4,
            States.InstSMB2zpg5),
        new(OpCodes.SMB3zpg, 3, true, States.InstSMB3zpg2, States.InstSMB3zpg3, States.InstSMB3zpg4,
            States.InstSMB3zpg5),
        new(OpCodes.SMB4zpg, 4, true, States.InstSMB4zpg2, States.InstSMB4zpg3, States.InstSMB4zpg4,
            States.InstSMB4zpg5),
        new(OpCodes.SMB5zpg, 5, true, States.InstSMB5zpg2, States.InstSMB5zpg3, States.InstSMB5zpg4,
            States.InstSMB5zpg5),
        new(OpCodes.SMB6zpg, 6, true, States.InstSMB6zpg2, States.InstSMB6zpg3, States.InstSMB6zpg4,
            States.InstSMB6zpg5),
        new(OpCodes.SMB7zpg, 7, true, States.InstSMB7zpg2, States.InstSMB7zpg3, States.InstSMB7zpg4,
            States.InstSMB7zpg5)
    ];

    public IInstruction RegisterT2State(IT2Registry registry)
    {
        foreach (var variant in Variants)
            registry.Map(variant.OpCode, variant.Zpg2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        foreach (var variant in Variants)
        {
            registry.Map(variant.Zpg2, ctx => Zpg2(ctx, variant));
            registry.Map(variant.Zpg3, ctx => Zpg3(ctx, variant));
            registry.Map(variant.Zpg4, ctx => Zpg4(ctx, variant));
            registry.Map(variant.Zpg5, Zpg5);
        }

        return this;
    }

    /////////////////////////////////////////////////////////////////////////////
    // RMBn - Reset Memory Bit n          SMBn - Set Memory Bit n
    // 0 -> Mn                            1 -> Mn
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // zeropage       RMBn oper     n7      2      5     (n = 0..7)
    // zeropage       SMBn oper     n7+80   2      5
    /////////////////////////////////////////////////////////////////////////////
    private void Zpg2(Context ctx, Variant variant)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(variant.Zpg3);
    }

    private void Zpg3(Context ctx, Variant variant)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(variant.Zpg4);
    }

    private static void Zpg4(Context ctx, Variant variant)
    {
        // internal operation: the address bus stays on the zero page address (dummy read)
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.Temp = variant.Set ? ctx.Regs.Temp.SetBit(variant.Bit) : ctx.Regs.Temp.ClearBit(variant.Bit);

        ctx.AdvanceState(variant.Zpg5);
    }

    private void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
