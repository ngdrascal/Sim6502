using Sim6502.types;

namespace Sim6502.Instructions;

// ASL - Arithmetic Shift Left
// C <- [76543210] <- 0
// N V B D I Z C
// + - - - - + +
//
// addressing     assembler     opc   bytes  cycles
// ------------------------------------------------
// accumulator    ASL A         0A      1      2
// zeropage       ASL oper      06      2      5
// zeropage,X     ASL oper,X    16      2      6
// absolute       ASL oper      0E      3      6
// absolute,X     ASL oper,X    1E      3      7
public class InstASL : InstBase
{
    public InstASL(IT2Registry it2Registry, IStateRegistry stateRegistry) : base(it2Registry, stateRegistry) { }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.ASLacc, States.InstASLacc2);
        registry.Map(OpCodes.ASLzpg, States.InstASLzpg2);
        registry.Map(OpCodes.ASLzpgx, States.InstASLzpgx2);
        registry.Map(OpCodes.ASLabs, States.InstASLabs2);
        registry.Map(OpCodes.ASLabsx, States.InstASLabsx2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstASLacc2, ctx => Acc2(ctx));

        stateRegistry.Map(States.InstASLzpg2, ctx => Zpg2(ctx));
        stateRegistry.Map(States.InstASLzpg3, ctx => Zpg3(ctx));
        stateRegistry.Map(States.InstASLzpg4, ctx => Zpg4(ctx));
        stateRegistry.Map(States.InstASLzpg5, ctx => Zpg5(ctx));

        stateRegistry.Map(States.InstASLzpgx2, ctx => Zpgx2(ctx));
        stateRegistry.Map(States.InstASLzpgx3, ctx => Zpgx3(ctx));
        stateRegistry.Map(States.InstASLzpgx4, ctx => Zpgx4(ctx));
        stateRegistry.Map(States.InstASLzpgx5, ctx => Zpgx5(ctx));
        stateRegistry.Map(States.InstASLzpgx6, ctx => Zpgx6(ctx));

        stateRegistry.Map(States.InstASLabs2, ctx => Abs2(ctx));
        stateRegistry.Map(States.InstASLabs3, ctx => Abs3(ctx));
        stateRegistry.Map(States.InstASLabs4, ctx => Abs4(ctx));
        stateRegistry.Map(States.InstASLabs5, ctx => Abs5(ctx));
        stateRegistry.Map(States.InstASLabs6, ctx => Abs6(ctx));

        stateRegistry.Map(States.InstASLabsx2, ctx => Absx2(ctx));
        stateRegistry.Map(States.InstASLabsx3, ctx => Absx3(ctx));
        stateRegistry.Map(States.InstASLabsx4, ctx => Absx4(ctx));
        stateRegistry.Map(States.InstASLabsx5, ctx => Absx5(ctx));
        stateRegistry.Map(States.InstASLabsx6, ctx => Absx6(ctx));
        stateRegistry.Map(States.InstASLabsx7, ctx => Absx7(ctx));
    }

    private void AslUpdateFlags(Context ctx, UInt8 value)
    {
        ctx.Regs.P.Carry.UpdateValue(value.IsBitSet(7));
        ctx.Regs.P.Negative.UpdateValue(value.IsBitSet(6));
        value.Shl();
        ctx.Regs.P.Zero.UpdateValue(value.EqualsZero());
    }

    // [0xA0] ASL accumulator
    public void Acc2(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            AslUpdateFlags(ctx, ctx.Regs.A);
        ctx.AdvanceState(States.Fetch);
    }

    // [0xA6] ASL zeropage
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstASLzpg3);
    }

    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstASLzpg4);
    }

    public void Zpg4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            AslUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstASLzpg5);
    }

    public void Zpg5(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x16] ASL zeropage,X
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstASLzpgx3);
    }

    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstASLzpgx4);
    }

    public void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstASLzpgx5);
    }

    public void Zpgx5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            AslUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstASLzpgx6);
    }

    public void Zpgx6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x0E] ASL absolute
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstASLabs3);
    }

    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstASLabs4);
    }

    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstASLabs5);
    }

    public void Abs5(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            AslUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstASLabs6);
    }

    public void Abs6(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x1E] ASL absolute,X
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstASLabsx3);
    }

    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstASLabsx4);
    }

    public void Absx4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            ctx.Regs.IncEAWithX();
        ctx.AdvanceState(States.InstASLabsx5);
    }

    public void Absx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        ctx.AdvanceState(States.InstASLabsx6);
    }

    public void Absx6(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            AslUpdateFlags(ctx, ctx.Regs.Temp);
        ctx.AdvanceState(States.InstASLabsx7);
    }

    public void Absx7(Context ctx)
    {
        StoreTempToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}