namespace Sim6502.Instructions;

public class InstCPX : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.CPXimm, States.InstCPXimm2);
        registry.Map(OpCodes.CPXzpg, States.InstCPXzpg2);
        registry.Map(OpCodes.CPXabs, States.InstCPXabs2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstCPXimm2, Imm2);
        stateRegistry.Map(States.InstCPXzpg2, Zpg2);
        stateRegistry.Map(States.InstCPXzpg3, Zpg3);
        stateRegistry.Map(States.InstCPXabs2, Abs2);
        stateRegistry.Map(States.InstCPXabs3, Abs3);
        stateRegistry.Map(States.InstCPXabs4, Abs4);

        return this;
    }

    private void CpxWithTemp(Context ctx)
    {
        var flags = ctx.Regs.P;
        var x = ctx.Regs.X.ToInt();
        var operand = ctx.Regs.Temp.ToInt();
        var result = x - operand;
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

    // -------------------------------------------------------------------------
    // [0xE0] CPX immediate
    // -------------------------------------------------------------------------
    private void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            CpxWithTemp(ctx);
            ctx.DbgOperand1 = data;
        }

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xE4] CPX zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstCPXzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CpxWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xEC] CPX absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstCPXabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstCPXabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CpxWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
