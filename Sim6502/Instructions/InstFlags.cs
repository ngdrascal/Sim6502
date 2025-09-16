namespace Sim6502.Instructions;

public class InstFlags : InstBase
{
    public InstFlags(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.CLCimp, States.InstCLCimp2);
        registry.Map(OpCodes.CLDimp, States.InstCLDimp2);
        registry.Map(OpCodes.CLIimp, States.InstCLIimp2);
        registry.Map(OpCodes.CLVimp, States.InstCLVimp2);
        registry.Map(OpCodes.SECimp, States.InstSECimp2);
        registry.Map(OpCodes.SEDimp, States.InstSEDimp2);
        registry.Map(OpCodes.SEIimp, States.InstSEIimp2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstCLCimp2, ctx => ClcImp2(ctx));
        stateRegistry.Map(States.InstCLDimp2, ctx => CldImp2(ctx));
        stateRegistry.Map(States.InstCLIimp2, ctx => CliImp2(ctx));
        stateRegistry.Map(States.InstCLVimp2, ctx => ClvImp2(ctx));
        stateRegistry.Map(States.InstSECimp2, ctx => SecImp2(ctx));
        stateRegistry.Map(States.InstSEDimp2, ctx => SedImp2(ctx));
        stateRegistry.Map(States.InstSEIimp2, ctx => SeiImp2(ctx));
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
    public void ClcImp2(Context ctx)
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
    public void CldImp2(Context ctx)
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
    public void CliImp2(Context ctx)
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
    public void ClvImp2(Context ctx)
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
    public void SecImp2(Context ctx)
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
    public void SedImp2(Context ctx)
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
    public void SeiImp2(Context ctx)
    {
        ctx.Regs.P.SetIRQDisabled();
        ctx.AdvanceState(States.Fetch);
    }
}
