// File-scoped namespace for Sim6502.Instructions
namespace Sim6502.Instructions;

internal class InstCPY : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.CPYimm, States.InstCPYimm2);
        registry.Map(OpCodes.CPYzpg, States.InstCPYzpg2);
        registry.Map(OpCodes.CPYabs, States.InstCPYabs2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstCPYimm2, Imm2);
        stateRegistry.Map(States.InstCPYzpg2, Zpg2);
        stateRegistry.Map(States.InstCPYzpg3, Zpg3);
        stateRegistry.Map(States.InstCPYabs2, Abs2);
        stateRegistry.Map(States.InstCPYabs3, Abs3);
        stateRegistry.Map(States.InstCPYabs4, Abs4);

        return this;
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
    private void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp.UpdateValue(data);
            CpyWithTemp(ctx);
            ctx.DbgOperand1 = data;
        }

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xC4] CPY zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstCPYzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CpyWithTemp(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xCC] CPY absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstCPYabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstCPYabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            CpyWithTemp(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
