namespace Sim6502.Instructions;

internal class InstCMP : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.CMPimm, States.InstCMPimm2);
        registry.Map(OpCodes.CMPzpg, States.InstCMPzpg2);
        registry.Map(OpCodes.CMPzpgx, States.InstCMPzpgx2);
        registry.Map(OpCodes.CMPabs, States.InstCMPabs2);
        registry.Map(OpCodes.CMPabsx, States.InstCMPabsx2);
        registry.Map(OpCodes.CMPabsy, States.InstCMPabsy2);
        registry.Map(OpCodes.CMPindx, States.InstCMPindx2);
        registry.Map(OpCodes.CMPindy, States.InstCMPindy2);
        registry.Map(OpCodes.CMPind, States.InstCMPind2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstCMPimm2, Imm2);
        stateRegistry.Map(States.InstCMPzpg2, Zpg2);
        stateRegistry.Map(States.InstCMPzpg3, Zpg3);
        stateRegistry.Map(States.InstCMPzpgx2, Zpgx2);
        stateRegistry.Map(States.InstCMPzpgx3, Zpgx3);
        stateRegistry.Map(States.InstCMPzpgx4, Zpgx4);
        stateRegistry.Map(States.InstCMPabs2, Abs2);
        stateRegistry.Map(States.InstCMPabs3, Abs3);
        stateRegistry.Map(States.InstCMPabs4, Abs4);
        stateRegistry.Map(States.InstCMPabsx2, Absx2);
        stateRegistry.Map(States.InstCMPabsx3, Absx3);
        stateRegistry.Map(States.InstCMPabsx4, Absx4);
        stateRegistry.Map(States.InstCMPabsx5, Absx5);
        stateRegistry.Map(States.InstCMPabsy2, Absy2);
        stateRegistry.Map(States.InstCMPabsy3, Absy3);
        stateRegistry.Map(States.InstCMPabsy4, Absy4);
        stateRegistry.Map(States.InstCMPabsy5, Absy5);
        stateRegistry.Map(States.InstCMPindx2, Indx2);
        stateRegistry.Map(States.InstCMPindx3, Indx3);
        stateRegistry.Map(States.InstCMPindx4, Indx4);
        stateRegistry.Map(States.InstCMPindx5, Indx5);
        stateRegistry.Map(States.InstCMPindx6, Indx6);
        stateRegistry.Map(States.InstCMPindy2, Indy2);
        stateRegistry.Map(States.InstCMPindy3, Indy3);
        stateRegistry.Map(States.InstCMPindy4, Indy4);
        stateRegistry.Map(States.InstCMPindy5, Indy5);
        stateRegistry.Map(States.InstCMPindy6, Indy6);
        stateRegistry.Map(States.InstCMPind2, Ind2);
        stateRegistry.Map(States.InstCMPind3, Ind3);
        stateRegistry.Map(States.InstCMPind4, Ind4);
        stateRegistry.Map(States.InstCMPind5, Ind5);

        return this;
    }

    private void CmpWithTemp(Context ctx)
    {
        var flags = ctx.Regs.P;
        var a = ctx.Regs.A.ToInt();
        var operand = ctx.Regs.Temp.ToInt();
        var result = a - operand;
        flags.Carry.UpdateValue(result >= 0);
        flags.Zero.UpdateValue(result == 0);
        flags.Negative.UpdateValue((result & 0x80) > 0);
    }

    /////////////////////////////////////////////////////////////////////////////
    // CMP - Compare Memory with Accumulator
    // A - M
    // N V B D I Z C
    // + - - - - + +
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // immediate      CMP #oper     C9      2      2
    // zeropage       CMP oper      C5      2      3
    // zeropage,X     CMP oper,X    D5      2      4
    // absolute       CMP oper      CD      3      4
    // absolute,X     CMP oper,X    DD      3      4+p
    // absolute,Y     CMP oper,Y    D9      3      4+p
    // (indirect,X)   CMP (oper,X)  C1      2      6
    // (indirect),Y   CMP (oper),Y  D1      2      5+p
    // (indirect)     CMP (oper)    D2      2      5
    //
    // p: =1 if page is crossed
    /////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0xC9] CMP immediate
    // -------------------------------------------------------------------------
    private void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            CmpWithTemp(ctx);
            ctx.DbgOperand1 = data;
        }

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xC5] CMP zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstCMPzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xD5] CMP zeropage,X
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstCMPzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstCMPzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xCD] CMP absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstCMPabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstCMPabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xDD] CMP absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstCMPabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstCMPabsx4);
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
            if (!ctx.CrossedPageBoundary)
            {
                LoadTempFromEffAddr(ctx);
                CmpWithTemp(ctx);
            }
            else
            {
                nextState = States.InstCMPabsx5;
            }
        }

        ctx.AdvanceState(nextState);
    }

    private void Absx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xD9] CMP absolute,Y
    // -------------------------------------------------------------------------
    private void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstCMPabsy3);
    }

    private void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstCMPabsy4);
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
            if (!ctx.CrossedPageBoundary)
            {
                LoadTempFromEffAddr(ctx);
                CmpWithTemp(ctx);
            }
            else
            {
                nextState = States.InstCMPabsy5;
            }
        }

        ctx.AdvanceState(nextState);
    }

    private void Absy5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        CmpWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xC1] CMP (indirect,X)
    // -------------------------------------------------------------------------
    private void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstCMPindx3);
    }

    private void Indx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            ctx.Regs.EA.Lsb().AddWithWrapAround(ctx.Regs.X);
            ctx.Regs.EA.Msb().Zero();
        }

        ctx.AdvanceState(States.InstCMPindx4);
    }

    private void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstCMPindx5);
    }

    private void Indx5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstCMPindx6);
    }

    private void Indx6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xD1] CMP (indirect,Y)
    // -------------------------------------------------------------------------
    private void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstCMPindy3);
    }

    private void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstCMPindy4);
    }

    private void Indy4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstCMPindy5);
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
            if (!ctx.CrossedPageBoundary)
            {
                LoadTempFromEffAddr(ctx);
                CmpWithTemp(ctx);
            }
            else
            {
                nextState = States.InstCMPindy6;
            }
        }

        ctx.AdvanceState(nextState);
    }

    private void Indy6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xD2] CMP (indirect)
    // -------------------------------------------------------------------------
    private void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstCMPind3);
    }

    private void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstCMPind4);
    }

    private void Ind4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstCMPind5);
    }

    private void Ind5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
