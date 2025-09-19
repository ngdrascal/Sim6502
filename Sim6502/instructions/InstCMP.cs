namespace Sim6502.Instructions;

public class InstCMP : InstBase
{
    public InstCMP(IT2Registry it2Registry, IStateRegistry stateRegistry) : base(it2Registry, stateRegistry) { }

    protected override void RegisterT2State(IT2Registry registry)
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
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstCMPimm2, ctx => Imm2(ctx));

        stateRegistry.Map(States.InstCMPzpg2, ctx => Zpg2(ctx));
        stateRegistry.Map(States.InstCMPzpg3, ctx => Zpg3(ctx));

        stateRegistry.Map(States.InstCMPzpgx2, ctx => Zpgx2(ctx));
        stateRegistry.Map(States.InstCMPzpgx3, ctx => Zpgx3(ctx));
        stateRegistry.Map(States.InstCMPzpgx4, ctx => Zpgx4(ctx));

        stateRegistry.Map(States.InstCMPabs2, ctx => Abs2(ctx));
        stateRegistry.Map(States.InstCMPabs3, ctx => Abs3(ctx));
        stateRegistry.Map(States.InstCMPabs4, ctx => Abs4(ctx));

        stateRegistry.Map(States.InstCMPabsx2, ctx => Absx2(ctx));
        stateRegistry.Map(States.InstCMPabsx3, ctx => Absx3(ctx));
        stateRegistry.Map(States.InstCMPabsx4, ctx => Absx4(ctx));
        stateRegistry.Map(States.InstCMPabsx5, ctx => Absx5(ctx));

        stateRegistry.Map(States.InstCMPabsy2, ctx => Absy2(ctx));
        stateRegistry.Map(States.InstCMPabsy3, ctx => Absy3(ctx));
        stateRegistry.Map(States.InstCMPabsy4, ctx => Absy4(ctx));
        stateRegistry.Map(States.InstCMPabsy5, ctx => Absy5(ctx));

        stateRegistry.Map(States.InstCMPindx2, ctx => Indx2(ctx));
        stateRegistry.Map(States.InstCMPindx3, ctx => Indx3(ctx));
        stateRegistry.Map(States.InstCMPindx4, ctx => Indx4(ctx));
        stateRegistry.Map(States.InstCMPindx5, ctx => Indx5(ctx));
        stateRegistry.Map(States.InstCMPindx6, ctx => Indx6(ctx));

        stateRegistry.Map(States.InstCMPindy2, ctx => Indy2(ctx));
        stateRegistry.Map(States.InstCMPindy3, ctx => Indy3(ctx));
        stateRegistry.Map(States.InstCMPindy4, ctx => Indy4(ctx));
        stateRegistry.Map(States.InstCMPindy5, ctx => Indy5(ctx));
        stateRegistry.Map(States.InstCMPindy6, ctx => Indy6(ctx));

        stateRegistry.Map(States.InstCMPind2, ctx => Ind2(ctx));
        stateRegistry.Map(States.InstCMPind3, ctx => Ind3(ctx));
        stateRegistry.Map(States.InstCMPind4, ctx => Ind4(ctx));
        stateRegistry.Map(States.InstCMPind5, ctx => Ind5(ctx));
    }

    private void CmpWithTemp(Context ctx)
    {
        var flags = ctx.Regs.P;
        int a = ctx.Regs.A.ToInt();
        int operand = ctx.Regs.Temp.ToInt();
        int result = a - operand;
        flags.Carry.UpdateValue(result >= 0);
        flags.Zero.UpdateValue(result == 0);
        flags.Negative.UpdateValue((result & 0x80) > 0);
    }

    // [0xC9] CMP immediate
    public void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            CmpWithTemp(ctx);
            ctx.DbgOperand1 = data;
        }
        ctx.AdvanceState(States.Fetch);
    }

    // [0xC5] CMP zeropage
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstCMPzpg3);
    }

    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xD5] CMP zeropage,X
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstCMPzpgx3);
    }

    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstCMPzpgx4);
    }

    public void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xCD] CMP absolute
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstCMPabs3);
    }

    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstCMPabs4);
    }

    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xDD] CMP absolute,X
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstCMPabsx3);
    }

    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstCMPabsx4);
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

    public void Absx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xD9] CMP absolute,Y
    public void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstCMPabsy3);
    }

    public void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstCMPabsy4);
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

    public void Absy5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        CmpWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xC1] CMP (indirect,X)
    public void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstCMPindx3);
    }

    public void Indx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            ctx.Regs.EA.Lsb().AddWithWrapAround(ctx.Regs.X);
            ctx.Regs.EA.Msb().Zero();
        }
        ctx.AdvanceState(States.InstCMPindx4);
    }

    public void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstCMPindx5);
    }

    public void Indx5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstCMPindx6);
    }

    public void Indx6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xD1] CMP (indirect),Y
    public void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstCMPindy3);
    }

    public void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstCMPindy4);
    }

    public void Indy4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstCMPindy5);
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

    public void Indy6(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xD2] CMP (indirect)
    public void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstCMPind3);
    }

    public void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstCMPind4);
    }

    public void Ind4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstCMPind5);
    }

    public void Ind5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CmpWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}