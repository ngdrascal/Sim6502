using Sim6502.types;

namespace Sim6502.Instructions;

public class InstSTZ : InstBase
{
    public InstSTZ(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.STZzpg, States.InstSTZzpg2);
        registry.Map(OpCodes.STZzpgx, States.InstSTZzpgx2);
        registry.Map(OpCodes.STZabs, States.InstSTZabs2);
        registry.Map(OpCodes.STZabsx, States.InstSTZabsx2);
    }

    protected override void RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstSTZzpg2, ctx => Zpg2(ctx));
        registry.Map(States.InstSTZzpg3, ctx => Zpg3(ctx));

        registry.Map(States.InstSTZzpgx2, ctx => Zpgx2(ctx));
        registry.Map(States.InstSTZzpgx3, ctx => Zpgx3(ctx));
        registry.Map(States.InstSTZzpgx4, ctx => Zpgx4(ctx));

        registry.Map(States.InstSTZabs2, ctx => Abs2(ctx));
        registry.Map(States.InstSTZabs3, ctx => Abs3(ctx));
        registry.Map(States.InstSTZabs4, ctx => Abs4(ctx));

        registry.Map(States.InstSTZabsx2, ctx => Absx2(ctx));
        registry.Map(States.InstSTZabsx3, ctx => Absx3(ctx));
        registry.Map(States.InstSTZabsx4, ctx => Absx4(ctx));
        registry.Map(States.InstSTZabsx5, ctx => Absx5(ctx));
    }

    protected void StoreZeroToEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P1MiddleStep)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Constants.Write);
            ctx.Pins.SetDataBusMode(DataBusMode.Output);
        }
        else if (ctx.GetSubStep() == Constants.P2MiddleStep)
        {
            ctx.Pins.SetDataBusPins(new UInt8(0));
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    // STZ - Store Zero in Memory
    // 0 -> M
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // zeropage       STZ oper      64      2      3  
    // zeropage,X     STZ oper,X    74      2      4  
    // absolute       STZ oper      9C      3      4  
    // absolute,X     STZ oper,X    9E      3      5  
    /////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0x64] STZ zeropage
    // -------------------------------------------------------------------------
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTZzpg3);
    }

    public void Zpg3(Context ctx)
    {
        StoreZeroToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x74] STZ zeropage,X
    // -------------------------------------------------------------------------
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTZzpgx3);
    }

    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstSTZzpgx4);
    }

    public void Zpgx4(Context ctx)
    {
        StoreZeroToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x9C] STZ absolute
    // -------------------------------------------------------------------------
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTZabs3);
    }

    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstSTZabs4);
    }

    public void Abs4(Context ctx)
    {
        StoreZeroToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x9E] STZ absolute,X
    // -------------------------------------------------------------------------
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTZabsx3);
    }

    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstSTZabsx4);
    }

    public void Absx4(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.IncEALWithX();
        }
        ctx.AdvanceState(States.InstSTZabsx5);
    }

    public void Absx5(Context ctx)
    {
        StoreZeroToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
