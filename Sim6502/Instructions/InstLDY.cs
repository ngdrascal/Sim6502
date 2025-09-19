namespace Sim6502.Instructions;

public class InstLDY : InstBase
{
    public InstLDY(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.LDYimm, States.InstLDYimm2);
        registry.Map(OpCodes.LDYzpg, States.InstLDYzpg2);
        registry.Map(OpCodes.LDYzpgx, States.InstLDYzpgx2);
        registry.Map(OpCodes.LDYabs, States.InstLDYabs2);
        registry.Map(OpCodes.LDYabsx, States.InstLDYabsx2);
    }

    protected override void RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstLDYimm2, ctx => Imm2(ctx));
        registry.Map(States.InstLDYzpg2, ctx => Zpg2(ctx));
        registry.Map(States.InstLDYzpg3, ctx => Zpg3(ctx));
        registry.Map(States.InstLDYzpgx2, ctx => Zpgx2(ctx));
        registry.Map(States.InstLDYzpgx3, ctx => Zpgx3(ctx));
        registry.Map(States.InstLDYzpgx4, ctx => Zpgx4(ctx));
        registry.Map(States.InstLDYabs2, ctx => Abs2(ctx));
        registry.Map(States.InstLDYabs3, ctx => Abs3(ctx));
        registry.Map(States.InstLDYabs4, ctx => Abs4(ctx));
        registry.Map(States.InstLDYabsx2, ctx => Absx2(ctx));
        registry.Map(States.InstLDYabsx3, ctx => Absx3(ctx));
        registry.Map(States.InstLDYabsx4, ctx => Absx4(ctx));
        registry.Map(States.InstLDYabsx5, ctx => Absx5(ctx));
    }

    private void LoadYFromEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var y = ctx.Pins.GetDataBusPins();
            ctx.Regs.SetYUpdateFlags(y);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // LDY - Load Index Y with Memory
    // M -> Y
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // immediate      LDY #oper     A0      2      2  
    // zeropage       LDY oper      A4      2      3  
    // zeropage,X     LDY oper,X    B4      2      4  
    // absolute       LDY oper      AC      3      4  
    // absolute,X     LDY oper,X    BC      3      4+1 
    ///////////////////////////////////////////////////////////////////////////////

    // [0xA0] LDY immediate
    public void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.SetYUpdateFlags(data);
            ctx.DbgOperand1 = data;
        }
        ctx.AdvanceState(States.Fetch);
    }

    // [0xA4] LDY zeropage
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLDYzpg3);
    }
    public void Zpg3(Context ctx)
    {
        LoadYFromEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xB4] LDY zeropage,X
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLDYzpgx3);
    }
    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstLDYzpgx4);
    }
    public void Zpgx4(Context ctx)
    {
        LoadYFromEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xAC] LDY absolute
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLDYabs3);
    }
    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstLDYabs4);
    }
    public void Abs4(Context ctx)
    {
        LoadYFromEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xBC] LDY absolute,X
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLDYabsx3);
    }
    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstLDYabsx4);
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
            if (!ctx.CrossedPageBoundary)
                ctx.Regs.SetYUpdateFlags(data);
            else
                nextState = States.InstLDYabsx5;
        }
        ctx.AdvanceState(nextState);
    }
    public void Absx5(Context ctx)
    {
        LoadYFromEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
