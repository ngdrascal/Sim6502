namespace Sim6502.Instructions;

public class InstEOR : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.EORimm, States.InstEORimm2);
        registry.Map(OpCodes.EORzpg, States.InstEORzpg2);
        registry.Map(OpCodes.EORzpgx, States.InstEORzpgx2);
        registry.Map(OpCodes.EORabs, States.InstEORabs2);
        registry.Map(OpCodes.EORabsx, States.InstEORabsx2);
        registry.Map(OpCodes.EORabsy, States.InstEORabsy2);
        registry.Map(OpCodes.EORindx, States.InstEORindx2);
        registry.Map(OpCodes.EORindy, States.InstEORindy2);
        registry.Map(OpCodes.EORind, States.InstEORind2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstEORimm2, Imm2);
        stateRegistry.Map(States.InstEORzpg2, Zpg2);
        stateRegistry.Map(States.InstEORzpg3, Zpg3);
        stateRegistry.Map(States.InstEORzpgx2, Zpgx2);
        stateRegistry.Map(States.InstEORzpgx3, Zpgx3);
        stateRegistry.Map(States.InstEORzpgx4, Zpgx4);
        stateRegistry.Map(States.InstEORabs2, Abs2);
        stateRegistry.Map(States.InstEORabs3, Abs3);
        stateRegistry.Map(States.InstEORabs4, Abs4);
        stateRegistry.Map(States.InstEORabsx2, Absx2);
        stateRegistry.Map(States.InstEORabsx3, Absx3);
        stateRegistry.Map(States.InstEORabsx4, Absx4);
        stateRegistry.Map(States.InstEORabsx5, Absx5);
        stateRegistry.Map(States.InstEORabsy2, Absy2);
        stateRegistry.Map(States.InstEORabsy3, Absy3);
        stateRegistry.Map(States.InstEORabsy4, Absy4);
        stateRegistry.Map(States.InstEORabsy5, Absy5);
        stateRegistry.Map(States.InstEORindx2, Indx2);
        stateRegistry.Map(States.InstEORindx3, Indx3);
        stateRegistry.Map(States.InstEORindx4, Indx4);
        stateRegistry.Map(States.InstEORindx5, Indx5);
        stateRegistry.Map(States.InstEORindx6, Indx6);
        stateRegistry.Map(States.InstEORindy2, Indy2);
        stateRegistry.Map(States.InstEORindy3, Indy3);
        stateRegistry.Map(States.InstEORindy4, Indy4);
        stateRegistry.Map(States.InstEORindy5, Indy5);
        stateRegistry.Map(States.InstEORindy6, Indy6);
        stateRegistry.Map(States.InstEORind2, Ind2);
        stateRegistry.Map(States.InstEORind3, Ind3);
        stateRegistry.Map(States.InstEORind4, Ind4);
        stateRegistry.Map(States.InstEORind5, Ind5);

        return this;
    }

    protected void XorAWithTemp(Context ctx)
    {
        var memValue = ctx.Regs.Temp;
        var result = ctx.Regs.A.Copy().Xor(memValue);
        ctx.Regs.UpdateAUpdateFlags(result);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // EOR - Exclusive OR Memory with Accumulator
    // A EOR M -> A
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // immediate      EOR #oper     49      2      2
    // zeropage       EOR oper      45      2      3
    // zeropage,X     EOR oper,X    55      2      4
    // absolute       EOR oper      4D      3      4
    // absolute,X     EOR oper,X    5D      3      4*
    // absolute,Y     EOR oper,Y    59      3      4*
    // (indirect,X)   EOR (oper,X)  41      2      6
    // (indirect),Y   EOR (oper),Y  51      2      5*
    // (indirect)     EOR (oper)    52      2      5
    ///////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0x49] EOR immediate
    // -------------------------------------------------------------------------
    private void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            var result = ctx.Regs.A.Copy().Xor(data);
            ctx.Regs.UpdateAUpdateFlags(result);
            ctx.DbgOperand1 = data;
        }
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x45] EOR zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstEORzpg3);
    }
    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            XorAWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x55] EOR zeropage,X
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstEORzpgx3);
    }
    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstEORzpgx4);
    }
    private void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            XorAWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x4D] EOR absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstEORabs3);
    }
    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstEORabs4);
    }
    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            XorAWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x5D] EOR absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstEORabsx3);
    }
    private void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstEORabsx4);
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
                XorAWithTemp(ctx);
            }
            else
                nextState = States.InstEORabsx5;
        }

        ctx.AdvanceState(nextState);
    }
    private void Absx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            XorAWithTemp(ctx);
        }
        
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x59] EOR absolute,Y
    // -------------------------------------------------------------------------
    private void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstEORabsy3);
    }
    private void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstEORabsy4);
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
                XorAWithTemp(ctx);
            }
            else
                nextState = States.InstEORabsy5;
        }

        ctx.AdvanceState(nextState);
    }
    private void Absy5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            XorAWithTemp(ctx);
        }

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x41] EOR (indirect,X)
    // -------------------------------------------------------------------------
    private void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstEORindx3);
    }
    private void Indx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            ctx.Regs.EA.Lsb().AddWithWrapAround(ctx.Regs.X);
            ctx.Regs.EA.Msb().Zero();
        }

        ctx.AdvanceState(States.InstEORindx4);
    }
    private void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstEORindx5);
    }
    private void Indx5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstEORindx6);
    }
    private void Indx6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            XorAWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x51] EOR (indirect),Y
    // -------------------------------------------------------------------------
    private void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstEORindy3);
    }
    private void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstEORindy4);
    }
    private void Indy4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstEORindy5);
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
                XorAWithTemp(ctx);
            else
                nextState = States.InstEORindy6;
        }

        ctx.AdvanceState(nextState);
    }
    private void Indy6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            XorAWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x2] EOR (indirect)
    // -------------------------------------------------------------------------
    private void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstEORind3);
    }
    private void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstEORind4);
    }
    private void Ind4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstEORind5);
    }
    private void Ind5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            XorAWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
