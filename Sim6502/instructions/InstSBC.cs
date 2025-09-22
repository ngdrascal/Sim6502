namespace Sim6502.Instructions;

public class InstSBC : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
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
        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstSBCimm2, Imm2);
        stateRegistry.Map(States.InstSBCimm3, Imm3);

        stateRegistry.Map(States.InstSBCzpg2, Zpg2);
        stateRegistry.Map(States.InstSBCzpg3, Zpg3);
        stateRegistry.Map(States.InstSBCzpg4, Zpg4);

        stateRegistry.Map(States.InstSBCzpgx2, Zpgx2);
        stateRegistry.Map(States.InstSBCzpgx3, Zpgx3);
        stateRegistry.Map(States.InstSBCzpgx4, Zpgx4);
        stateRegistry.Map(States.InstSBCzpgx5, Zpgx5);

        stateRegistry.Map(States.InstSBCabs2, Abs2);
        stateRegistry.Map(States.InstSBCabs3, Abs3);
        stateRegistry.Map(States.InstSBCabs4, Abs4);
        stateRegistry.Map(States.InstSBCabs5, Abs5);

        stateRegistry.Map(States.InstSBCabsx2, Absx2);
        stateRegistry.Map(States.InstSBCabsx3, Absx3);
        stateRegistry.Map(States.InstSBCabsx4, Absx4);
        stateRegistry.Map(States.InstSBCabsx5, Absx5);
        stateRegistry.Map(States.InstSBCabsx6, Absx6);

        stateRegistry.Map(States.InstSBCabsy2, Absy2);
        stateRegistry.Map(States.InstSBCabsy3, Absy3);
        stateRegistry.Map(States.InstSBCabsy4, Absy4);
        stateRegistry.Map(States.InstSBCabsy5, Absy5);
        stateRegistry.Map(States.InstSBCabsy6, Absy6);

        stateRegistry.Map(States.InstSBCindx2, Indx2);
        stateRegistry.Map(States.InstSBCindx3, Indx3);
        stateRegistry.Map(States.InstSBCindx4, Indx4);
        stateRegistry.Map(States.InstSBCindx5, Indx5);
        stateRegistry.Map(States.InstSBCindx6, Indx6);
        stateRegistry.Map(States.InstSBCindx7, Indx7);

        stateRegistry.Map(States.InstSBCindy2, Indy2);
        stateRegistry.Map(States.InstSBCindy3, Indy3);
        stateRegistry.Map(States.InstSBCindy4, Indy4);
        stateRegistry.Map(States.InstSBCindy5, Indy5);
        stateRegistry.Map(States.InstSBCindy6, Indy6);
        stateRegistry.Map(States.InstSBCindy7, Indy7);

        stateRegistry.Map(States.InstSBCind2, Ind2);
        stateRegistry.Map(States.InstSBCind3, Ind3);
        stateRegistry.Map(States.InstSBCind4, Ind4);
        stateRegistry.Map(States.InstSBCind5, Ind5);
        stateRegistry.Map(States.InstSBCind6, Ind6);
        return this;
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
    private void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            SbcThenUpdateFlags(ctx);
            ctx.DbgOperand1 = data;
        }

        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCimm3);
        else
            ctx.AdvanceState(States.Fetch);
    }

    private void Imm3(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xE5] SBC zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSBCzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            SbcThenUpdateFlags(ctx);

        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCzpg4);
        else
            ctx.AdvanceState(States.Fetch);
    }

    private void Zpg4(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xF5] SBC zeropage,X
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSBCzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstSBCzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            SbcThenUpdateFlags(ctx);

        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCzpgx5);
        else
            ctx.AdvanceState(States.Fetch);
    }

    private void Zpgx5(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xED] SBC absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSBCabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstSBCabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            SbcThenUpdateFlags(ctx);

        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCabs5);
        else
            ctx.AdvanceState(States.Fetch);
    }

    private void Abs5(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xFD] SBC absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSBCabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstSBCabsx4);
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
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
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

    private void Absx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            SbcThenUpdateFlags(ctx);
        }

        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCabsx6);
        else
            ctx.AdvanceState(States.Fetch);
    }

    private void Absx6(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xF9] SBC absolute,Y
    // -------------------------------------------------------------------------
    private void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSBCabsy3);
    }

    private void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstSBCabsy4);
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
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
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

    private void Absy5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            SbcThenUpdateFlags(ctx);
        }

        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCabsy6);
        else
            ctx.AdvanceState(States.Fetch);
    }

    private void Absy6(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xE1] SBC (indirect,X)
    // -------------------------------------------------------------------------
    private void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        
        ctx.AdvanceState(States.InstSBCindx3);
    }

    private void Indx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            ctx.Regs.EA.Lsb().AddWithWrapAround(ctx.Regs.X);
            ctx.Regs.EA.Msb().Zero();
        }
        
        ctx.AdvanceState(States.InstSBCindx4);
    }

    private void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstSBCindx5);
    }

    private void Indx5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstSBCindx6);
    }

    private void Indx6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            SbcThenUpdateFlags(ctx);

        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCindx7);
        else
            ctx.AdvanceState(States.Fetch);
    }

    private void Indx7(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xF1] SBC (indirect),Y
    // -------------------------------------------------------------------------
    private void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSBCindy3);
    }

    private void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstSBCindy4);
    }

    private void Indy4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstSBCindy5);
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
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
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

    private void Indy6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            SbcThenUpdateFlags(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    private void Indy7(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xF2] SBC (indirect)
    // -------------------------------------------------------------------------
    private void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        
        ctx.AdvanceState(States.InstSBCind3);
    }

    private void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        
        ctx.AdvanceState(States.InstSBCind4);
    }

    private void Ind4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        
        ctx.AdvanceState(States.InstSBCind5);
    }

    private void Ind5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            SbcThenUpdateFlags(ctx);

        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstSBCind6);
        else
            ctx.AdvanceState(States.Fetch);
    }

    private void Ind6(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }
}
