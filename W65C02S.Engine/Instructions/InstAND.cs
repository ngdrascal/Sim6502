namespace W65C02S.Engine;

internal class InstAND : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.ANDimm, States.InstANDimm2);
        registry.Map(OpCodes.ANDzpg, States.InstANDzpg2);
        registry.Map(OpCodes.ANDzpgx, States.InstANDzpgx2);
        registry.Map(OpCodes.ANDabs, States.InstANDabs2);
        registry.Map(OpCodes.ANDabsx, States.InstANDabsx2);
        registry.Map(OpCodes.ANDabsy, States.InstANDabsy2);
        registry.Map(OpCodes.ANDindx, States.InstANDindx2);
        registry.Map(OpCodes.ANDindy, States.InstANDindy2);
        registry.Map(OpCodes.ANDind, States.InstANDind2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstANDimm2, Imm2);

        stateRegistry.Map(States.InstANDzpg2, Zpg2);
        stateRegistry.Map(States.InstANDzpg3, Zpg3);

        stateRegistry.Map(States.InstANDzpgx2, Zpgx2);
        stateRegistry.Map(States.InstANDzpgx3, Zpgx3);
        stateRegistry.Map(States.InstANDzpgx4, Zpgx4);

        stateRegistry.Map(States.InstANDabs2, Abs2);
        stateRegistry.Map(States.InstANDabs3, Abs3);
        stateRegistry.Map(States.InstANDabs4, Abs4);

        stateRegistry.Map(States.InstANDabsx2, Absx2);
        stateRegistry.Map(States.InstANDabsx3, Absx3);
        stateRegistry.Map(States.InstANDabsx4, Absx4);
        stateRegistry.Map(States.InstANDabsx5, Absx5);

        stateRegistry.Map(States.InstANDabsy2, Absy2);
        stateRegistry.Map(States.InstANDabsy3, Absy3);
        stateRegistry.Map(States.InstANDabsy4, Absy4);
        stateRegistry.Map(States.InstANDabsy5, Absy5);

        stateRegistry.Map(States.InstANDindx2, Indx2);
        stateRegistry.Map(States.InstANDindx3, Indx3);
        stateRegistry.Map(States.InstANDindx4, Indx4);
        stateRegistry.Map(States.InstANDindx5, Indx5);
        stateRegistry.Map(States.InstANDindx6, Indx6);

        stateRegistry.Map(States.InstANDindy2, Indy2);
        stateRegistry.Map(States.InstANDindy3, Indy3);
        stateRegistry.Map(States.InstANDindy4, Indy4);
        stateRegistry.Map(States.InstANDindy5, Indy5);
        stateRegistry.Map(States.InstANDindy6, Indy6);

        stateRegistry.Map(States.InstANDind2, Ind2);
        stateRegistry.Map(States.InstANDind3, Ind3);
        stateRegistry.Map(States.InstANDind4, Ind4);
        stateRegistry.Map(States.InstANDind5, Ind5);

        return this;
    }

    private void AndAWithTemp(Context ctx)
    {
        var memValue = ctx.Regs.Temp;
        var result = ctx.Regs.A.Copy().And(memValue);
        ctx.Regs.UpdateAUpdateFlags(result);
    }

    /////////////////////////////////////////////////////////////////////////////
    // AND - "AND" Memory with Accumulator
    // A AND M -> A
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // immediate      AND #oper     29      2      2
    // zeropage       AND oper      25      2      3
    // zeropage,X     AND oper,X    35      2      4
    // absolute       AND oper      2D      3      4
    // absolute,X     AND oper,X    3D      3      4*
    // absolute,Y     AND oper,Y    39      3      4*
    // (indirect,X)   AND (oper,X)  21      2      6
    // (indirect),Y   AND (oper),Y  31      2      5*
    // (indirect)     AND (oper)    32      2      5
    /////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0x29] AND immediate
    // -------------------------------------------------------------------------
    private void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            var result = ctx.Regs.A.Copy().And(data);
            ctx.Regs.UpdateAUpdateFlags(result);
            ctx.DbgOperand1 = data;
        }
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x25] AND zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstANDzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AndAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x35] AND zeropage,X
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstANDzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstANDzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AndAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x2D] AND absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstANDabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstANDabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AndAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x3D] AND absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstANDabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstANDabsx4);
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
            ctx.Regs.Temp = data;
            if (!ctx.CrossedPageBoundary)
            {
                AndAWithTemp(ctx);
            }
            else
            {
                nextState = States.InstANDabsx5;
            }
        }
        ctx.AdvanceState(nextState);
    }

    private void Absx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp = data;
            AndAWithTemp(ctx);
        }
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x39] AND absolute,Y
    // -------------------------------------------------------------------------
    private void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstANDabsy3);
    }

    private void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstANDabsy4);
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
            ctx.Regs.Temp = data;
            if (!ctx.CrossedPageBoundary)
            {
                AndAWithTemp(ctx);
            }
            else
            {
                nextState = States.InstANDabsy5;
            }
        }
        ctx.AdvanceState(nextState);
    }

    private void Absy5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp = data;
            AndAWithTemp(ctx);
        }
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x21] AND (indirect,X)
    // -------------------------------------------------------------------------
    private void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstANDindx3);
    }

    private void Indx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            ctx.Regs.EA = ctx.Regs.EA.WithLsb(ctx.Regs.EA.Lsb().AddWithWrapAround(ctx.Regs.X));
            ctx.Regs.EA = ctx.Regs.EA.WithMsb(0);
        }
        ctx.AdvanceState(States.InstANDindx4);
    }

    private void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstANDindx5);
    }

    private void Indx5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstANDindx6);
    }

    private void Indx6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AndAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x31] AND (indirect),Y
    // -------------------------------------------------------------------------
    private void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstANDindy3);
    }

    private void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstANDindy4);
    }

    private void Indy4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstANDindy5);
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
            ctx.Regs.Temp = data;
            if (!ctx.CrossedPageBoundary)
            {
                AndAWithTemp(ctx);
            }
            else
            {
                nextState = States.InstANDindy6;
            }
        }
        ctx.AdvanceState(nextState);
    }

    private void Indy6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AndAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x32] AND (indirect)
    // -------------------------------------------------------------------------
    private void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstANDind3);
    }

    private void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstANDind4);
    }

    private void Ind4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstANDind5);
    }

    private void Ind5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AndAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}