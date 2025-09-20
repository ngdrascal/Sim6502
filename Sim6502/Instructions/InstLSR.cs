using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Instructions;

public class InstLSR : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.LSRacc, States.InstLSRacc2);
        registry.Map(OpCodes.LSRzpg, States.InstLSRzpg2);
        registry.Map(OpCodes.LSRzpgx, States.InstLSRzpgx2);
        registry.Map(OpCodes.LSRabs, States.InstLSRabs2);
        registry.Map(OpCodes.LSRabsx, States.InstLSRabsx2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstLSRacc2, Acc2);
        stateRegistry.Map(States.InstLSRzpg2, Zpg2);
        stateRegistry.Map(States.InstLSRzpg3, Zpg3);
        stateRegistry.Map(States.InstLSRzpg4, Zpg4);
        stateRegistry.Map(States.InstLSRzpg5, Zpg5);
        stateRegistry.Map(States.InstLSRzpgx2, Zpgx2);
        stateRegistry.Map(States.InstLSRzpgx3, Zpgx3);
        stateRegistry.Map(States.InstLSRzpgx4, Zpgx4);
        stateRegistry.Map(States.InstLSRzpgx5, Zpgx5);
        stateRegistry.Map(States.InstLSRzpgx6, Zpgx6);
        stateRegistry.Map(States.InstLSRabs2, Abs2);
        stateRegistry.Map(States.InstLSRabs3, Abs3);
        stateRegistry.Map(States.InstLSRabs4, Abs4);
        stateRegistry.Map(States.InstLSRabs5, Abs5);
        stateRegistry.Map(States.InstLSRabs6, Abs6);
        stateRegistry.Map(States.InstLSRabsx2, Absx2);
        stateRegistry.Map(States.InstLSRabsx3, Absx3);
        stateRegistry.Map(States.InstLSRabsx4, Absx4);
        stateRegistry.Map(States.InstLSRabsx5, Absx5);
        stateRegistry.Map(States.InstLSRabsx6, Absx6);
        stateRegistry.Map(States.InstLSRabsx7, Absx7);

        return this;
    }

    private void LsrUpdateFlags(Context ctx, UInt8 value)
    {
        ctx.Regs.P.Carry.UpdateValue(value.IsBitSet(0));
        ctx.Regs.P.Negative.Clear();
        value.Shr();
        ctx.Regs.P.Zero.UpdateValue(value.EqualsZero());
    }

    ///////////////////////////////////////////////////////////////////////////////
    // LSR - Logical Shift Right
    // 0 -> [76543210] -> C
    // N V B D I Z C
    // 0 - - - - + +
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // accumulator    LSR A         4A      1      2  
    // zeropage       LSR oper      46      2      5  
    // zeropage,X     LSR oper,X    56      2      6  
    // absolute       LSR oper      4E      3      6  
    // absolute,X     LSR oper,X    5E      3      7  
    /////////////////////////////////////////////////////////////////////////////// 

    // -------------------------------------------------------------------------
    // [0x4A] LSR accumulator
    // -------------------------------------------------------------------------
    private void Acc2(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            LsrUpdateFlags(ctx, ctx.Regs.A);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x46] LSR zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstLSRzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstLSRzpg4);
    }

    private void Zpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            LsrUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstLSRzpg5);
    }

    private void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x56] LSR zeropage,X 
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstLSRzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstLSRzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstLSRzpgx5);
    }

    private void Zpgx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            LsrUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstLSRzpgx6);
    }
    private void Zpgx6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x4E] LSR absolute 
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstLSRabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstLSRabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstLSRabs5);
    }

    private void Abs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            LsrUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstLSRabs6);
    }

    private void Abs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x5E] LSR absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstLSRabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstLSRabsx4);
    }

    private void Absx4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEAWithX();

        ctx.AdvanceState(States.InstLSRabsx5);
    }

    private void Absx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstLSRabsx6);
    }

    private void Absx6(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            LsrUpdateFlags(ctx, ctx.Regs.Temp);

        ctx.AdvanceState(States.InstLSRabsx7);
    }

    private void Absx7(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
