namespace Sim6502.Instructions;

public class InstSBC : InstBase
{
    public InstSBC(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.SBCimm, States.InstSBCimm2);
        registry.Map(OpCodes.SBCzpg, States.InstSBCzpg2);
        registry.Map(OpCodes.SBCzpgx, States.InstSBCzpgx2);
        registry.Map(OpCodes.SBCabs, States.InstSBCabs2);
        registry.Map(OpCodes.SBCabsx, States.InstSBCabsx2);
        registry.Map(OpCodes.SBCabsy, States.InstSBCabsy2);
        registry.Map(OpCodes.SBCindx, States.InstSBCindx2);
        registry.Map(OpCodes.SBCindy, States.InstSBCindy2);
        registry.Map(OpCodes.SBCind, States.InstSBCind2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstSBCimm2, ctx => Imm2(ctx));
        stateRegistry.Map(States.InstSBCimm3, ctx => Imm3(ctx));

        stateRegistry.Map(States.InstSBCzpg2, ctx => Zpg2(ctx));
        stateRegistry.Map(States.InstSBCzpg3, ctx => Zpg3(ctx));
        stateRegistry.Map(States.InstSBCzpg4, ctx => Zpg4(ctx));

        stateRegistry.Map(States.InstSBCzpgx2, ctx => Zpgx2(ctx));
        stateRegistry.Map(States.InstSBCzpgx3, ctx => Zpgx3(ctx));
        stateRegistry.Map(States.InstSBCzpgx4, ctx => Zpgx4(ctx));
        stateRegistry.Map(States.InstSBCzpgx5, ctx => Zpgx5(ctx));

        stateRegistry.Map(States.InstSBCabs2, ctx => Abs2(ctx));
        stateRegistry.Map(States.InstSBCabs3, ctx => Abs3(ctx));
        stateRegistry.Map(States.InstSBCabs4, ctx => Abs4(ctx));
        stateRegistry.Map(States.InstSBCabs5, ctx => Abs5(ctx));

        stateRegistry.Map(States.InstSBCabsx2, ctx => Absx2(ctx));
        stateRegistry.Map(States.InstSBCabsx3, ctx => Absx3(ctx));
        stateRegistry.Map(States.InstSBCabsx4, ctx => Absx4(ctx));
        stateRegistry.Map(States.InstSBCabsx5, ctx => Absx5(ctx));
        stateRegistry.Map(States.InstSBCabsx6, ctx => Absx6(ctx));

        stateRegistry.Map(States.InstSBCabsy2, ctx => Absy2(ctx));
        stateRegistry.Map(States.InstSBCabsy3, ctx => Absy3(ctx));
        stateRegistry.Map(States.InstSBCabsy4, ctx => Absy4(ctx));
        stateRegistry.Map(States.InstSBCabsy5, ctx => Absy5(ctx));
        stateRegistry.Map(States.InstSBCabsy6, ctx => Absy6(ctx));

        stateRegistry.Map(States.InstSBCindx2, ctx => Indx2(ctx));
        stateRegistry.Map(States.InstSBCindx3, ctx => Indx3(ctx));
        stateRegistry.Map(States.InstSBCindx4, ctx => Indx4(ctx));
        stateRegistry.Map(States.InstSBCindx5, ctx => Indx5(ctx));
        stateRegistry.Map(States.InstSBCindx6, ctx => Indx6(ctx));
        stateRegistry.Map(States.InstSBCindx7, ctx => Indx7(ctx));

        stateRegistry.Map(States.InstSBCindy2, ctx => Indy2(ctx));
        stateRegistry.Map(States.InstSBCindy3, ctx => Indy3(ctx));
        stateRegistry.Map(States.InstSBCindy4, ctx => Indy4(ctx));
        stateRegistry.Map(States.InstSBCindy5, ctx => Indy5(ctx));
        stateRegistry.Map(States.InstSBCindy6, ctx => Indy6(ctx));
        stateRegistry.Map(States.InstSBCindy7, ctx => Indy7(ctx));

        stateRegistry.Map(States.InstSBCind2, ctx => Ind2(ctx));
        stateRegistry.Map(States.InstSBCind3, ctx => Ind3(ctx));
        stateRegistry.Map(States.InstSBCind4, ctx => Ind4(ctx));
        stateRegistry.Map(States.InstSBCind5, ctx => Ind5(ctx));
        stateRegistry.Map(States.InstSBCind6, ctx => Ind6(ctx));
    }

    private void SbcThenUpdateFlags(Context ctx)
    {
        var p = ctx.Regs.P;
        var operand = ctx.Regs.Temp;
        var result = ctx.Regs.A.Sbc(operand, p.Carry, p.Decimal);
        ctx.Regs.A.UpdateValue(result.Value());
        p.Negative.UpdateValue(result.Value().IsBitSet(7));
        p.Overflow.UpdateValue(result.Overflow());
        p.Zero.UpdateValue(result.Value().EqualsZero());
        p.Carry.UpdateValue(result.Carry());
    }

    /////////////////////////////////////////////////////////////////////////////
    // SBC - Subtract Memory from Accumulator with Borrow
    //  A - M - ~C -> A
    // N V B D I Z C
    // + + - - - + +
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------   
    // immediate      SBC #oper     E9      2     2+d
    // zeropage       SBC oper      E5      2     3+d
    // zeropage,X     SBC oper,X    F5      2     4+d
    // absolute       SBC oper      ED      3     4+d
    // absolute,X     SBC oper,X    FD      3     4+p+d
    // absolute,Y     SBC oper,Y    F9      3     4+p+d
    // (indirect,X)   SBC (oper,X)  E1      2     6+d
    // (indirect),Y   SBC (oper),Y  F1      2     5+p+d
    // (zeropage)     SBC (oper)    F2      2     5+d
    // 
    // p: =1 if page is crossed.
    // d: =1 if in decimal mode
    /////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0xE9] SBC immediate
    // -------------------------------------------------------------------------
    public void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            SbcThenUpdateFlags(ctx);
            ctx.DbgOperand1 = data;
        }
        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCimm3);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Imm3(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xE5] SBC zeropage
    // -------------------------------------------------------------------------
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSBCzpg3);
    }

    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            SbcThenUpdateFlags(ctx);
        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCzpg4);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Zpg4(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xF5] SBC zeropage,X
    // -------------------------------------------------------------------------
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSBCzpgx3);
    }

    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstSBCzpgx4);
    }

    public void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            SbcThenUpdateFlags(ctx);
        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCzpgx5);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Zpgx5(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xED] SBC absolute
    // -------------------------------------------------------------------------
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSBCabs3);
    }

    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstSBCabs4);
    }

    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            SbcThenUpdateFlags(ctx);
        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCabs5);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Abs5(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xFD] SBC absolute,X
    // -------------------------------------------------------------------------
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSBCabsx3);
    }

    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstSBCabsx4);
    }

    public void Absx4(Context ctx)
    {
        var nextState = States.Fetch;
        if (ctx.GetSubStep() == P1Middlestep)
        {
            var beforePage = ctx.Regs.EA.Msb().Copy();
            ctx.Regs.IncEAWithX();
            var afterPage = ctx.Regs.EA.Msb().Copy();
            ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            if (!ctx.CrossedPageBoundary)
            {
                SbcThenUpdateFlags(ctx);
                if (ctx.Regs.P.Decimal.IsSet())
                    nextState = States.InstSBCabsx6;
            }
            else
                nextState = States.InstSBCabsx5;
        }
        ctx.AdvanceState(nextState);
    }

    public void Absx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            SbcThenUpdateFlags(ctx);
        }
        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCabsx6);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Absx6(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xF9] SBC absolute,Y
    // -------------------------------------------------------------------------
    public void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSBCabsy3);
    }

    public void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstSBCabsy4);
    }

    public void Absy4(Context ctx)
    {
        var nextState = States.Fetch;
        if (ctx.GetSubStep() == P1Middlestep)
        {
            var beforePage = ctx.Regs.EA.Msb().Copy();
            ctx.Regs.IncEAWithY();
            var afterPage = ctx.Regs.EA.Msb().Copy();
            ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            if (!ctx.CrossedPageBoundary)
            {
                SbcThenUpdateFlags(ctx);
                if (ctx.Regs.P.Decimal.IsSet())
                    nextState = States.InstSBCabsy6;
            }
            else
                nextState = States.InstSBCabsy5;
        }
        ctx.AdvanceState(nextState);
    }

    public void Absy5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            SbcThenUpdateFlags(ctx);
        }
        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCabsy6);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Absy6(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xE1] SBC (indirect,X)
    // -------------------------------------------------------------------------
    public void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSBCindx3);
    }

    public void Indx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
        {
            ctx.Regs.EA.Lsb().AddWithWrapAround(ctx.Regs.X);
            ctx.Regs.EA.Msb().Zero();
        }
        ctx.AdvanceState(States.InstSBCindx4);
    }

    public void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstSBCindx5);
    }

    public void Indx5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstSBCindx6);
    }

    public void Indx6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            SbcThenUpdateFlags(ctx);
        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCindx7);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Indx7(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xF1] SBC (indirect),Y
    // -------------------------------------------------------------------------
    public void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSBCindy3);
    }

    public void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstSBCindy4);
    }

    public void Indy4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstSBCindy5);
    }

    public void Indy5(Context ctx)
    {
        var nextState = States.Fetch;
        if (ctx.GetSubStep() == P1Middlestep)
        {
            var beforePage = ctx.Regs.EA.Msb().Copy();
            ctx.Regs.IncEAWithY();
            var afterPage = ctx.Regs.EA.Msb().Copy();
            ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            if (!ctx.CrossedPageBoundary)
            {
                SbcThenUpdateFlags(ctx);
                if (ctx.Regs.P.Decimal.IsSet())
                    nextState = States.InstSBCindy7;
            }
            else
                nextState = States.InstSBCindy6;
        }
        ctx.AdvanceState(nextState);
    }

    public void Indy6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            SbcThenUpdateFlags(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    public void Indy7(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xF2] SBC (indirect)
    // -------------------------------------------------------------------------
    public void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSBCind3);
    }

    public void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstSBCind4);
    }

    public void Ind4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstSBCind5);
    }

    public void Ind5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            SbcThenUpdateFlags(ctx);
        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCind6);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Ind6(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }
}
