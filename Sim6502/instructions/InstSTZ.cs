using Sim6502.types;

namespace Sim6502.Instructions;

public class InstSTZ : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.STZzpg, States.InstSTZzpg2);
        registry.Map(OpCodes.STZzpgx, States.InstSTZzpgx2);
        registry.Map(OpCodes.STZabs, States.InstSTZabs2);
        registry.Map(OpCodes.STZabsx, States.InstSTZabsx2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstSTZzpg2, Zpg2);
        registry.Map(States.InstSTZzpg3, Zpg3);

        registry.Map(States.InstSTZzpgx2, Zpgx2);
        registry.Map(States.InstSTZzpgx3, Zpgx3);
        registry.Map(States.InstSTZzpgx4, Zpgx4);

        registry.Map(States.InstSTZabs2, Abs2);
        registry.Map(States.InstSTZabs3, Abs3);
        registry.Map(States.InstSTZabs4, Abs4);

        registry.Map(States.InstSTZabsx2, Absx2);
        registry.Map(States.InstSTZabsx3, Absx3);
        registry.Map(States.InstSTZabsx4, Absx4);
        registry.Map(States.InstSTZabsx5, Absx5);

        return this;
    }

    private void StoreZeroToEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.EA;
            ctx.Pins.RWB = Constants.Write;
            ctx.Pins.DataBusMode = DataBusMode.Output;
        }
        else if (ctx.GetSubStep() == Constants.P2MiddleStep)
        {
            ctx.Pins.DataBus = new UInt8(0);
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
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTZzpg3);
    }

    private void Zpg3(Context ctx)
    {
        StoreZeroToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x74] STZ zeropage,X
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTZzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstSTZzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        StoreZeroToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x9C] STZ absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTZabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstSTZabs4);
    }

    private void Abs4(Context ctx)
    {
        StoreZeroToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x9E] STZ absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTZabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstSTZabsx4);
    }

    private void Absx4(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.IncEALWithX();
        }

        ctx.AdvanceState(States.InstSTZabsx5);
    }

    private void Absx5(Context ctx)
    {
        StoreZeroToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
