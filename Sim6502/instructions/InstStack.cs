namespace Sim6502.Instructions;

public class InstStack : InstBase
{
    public InstStack(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.PHAimp, States.InstPHAimp2);
        registry.Map(OpCodes.PHPimp, States.InstPHPimp2);
        registry.Map(OpCodes.PHXimp, States.InstPHXimp2);
        registry.Map(OpCodes.PHYimp, States.InstPHYimp2);
        registry.Map(OpCodes.PLAimp, States.InstPLAimp2);
        registry.Map(OpCodes.PLPimp, States.InstPLPimp2);
        registry.Map(OpCodes.PLXimp, States.InstPLXimp2);
        registry.Map(OpCodes.PLYimp, States.InstPLYimp2);
    }

    protected override void RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstPHAimp2, ctx => InstPHAimp2(ctx));
        registry.Map(States.InstPHAimp3, ctx => InstPHAimp3(ctx));

        registry.Map(States.InstPHPimp2, ctx => InstPHPimp2(ctx));
        registry.Map(States.InstPHPimp3, ctx => InstPHPimp3(ctx));

        registry.Map(States.InstPHXimp2, ctx => InstPHXimp2(ctx));
        registry.Map(States.InstPHXimp3, ctx => InstPHXimp3(ctx));

        registry.Map(States.InstPHYimp2, ctx => InstPHYimp2(ctx));
        registry.Map(States.InstPHYimp3, ctx => InstPHYimp3(ctx));

        registry.Map(States.InstPLAimp2, ctx => InstPLAimp2(ctx));
        registry.Map(States.InstPLAimp3, ctx => InstPLAimp3(ctx));
        registry.Map(States.InstPLAimp4, ctx => InstPLAimp4(ctx));

        registry.Map(States.InstPLPimp2, ctx => InstPLPimp2(ctx));
        registry.Map(States.InstPLPimp3, ctx => InstPLPimp3(ctx));
        registry.Map(States.InstPLPimp4, ctx => InstPLPimp4(ctx));

        registry.Map(States.InstPLXimp2, ctx => InstPLXimp2(ctx));
        registry.Map(States.InstPLXimp3, ctx => InstPLXimp3(ctx));
        registry.Map(States.InstPLXimp4, ctx => InstPLXimp4(ctx));

        registry.Map(States.InstPLYimp2, ctx => InstPLYimp2(ctx));
        registry.Map(States.InstPLYimp3, ctx => InstPLYimp3(ctx));
        registry.Map(States.InstPLYimp4, ctx => InstPLYimp4(ctx));
    }

    // PHA - Push Accumulator On Stack
    public void InstPHAimp2(Context ctx)
    {
        ReadAndDiscard(ctx);
        ctx.AdvanceState(States.InstPHAimp3);
    }

    public void InstPHAimp3(Context ctx)
    {
        PrepareStackWrite(ctx);
        if (ctx.GetSubStep() == Constants.P2MIDDLESTEP)
        {
            var value = ctx.Regs.A;
            ctx.Pins.SetDataBusPins(value);
        }
        else if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            ctx.Regs.S.Dec();
        }
        ctx.AdvanceState(States.Fetch);
    }

    // PHP - Push Processor Status On Stack
    public void InstPHPimp2(Context ctx)
    {
        ReadAndDiscard(ctx);
        ctx.AdvanceState(States.InstPHPimp3);
    }

    public void InstPHPimp3(Context ctx)
    {
        PrepareStackWrite(ctx);
        if (ctx.GetSubStep() == Constants.P2MIDDLESTEP)
        {
            var data = ctx.Regs.P.ToUInt8();
            ctx.Pins.SetDataBusPins(data);
        }
        else if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            ctx.Regs.S.Dec();
        }
        ctx.AdvanceState(States.Fetch);
    }

    // PHX - Push Index Register X On Stack
    public void InstPHXimp2(Context ctx)
    {
        ReadAndDiscard(ctx);
        ctx.AdvanceState(States.InstPHXimp3);
    }

    public void InstPHXimp3(Context ctx)
    {
        PrepareStackWrite(ctx);
        if (ctx.GetSubStep() == Constants.P2MIDDLESTEP)
        {
            var data = ctx.Regs.X;
            ctx.Pins.SetDataBusPins(data);
        }
        else if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            ctx.Regs.S.Dec();
        }
        ctx.AdvanceState(States.Fetch);
    }

    // PHY - Push Index Register Y On Stack
    public void InstPHYimp2(Context ctx)
    {
        ReadAndDiscard(ctx);
        ctx.AdvanceState(States.InstPHYimp3);
    }

    public void InstPHYimp3(Context ctx)
    {
        PrepareStackWrite(ctx);
        if (ctx.GetSubStep() == Constants.P2MIDDLESTEP)
        {
            var data = ctx.Regs.Y;
            ctx.Pins.SetDataBusPins(data);
        }
        else if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            ctx.Regs.S.Dec();
        }
        ctx.AdvanceState(States.Fetch);
    }

    // PLA - Pull Accumulator From Stack
    public void InstPLAimp2(Context ctx)
    {
        ReadAndDiscard(ctx);
        ctx.AdvanceState(States.InstPLAimp3);
    }

    public void InstPLAimp3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            ctx.Regs.S.Inc();
        }
        ctx.AdvanceState(States.InstPLAimp4);
    }

    public void InstPLAimp4(Context ctx)
    {
        PrepareStackRead(ctx);
        if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.UpdateAUpdateFlags(data);
        }
        ctx.AdvanceState(States.Fetch);
    }

    // PLP - Pull Processor Status From Stack
    public void InstPLPimp2(Context ctx)
    {
        ReadAndDiscard(ctx);
        ctx.AdvanceState(States.InstPLPimp3);
    }

    public void InstPLPimp3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            ctx.Regs.S.Inc();
        }
        ctx.AdvanceState(States.InstPLPimp4);
    }

    public void InstPLPimp4(Context ctx)
    {
        PrepareStackRead(ctx);
        if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.P.SetFlags(data);
        }
        ctx.AdvanceState(States.Fetch);
    }

    // PLX - Pull Index Register X From Stack
    public void InstPLXimp2(Context ctx)
    {
        ReadAndDiscard(ctx);
        ctx.AdvanceState(States.InstPLXimp3);
    }

    public void InstPLXimp3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            ctx.Regs.S.Inc();
        }
        ctx.AdvanceState(States.InstPLXimp4);
    }

    public void InstPLXimp4(Context ctx)
    {
        PrepareStackRead(ctx);
        if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.SetXUpdateFlags(data);
        }
        ctx.AdvanceState(States.Fetch);
    }

    // PLY - Pull Index Register Y From Stack
    public void InstPLYimp2(Context ctx)
    {
        ReadAndDiscard(ctx);
        ctx.AdvanceState(States.InstPLYimp3);
    }

    public void InstPLYimp3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            ctx.Regs.S.Inc();
        }
        ctx.AdvanceState(States.InstPLYimp4);
    }

    public void InstPLYimp4(Context ctx)
    {
        PrepareStackRead(ctx);
        if (ctx.GetSubStep() == Constants.P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.SetYUpdateFlags(data);
        }
        ctx.AdvanceState(States.Fetch);
    }
}