namespace Sim6502.Instructions;

public class InstEOR : InstBase
{
    public InstEOR(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
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
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstEORimm2, ctx => Imm2(ctx));
        stateRegistry.Map(States.InstEORzpg2, ctx => Zpg2(ctx));
        stateRegistry.Map(States.InstEORzpg3, ctx => Zpg3(ctx));
        stateRegistry.Map(States.InstEORzpgx2, ctx => Zpgx2(ctx));
        stateRegistry.Map(States.InstEORzpgx3, ctx => Zpgx3(ctx));
        stateRegistry.Map(States.InstEORzpgx4, ctx => Zpgx4(ctx));
        stateRegistry.Map(States.InstEORabs2, ctx => Abs2(ctx));
        stateRegistry.Map(States.InstEORabs3, ctx => Abs3(ctx));
        stateRegistry.Map(States.InstEORabs4, ctx => Abs4(ctx));
        stateRegistry.Map(States.InstEORabsx2, ctx => Absx2(ctx));
        stateRegistry.Map(States.InstEORabsx3, ctx => Absx3(ctx));
        stateRegistry.Map(States.InstEORabsx4, ctx => Absx4(ctx));
        stateRegistry.Map(States.InstEORabsx5, ctx => Absx5(ctx));
        stateRegistry.Map(States.InstEORabsy2, ctx => Absy2(ctx));
        stateRegistry.Map(States.InstEORabsy3, ctx => Absy3(ctx));
        stateRegistry.Map(States.InstEORabsy4, ctx => Absy4(ctx));
        stateRegistry.Map(States.InstEORabsy5, ctx => Absy5(ctx));
        stateRegistry.Map(States.InstEORindx2, ctx => Indx2(ctx));
        stateRegistry.Map(States.InstEORindx3, ctx => Indx3(ctx));
        stateRegistry.Map(States.InstEORindx4, ctx => Indx4(ctx));
        stateRegistry.Map(States.InstEORindx5, ctx => Indx5(ctx));
        stateRegistry.Map(States.InstEORindx6, ctx => Indx6(ctx));
        stateRegistry.Map(States.InstEORindy2, ctx => Indy2(ctx));
        stateRegistry.Map(States.InstEORindy3, ctx => Indy3(ctx));
        stateRegistry.Map(States.InstEORindy4, ctx => Indy4(ctx));
        stateRegistry.Map(States.InstEORindy5, ctx => Indy5(ctx));
        stateRegistry.Map(States.InstEORindy6, ctx => Indy6(ctx));
        stateRegistry.Map(States.InstEORind2, ctx => Ind2(ctx));
        stateRegistry.Map(States.InstEORind3, ctx => Ind3(ctx));
        stateRegistry.Map(States.InstEORind4, ctx => Ind4(ctx));
        stateRegistry.Map(States.InstEORind5, ctx => Ind5(ctx));
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

    // [0x49] EOR immediate
    public void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            var result = ctx.Regs.A.Copy().Xor(data);
            ctx.Regs.UpdateAUpdateFlags(result);
            ctx.DbgOperand1 = data;
        }
        ctx.AdvanceState(States.Fetch);
    }

    // [0x45] EOR zeropage
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstEORzpg3);
    }
    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            XorAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x55] EOR zeropage,X
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstEORzpgx3);
    }
    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstEORzpgx4);
    }
    public void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            XorAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x4D] EOR absolute
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstEORabs3);
    }
    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstEORabs4);
    }
    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            XorAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x5D] EOR absolute,X
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstEORabsx3);
    }
    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstEORabsx4);
    }
    public void Absx4(Context ctx)
    {
        var nextState = States.Fetch;
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            var beforePage = ctx.Regs.EA.Msb().Copy();
            ctx.Regs.IncEAWithX();
            var afterPage = ctx.Regs.EA.Msb().Copy();
            ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
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
    public void Absx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            XorAWithTemp(ctx);
        }
        ctx.AdvanceState(States.Fetch);
    }

    // [0x59] EOR absolute,Y
    public void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstEORabsy3);
    }
    public void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstEORabsy4);
    }
    public void Absy4(Context ctx)
    {
        var nextState = States.Fetch;
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            var beforePage = ctx.Regs.EA.Msb().Copy();
            ctx.Regs.IncEAWithY();
            var afterPage = ctx.Regs.EA.Msb().Copy();
            ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
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
    public void Absy5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            XorAWithTemp(ctx);
        }
        ctx.AdvanceState(States.Fetch);
    }

    // [0x41] EOR (indirect,X)
    public void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstEORindx3);
    }
    public void Indx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            ctx.Regs.EA.Lsb().AddWithWrapAround(ctx.Regs.X);
            ctx.Regs.EA.Msb().Zero();
        }
        ctx.AdvanceState(States.InstEORindx4);
    }
    public void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstEORindx5);
    }
    public void Indx5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstEORindx6);
    }
    public void Indx6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            XorAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x51] EOR (indirect),Y
    public void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstEORindy3);
    }
    public void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstEORindy4);
    }
    public void Indy4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstEORindy5);
    }
    public void Indy5(Context ctx)
    {
        var nextState = States.Fetch;
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            var beforePage = ctx.Regs.EA.Msb().Copy();
            ctx.Regs.IncEAWithY();
            var afterPage = ctx.Regs.EA.Msb().Copy();
            ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            if (!ctx.CrossedPageBoundary)
                XorAWithTemp(ctx);
            else
                nextState = States.InstEORindy6;
        }
        ctx.AdvanceState(nextState);
    }
    public void Indy6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            XorAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x2] EOR (indirect)
    public void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstEORind3);
    }
    public void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstEORind4);
    }
    public void Ind4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstEORind5);
    }
    public void Ind5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            XorAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
