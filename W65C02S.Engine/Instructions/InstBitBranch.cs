namespace W65C02S.Engine;

internal class InstBitBranch : InstBase, IInstruction
{
    // one entry per opcode: the eight BBR and eight BBS variants differ only in bit and polarity.
    // States holds the cycle 2..7 states in order.
    private readonly record struct Variant(OpCodes OpCode, int Bit, bool BranchIfSet, States[] States);

    private static readonly Variant[] Variants =
    [
        new(OpCodes.BBR0zpgrel, 0, false,
            [States.InstBBR0zpgrel2, States.InstBBR0zpgrel3, States.InstBBR0zpgrel4, States.InstBBR0zpgrel5,
             States.InstBBR0zpgrel6, States.InstBBR0zpgrel7]),
        new(OpCodes.BBR1zpgrel, 1, false,
            [States.InstBBR1zpgrel2, States.InstBBR1zpgrel3, States.InstBBR1zpgrel4, States.InstBBR1zpgrel5,
             States.InstBBR1zpgrel6, States.InstBBR1zpgrel7]),
        new(OpCodes.BBR2zpgrel, 2, false,
            [States.InstBBR2zpgrel2, States.InstBBR2zpgrel3, States.InstBBR2zpgrel4, States.InstBBR2zpgrel5,
             States.InstBBR2zpgrel6, States.InstBBR2zpgrel7]),
        new(OpCodes.BBR3zpgrel, 3, false,
            [States.InstBBR3zpgrel2, States.InstBBR3zpgrel3, States.InstBBR3zpgrel4, States.InstBBR3zpgrel5,
             States.InstBBR3zpgrel6, States.InstBBR3zpgrel7]),
        new(OpCodes.BBR4zpgrel, 4, false,
            [States.InstBBR4zpgrel2, States.InstBBR4zpgrel3, States.InstBBR4zpgrel4, States.InstBBR4zpgrel5,
             States.InstBBR4zpgrel6, States.InstBBR4zpgrel7]),
        new(OpCodes.BBR5zpgrel, 5, false,
            [States.InstBBR5zpgrel2, States.InstBBR5zpgrel3, States.InstBBR5zpgrel4, States.InstBBR5zpgrel5,
             States.InstBBR5zpgrel6, States.InstBBR5zpgrel7]),
        new(OpCodes.BBR6zpgrel, 6, false,
            [States.InstBBR6zpgrel2, States.InstBBR6zpgrel3, States.InstBBR6zpgrel4, States.InstBBR6zpgrel5,
             States.InstBBR6zpgrel6, States.InstBBR6zpgrel7]),
        new(OpCodes.BBR7zpgrel, 7, false,
            [States.InstBBR7zpgrel2, States.InstBBR7zpgrel3, States.InstBBR7zpgrel4, States.InstBBR7zpgrel5,
             States.InstBBR7zpgrel6, States.InstBBR7zpgrel7]),
        new(OpCodes.BBS0zpgrel, 0, true,
            [States.InstBBS0zpgrel2, States.InstBBS0zpgrel3, States.InstBBS0zpgrel4, States.InstBBS0zpgrel5,
             States.InstBBS0zpgrel6, States.InstBBS0zpgrel7]),
        new(OpCodes.BBS1zpgrel, 1, true,
            [States.InstBBS1zpgrel2, States.InstBBS1zpgrel3, States.InstBBS1zpgrel4, States.InstBBS1zpgrel5,
             States.InstBBS1zpgrel6, States.InstBBS1zpgrel7]),
        new(OpCodes.BBS2zpgrel, 2, true,
            [States.InstBBS2zpgrel2, States.InstBBS2zpgrel3, States.InstBBS2zpgrel4, States.InstBBS2zpgrel5,
             States.InstBBS2zpgrel6, States.InstBBS2zpgrel7]),
        new(OpCodes.BBS3zpgrel, 3, true,
            [States.InstBBS3zpgrel2, States.InstBBS3zpgrel3, States.InstBBS3zpgrel4, States.InstBBS3zpgrel5,
             States.InstBBS3zpgrel6, States.InstBBS3zpgrel7]),
        new(OpCodes.BBS4zpgrel, 4, true,
            [States.InstBBS4zpgrel2, States.InstBBS4zpgrel3, States.InstBBS4zpgrel4, States.InstBBS4zpgrel5,
             States.InstBBS4zpgrel6, States.InstBBS4zpgrel7]),
        new(OpCodes.BBS5zpgrel, 5, true,
            [States.InstBBS5zpgrel2, States.InstBBS5zpgrel3, States.InstBBS5zpgrel4, States.InstBBS5zpgrel5,
             States.InstBBS5zpgrel6, States.InstBBS5zpgrel7]),
        new(OpCodes.BBS6zpgrel, 6, true,
            [States.InstBBS6zpgrel2, States.InstBBS6zpgrel3, States.InstBBS6zpgrel4, States.InstBBS6zpgrel5,
             States.InstBBS6zpgrel6, States.InstBBS6zpgrel7]),
        new(OpCodes.BBS7zpgrel, 7, true,
            [States.InstBBS7zpgrel2, States.InstBBS7zpgrel3, States.InstBBS7zpgrel4, States.InstBBS7zpgrel5,
             States.InstBBS7zpgrel6, States.InstBBS7zpgrel7])
    ];

    public IInstruction RegisterT2State(IT2Registry registry)
    {
        foreach (var variant in Variants)
            registry.Map(variant.OpCode, variant.States[0]);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        foreach (var variant in Variants)
        {
            var s = variant.States;
            registry.Map(s[0], ctx => ZpgRel2(ctx, s[1]));
            registry.Map(s[1], ctx => ZpgRel3(ctx, s[2]));
            registry.Map(s[2], ctx => ZpgRel4(ctx, s[3]));
            registry.Map(s[3], ctx => ZpgRel5(ctx, variant));
            registry.Map(s[4], ctx => Branch3(ctx, s[5]));
            registry.Map(s[5], ctx => Branch4(ctx, true));
        }

        return this;
    }

    /////////////////////////////////////////////////////////////////////////////
    // BBRn - Branch on Bit n Reset       BBSn - Branch on Bit n Set
    // branch on Mn = 0                   branch on Mn = 1
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler          opc     bytes  cycles
    // -------------------------------------------------------
    // zeropage,rel   BBRn oper,offset   nF        3    5+t+p   (n = 0..7)
    // zeropage,rel   BBSn oper,offset   nF+80     3    5+t+p
    //
    // Notes: t: =1 if the branch is taken, p: =1 if the branch crosses a page.
    /////////////////////////////////////////////////////////////////////////////
    private void ZpgRel2(Context ctx, States next)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(next);
    }

    private void ZpgRel3(Context ctx, States next)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(next);
    }

    private static void ZpgRel4(Context ctx, States next)
    {
        // internal operation: the address bus stays on the zero page address (dummy read)
        ctx.AdvanceState(next);
    }

    private void ZpgRel5(Context ctx, Variant variant)
    {
        // test the bit read in cycle 3 before Temp is replaced by the branch offset
        var taken = ctx.Regs.Temp.IsBitSet(variant.Bit) == variant.BranchIfSet;
        var zpAddr = ctx.DbgOperand1;
        LoadTempFromPC(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            ctx.DbgOperand2 = ctx.DbgOperand1;
            ctx.DbgOperand1 = zpAddr;
        }

        ctx.AdvanceState(taken ? variant.States[4] : States.Fetch);
    }
}
