namespace Sim6502.Instructions;

public class InstStack : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.PHAimp, States.InstPHAimp2);
        registry.Map(OpCodes.PHPimp, States.InstPHPimp2);
        registry.Map(OpCodes.PHXimp, States.InstPHXimp2);
        registry.Map(OpCodes.PHYimp, States.InstPHYimp2);
        registry.Map(OpCodes.PLAimp, States.InstPLAimp2);
        registry.Map(OpCodes.PLPimp, States.InstPLPimp2);
        registry.Map(OpCodes.PLXimp, States.InstPLXimp2);
        registry.Map(OpCodes.PLYimp, States.InstPLYimp2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstPHAimp2, InstPHAimp2);
        registry.Map(States.InstPHAimp3, InstPHAimp3);

        registry.Map(States.InstPHPimp2, InstPHPimp2);
        registry.Map(States.InstPHPimp3, InstPHPimp3);

        registry.Map(States.InstPHXimp2, InstPHXimp2);
        registry.Map(States.InstPHXimp3, InstPHXimp3);

        registry.Map(States.InstPHYimp2, InstPHYimp2);
        registry.Map(States.InstPHYimp3, InstPHYimp3);

        registry.Map(States.InstPLAimp2, InstPLAimp2);
        registry.Map(States.InstPLAimp3, InstPLAimp3);
        registry.Map(States.InstPLAimp4, InstPLAimp4);

        registry.Map(States.InstPLPimp2, InstPLPimp2);
        registry.Map(States.InstPLPimp3, InstPLPimp3);
        registry.Map(States.InstPLPimp4, InstPLPimp4);

        registry.Map(States.InstPLXimp2, InstPLXimp2);
        registry.Map(States.InstPLXimp3, InstPLXimp3);
        registry.Map(States.InstPLXimp4, InstPLXimp4);

        registry.Map(States.InstPLYimp2, InstPLYimp2);
        registry.Map(States.InstPLYimp3, InstPLYimp3);
        registry.Map(States.InstPLYimp4, InstPLYimp4);

        return this;
    }

    // PHA - Push Accumulator On Stack
    private void InstPHAimp2(Context ctx)
    {
        ReadAndDiscard(ctx);
        ctx.AdvanceState(States.InstPHAimp3);
    }

    private void InstPHAimp3(Context ctx)
    {
        PrepareStackWrite(ctx);
        if (ctx.GetSubStep() == Constants.P2MiddleStep)
        {
            var value = ctx.Regs.A;
            ctx.Pins.SetDataBusPins(value);
        }
        else if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.S.Dec();
        }

        ctx.AdvanceState(States.Fetch);
    }

    // PHP - Push Processor Status On Stack
    private void InstPHPimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPHPimp3);
    }

    private void InstPHPimp3(Context ctx)
    {
        PrepareStackWrite(ctx);
        if (ctx.GetSubStep() == Constants.P2MiddleStep)
        {
            var data = ctx.Regs.P.ToUInt8();
            ctx.Pins.SetDataBusPins(data);
        }
        else if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.S.Dec();
        }

        ctx.AdvanceState(States.Fetch);
    }

    // PHX - Push Index Register X On Stack
    private void InstPHXimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPHXimp3);
    }

    private void InstPHXimp3(Context ctx)
    {
        PrepareStackWrite(ctx);
        if (ctx.GetSubStep() == Constants.P2MiddleStep)
        {
            var data = ctx.Regs.X;
            ctx.Pins.SetDataBusPins(data);
        }
        else if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.S.Dec();
        }

        ctx.AdvanceState(States.Fetch);
    }

    // PHY - Push Index Register Y On Stack
    private void InstPHYimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPHYimp3);
    }

    private void InstPHYimp3(Context ctx)
    {
        PrepareStackWrite(ctx);
        if (ctx.GetSubStep() == Constants.P2MiddleStep)
        {
            var data = ctx.Regs.Y;
            ctx.Pins.SetDataBusPins(data);
        }
        else if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.S.Dec();
        }

        ctx.AdvanceState(States.Fetch);
    }

    // PLA - Pull Accumulator From Stack
    private void InstPLAimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPLAimp3);
    }

    private void InstPLAimp3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.S.Inc();
        }

        ctx.AdvanceState(States.InstPLAimp4);
    }

    private void InstPLAimp4(Context ctx)
    {
        PrepareStackRead(ctx);
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.UpdateAUpdateFlags(data);
        }

        ctx.AdvanceState(States.Fetch);
    }

    // PLP - Pull Processor Status From Stack
    private void InstPLPimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPLPimp3);
    }

    private void InstPLPimp3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.S.Inc();
        }
        ctx.AdvanceState(States.InstPLPimp4);
    }

    private void InstPLPimp4(Context ctx)
    {
        PrepareStackRead(ctx);
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.P.SetFlags(data);
        }

        ctx.AdvanceState(States.Fetch);
    }

    // PLX - Pull Index Register X From Stack
    private void InstPLXimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPLXimp3);
    }

    private void InstPLXimp3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.S.Inc();
        }

        ctx.AdvanceState(States.InstPLXimp4);
    }

    private void InstPLXimp4(Context ctx)
    {
        PrepareStackRead(ctx);
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.SetXUpdateFlags(data);
        }

        ctx.AdvanceState(States.Fetch);
    }

    // PLY - Pull Index Register Y From Stack
    private void InstPLYimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPLYimp3);
    }

    private void InstPLYimp3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.S.Inc();
        }

        ctx.AdvanceState(States.InstPLYimp4);
    }

    private void InstPLYimp4(Context ctx)
    {
        PrepareStackRead(ctx);
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.SetYUpdateFlags(data);
        }

        ctx.AdvanceState(States.Fetch);
    }
}
