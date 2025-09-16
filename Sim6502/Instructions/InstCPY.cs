// File-scoped namespace for Sim6502.Instructions
namespace Sim6502.Instructions;

public class InstCPY : InstBase
{
    public InstCPY(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.CPYimm, States.InstCPYimm2);
        registry.Map(OpCodes.CPYzpg, States.InstCPYzpg2);
        registry.Map(OpCodes.CPYabs, States.InstCPYabs2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstCPYimm2, ctx => Imm2(ctx));

        stateRegistry.Map(States.InstCPYzpg2, ctx => Zpg2(ctx));
        stateRegistry.Map(States.InstCPYzpg3, ctx => Zpg3(ctx));

        stateRegistry.Map(States.InstCPYabs2, ctx => Abs2(ctx));
        stateRegistry.Map(States.InstCPYabs3, ctx => Abs3(ctx));
        stateRegistry.Map(States.InstCPYabs4, ctx => Abs4(ctx));
    }

    private void CpyWithTemp(Context ctx)
    {
        var flags = ctx.Regs.P;
        int y = ctx.Regs.Y.ToInt();
        int operand = ctx.Regs.Temp.ToInt();

        int result = y - operand;
        flags.Carry.UpdateValue(result >= 0);
        flags.Zero.UpdateValue(result == 0);
        flags.Negative.UpdateValue((result & 0x80) > 0);
    }

    /////////////////////////////////////////////////////////////////////////////
    // CPY - Compare Memory and Index Y
    // Y - M
    // N V B D I Z C
    // + - - - - + +
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // immediate      CPY #oper     C0      2      2  
    // zeropage       CPY oper      C4      2      3  
    // absolute       CPY oper      CC      3      4 
    /////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0xC0] CPY immediate
    // -------------------------------------------------------------------------
    public void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);

        if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            CpyWithTemp(ctx);
            ctx.DbgOperand1 = data;
        }

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xC4] CPY zeropage
    // -------------------------------------------------------------------------
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstCPYzpg3);
    }

    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            CpyWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xCC] CPY absolute
    // -------------------------------------------------------------------------
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstCPYabs3);
    }

    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstCPYabs4);
    }

    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2Lastsubstep)
            CpyWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
