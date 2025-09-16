using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Instructions;

public class InstROR : InstBase
{
    public InstROR(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.RORacc, States.InstRORacc2);
        registry.Map(OpCodes.RORzpg, States.InstRORzpg2);
        registry.Map(OpCodes.RORzpgx, States.InstRORzpgx2);
        registry.Map(OpCodes.RORabs, States.InstRORabs2);
        registry.Map(OpCodes.RORabsx, States.InstRORabsx2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstRORacc2, ctx => Acc2(ctx));
        stateRegistry.Map(States.InstRORzpg2, ctx => Zpg2(ctx));
        stateRegistry.Map(States.InstRORzpg3, ctx => Zpg3(ctx));
        stateRegistry.Map(States.InstRORzpg4, ctx => Zpg4(ctx));
        stateRegistry.Map(States.InstRORzpg5, ctx => Zpg5(ctx));
        stateRegistry.Map(States.InstRORzpgx2, ctx => Zpgx2(ctx));
        stateRegistry.Map(States.InstRORzpgx3, ctx => Zpgx3(ctx));
        stateRegistry.Map(States.InstRORzpgx4, ctx => Zpgx4(ctx));
        stateRegistry.Map(States.InstRORzpgx5, ctx => Zpgx5(ctx));
        stateRegistry.Map(States.InstRORzpgx6, ctx => Zpgx6(ctx));
        stateRegistry.Map(States.InstRORabs2, ctx => Abs2(ctx));
        stateRegistry.Map(States.InstRORabs3, ctx => Abs3(ctx));
        stateRegistry.Map(States.InstRORabs4, ctx => Abs4(ctx));
        stateRegistry.Map(States.InstRORabs5, ctx => Abs5(ctx));
        stateRegistry.Map(States.InstRORabs6, ctx => Abs6(ctx));
        stateRegistry.Map(States.InstRORabsx2, ctx => Absx2(ctx));
        stateRegistry.Map(States.InstRORabsx3, ctx => Absx3(ctx));
        stateRegistry.Map(States.InstRORabsx4, ctx => Absx4(ctx));
        stateRegistry.Map(States.InstRORabsx5, ctx => Absx5(ctx));
        stateRegistry.Map(States.InstRORabsx6, ctx => Absx6(ctx));
        stateRegistry.Map(States.InstRORabsx7, ctx => Absx7(ctx));
    }

    private void RorUpdateFlags(Context ctx, UInt8 value)
    {
        var oldCarry = ctx.Regs.P.Carry.Copy();
        ctx.Regs.P.Carry.UpdateValue(value.IsBitSet(0));
        value.Shr(oldCarry);
        ctx.Regs.P.Zero.UpdateValue(value.EqualsZero());
        ctx.Regs.P.Negative.UpdateValue(value.IsBitSet(7));
    }

    ///////////////////////////////////////////////////////////////////////////////
    // ROR - Rotate Right
    // C -> [76543210] -> C
    // N V B D I Z C
    // + - - - - + +
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // accumulator    ROR A         6A      1      2  
    // zeropage       ROR oper      66      2      5  
    // zeropage,X     ROR oper,X    76      2      6  
    // absolute       ROR oper      6E      3      6  
    // absolute,X     ROR oper,X    7E      3      7  
    /////////////////////////////////////////////////////////////////////////////// 

    // [0x6A] ROR accumulator
    public void Acc2(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            RorUpdateFlags(ctx, ctx.Regs.A);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x66] ROR zeropage
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstRORzpg3);
    }
    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstRORzpg4);
    }
    public void Zpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            RorUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstRORzpg5);
    }
    public void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x76] ROR zeropage,X 
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstRORzpgx3);
    }
    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstRORzpgx4);
    }
    public void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstRORzpgx5);
    }
    public void Zpgx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            RorUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstRORzpgx6);
    }
    public void Zpgx6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x6E] ROR absolute 
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstRORabs3);
    }
    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstRORabs4);
    }
    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstRORabs5);
    }
    public void Abs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            RorUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstRORabs6);
    }
    public void Abs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x7E] ROR absolute,X
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstRORabsx3);
    }
    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstRORabsx4);
    }
    public void Absx4(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            ctx.Regs.IncEAWithX();
        ctx.AdvanceState(States.InstRORabsx5);
    }
    public void Absx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstRORabsx6);
    }
    public void Absx6(Context ctx)
    {
        if (ctx.GetSubStep() == P2Lastsubstep)
            RorUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstRORabsx7);
    }
    public void Absx7(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
