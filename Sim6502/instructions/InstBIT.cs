namespace Sim6502.Instructions;

public class InstBIT : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.BITimm, States.InstBITimm2);
        registry.Map(OpCodes.BITzpg, States.InstBITzpg2);
        registry.Map(OpCodes.BITzpgx, States.InstBITzpgx2);
        registry.Map(OpCodes.BITabs, States.InstBITabs2);
        registry.Map(OpCodes.BITabsx, States.InstBITabsx2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstBITimm2, Imm2);
        stateRegistry.Map(States.InstBITzpg2, Zpg2);
        stateRegistry.Map(States.InstBITzpg3, Zpg3);
        stateRegistry.Map(States.InstBITzpgx2, Zpgx2);
        stateRegistry.Map(States.InstBITzpgx3, Zpgx3);
        stateRegistry.Map(States.InstBITzpgx4, Zpgx4);
        stateRegistry.Map(States.InstBITabs2, Abs2);
        stateRegistry.Map(States.InstBITabs3, Abs3);
        stateRegistry.Map(States.InstBITabs4, Abs4);
        stateRegistry.Map(States.InstBITabsx2, Absx2);
        stateRegistry.Map(States.InstBITabsx3, Absx3);
        stateRegistry.Map(States.InstBITabsx4, Absx4);
        stateRegistry.Map(States.InstBITabsx5, Absx5);

        return this;
    }

    private void AndAWithTempSetFlags(Context ctx)
    {
        var p = ctx.Regs.P;
        var memValue = ctx.Regs.Temp;
        if (memValue.IsBitSet(7))
            p.SetNegative();
        else
            p.ClearNegative();
        if (memValue.IsBitSet(6))
            p.SetOverflow();
        else
            p.ClearOverflow();
        var andResult = ctx.Regs.A.Copy().And(memValue);
        if (andResult.EqualsZero())
            p.SetZero();
        else
            p.ClearZero();
    }

    /////////////////////////////////////////////////////////////////////////////
    // BIT - Test Bits in Memory with Accumulator
    // A AND M, M7 -> N, M6 -> V
    // N V B D I Z C
    // + + - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // immediate      BIT #oper     89      2      3
    // zeropage       BIT oper      24      2      3
    // zeropage,X     BIT oper,X    34      2      3
    // absolute       BIT oper      2C      3      4
    // absolute,X     BIT oper,X    3C      3      4
    /////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0x89] BIT immediate
    // -------------------------------------------------------------------------
    private void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            ctx.DbgOperand1 = data;
            // NOTE: in the imm addressing mode only the Z flag is effected.  Unlike the other
            // addressing modes the V and N flags are unaffected.
            var p = ctx.Regs.P;
            var memValue = ctx.Regs.Temp;
            var andResult = ctx.Regs.A.Copy().And(memValue);
            if (andResult.EqualsZero())
                p.SetZero();
            else
                p.ClearZero();
        }

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x24] BIT zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstBITzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AndAWithTempSetFlags(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x34] BIT zeropage,X
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstBITzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstBITzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AndAWithTempSetFlags(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x2C] BIT absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstBITabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstBITabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AndAWithTempSetFlags(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x3C] BIT absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstBITabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstBITabsx4);
    }

    private void Absx4(Context ctx)
    {
        var nextState = States.Fetch;

        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var beforePage = ctx.Regs.EA.Msb().Copy();
            ctx.Regs.IncEAWithX();
            var afterPage = ctx.Regs.EA.Msb().Copy();
            ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
            ctx.Pins.AddrBus = ctx.Regs.EA;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);

            // if adding Y did not cross the page boundary, then a fifth cycle is not needed
            if (!ctx.CrossedPageBoundary)
            {
                AndAWithTempSetFlags(ctx);
            }
            else
                // adding Y crossed a page boundary, execute a fifth cycle
                nextState = States.InstBITabsx5;
        }

        ctx.AdvanceState(nextState);
    }

    private void Absx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            AndAWithTempSetFlags(ctx);
        }

        ctx.AdvanceState(States.Fetch);
    }
}