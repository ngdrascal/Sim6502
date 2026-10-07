namespace W65C02S.Engine;

internal class InstControl : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.JMPabs, States.InstJMPabs2);
        registry.Map(OpCodes.JMPind, States.InstJMPind2);
        registry.Map(OpCodes.JMPabsxind, States.InstJMPabsxind2);
        registry.Map(OpCodes.JSRabs, States.InstJSRabs2);
        registry.Map(OpCodes.RTIimp, States.InstRTIimp2);
        registry.Map(OpCodes.RTSimp, States.InstRTSimp2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstJMPabs2, JmpAbs2);
        stateRegistry.Map(States.InstJMPabs3, JmpAbs3);
        stateRegistry.Map(States.InstJMPind2, JmpInd2);
        stateRegistry.Map(States.InstJMPind3, JmpInd3);
        stateRegistry.Map(States.InstJMPind4, JmpInd4);
        stateRegistry.Map(States.InstJMPind5, JmpInd5);
        stateRegistry.Map(States.InstJMPabsxind2, JmpAbsxInd2);
        stateRegistry.Map(States.InstJMPabsxind3, JmpAbsxInd3);
        stateRegistry.Map(States.InstJMPabsxind4, JmpAbsxInd4);
        stateRegistry.Map(States.InstJMPabsxind5, JmpAbsxInd5);
        stateRegistry.Map(States.InstJMPabsxind6, JmpAbsxInd6);
        stateRegistry.Map(States.InstJSRabs2, JsrAbs2);
        stateRegistry.Map(States.InstJSRabs3, JsrAbs3);
        stateRegistry.Map(States.InstJSRabs4, JsrAbs4);
        stateRegistry.Map(States.InstJSRabs5, JsrAbs5);
        stateRegistry.Map(States.InstJSRabs6, JsrAbs6);
        stateRegistry.Map(States.InstRTIimp2, RtiImp2);
        stateRegistry.Map(States.InstRTIimp3, RtiImp3);
        stateRegistry.Map(States.InstRTIimp4, RtiImp4);
        stateRegistry.Map(States.InstRTIimp5, RtiImp5);
        stateRegistry.Map(States.InstRTIimp6, RtiImp6);
        stateRegistry.Map(States.InstRTSimp2, RtsImp2);
        stateRegistry.Map(States.InstRTSimp3, RtsImp3);
        stateRegistry.Map(States.InstRTSimp4, RtsImp4);
        stateRegistry.Map(States.InstRTSimp5, RtsImp5);
        stateRegistry.Map(States.InstRTSimp6, RtsImp6);

        return this;
    }

    /////////////////////////////////////////////////////////////////////////////
    // JMP - Jump to New Location
    // (PC+1) -> PCL, (PC+2) -> PCH
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // absolute       JMP oper      4C      3      3
    // indirect       JMP (oper)    6C      3      5
    // abs,X indirect JMP (oper,X)  7C      3      6
    /////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // JMP absolute
    // -------------------------------------------------------------------------
    private void JmpAbs2(Context ctx)
    {
        // fetch the low byte of the jump to address into the EA reg
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstJMPabs3);
    }

    private void JmpAbs3(Context ctx)
    {
        // fetch the high byte of the jump to address into the EA reg
        FetchEffAddrHigh(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.PC = ctx.Regs.EA;

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // JMP indirect
    // -------------------------------------------------------------------------
    private void JmpInd2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstJMPind3);
    }

    private void JmpInd3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstJMPind4);
    }

    private void JmpInd4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstJMPind5);
    }

    private void JmpInd5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        ctx.Regs.PC = ctx.Regs.EA2;

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // JMP absolute indexed indirect
    // -------------------------------------------------------------------------
    private void JmpAbsxInd2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstJMPabsxind3);
    }

    private void JmpAbsxInd3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstJMPabsxind4);
    }

    private void JmpAbsxInd4(Context ctx)
    {
        // internal operation: the address bus keeps PC+2 from the previous cycle while X is
        // added to the base address (full 16-bit add, carries into the high byte)
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.IncEAWithX();

        ctx.AdvanceState(States.InstJMPabsxind5);
    }

    private void JmpAbsxInd5(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstJMPabsxind6);
    }

    private void JmpAbsxInd6(Context ctx)
    {
        // pointer + 1 is a 16-bit increment: no page wrap on the 65C02
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.PC = ctx.Regs.EA2;

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // JSR - Jump To Subroutine
    // push (PC+2), (PC+1) -> PCL, (PC+2) -> PCH
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // absolute       JSR oper      20      3      6
    /////////////////////////////////////////////////////////////////////////////
    private void JsrAbs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstJSRabs3);
    }

    private void JsrAbs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstJSRabs4);
    }

    private void JsrAbs4(Context ctx)
    {
        // push PCH
        // The JSR instruction pushes the address of the second operand and not the address of the
        // next inst as one would expect.
        if (ctx.GetSubStep() == 1)
            ctx.Regs.PC = ctx.Regs.PC.Dec();
        PushOnStack(ctx, ctx.Regs.PC.Msb());

        ctx.AdvanceState(States.InstJSRabs5);
    }

    private void JsrAbs5(Context ctx)
    {
        // push PCL
        PushOnStack(ctx, ctx.Regs.PC.Lsb());
        ctx.AdvanceState(States.InstJSRabs6);
    }

    private void JsrAbs6(Context ctx)
    {
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.PC = ctx.Regs.EA;

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // RTI - Return from Interrupt
    // pull SR, pull PC
    // N V B D I Z C
    // <from stack>
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        RTI           40      1      6
    /////////////////////////////////////////////////////////////////////////////
    private void RtiImp2(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.InstRTIimp3);
    }

    private void RtiImp3(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.InstRTIimp4);
    }

    private void RtiImp4(Context ctx)
    {
        // pull status register
        if (PullFromStack(ctx, out var flags))
            ctx.Regs.P.SetFlags(flags);

        ctx.AdvanceState(States.InstRTIimp5);
    }

    private void RtiImp5(Context ctx)
    {
        // pull PC low byte
        if (PullFromStack(ctx, out var pcLsb))
            ctx.Regs.PC = ctx.Regs.PC.WithLsb(pcLsb);

        ctx.AdvanceState(States.InstRTIimp6);
    }

    private void RtiImp6(Context ctx)
    {
        // pull PC high byte
        if (PullFromStack(ctx, out var pcMsb))
            ctx.Regs.PC = ctx.Regs.PC.WithMsb(pcMsb);

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // RTS - Return from Subroutine
    // pull PC & PC+1 -> PC
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        RTS           60      1      6
    /////////////////////////////////////////////////////////////////////////////
    private void RtsImp2(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.InstRTSimp3);
    }

    private void RtsImp3(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.InstRTSimp4);
    }

    private void RtsImp4(Context ctx)
    {
        // pull PCL
        if (PullFromStack(ctx, out var pcLsb))
            ctx.Regs.PC = ctx.Regs.PC.WithLsb(pcLsb);

        ctx.AdvanceState(States.InstRTSimp5);
    }

    private void RtsImp5(Context ctx)
    {
        // pull PCH
        if (PullFromStack(ctx, out var pcMsb))
            ctx.Regs.PC = ctx.Regs.PC.WithMsb(pcMsb);

        ctx.AdvanceState(States.InstRTSimp6);
    }

    private void RtsImp6(Context ctx)
    {
        // NOTE: the address pushed on the stack was the address of the second operand and
        // not the address of the next instruction.  The increment points it to the next instruction.
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.PC = ctx.Regs.PC.Inc();

        ctx.AdvanceState(States.Fetch);
    }
}
