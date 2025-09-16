using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Instructions;

public class InstIncrement : InstBase
{
    public InstIncrement(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.INCacc, States.InstINCacc2);
        registry.Map(OpCodes.INCzpg, States.InstINCzpg2);
        registry.Map(OpCodes.INCzpgx, States.InstINCzpgx2);
        registry.Map(OpCodes.INCabs, States.InstINCabs2);
        registry.Map(OpCodes.INCabsx, States.InstINCabsx2);
        registry.Map(OpCodes.INXimp, States.InstINXimp2);
        registry.Map(OpCodes.INYimp, States.InstINYimp2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstINCacc2, ctx => IncAcc2(ctx));
        stateRegistry.Map(States.InstINCzpg2, ctx => IncZpg2(ctx));
        stateRegistry.Map(States.InstINCzpg3, ctx => IncZpg3(ctx));
        stateRegistry.Map(States.InstINCzpg4, ctx => IncZpg4(ctx));
        stateRegistry.Map(States.InstINCzpg5, ctx => IncZpg5(ctx));
        stateRegistry.Map(States.InstINCzpgx2, ctx => IncZpgx2(ctx));
        stateRegistry.Map(States.InstINCzpgx3, ctx => IncZpgx3(ctx));
        stateRegistry.Map(States.InstINCzpgx4, ctx => IncZpgx4(ctx));
        stateRegistry.Map(States.InstINCzpgx5, ctx => IncZpgx5(ctx));
        stateRegistry.Map(States.InstINCzpgx6, ctx => IncZpgx6(ctx));
        stateRegistry.Map(States.InstINCabs2, ctx => IncAbs2(ctx));
        stateRegistry.Map(States.InstINCabs3, ctx => IncAbs3(ctx));
        stateRegistry.Map(States.InstINCabs4, ctx => IncAbs4(ctx));
        stateRegistry.Map(States.InstINCabs5, ctx => IncAbs5(ctx));
        stateRegistry.Map(States.InstINCabs6, ctx => IncAbs6(ctx));
        stateRegistry.Map(States.InstINCabsx2, ctx => IncAbsx2(ctx));
        stateRegistry.Map(States.InstINCabsx3, ctx => IncAbsx3(ctx));
        stateRegistry.Map(States.InstINCabsx4, ctx => IncAbsx4(ctx));
        stateRegistry.Map(States.InstINCabsx5, ctx => IncAbsx5(ctx));
        stateRegistry.Map(States.InstINCabsx6, ctx => IncAbsx6(ctx));
        stateRegistry.Map(States.InstINCabsx7, ctx => IncAbsx7(ctx));
        stateRegistry.Map(States.InstINXimp2, ctx => InxImp2(ctx));
        stateRegistry.Map(States.InstINYimp2, ctx => InyImp2(ctx));
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

    // [0x1A] INC accumulator
    public void IncAcc2(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            IncAndUpdateFlags(ctx, ctx.Regs.A);
        ctx.AdvanceState(States.Fetch);
    }
    // [0xE6] INC zeropage
    public void IncZpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstINCzpg3);
    }
    public void IncZpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstINCzpg4);
    }
    public void IncZpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            IncTempAndUpdateFlags(ctx);
        ctx.AdvanceState(States.InstINCzpg5);
    }
    public void IncZpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xF6] INC zeropage,X
    public void IncZpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstINCzpgx3);
    }
    public void IncZpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstINCzpgx4);
    }
    public void IncZpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstINCzpgx5);
    }
    public void IncZpgx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            IncTempAndUpdateFlags(ctx);
        ctx.AdvanceState(States.InstINCzpgx6);
    }
    public void IncZpgx6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xEE] INC absolute
    public void IncAbs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstINCabs3);
    }
    public void IncAbs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstINCabs4);
    }
    public void IncAbs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstINCabs5);
    }
    public void IncAbs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            IncTempAndUpdateFlags(ctx);
        ctx.AdvanceState(States.InstINCabs6);
    }
    public void IncAbs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xFE] INC absolute,X
    public void IncAbsx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstINCabsx3);
    }
    public void IncAbsx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstINCabsx4);
    }
    public void IncAbsx4(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.IncEAWithX();
        ctx.AdvanceState(States.InstINCabsx5);
    }
    public void IncAbsx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstINCabsx6);
    }
    public void IncAbsx6(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            IncTempAndUpdateFlags(ctx);
        ctx.AdvanceState(States.InstINCabsx7);
    }
    public void IncAbsx7(Context ctx)
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
    public void InxImp2(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
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
    public void InyImp2(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            IncAndUpdateFlags(ctx, ctx.Regs.Y);
        ctx.AdvanceState(States.Fetch);
    }
}
