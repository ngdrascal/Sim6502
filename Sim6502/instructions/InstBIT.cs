namespace Sim6502.Instructions;

public class InstBIT : InstBase
{
    public InstBIT(IT2Registry it2Registry, IStateRegistry stateRegistry) : base(it2Registry, stateRegistry) { }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.BITimm, States.InstBITimm2);
        registry.Map(OpCodes.BITzpg, States.InstBITzpg2);
        registry.Map(OpCodes.BITzpgx, States.InstBITzpgx2);
        registry.Map(OpCodes.BITabs, States.InstBITabs2);
        registry.Map(OpCodes.BITabsx, States.InstBITabsx2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstBITimm2, ctx => Imm2(ctx));

        stateRegistry.Map(States.InstBITzpg2, ctx => Zpg2(ctx));
        stateRegistry.Map(States.InstBITzpg3, ctx => Zpg3(ctx));

        stateRegistry.Map(States.InstBITzpgx2, ctx => Zpgx2(ctx));
        stateRegistry.Map(States.InstBITzpgx3, ctx => Zpgx3(ctx));
        stateRegistry.Map(States.InstBITzpgx4, ctx => Zpgx4(ctx));

        stateRegistry.Map(States.InstBITabs2, ctx => Abs2(ctx));
        stateRegistry.Map(States.InstBITabs3, ctx => Abs3(ctx));
        stateRegistry.Map(States.InstBITabs4, ctx => Abs4(ctx));

        stateRegistry.Map(States.InstBITabsx2, ctx => Absx2(ctx));
        stateRegistry.Map(States.InstBITabsx3, ctx => Absx3(ctx));
        stateRegistry.Map(States.InstBITabsx4, ctx => Absx4(ctx));
        stateRegistry.Map(States.InstBITabsx5, ctx => Absx5(ctx));
    }

    protected void AndAWithTempSetFlags(Context ctx)
    {
        var p = ctx.Regs.P;
        var memValue = ctx.Regs.Temp;
        if (memValue.IsBitSet(7))
            p.SetNegative();
        else
            p.ClearNegative();
        if (memValue.IsBitSet(6))
            p.SetOverflow();
        else
            p.ClearOverflow();
        var andResult = ctx.Regs.A.Copy().And(memValue);
        if (andResult.EqualsZero())
            p.SetZero();
        else
            p.ClearZero();
    }

    // [0x89] BIT immediate
    public void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            ctx.DbgOperand1 = data;
            // NOTE: in the imm addressing mode only the Z flag is effected.  Unlike the other
            // addressing modes the V and N flags are unaffected.
            var p = ctx.Regs.P;
            var memValue = ctx.Regs.Temp;
            var andResult = ctx.Regs.A.Copy().And(memValue);
            if (andResult.EqualsZero())
                p.SetZero();
            else
                p.ClearZero();
        }
        ctx.AdvanceState(States.Fetch);
    }

    // [0x24] BIT zeropage
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstBITzpg3);
    }

    public void Zpg3(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            AndAWithTempSetFlags(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x34] BIT zeropage,X
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstBITzpgx3);
    }

    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            ctx.Regs.IncEALWithX();
        ctx.AdvanceState(States.InstBITzpgx4);
    }

    public void Zpgx4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            AndAWithTempSetFlags(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x2C] BIT absolute
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstBITabs3);
    }

    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstBITabs4);
    }

    public void Abs4(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            AndAWithTempSetFlags(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // [0x3C] BIT absolute,X
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstBITabsx3);
    }

    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstBITabsx4);
    }

    public void Absx4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            ctx.Regs.IncEAWithX();
        ctx.AdvanceState(States.InstBITabsx5);
    }

    public void Absx5(Context ctx)
    {
        LoadTempFromEffAddr(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            AndAWithTempSetFlags(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}