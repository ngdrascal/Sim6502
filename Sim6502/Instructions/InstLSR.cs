using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Instructions;

public class InstLSR : InstBase
{
    public InstLSR(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.LSRacc, States.InstLSRacc2);
        registry.Map(OpCodes.LSRzpg, States.InstLSRzpg2);
        registry.Map(OpCodes.LSRzpgx, States.InstLSRzpgx2);
        registry.Map(OpCodes.LSRabs, States.InstLSRabs2);
        registry.Map(OpCodes.LSRabsx, States.InstLSRabsx2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstLSRacc2, ctx => Acc2(ctx));
        stateRegistry.Map(States.InstLSRzpg2, ctx => Zpg2(ctx));
        stateRegistry.Map(States.InstLSRzpg3, ctx => Zpg3(ctx));
        stateRegistry.Map(States.InstLSRzpg4, ctx => Zpg4(ctx));
        stateRegistry.Map(States.InstLSRzpg5, ctx => Zpg5(ctx));
        stateRegistry.Map(States.InstLSRzpgx2, ctx => Zpgx2(ctx));
        stateRegistry.Map(States.InstLSRzpgx3, ctx => Zpgx3(ctx));
        stateRegistry.Map(States.InstLSRzpgx4, ctx => Zpgx4(ctx));
        stateRegistry.Map(States.InstLSRzpgx5, ctx => Zpgx5(ctx));
        stateRegistry.Map(States.InstLSRzpgx6, ctx => Zpgx6(ctx));
        stateRegistry.Map(States.InstLSRabs2, ctx => Abs2(ctx));
        stateRegistry.Map(States.InstLSRabs3, ctx => Abs3(ctx));
        stateRegistry.Map(States.InstLSRabs4, ctx => Abs4(ctx));
        stateRegistry.Map(States.InstLSRabs5, ctx => Abs5(ctx));
        stateRegistry.Map(States.InstLSRabs6, ctx => Abs6(ctx));
        stateRegistry.Map(States.InstLSRabsx2, ctx => Absx2(ctx));
        stateRegistry.Map(States.InstLSRabsx3, ctx => Absx3(ctx));
        stateRegistry.Map(States.InstLSRabsx4, ctx => Absx4(ctx));
        stateRegistry.Map(States.InstLSRabsx5, ctx => Absx5(ctx));
        stateRegistry.Map(States.InstLSRabsx6, ctx => Absx6(ctx));
        stateRegistry.Map(States.InstLSRabsx7, ctx => Absx7(ctx));
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

    // [0x4A] LSR accumulator
    public void Acc2(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            LsrUpdateFlags(ctx, ctx.Regs.A);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x46] LSR zeropage
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLSRzpg3);
    }
    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstLSRzpg4);
    }
    public void Zpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            LsrUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstLSRzpg5);
    }
    public void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x56] LSR zeropage,X 
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLSRzpgx3);
    }
    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstLSRzpgx4);
    }
    public void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstLSRzpgx5);
    }
    public void Zpgx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            LsrUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstLSRzpgx6);
    }
    public void Zpgx6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x4E] LSR absolute 
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLSRabs3);
    }
    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstLSRabs4);
    }
    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstLSRabs5);
    }
    public void Abs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            LsrUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstLSRabs6);
    }
    public void Abs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x5E] LSR absolute,X
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLSRabsx3);
    }
    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstLSRabsx4);
    }
    public void Absx4(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.IncEAWithX();
        ctx.AdvanceState(States.InstLSRabsx5);
    }
    public void Absx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstLSRabsx6);
    }
    public void Absx6(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            LsrUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstLSRabsx7);
    }
    public void Absx7(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
