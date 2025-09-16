namespace Sim6502.Instructions;

public class InstORA : InstBase
{
    public InstORA(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
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
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstORAimm2, ctx => Imm2(ctx));
        stateRegistry.Map(States.InstORAzpg2, ctx => Zpg2(ctx));
        stateRegistry.Map(States.InstORAzpg3, ctx => Zpg3(ctx));
        stateRegistry.Map(States.InstORAzpgx2, ctx => Zpgx2(ctx));
        stateRegistry.Map(States.InstORAzpgx3, ctx => Zpgx3(ctx));
        stateRegistry.Map(States.InstORAzpgx4, ctx => Zpgx4(ctx));
        stateRegistry.Map(States.InstORAabs2, ctx => Abs2(ctx));
        stateRegistry.Map(States.InstORAabs3, ctx => Abs3(ctx));
        stateRegistry.Map(States.InstORAabs4, ctx => Abs4(ctx));
        stateRegistry.Map(States.InstORAabsx2, ctx => Absx2(ctx));
        stateRegistry.Map(States.InstORAabsx3, ctx => Absx3(ctx));
        stateRegistry.Map(States.InstORAabsx4, ctx => Absx4(ctx));
        stateRegistry.Map(States.InstORAabsx5, ctx => Absx5(ctx));
        stateRegistry.Map(States.InstORAabsy2, ctx => Absy2(ctx));
        stateRegistry.Map(States.InstORAabsy3, ctx => Absy3(ctx));
        stateRegistry.Map(States.InstORAabsy4, ctx => Absy4(ctx));
        stateRegistry.Map(States.InstORAabsy5, ctx => Absy5(ctx));
        stateRegistry.Map(States.InstORAindx2, ctx => Indx2(ctx));
        stateRegistry.Map(States.InstORAindx3, ctx => Indx3(ctx));
        stateRegistry.Map(States.InstORAindx4, ctx => Indx4(ctx));
        stateRegistry.Map(States.InstORAindx5, ctx => Indx5(ctx));
        stateRegistry.Map(States.InstORAindx6, ctx => Indx6(ctx));
        stateRegistry.Map(States.InstORAindy2, ctx => Indy2(ctx));
        stateRegistry.Map(States.InstORAindy3, ctx => Indy3(ctx));
        stateRegistry.Map(States.InstORAindy4, ctx => Indy4(ctx));
        stateRegistry.Map(States.InstORAindy5, ctx => Indy5(ctx));
        stateRegistry.Map(States.InstORAindy6, ctx => Indy6(ctx));
        stateRegistry.Map(States.InstORAind2, ctx => Ind2(ctx));
        stateRegistry.Map(States.InstORAind3, ctx => Ind3(ctx));
        stateRegistry.Map(States.InstORAind4, ctx => Ind4(ctx));
        stateRegistry.Map(States.InstORAind5, ctx => Ind5(ctx));
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

    // [0x09] ORA immediate
    public void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            var result = ctx.Regs.A.Copy().Or(data);
            ctx.Regs.UpdateAUpdateFlags(result);
            ctx.DbgOperand1 = data;
        }
        ctx.AdvanceState(States.Fetch);
    }

    // [0x05] ORA zeropage
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstORAzpg3);
    }
    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            OrAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x15] ORA zeropage,X
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstORAzpgx3);
    }
    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstORAzpgx4);
    }
    public void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            OrAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x0D] ORA absolute
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstORAabs3);
    }
    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstORAabs4);
    }
    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            OrAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [01D] ORA absolute,X
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstORAabsx3);
    }
    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstORAabsx4);
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
                OrAWithTemp(ctx);
            }
            else
                nextState = States.InstORAabsx5;
        }
        ctx.AdvanceState(nextState);
    }
    public void Absx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            OrAWithTemp(ctx);
        }
        ctx.AdvanceState(States.Fetch);
    }

    // [0x19] ORA absolute,Y
    public void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstORAabsy3);
    }
    public void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstORAabsy4);
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
                OrAWithTemp(ctx);
            }
            else
                nextState = States.InstORAabsy5;
        }
        ctx.AdvanceState(nextState);
    }
    public void Absy5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            OrAWithTemp(ctx);
        }
        ctx.AdvanceState(States.Fetch);
    }

    // [0x01] ORA (indirect,X)
    public void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstORAindx3);
    }
    public void Indx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
        {
            ctx.Regs.EA.Lsb().AddWithWrapAround(ctx.Regs.X);
            ctx.Regs.EA.Msb().Zero();
        }
        ctx.AdvanceState(States.InstORAindx4);
    }
    public void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstORAindx5);
    }
    public void Indx5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstORAindx6);
    }
    public void Indx6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            OrAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x11] ORA (indirect),Y
    public void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstORAindy3);
    }
    public void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstORAindy4);
    }
    public void Indy4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstORAindy5);
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
                OrAWithTemp(ctx);
            else
                nextState = States.InstORAindy6;
        }
        ctx.AdvanceState(nextState);
    }
    public void Indy6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            OrAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x12] ORA (indirect)
    public void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstORAind3);
    }
    public void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstORAind4);
    }
    public void Ind4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstORAind5);
    }
    public void Ind5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            OrAWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
