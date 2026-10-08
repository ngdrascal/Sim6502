namespace W65C02S.Engine;

internal class InstSTX : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.STXzpg, States.InstSTXzpg2);
        registry.Map(OpCodes.STXzpgy, States.InstSTXzpgy2);
        registry.Map(OpCodes.STXabs, States.InstSTXabs2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstSTXzpg2, Zpg2);
        registry.Map(States.InstSTXzpg3, Zpg3);

        registry.Map(States.InstSTXzpgy2, Zpgy2);
        registry.Map(States.InstSTXzpgy3, Zpgy3);
        registry.Map(States.InstSTXzpgy4, Zpgy4);

        registry.Map(States.InstSTXabs2, Abs2);
        registry.Map(States.InstSTXabs3, Abs3);
        registry.Map(States.InstSTXabs4, Abs4);

        return this;
    }

    private void StoreXToEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.EA;
            ctx.Pins.RWB = Constants.Write;
            ctx.Pins.DataBusMode = DataBusMode.Output;
        }
        else if (ctx.GetSubStep() == Constants.P2MiddleStep)
        {
            ctx.Pins.DataBus = ctx.Regs.X;
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
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTXzpg3);
    }

    private void Zpg3(Context ctx)
    {
        StoreXToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // ------------------------------------------------------------------------- 
    // [0x96] STX zeropage,Y
    // ------------------------------------------------------------------------- 
    private void Zpgy2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTXzpgy3);
    }

    private void Zpgy3(Context ctx)
    {
        IndexZpWithY(ctx);

        ctx.AdvanceState(States.InstSTXzpgy4);
    }

    private void Zpgy4(Context ctx)
    {
        StoreXToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // ------------------------------------------------------------------------- 
    // [0x8E] STX absolute
    // ------------------------------------------------------------------------- 
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTXabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstSTXabs4);
    }

    private void Abs4(Context ctx)
    {
        StoreXToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
