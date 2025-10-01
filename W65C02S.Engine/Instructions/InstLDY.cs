namespace W65C02S.Engine;

internal class InstLDY : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.LDYimm, States.InstLDYimm2);
        registry.Map(OpCodes.LDYzpg, States.InstLDYzpg2);
        registry.Map(OpCodes.LDYzpgx, States.InstLDYzpgx2);
        registry.Map(OpCodes.LDYabs, States.InstLDYabs2);
        registry.Map(OpCodes.LDYabsx, States.InstLDYabsx2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstLDYimm2, Imm2);
        registry.Map(States.InstLDYzpg2, Zpg2);
        registry.Map(States.InstLDYzpg3, Zpg3);
        registry.Map(States.InstLDYzpgx2, Zpgx2);
        registry.Map(States.InstLDYzpgx3, Zpgx3);
        registry.Map(States.InstLDYzpgx4, Zpgx4);
        registry.Map(States.InstLDYabs2, Abs2);
        registry.Map(States.InstLDYabs3, Abs3);
        registry.Map(States.InstLDYabs4, Abs4);
        registry.Map(States.InstLDYabsx2, Absx2);
        registry.Map(States.InstLDYabsx3, Absx3);
        registry.Map(States.InstLDYabsx4, Absx4);
        registry.Map(States.InstLDYabsx5, Absx5);

        return this;
    }

    private void LoadYFromEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.EA;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var y = ctx.Pins.DataBus;
            ctx.Regs.SetYUpdateFlags(y);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // LDY - Load Index Y with Memory
    // M -> Y
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // immediate      LDY #oper     A0      2      2  
    // zeropage       LDY oper      A4      2      3  
    // zeropage,X     LDY oper,X    B4      2      4  
    // absolute       LDY oper      AC      3      4  
    // absolute,X     LDY oper,X    BC      3      4+1 
    ///////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0xA0] LDY immediate
    // -------------------------------------------------------------------------
    private void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.SetYUpdateFlags(data);
            ctx.DbgOperand1 = data;
        }

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xA4] LDY zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstLDYzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadYFromEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xB4] LDY zeropage,X
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstLDYzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstLDYzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        LoadYFromEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xAC] LDY absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstLDYabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstLDYabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadYFromEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xBC] LDY absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstLDYabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstLDYabsx4);
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
            if (!ctx.CrossedPageBoundary)
                ctx.Regs.SetYUpdateFlags(data);
            else
                nextState = States.InstLDYabsx5;
        }

        ctx.AdvanceState(nextState);
    }

    private void Absx5(Context ctx)
    {
        LoadYFromEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
