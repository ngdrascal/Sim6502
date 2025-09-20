using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Instructions;

public class InstIncrement : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.INCacc, States.InstINCacc2);
        registry.Map(OpCodes.INCzpg, States.InstINCzpg2);
        registry.Map(OpCodes.INCzpgx, States.InstINCzpgx2);
        registry.Map(OpCodes.INCabs, States.InstINCabs2);
        registry.Map(OpCodes.INCabsx, States.InstINCabsx2);
        registry.Map(OpCodes.INXimp, States.InstINXimp2);
        registry.Map(OpCodes.INYimp, States.InstINYimp2);
        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstINCacc2, IncAcc2);
        stateRegistry.Map(States.InstINCzpg2, IncZpg2);
        stateRegistry.Map(States.InstINCzpg3, IncZpg3);
        stateRegistry.Map(States.InstINCzpg4, IncZpg4);
        stateRegistry.Map(States.InstINCzpg5, IncZpg5);
        stateRegistry.Map(States.InstINCzpgx2, IncZpgx2);
        stateRegistry.Map(States.InstINCzpgx3, IncZpgx3);
        stateRegistry.Map(States.InstINCzpgx4, IncZpgx4);
        stateRegistry.Map(States.InstINCzpgx5, IncZpgx5);
        stateRegistry.Map(States.InstINCzpgx6, IncZpgx6);
        stateRegistry.Map(States.InstINCabs2, IncAbs2);
        stateRegistry.Map(States.InstINCabs3, IncAbs3);
        stateRegistry.Map(States.InstINCabs4, IncAbs4);
        stateRegistry.Map(States.InstINCabs5, IncAbs5);
        stateRegistry.Map(States.InstINCabs6, IncAbs6);
        stateRegistry.Map(States.InstINCabsx2, IncAbsx2);
        stateRegistry.Map(States.InstINCabsx3, IncAbsx3);
        stateRegistry.Map(States.InstINCabsx4, IncAbsx4);
        stateRegistry.Map(States.InstINCabsx5, IncAbsx5);
        stateRegistry.Map(States.InstINCabsx6, IncAbsx6);
        stateRegistry.Map(States.InstINCabsx7, IncAbsx7);
        stateRegistry.Map(States.InstINXimp2, InxImp2);
        stateRegistry.Map(States.InstINYimp2, InyImp2);
        return this;
    }

    private void IncTempAndUpdateFlags(Context ctx)
    {
        var temp = ctx.Regs.Temp;
        temp.Inc();
        ctx.Regs.P.Zero.UpdateValue(temp.EqualsZero());
        ctx.Regs.P.Negative.UpdateValue(temp.IsBitSet(7));
    }

    private void IncAndUpdateFlags(Context ctx, UInt8 value)
    {
        value.Inc();
        ctx.Regs.P.Zero.UpdateValue(value.EqualsZero());
        ctx.Regs.P.Negative.UpdateValue(value.IsBitSet(7));
    }

    ///////////////////////////////////////////////////////////////////////////////
    // INC - Increment Memory by One
    // M + 1 -> M
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // accumulator    INC           1A      1      2
    // zeropage       INC oper      E6      2      5
    // zeropage,X     INC oper,X    F6      2      6
    // absolute       INC oper      EE      3      6
    // absolute,X     INC oper,X    FE      3      7
    ///////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0x1A] INC accumulator
    // -------------------------------------------------------------------------
    private void IncAcc2(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            IncAndUpdateFlags(ctx, ctx.Regs.A);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xE6] INC zeropage
    // -------------------------------------------------------------------------
    private void IncZpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstINCzpg3);
    }

    private void IncZpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstINCzpg4);
    }

    private void IncZpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            IncTempAndUpdateFlags(ctx);

        ctx.AdvanceState(States.InstINCzpg5);
    }

    private void IncZpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xF6] INC zeropage,X
    // -------------------------------------------------------------------------
    private void IncZpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstINCzpgx3);
    }

    private void IncZpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstINCzpgx4);
    }

    private void IncZpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstINCzpgx5);
    }

    private void IncZpgx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            IncTempAndUpdateFlags(ctx);

        ctx.AdvanceState(States.InstINCzpgx6);
    }

    private void IncZpgx6(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xEE] INC absolute
    // -------------------------------------------------------------------------
    private void IncAbs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstINCabs3);
    }

    private void IncAbs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstINCabs4);
    }

    private void IncAbs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstINCabs5);
    }

    private void IncAbs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            IncTempAndUpdateFlags(ctx);

        ctx.AdvanceState(States.InstINCabs6);
    }

    private void IncAbs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xFE] INC absolute,X
    // -------------------------------------------------------------------------
    private void IncAbsx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstINCabsx3);
    }

    private void IncAbsx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstINCabsx4);
    }

    private void IncAbsx4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEAWithX();

        ctx.AdvanceState(States.InstINCabsx5);
    }

    private void IncAbsx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);

        ctx.AdvanceState(States.InstINCabsx6);
    }

    private void IncAbsx6(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            IncTempAndUpdateFlags(ctx);

        ctx.AdvanceState(States.InstINCabsx7);
    }

    private void IncAbsx7(Context ctx)
    {
        StoreTempToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // INX - Increment Index X by One
    // X + 1 -> X
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        INX           E8      1      2
    ///////////////////////////////////////////////////////////////////////////////
    private void InxImp2(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            IncAndUpdateFlags(ctx, ctx.Regs.X);

        ctx.AdvanceState(States.Fetch);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // INY - Increment Index Y by One
    // Y + 1 -> Y
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        INY           C8      1      2 
    ///////////////////////////////////////////////////////////////////////////////   
    private void InyImp2(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            IncAndUpdateFlags(ctx, ctx.Regs.Y);

        ctx.AdvanceState(States.Fetch);
    }
}
