namespace Sim6502.Instructions;

internal class InstORA : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.ORAimm, States.InstORAimm2);
        registry.Map(OpCodes.ORAzpg, States.InstORAzpg2);
        registry.Map(OpCodes.ORAzpgx, States.InstORAzpgx2);
        registry.Map(OpCodes.ORAabs, States.InstORAabs2);
        registry.Map(OpCodes.ORAabsx, States.InstORAabsx2);
        registry.Map(OpCodes.ORAabsy, States.InstORAabsy2);
        registry.Map(OpCodes.ORAindx, States.InstORAindx2);
        registry.Map(OpCodes.ORAindy, States.InstORAindy2);
        registry.Map(OpCodes.ORAind, States.InstORAind2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstORAimm2, Imm2);

        stateRegistry.Map(States.InstORAzpg2, Zpg2);
        stateRegistry.Map(States.InstORAzpg3, Zpg3);
        stateRegistry.Map(States.InstORAzpgx2, Zpgx2);
        stateRegistry.Map(States.InstORAzpgx3, Zpgx3);
        stateRegistry.Map(States.InstORAzpgx4, Zpgx4);

        stateRegistry.Map(States.InstORAabs2, Abs2);
        stateRegistry.Map(States.InstORAabs3, Abs3);
        stateRegistry.Map(States.InstORAabs4, Abs4);

        stateRegistry.Map(States.InstORAabsx2, Absx2);
        stateRegistry.Map(States.InstORAabsx3, Absx3);
        stateRegistry.Map(States.InstORAabsx4, Absx4);
        stateRegistry.Map(States.InstORAabsx5, Absx5);

        stateRegistry.Map(States.InstORAabsy2, Absy2);
        stateRegistry.Map(States.InstORAabsy3, Absy3);
        stateRegistry.Map(States.InstORAabsy4, Absy4);
        stateRegistry.Map(States.InstORAabsy5, Absy5);

        stateRegistry.Map(States.InstORAindx2, Indx2);
        stateRegistry.Map(States.InstORAindx3, Indx3);
        stateRegistry.Map(States.InstORAindx4, Indx4);
        stateRegistry.Map(States.InstORAindx5, Indx5);
        stateRegistry.Map(States.InstORAindx6, Indx6);

        stateRegistry.Map(States.InstORAindy2, Indy2);
        stateRegistry.Map(States.InstORAindy3, Indy3);
        stateRegistry.Map(States.InstORAindy4, Indy4);
        stateRegistry.Map(States.InstORAindy5, Indy5);
        stateRegistry.Map(States.InstORAindy6, Indy6);

        stateRegistry.Map(States.InstORAind2, Ind2);
        stateRegistry.Map(States.InstORAind3, Ind3);
        stateRegistry.Map(States.InstORAind4, Ind4);
        stateRegistry.Map(States.InstORAind5, Ind5);

        return this;
    }

    protected void OrAWithTemp(Context ctx)
    {
        var memValue = ctx.Regs.Temp;
        var result = ctx.Regs.A.Copy().Or(memValue);
        ctx.Regs.UpdateAUpdateFlags(result);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // ORA - "OR" Memory with Accumulator
    // A OR M -> A
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // immediate      ORA #oper     09      2   2
    // zeropage       ORA oper      05      2   3
    // zeropage,X     ORA oper,X    15      2   4
    // absolute       ORA oper      0D      3   4
    // absolute,X     ORA oper,X    1D      3   4*
    // absolute,Y     ORA oper,Y    19      3   4*
    // (indirect,X)   ORA (oper,X)  01      2   6
    // (indirect),Y   ORA (oper),Y  11      2   5*
    // (indirect)     ORA (oper)    12      2   5
    ///////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0x09] ORA immediate
    // -------------------------------------------------------------------------
    private void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            var result = ctx.Regs.A.Copy().Or(data);
            ctx.Regs.UpdateAUpdateFlags(result);
            ctx.DbgOperand1 = data;
        }

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x05] ORA zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstORAzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            OrAWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x15] ORA zeropage,X
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstORAzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstORAzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            OrAWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x0D] ORA absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstORAabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstORAabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            OrAWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [01D] ORA absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstORAabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstORAabsx4);
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
            if (!ctx.CrossedPageBoundary)
            {
                OrAWithTemp(ctx);
            }
            else
                nextState = States.InstORAabsx5;
        }

        ctx.AdvanceState(nextState);
    }

    private void Absx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            OrAWithTemp(ctx);
        }

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x19] ORA absolute,Y
    // -------------------------------------------------------------------------
    private void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstORAabsy3);
    }

    private void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstORAabsy4);
    }

    private void Absy4(Context ctx)
    {
        var nextState = States.Fetch;
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var beforePage = ctx.Regs.EA.Msb().Copy();
            ctx.Regs.IncEAWithY();
            var afterPage = ctx.Regs.EA.Msb().Copy();
            ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
            ctx.Pins.AddrBus = ctx.Regs.EA;
            ctx.Pins.RWB = Read;
        }

        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            if (!ctx.CrossedPageBoundary)
            {
                OrAWithTemp(ctx);
            }
            else
                nextState = States.InstORAabsy5;
        }

        ctx.AdvanceState(nextState);
    }

    private void Absy5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            OrAWithTemp(ctx);
        }

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x01] ORA (indirect,X)
    // -------------------------------------------------------------------------
    private void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstORAindx3);
    }

    private void Indx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            ctx.Regs.EA.Lsb().AddWithWrapAround(ctx.Regs.X);
            ctx.Regs.EA.Msb().Zero();
        }

        ctx.AdvanceState(States.InstORAindx4);
    }

    private void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstORAindx5);
    }

    private void Indx5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstORAindx6);
    }

    private void Indx6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            OrAWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x11] ORA (indirect),Y
    // -------------------------------------------------------------------------
    private void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstORAindy3);
    }

    private void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstORAindy4);
    }

    private void Indy4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstORAindy5);
    }

    private void Indy5(Context ctx)
    {
        var nextState = States.Fetch;
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var beforePage = ctx.Regs.EA.Msb().Copy();
            ctx.Regs.IncEAWithY();
            var afterPage = ctx.Regs.EA.Msb().Copy();
            ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
            ctx.Pins.AddrBus = ctx.Regs.EA;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            if (!ctx.CrossedPageBoundary)
                OrAWithTemp(ctx);
            else
                nextState = States.InstORAindy6;
        }

        ctx.AdvanceState(nextState);
    }

    private void Indy6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            OrAWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x12] ORA (indirect)
    // -------------------------------------------------------------------------
    private void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstORAind3);
    }

    private void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstORAind4);
    }

    private void Ind4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstORAind5);
    }

    private void Ind5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            OrAWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
