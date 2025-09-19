namespace Sim6502.Instructions;

public class InstFlags : InstBase2, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.CLCimp, States.InstCLCimp2);
        registry.Map(OpCodes.CLDimp, States.InstCLDimp2);
        registry.Map(OpCodes.CLIimp, States.InstCLIimp2);
        registry.Map(OpCodes.CLVimp, States.InstCLVimp2);
        registry.Map(OpCodes.SECimp, States.InstSECimp2);
        registry.Map(OpCodes.SEDimp, States.InstSEDimp2);
        registry.Map(OpCodes.SEIimp, States.InstSEIimp2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstCLCimp2, ClcImp2);
        stateRegistry.Map(States.InstCLDimp2, CldImp2);
        stateRegistry.Map(States.InstCLIimp2, CliImp2);
        stateRegistry.Map(States.InstCLVimp2, ClvImp2);
        stateRegistry.Map(States.InstSECimp2, SecImp2);
        stateRegistry.Map(States.InstSEDimp2, SedImp2);
        stateRegistry.Map(States.InstSEIimp2, SeiImp2);

        return this;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // CLC - Clear Carry Flag
    // 0 -> C
    // N V B D I Z C
    // - - - - - - 0
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        CLC           18      1      2
    ///////////////////////////////////////////////////////////////////////////////
    private void ClcImp2(Context ctx)
    {
        ctx.Regs.P.ClearCarry();
        ctx.AdvanceState(States.Fetch);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // CLD - Clear Decimal Mode
    // 0 -> D
    // N V B D I Z C
    // - - - 0 - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        CLD           D8      1      2
    ///////////////////////////////////////////////////////////////////////////////   
    private void CldImp2(Context ctx)
    {
        ctx.Regs.P.ClearDecimal();
        ctx.AdvanceState(States.Fetch);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // CLI - Clear Interrupt Disable
    // 0 -> 
    // N V B D I Z C
    // - - - - 0 - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        CLI           58      1      2
    ///////////////////////////////////////////////////////////////////////////////
    private void CliImp2(Context ctx)
    {
        ctx.Regs.P.ClearIRQDisabled();
        ctx.AdvanceState(States.Fetch);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // CLV - Clear Overflow Flag
    // 0 -> V
    // N V B D I Z C
    // - 0 - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        CLV           B8      1      2
    ///////////////////////////////////////////////////////////////////////////////
    private void ClvImp2(Context ctx)
    {
        ctx.Regs.P.ClearOverflow();
        ctx.AdvanceState(States.Fetch);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // SEC - Set Carry Flag
    // 1 -> 
    // N V B D I Z C
    // - - - - - - 1
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        SEC           38      1      2
    ///////////////////////////////////////////////////////////////////////////////
    private void SecImp2(Context ctx)
    {
        ctx.Regs.P.SetCarry();
        ctx.AdvanceState(States.Fetch);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // SED - Set Decimal Mode
    // 1 -> D
    // N V B D I Z C
    // - - - 1 - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        SED           F8      1      2
    ///////////////////////////////////////////////////////////////////////////////
    private void SedImp2(Context ctx)
    {
        ctx.Regs.P.SetDecimal();
        ctx.AdvanceState(States.Fetch);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // SEI - Set Interrupt Disable
    // 1 -> I
    // N V B D I Z C
    // - - - - 1 - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        SEI           78      1      2
    ///////////////////////////////////////////////////////////////////////////////
    private void SeiImp2(Context ctx)
    {
        ctx.Regs.P.SetIRQDisabled();
        ctx.AdvanceState(States.Fetch);
    }
}
