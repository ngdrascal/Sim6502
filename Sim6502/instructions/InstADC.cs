using Sim6502.types;

namespace Sim6502.Instructions;

public class InstADC : InstBase
{
    public InstADC(IT2Registry it2Registry, IStateRegistry stateRegistry)
        : base(it2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.ADCimm, States.InstADCimm2);
        registry.Map(OpCodes.ADCzpg, States.InstADCzpg2);
        registry.Map(OpCodes.ADCzpgx, States.InstADCzpgx2);
        registry.Map(OpCodes.ADCabs, States.InstADCabs2);
        registry.Map(OpCodes.ADCabsx, States.InstADCabsx2);
        registry.Map(OpCodes.ADCabsy, States.InstADCabsy2);
        registry.Map(OpCodes.ADCindx, States.InstADCindx2);
        registry.Map(OpCodes.ADCindy, States.InstADCindy2);
        registry.Map(OpCodes.ADCind, States.InstADCind2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstADCimm2, Imm2);
        stateRegistry.Map(States.InstADCimm3, Imm3);

        stateRegistry.Map(States.InstADCzpg2, Zpg2);
        stateRegistry.Map(States.InstADCzpg3, Zpg3);
        stateRegistry.Map(States.InstADCzpg4, Zpg4);

        stateRegistry.Map(States.InstADCzpgx2, Zpgx2);
        stateRegistry.Map(States.InstADCzpgx3, Zpgx3);
        stateRegistry.Map(States.InstADCzpgx4, Zpgx4);
        stateRegistry.Map(States.InstADCzpgx5, Zpgx5);

        stateRegistry.Map(States.InstADCabs2, Abs2);
        stateRegistry.Map(States.InstADCabs3, Abs3);
        stateRegistry.Map(States.InstADCabs4, Abs4);
        stateRegistry.Map(States.InstADCabs5, Abs5);

        stateRegistry.Map(States.InstADCabsx2, Absx2);
        stateRegistry.Map(States.InstADCabsx3, Absx3);
        stateRegistry.Map(States.InstADCabsx4, Absx4);
        stateRegistry.Map(States.InstADCabsx5, Absx5);
        stateRegistry.Map(States.InstADCabsx6, Absx6);

        stateRegistry.Map(States.InstADCabsy2, Absy2);
        stateRegistry.Map(States.InstADCabsy3, Absy3);
        stateRegistry.Map(States.InstADCabsy4, Absy4);
        stateRegistry.Map(States.InstADCabsy5, Absy5);
        stateRegistry.Map(States.InstADCabsy6, Absy6);

        stateRegistry.Map(States.InstADCindx2, Indx2);
        stateRegistry.Map(States.InstADCindx3, Indx3);
        stateRegistry.Map(States.InstADCindx4, Indx4);
        stateRegistry.Map(States.InstADCindx5, Indx5);
        stateRegistry.Map(States.InstADCindx6, Indx6);
        stateRegistry.Map(States.InstADCindx7, Indx7);

        stateRegistry.Map(States.InstADCindy2, Indy2);
        stateRegistry.Map(States.InstADCindy3, Indy3);
        stateRegistry.Map(States.InstADCindy4, Indy4);
        stateRegistry.Map(States.InstADCindy5, Indy5);
        stateRegistry.Map(States.InstADCindy6, Indy6);
        stateRegistry.Map(States.InstADCindy7, Indy7);

        stateRegistry.Map(States.InstADCind2,  Ind2);
        stateRegistry.Map(States.InstADCind3,  Ind3);
        stateRegistry.Map(States.InstADCind4,  Ind4);
        stateRegistry.Map(States.InstADCind5,  Ind5);
        stateRegistry.Map(States.InstADCind6,  Ind6);
    }

    private void AdcThenUpdateFlags(Context ctx)
    {
        var p = ctx.Regs.P;
        var operand = ctx.Regs.Temp;
        // Use static MathResult.Adc method
        var result = MathResult.Adc(ctx.Regs.A, operand, p.Carry, p.Decimal);
        ctx.Regs.A.UpdateValue(result.Value());
        p.Negative.UpdateValue(result.Value().IsBitSet(7));
        p.Overflow.UpdateValue(result.Overflow());
        p.Zero.UpdateValue(result.Value().EqualsZero());
        p.Carry.UpdateValue(result.Carry());
    }

    /////////////////////////////////////////////////////////////////////////////
    // ADC - Add Memory to Accumulator with Carry
    // A AND M -> A
    // N V B D I Z C
    // + + - - - + +
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // immediate      ADC #oper     69      2     2+d
    // zeropage       ADC oper      65      2     3+d
    // zeropage,X     ADC oper,X    75      2     4+d
    // absolute       ADC oper      6D      3     4+d
    // absolute,X     ADC oper,X    7D      3     4+p+d
    // absolute,Y     ADC oper,Y    79      3     4+p+d
    // (indirect,X)   ADC (oper,X)  61      2     6+d
    // (indirect),Y   ADC (oper),Y  71      2     5+p+d
    // (zeropage)     ADC (oper)    72      2     5+d
    //
    // p: =1 if page is crossed.
    // d: =1 if in decimal mode
    /////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0x69] ADC immediate
    // -------------------------------------------------------------------------
    public void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            AdcThenUpdateFlags(ctx);
            ctx.DbgOperand1 = data;
        }
        if (ctx.Regs.P.Decimal.IsSet())
            ctx.AdvanceState(States.InstADCimm3);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Imm3(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x65] ADC zeropage
    // -------------------------------------------------------------------------
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstADCzpg3);
    }

    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AdcThenUpdateFlags(ctx);
        if (ctx.Regs.P.Decimal.GetValue())
            ctx.AdvanceState(States.InstADCzpg4);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Zpg4(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x75] ADC zeropage,X
    // -------------------------------------------------------------------------
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstADCzpgx3);
    }

    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstADCzpgx4);
    }

    public void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AdcThenUpdateFlags(ctx);
        if (ctx.Regs.P.Decimal.GetValue())
            ctx.AdvanceState(States.InstADCzpgx5);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Zpgx5(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x6D] ADC absolute
    // -------------------------------------------------------------------------
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstADCabs3);
    }

    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstADCabs4);
    }

    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AdcThenUpdateFlags(ctx);
        if (ctx.Regs.P.Decimal.GetValue())
            ctx.AdvanceState(States.InstADCabs5);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Abs5(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x7D] ADC absolute,X
    // -------------------------------------------------------------------------
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstADCabsx3);
    }

    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstADCabsx4);
    }

    public void Absx4(Context ctx)
    {
        var nextState = States.Fetch;
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var beforePage = ctx.Regs.EA.Msb().Copy();
            ctx.Regs.IncEAWithX();
            var afterPage = ctx.Regs.EA.Msb().Copy();
            ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            if (!ctx.CrossedPageBoundary)
            {
                AdcThenUpdateFlags(ctx);
                if (ctx.Regs.P.Decimal.GetValue())
                    nextState = States.InstADCabsx6;
            }
            else
                nextState = States.InstADCabsx5;
        }
        ctx.AdvanceState(nextState);
    }

    public void Absx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            AdcThenUpdateFlags(ctx);
        }
        if (ctx.Regs.P.Decimal.GetValue())
            ctx.AdvanceState(States.InstADCabsx6);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Absx6(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x79] ADC absolute,Y
    // -------------------------------------------------------------------------
    public void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstADCabsy3);
    }

    public void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstADCabsy4);
    }

    public void Absy4(Context ctx)
    {
        var nextState = States.Fetch;
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var beforePage = ctx.Regs.EA.Msb().Copy();
            ctx.Regs.IncEAWithY();
            var afterPage = ctx.Regs.EA.Msb().Copy();
            ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            if (!ctx.CrossedPageBoundary)
            {
                AdcThenUpdateFlags(ctx);
                if (ctx.Regs.P.Decimal.GetValue())
                    nextState = States.InstADCabsy6;
            }
            else
                nextState = States.InstADCabsy5;
        }
        ctx.AdvanceState(nextState);
    }

    public void Absy5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            AdcThenUpdateFlags(ctx);
        }
        if (ctx.Regs.P.Decimal.GetValue())
            ctx.AdvanceState(States.InstADCabsy6);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Absy6(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x61] ADC (indirect,X)
    // -------------------------------------------------------------------------
    public void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstADCindx3);
    }

    public void Indx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            ctx.Regs.EA.Lsb().AddWithWrapAround(ctx.Regs.X);
            ctx.Regs.EA.Msb().Zero();
        }
        ctx.AdvanceState(States.InstADCindx4);
    }

    public void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstADCindx5);
    }

    public void Indx5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstADCindx6);
    }

    public void Indx6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AdcThenUpdateFlags(ctx);
        if (ctx.Regs.P.Decimal.GetValue())
            ctx.AdvanceState(States.InstADCindx7);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Indx7(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x71] ADC (indirect),Y
    // -------------------------------------------------------------------------
    public void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstADCindy3);
    }

    public void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstADCindy4);
    }

    public void Indy4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstADCindy5);
    }

    public void Indy5(Context ctx)
    {
        var nextState = States.Fetch;
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var beforePage = ctx.Regs.EA.Msb().Copy();
            ctx.Regs.IncEAWithY();
            var afterPage = ctx.Regs.EA.Msb().Copy();
            ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            if (!ctx.CrossedPageBoundary)
            {
                AdcThenUpdateFlags(ctx);
                if (ctx.Regs.P.Decimal.GetValue())
                    nextState = States.InstADCindy7;
            }
            else
                nextState = States.InstADCindy6;
        }
        ctx.AdvanceState(nextState);
    }

    public void Indy6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AdcThenUpdateFlags(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    public void Indy7(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x72] ADC (indirect)
    // -------------------------------------------------------------------------
    public void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstADCind3);
    }

    public void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstADCind4);
    }

    public void Ind4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstADCind5);
    }

    public void Ind5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            AdcThenUpdateFlags(ctx);
        if (ctx.Regs.P.Decimal.GetValue())
            ctx.AdvanceState(States.InstADCind6);
        else
            ctx.AdvanceState(States.Fetch);
    }

    public void Ind6(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.Fetch);
    }
}
