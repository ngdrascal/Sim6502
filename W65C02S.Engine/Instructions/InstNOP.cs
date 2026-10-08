namespace W65C02S.Engine;

internal class InstNOP : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.NOP, States.InstNOP2);

        // reserved 1 byte, 1 cycle: the next cycle is already the next opcode fetch
        registry.Map(OpCodes.NOP03, States.Fetch);
        registry.Map(OpCodes.NOP0B, States.Fetch);
        registry.Map(OpCodes.NOP13, States.Fetch);
        registry.Map(OpCodes.NOP1B, States.Fetch);
        registry.Map(OpCodes.NOP23, States.Fetch);
        registry.Map(OpCodes.NOP2B, States.Fetch);
        registry.Map(OpCodes.NOP33, States.Fetch);
        registry.Map(OpCodes.NOP3B, States.Fetch);
        registry.Map(OpCodes.NOP43, States.Fetch);
        registry.Map(OpCodes.NOP4B, States.Fetch);
        registry.Map(OpCodes.NOP53, States.Fetch);
        registry.Map(OpCodes.NOP5B, States.Fetch);
        registry.Map(OpCodes.NOP63, States.Fetch);
        registry.Map(OpCodes.NOP6B, States.Fetch);
        registry.Map(OpCodes.NOP73, States.Fetch);
        registry.Map(OpCodes.NOP7B, States.Fetch);
        registry.Map(OpCodes.NOP83, States.Fetch);
        registry.Map(OpCodes.NOP8B, States.Fetch);
        registry.Map(OpCodes.NOP93, States.Fetch);
        registry.Map(OpCodes.NOP9B, States.Fetch);
        registry.Map(OpCodes.NOPA3, States.Fetch);
        registry.Map(OpCodes.NOPAB, States.Fetch);
        registry.Map(OpCodes.NOPB3, States.Fetch);
        registry.Map(OpCodes.NOPBB, States.Fetch);
        registry.Map(OpCodes.NOPC3, States.Fetch);
        registry.Map(OpCodes.NOPD3, States.Fetch);
        registry.Map(OpCodes.NOPE3, States.Fetch);
        registry.Map(OpCodes.NOPEB, States.Fetch);
        registry.Map(OpCodes.NOPF3, States.Fetch);
        registry.Map(OpCodes.NOPFB, States.Fetch);

        // reserved 2 bytes, 2 cycles
        registry.Map(OpCodes.NOP02, States.InstNOPimm2);
        registry.Map(OpCodes.NOP22, States.InstNOPimm2);
        registry.Map(OpCodes.NOP42, States.InstNOPimm2);
        registry.Map(OpCodes.NOP62, States.InstNOPimm2);
        registry.Map(OpCodes.NOP82, States.InstNOPimm2);
        registry.Map(OpCodes.NOPC2, States.InstNOPimm2);
        registry.Map(OpCodes.NOPE2, States.InstNOPimm2);

        // reserved 2 bytes, 3 cycles
        registry.Map(OpCodes.NOP44, States.InstNOPzpg2);

        // reserved 2 bytes, 4 cycles
        registry.Map(OpCodes.NOP54, States.InstNOPzpgx2);
        registry.Map(OpCodes.NOPD4, States.InstNOPzpgx2);
        registry.Map(OpCodes.NOPF4, States.InstNOPzpgx2);

        // reserved 3 bytes, 4 cycles
        registry.Map(OpCodes.NOPDC, States.InstNOPabsx2);
        registry.Map(OpCodes.NOPFC, States.InstNOPabsx2);

        // reserved 3 bytes, 8 cycles
        registry.Map(OpCodes.NOP5C, States.InstNOP5C2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstNOP2, Nop2);

        registry.Map(States.InstNOPimm2, NopImm2);

        registry.Map(States.InstNOPzpg2, NopZpg2);
        registry.Map(States.InstNOPzpg3, NopZpg3);

        registry.Map(States.InstNOPzpgx2, NopZpgx2);
        registry.Map(States.InstNOPzpgx3, NopZpgx3);
        registry.Map(States.InstNOPzpgx4, NopZpgx4);

        registry.Map(States.InstNOPabsx2, NopAbsx2);
        registry.Map(States.InstNOPabsx3, NopAbsx3);
        registry.Map(States.InstNOPabsx4, NopAbsx4);

        registry.Map(States.InstNOP5C2, Nop5C2);
        registry.Map(States.InstNOP5C3, Nop5C3);
        registry.Map(States.InstNOP5C4, Nop5C4);
        registry.Map(States.InstNOP5C5, ctx => ctx.AdvanceState(States.InstNOP5C6));
        registry.Map(States.InstNOP5C6, ctx => ctx.AdvanceState(States.InstNOP5C7));
        registry.Map(States.InstNOP5C7, ctx => ctx.AdvanceState(States.InstNOP5C8));
        registry.Map(States.InstNOP5C8, ctx => ctx.AdvanceState(States.Fetch));

        return this;
    }

    /////////////////////////////////////////////////////////////////////////////
    // NOP - No Operation
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        NOP           EA      1      2
    //
    // Reserved opcodes execute as NOPs that consume operand bytes and do dummy reads.
    // Lengths and cycle counts are fixed by WDC; the per-cycle bus follows MAME's W65C02S
    // model. Other write-ups describe different dummy-read addresses for $5C, $DC and $FC.
    //
    // reserved       x3, xB                1      1
    // reserved       02 22 42 62 82 C2 E2  2      2    read operand
    // reserved       44                    2      3    read zp
    // reserved       54 D4 F4              2      4    re-read operand, read (zp+X) & $FF
    // reserved       DC FC                 3      4    re-read operand high byte
    // reserved       5C                    3      8    5 reads at the next opcode address
    /////////////////////////////////////////////////////////////////////////////
    private void Nop2(Context ctx)
    {
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // 2 bytes, 2 cycles
    // -------------------------------------------------------------------------
    private void NopImm2(Context ctx)
    {
        LoadTempFromPC(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // 2 bytes, 3 cycles
    // -------------------------------------------------------------------------
    private void NopZpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstNOPzpg3);
    }

    private void NopZpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // 2 bytes, 4 cycles
    // -------------------------------------------------------------------------
    private void NopZpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstNOPzpgx3);
    }

    private void NopZpgx3(Context ctx)
    {
        // internal operation: the address bus stays on the operand (dummy read) while X is
        // added to the zero page address, wrapping within page 0
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstNOPzpgx4);
    }

    private void NopZpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // 3 bytes, 4 cycles
    // -------------------------------------------------------------------------
    private void NopAbsx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstNOPabsx3);
    }

    private void NopAbsx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstNOPabsx4);
    }

    private static void NopAbsx4(Context ctx)
    {
        // internal operation: the address bus stays on the operand high byte (dummy read)
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // 3 bytes, 8 cycles
    // -------------------------------------------------------------------------
    private void Nop5C2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstNOP5C3);
    }

    private void Nop5C3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstNOP5C4);
    }

    private static void Nop5C4(Context ctx)
    {
        // dummy reads at the next opcode address; the bus stays there through cycle 8
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.PC;
            ctx.Pins.RWB = Read;
        }

        ctx.AdvanceState(States.InstNOP5C5);
    }
}
