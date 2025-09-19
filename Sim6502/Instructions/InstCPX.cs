namespace Sim6502.Instructions;

public class InstCPX : InstBase
{
    public InstCPX(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.CPXimm, States.InstCPXimm2);
        registry.Map(OpCodes.CPXzpg, States.InstCPXzpg2);
        registry.Map(OpCodes.CPXabs, States.InstCPXabs2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstCPXimm2, ctx => Imm2(ctx));
        stateRegistry.Map(States.InstCPXzpg2, ctx => Zpg2(ctx));
        stateRegistry.Map(States.InstCPXzpg3, ctx => Zpg3(ctx));
        stateRegistry.Map(States.InstCPXabs2, ctx => Abs2(ctx));
        stateRegistry.Map(States.InstCPXabs3, ctx => Abs3(ctx));
        stateRegistry.Map(States.InstCPXabs4, ctx => Abs4(ctx));
    }

    private void CpxWithTemp(Context ctx)
    {
        var flags = ctx.Regs.P;
        int x = ctx.Regs.X.ToInt();
        int operand = ctx.Regs.Temp.ToInt();
        int result = x - operand;
        flags.Carry.UpdateValue(result >= 0);
        flags.Zero.UpdateValue(result == 0);
        flags.Negative.UpdateValue((result & 0x80) > 0);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // CPX - Compare Memory and Index Y
    // X - M
    // N V B D I Z C
    // + - - - - + +
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // immediate      CPX #oper     E0      2      2
    // zeropage       CPX oper      E4      2      3
    // absolute       CPX oper      EC      3      4
    ///////////////////////////////////////////////////////////////////////////////

    // [0xE0] CPX immediate
    public void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            CpxWithTemp(ctx);
            ctx.DbgOperand1 = data;
        }
        ctx.AdvanceState(States.Fetch);
    }

    // [0xE4] CPX zeropage
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstCPXzpg3);
    }
    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CpxWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xEC] CPX absolute
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstCPXabs3);
    }
    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstCPXabs4);
    }
    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CpxWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
