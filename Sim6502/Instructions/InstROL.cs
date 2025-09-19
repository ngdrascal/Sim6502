using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Instructions;

public class InstROL : InstBase
{
    public InstROL(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.ROLacc, States.InstROLacc2);
        registry.Map(OpCodes.ROLzpg, States.InstROLzpg2);
        registry.Map(OpCodes.ROLzpgx, States.InstROLzpgx2);
        registry.Map(OpCodes.ROLabs, States.InstROLabs2);
        registry.Map(OpCodes.ROLabsx, States.InstROLabsx2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstROLacc2, ctx => Acc2(ctx));
        stateRegistry.Map(States.InstROLzpg2, ctx => Zpg2(ctx));
        stateRegistry.Map(States.InstROLzpg3, ctx => Zpg3(ctx));
        stateRegistry.Map(States.InstROLzpg4, ctx => Zpg4(ctx));
        stateRegistry.Map(States.InstROLzpg5, ctx => Zpg5(ctx));
        stateRegistry.Map(States.InstROLzpgx2, ctx => Zpgx2(ctx));
        stateRegistry.Map(States.InstROLzpgx3, ctx => Zpgx3(ctx));
        stateRegistry.Map(States.InstROLzpgx4, ctx => Zpgx4(ctx));
        stateRegistry.Map(States.InstROLzpgx5, ctx => Zpgx5(ctx));
        stateRegistry.Map(States.InstROLzpgx6, ctx => Zpgx6(ctx));
        stateRegistry.Map(States.InstROLabs2, ctx => Abs2(ctx));
        stateRegistry.Map(States.InstROLabs3, ctx => Abs3(ctx));
        stateRegistry.Map(States.InstROLabs4, ctx => Abs4(ctx));
        stateRegistry.Map(States.InstROLabs5, ctx => Abs5(ctx));
        stateRegistry.Map(States.InstROLabs6, ctx => Abs6(ctx));
        stateRegistry.Map(States.InstROLabsx2, ctx => Absx2(ctx));
        stateRegistry.Map(States.InstROLabsx3, ctx => Absx3(ctx));
        stateRegistry.Map(States.InstROLabsx4, ctx => Absx4(ctx));
        stateRegistry.Map(States.InstROLabsx5, ctx => Absx5(ctx));
        stateRegistry.Map(States.InstROLabsx6, ctx => Absx6(ctx));
        stateRegistry.Map(States.InstROLabsx7, ctx => Absx7(ctx));
    }

    private void RolUpdateFlags(Context ctx, UInt8 value)
    {
        var oldCarry = ctx.Regs.P.Carry.Copy();
        ctx.Regs.P.Carry.UpdateValue(value.IsBitSet(7));
        value.Shl(oldCarry);
        ctx.Regs.P.Zero.UpdateValue(value.EqualsZero());
        ctx.Regs.P.Negative.UpdateValue(value.IsBitSet(7));
    }

    ///////////////////////////////////////////////////////////////////////////////
    // ROL - Rotate Left
    // C <- [76543210] <- C
    // N V B D I Z C
    // + - - - - + +
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // accumulator    ROL A         2A      1      2  
    // zeropage       ROL oper      26      2      5  
    // zeropage,X     ROL oper,X    36      2      6  
    // absolute       ROL oper      2E      3      6  
    // absolute,X     ROL oper,X    3E      3      7
    /////////////////////////////////////////////////////////////////////////////// 

    // [0x2A] ROL accumulator
    public void Acc2(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            RolUpdateFlags(ctx, ctx.Regs.A);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x26] ROL zeropage
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstROLzpg3);
    }
    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstROLzpg4);
    }
    public void Zpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            RolUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstROLzpg5);
    }
    public void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x36] ROL zeropage,X 
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstROLzpgx3);
    }
    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstROLzpgx4);
    }
    public void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstROLzpgx5);
    }
    public void Zpgx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            RolUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstROLzpgx6);
    }
    public void Zpgx6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x2E] ROL absolute 
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstROLabs3);
    }
    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstROLabs4);
    }
    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstROLabs5);
    }
    public void Abs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            RolUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstROLabs6);
    }
    public void Abs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x3E] ROL absolute,X
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstROLabsx3);
    }
    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstROLabsx4);
    }
    public void Absx4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEAWithX();
        ctx.AdvanceState(States.InstROLabsx5);
    }
    public void Absx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstROLabsx6);
    }
    public void Absx6(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            RolUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstROLabsx7);
    }
    public void Absx7(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
