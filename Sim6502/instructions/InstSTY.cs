namespace Sim6502.Instructions;

public class InstSTY : InstBase
{
    public InstSTY(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.STYzpg, States.InstSTYzpg2);
        registry.Map(OpCodes.STYzpgx, States.InstSTYzpgx2);
        registry.Map(OpCodes.STYabs, States.InstSTYabs2);
    }

    protected override void RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstSTYzpg2, ctx => Zpg2(ctx));
        registry.Map(States.InstSTYzpg3, ctx => Zpg3(ctx));

        registry.Map(States.InstSTYzpgx2, ctx => Zpgx2(ctx));
        registry.Map(States.InstSTYzpgx3, ctx => Zpgx3(ctx));
        registry.Map(States.InstSTYzpgx4, ctx => Zpgx4(ctx));

        registry.Map(States.InstSTYabs2, ctx => Abs2(ctx));
        registry.Map(States.InstSTYabs3, ctx => Abs3(ctx));
        registry.Map(States.InstSTYabs4, ctx => Abs4(ctx));
    }

    protected void StoreYToEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P1MiddleStep)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Constants.Write);
            ctx.Pins.SetDataBusMode(DataBusMode.Output);
        }
        else if (ctx.GetSubStep() == Constants.P2MiddleStep)
        {
            ctx.Pins.SetDataBusPins(ctx.Regs.Y);
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    // STY - Store Index Y in Memory
    // Y -> M
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // zeropage       STY oper      84      2      3  
    // zeropage,X     STY oper,X    94      2      4  
    // absolute       STY oper      8C      3      4  
    /////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0x84] STY zeropage
    // -------------------------------------------------------------------------
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTYzpg3);
    }

    public void Zpg3(Context ctx)
    {
        StoreYToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x94] STY zeropage,X
    // -------------------------------------------------------------------------
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTYzpgx3);
    }

    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstSTYzpgx4);
    }

    public void Zpgx4(Context ctx)
    {
        StoreYToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x8C] STY absolute
    // -------------------------------------------------------------------------
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTYabs3);
    }

    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstSTYabs4);
    }

    public void Abs4(Context ctx)
    {
        StoreYToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
