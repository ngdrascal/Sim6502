namespace Sim6502.Instructions;

public class InstSTY : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.STYzpg, States.InstSTYzpg2);
        registry.Map(OpCodes.STYzpgx, States.InstSTYzpgx2);
        registry.Map(OpCodes.STYabs, States.InstSTYabs2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstSTYzpg2, Zpg2);
        registry.Map(States.InstSTYzpg3, Zpg3);

        registry.Map(States.InstSTYzpgx2, Zpgx2);
        registry.Map(States.InstSTYzpgx3, Zpgx3);
        registry.Map(States.InstSTYzpgx4, Zpgx4);

        registry.Map(States.InstSTYabs2, Abs2);
        registry.Map(States.InstSTYabs3, Abs3);
        registry.Map(States.InstSTYabs4, Abs4);

        return this;
    }

    private void StoreYToEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.EA;
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
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTYzpg3);
    }

    private void Zpg3(Context ctx)
    {
        StoreYToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x94] STY zeropage,X
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTYzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstSTYzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        StoreYToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x8C] STY absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTYabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstSTYabs4);
    }

    private void Abs4(Context ctx)
    {
        StoreYToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
