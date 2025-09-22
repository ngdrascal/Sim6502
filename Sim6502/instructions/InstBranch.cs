using Sim6502.types;

namespace Sim6502.Instructions;

public class InstBranch : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.BRArel, States.InstBRArel2);
        registry.Map(OpCodes.BCCrel, States.InstBCCrel2);
        registry.Map(OpCodes.BCSrel, States.InstBCSrel2);
        registry.Map(OpCodes.BEQrel, States.InstBEQrel2);
        registry.Map(OpCodes.BMIrel, States.InstBMIrel2);
        registry.Map(OpCodes.BNErel, States.InstBNErel2);
        registry.Map(OpCodes.BPLrel, States.InstBPLrel2);
        registry.Map(OpCodes.BVCrel, States.InstBVCrel2);
        registry.Map(OpCodes.BVSrel, States.InstBVSrel2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstBRArel2, BraRel2);
        stateRegistry.Map(States.InstBRArel3, BraRel3);
        stateRegistry.Map(States.InstBRArel4, BraRel4);

        stateRegistry.Map(States.InstBCCrel2, BccRel2);
        stateRegistry.Map(States.InstBCCrel3, BccRel3);
        stateRegistry.Map(States.InstBCCrel4, BccRel4);

        stateRegistry.Map(States.InstBCSrel2, BcsRel2);
        stateRegistry.Map(States.InstBCSrel3, BcsRel3);
        stateRegistry.Map(States.InstBCSrel4, BcsRel4);

        stateRegistry.Map(States.InstBEQrel2, BeqRel2);
        stateRegistry.Map(States.InstBEQrel3, BeqRel3);
        stateRegistry.Map(States.InstBEQrel4, BeqRel4);

        stateRegistry.Map(States.InstBMIrel2, BmiRel2);
        stateRegistry.Map(States.InstBMIrel3, BmiRel3);
        stateRegistry.Map(States.InstBMIrel4, BmiRel4);

        stateRegistry.Map(States.InstBNErel2, BneRel2);
        stateRegistry.Map(States.InstBNErel3, BneRel3);
        stateRegistry.Map(States.InstBNErel4, BneRel4);

        stateRegistry.Map(States.InstBPLrel2, BplRel2);
        stateRegistry.Map(States.InstBPLrel3, BplRel3);
        stateRegistry.Map(States.InstBPLrel4, BplRel4);

        stateRegistry.Map(States.InstBVCrel2, BvcRel2);
        stateRegistry.Map(States.InstBVCrel3, BvcRel3);
        stateRegistry.Map(States.InstBVCrel4, BvcRel4);

        stateRegistry.Map(States.InstBVSrel2, BvsRel2);
        stateRegistry.Map(States.InstBVSrel3, BvsRel3);
        stateRegistry.Map(States.InstBVSrel4, BvsRel4);

        return this;
    }

    private void BranchCleared2(Context ctx, BitFlag flag, States nextState)
    {
        LoadTempFromPC(ctx);
        if (flag.IsCleared())
            ctx.AdvanceState(nextState);
        else
            ctx.AdvanceState(States.Fetch);
    }

    private void BranchSet2(Context ctx, BitFlag flag, States nextState)
    {
        LoadTempFromPC(ctx);
        if (flag.IsSet())
            ctx.AdvanceState(nextState);
        else
            ctx.AdvanceState(States.Fetch);
    }

    private void Branch3(Context ctx, States nextState)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var regs = ctx.Regs;
            var beforePage = regs.PC.Msb().Copy();
            regs.PC.AddSigned(regs.Temp);
            var afterPage = regs.PC.Msb().Copy();
            if (afterPage.Equals(beforePage))
            {
                ctx.Pins.SetAddrBusPins(ctx.Regs.PC);
                ctx.AdvanceState(States.Fetch);
            }
            else
            {
                ctx.AdvanceState(nextState);
            }
        }
    }

    private void Branch4(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Pins.SetAddrBusPins(ctx.Regs.PC);

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // BRA - Branch on Carry Clear
    // Branch on C = 0
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // relative       BRA oper      80      2    3+p
    //
    // Notes: p: =1 if page is crossed.
    /////////////////////////////////////////////////////////////////////////////
    private void BraRel2(Context ctx)
    {
        LoadTempFromPC(ctx);

        ctx.AdvanceState(States.InstBRArel3);
    }

    private void BraRel3(Context ctx)
    {
        Branch3(ctx, States.InstBRArel4);
    }

    private void BraRel4(Context ctx)
    {
        Branch4(ctx);
    }

    /////////////////////////////////////////////////////////////////////////////
    // BCC - Branch on Carry Clear
    // Branch on C = 0
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // relative       BCC oper      90      2    2+t+p
    //
    // Notes: p: =1 if page is crossed. t: =1 if branch is taken.
    /////////////////////////////////////////////////////////////////////////////
    private void BccRel2(Context ctx)
    {
        BranchCleared2(ctx, ctx.Regs.P.Carry, States.InstBCCrel3);
    }

    private void BccRel3(Context ctx)
    {
        Branch3(ctx, States.InstBCCrel4);
    }

    private void BccRel4(Context ctx)
    {
        Branch4(ctx);
    }

    /////////////////////////////////////////////////////////////////////////////
    // BCS - Branch on Carry Set
    // Branch on C = 1
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // relative       BCS oper      B0      2    2+t+p
    /////////////////////////////////////////////////////////////////////////////
    private void BcsRel2(Context ctx)
    {
        BranchSet2(ctx, ctx.Regs.P.Carry, States.InstBCSrel3);
    }

    private void BcsRel3(Context ctx)
    {
        Branch3(ctx, States.InstBCSrel4);
    }

    private void BcsRel4(Context ctx)
    {
        Branch4(ctx);
    }

    /////////////////////////////////////////////////////////////////////////////
    // BEQ - Branch on Result Zero
    // Branch on Z = 1
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // relative       BEQ oper      F0      2    2+t+p
    /////////////////////////////////////////////////////////////////////////////
    private void BeqRel2(Context ctx)
    {
        BranchSet2(ctx, ctx.Regs.P.Zero, States.InstBEQrel3);
    }

    private void BeqRel3(Context ctx)
    {
        Branch3(ctx, States.InstBEQrel4);
    }

    private void BeqRel4(Context ctx)
    {
        Branch4(ctx);
    }

    /////////////////////////////////////////////////////////////////////////////
    // BMI - Branch on Result Minus
    // Branch on N = 1
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // relative       BMI oper      30      2    2+t+p
    /////////////////////////////////////////////////////////////////////////////
    private void BmiRel2(Context ctx)
    {
        BranchSet2(ctx, ctx.Regs.P.Negative, States.InstBMIrel3);
    }

    private void BmiRel3(Context ctx)
    {
        Branch3(ctx, States.InstBMIrel4);
    }

    private void BmiRel4(Context ctx)
    {
        Branch4(ctx);
    }

    /////////////////////////////////////////////////////////////////////////////
    // BNE - Branch on Result Not Zero
    // Branch on Z = 0
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // relative       BNE oper      D0      2    2+t+p
    /////////////////////////////////////////////////////////////////////////////
    private void BneRel2(Context ctx)
    {
        BranchCleared2(ctx, ctx.Regs.P.Zero, States.InstBNErel3);
    }

    private void BneRel3(Context ctx)
    {
        Branch3(ctx, States.InstBNErel4);
    }

    private void BneRel4(Context ctx)
    {
        Branch4(ctx);
    }

    /////////////////////////////////////////////////////////////////////////////
    // BPL - Branch on Result Plus
    // Branch on N = 0
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // relative       BPL oper      10      2    2+t+p
    /////////////////////////////////////////////////////////////////////////////
    private void BplRel2(Context ctx)
    {
        BranchCleared2(ctx, ctx.Regs.P.Negative, States.InstBPLrel3);
    }

    private void BplRel3(Context ctx)
    {
        Branch3(ctx, States.InstBPLrel4);
    }

    private void BplRel4(Context ctx)
    {
        Branch4(ctx);
    }

    /////////////////////////////////////////////////////////////////////////////
    // BVC - Branch on Overflow Clear
    // Branch on V = 0
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // relative       BVC oper      50      2    2+t+p
    /////////////////////////////////////////////////////////////////////////////
    private void BvcRel2(Context ctx)
    {
        BranchCleared2(ctx, ctx.Regs.P.Overflow, States.InstBVCrel3);
    }

    private void BvcRel3(Context ctx)
    {
        Branch3(ctx, States.InstBVCrel4);
    }

    private void BvcRel4(Context ctx)
    {
        Branch4(ctx);
    }

    /////////////////////////////////////////////////////////////////////////////
    // BVS - Branch on Overflow Set
    // Branch on V = 1
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // relative       BVS oper      70      2    2+t+p
    /////////////////////////////////////////////////////////////////////////////
    private void BvsRel2(Context ctx)
    {
        BranchSet2(ctx, ctx.Regs.P.Overflow, States.InstBVSrel3);
    }

    private void BvsRel3(Context ctx)
    {
        Branch3(ctx, States.InstBVSrel4);
    }

    private void BvsRel4(Context ctx)
    {
        Branch4(ctx);
    }
}