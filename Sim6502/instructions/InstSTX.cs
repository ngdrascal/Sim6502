namespace Sim6502.Instructions;

public class InstSTX : InstBase
{
    public InstSTX(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.STXzpg, States.InstSTXzpg2);
        registry.Map(OpCodes.STXzpgy, States.InstSTXzpgy2);
        registry.Map(OpCodes.STXabs, States.InstSTXabs2);
    }

    protected override void RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstSTXzpg2, ctx => Zpg2(ctx));
        registry.Map(States.InstSTXzpg3, ctx => Zpg3(ctx));

        registry.Map(States.InstSTXzpgy2, ctx => Zpgy2(ctx));
        registry.Map(States.InstSTXzpgy3, ctx => Zpgy3(ctx));
        registry.Map(States.InstSTXzpgy4, ctx => Zpgy4(ctx));

        registry.Map(States.InstSTXabs2, ctx => Abs2(ctx));
        registry.Map(States.InstSTXabs3, ctx => Abs3(ctx));
        registry.Map(States.InstSTXabs4, ctx => Abs4(ctx));
    }

    protected void StoreXToEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P1MIDDLESTEP)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Constants.Write);
            ctx.Pins.SetDataBusMode(DataBusMode.Output);
        }
        else if (ctx.GetSubStep() == Constants.P2MIDDLESTEP)
        {
            ctx.Pins.SetDataBusPins(ctx.Regs.X);
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    // STX - Store Index X in Memory
    // X -> M
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // zeropage       STX oper      86      2      3  
    // zeropage,Y     STX oper,Y    96      2      4  
    // absolute       STX oper      8E      3      4  
    /////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------------- 
    // [0x86] STX zeropage
    // ------------------------------------------------------------------------- 
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTXzpg3);
    }

    public void Zpg3(Context ctx)
    {
        StoreXToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // ------------------------------------------------------------------------- 
    // [0x96] STX zeropage,Y
    // ------------------------------------------------------------------------- 
    public void Zpgy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTXzpgy3);
    }

    public void Zpgy3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
            ctx.Regs.IncEALWithY();

        ctx.AdvanceState(States.InstSTXzpgy4);
    }

    public void Zpgy4(Context ctx)
    {
        StoreXToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // ------------------------------------------------------------------------- 
    // [0x8E] STX absolute
    // ------------------------------------------------------------------------- 
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTXabs3);
    }

    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstSTXabs4);
    }

    public void Abs4(Context ctx)
    {
        StoreXToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}