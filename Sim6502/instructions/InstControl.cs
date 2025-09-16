namespace Sim6502.Instructions;

public class InstControl : InstBase
{
    public InstControl(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.JMPabs, States.InstJMPabs2);
        registry.Map(OpCodes.JMPind, States.InstJMPind2);
        registry.Map(OpCodes.JSRabs, States.InstJSRabs2);
        registry.Map(OpCodes.RTIimp, States.InstRTIimp2);
        registry.Map(OpCodes.RTSimp, States.InstRTSimp2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstJMPabs2, ctx => JmpAbs2(ctx));
        stateRegistry.Map(States.InstJMPabs3, ctx => JmpAbs3(ctx));

        stateRegistry.Map(States.InstJMPind2, ctx => JmpInd2(ctx));
        stateRegistry.Map(States.InstJMPind3, ctx => JmpInd3(ctx));
        stateRegistry.Map(States.InstJMPind4, ctx => JmpInd4(ctx));
        stateRegistry.Map(States.InstJMPind5, ctx => JmpInd5(ctx));

        stateRegistry.Map(States.InstJSRabs2, ctx => JsrAbs2(ctx));
        stateRegistry.Map(States.InstJSRabs3, ctx => JsrAbs3(ctx));
        stateRegistry.Map(States.InstJSRabs4, ctx => JsrAbs4(ctx));
        stateRegistry.Map(States.InstJSRabs5, ctx => JsrAbs5(ctx));
        stateRegistry.Map(States.InstJSRabs6, ctx => JsrAbs6(ctx));

        stateRegistry.Map(States.InstRTIimp2, ctx => RtiImp2(ctx));
        stateRegistry.Map(States.InstRTIimp3, ctx => RtiImp3(ctx));
        stateRegistry.Map(States.InstRTIimp4, ctx => RtiImp4(ctx));
        stateRegistry.Map(States.InstRTIimp5, ctx => RtiImp5(ctx));
        stateRegistry.Map(States.InstRTIimp6, ctx => RtiImp6(ctx));

        stateRegistry.Map(States.InstRTSimp2, ctx => RtsImp2(ctx));
        stateRegistry.Map(States.InstRTSimp3, ctx => RtsImp3(ctx));
        stateRegistry.Map(States.InstRTSimp4, ctx => RtsImp4(ctx));
        stateRegistry.Map(States.InstRTSimp5, ctx => RtsImp5(ctx));
        stateRegistry.Map(States.InstRTSimp6, ctx => RtsImp6(ctx));
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
    /////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // JMP absolute
    // -------------------------------------------------------------------------
    public void JmpAbs2(Context ctx)
    {
        // fetch the low byte of the jump to address into the EA reg
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstJMPabs3);
    }

    public void JmpAbs3(Context ctx)
    {
        // fetch the high byte of the jump to address into the EA reg
        FetchEffAddrHigh(ctx);
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            ctx.Regs.PC.UpdateValue(ctx.Regs.EA);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // JMP indirect
    // -------------------------------------------------------------------------
    public void JmpInd2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstJMPind3);
    }

    public void JmpInd3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstJMPind4);
    }

    public void JmpInd4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstJMPind5);
    }

    public void JmpInd5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        ctx.Regs.PC.UpdateValue(ctx.Regs.EA2);
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
    public void JsrAbs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstJSRabs3);
    }

    public void JsrAbs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstJSRabs4);
    }

    public void JsrAbs4(Context ctx)
    {
        // push PCH
        // The JSR instruction pushes the address of the second operand and not the address of the
        // next inst as one would expect.
        if (ctx.GetSubStep() == 1)
            ctx.Regs.PC.Dec();
        PushOnStack(ctx, ctx.Regs.PC.Msb());
        ctx.AdvanceState(States.InstJSRabs5);
    }

    public void JsrAbs5(Context ctx)
    {
        // push PCL
        PushOnStack(ctx, ctx.Regs.PC.Lsb());
        ctx.AdvanceState(States.InstJSRabs6);
    }

    public void JsrAbs6(Context ctx)
    {
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            ctx.Regs.PC.UpdateValue(ctx.Regs.EA);
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
    public void RtiImp2(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.InstRTIimp3);
    }

    public void RtiImp3(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.InstRTIimp4);
    }

    public void RtiImp4(Context ctx)
    {
        // pull status register
        var flags = ctx.Regs.P.GetFlags();
        PullFromStack(ctx, flags);
        ctx.Regs.P.SetFlags(flags);
        ctx.AdvanceState(States.InstRTIimp5);
    }

    public void RtiImp5(Context ctx)
    {
        // pull PC low byte
        PullFromStack(ctx, ctx.Regs.PC.Lsb());
        ctx.AdvanceState(States.InstRTIimp6);
    }

    public void RtiImp6(Context ctx)
    {
        // pull PC high byte
        PullFromStack(ctx, ctx.Regs.PC.Msb());
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
    public void RtsImp2(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.InstRTSimp3);
    }

    public void RtsImp3(Context ctx)
    {
        // internal operation, nothing to simulate
        ctx.AdvanceState(States.InstRTSimp4);
    }

    public void RtsImp4(Context ctx)
    {
        // pull PCL
        PullFromStack(ctx, ctx.Regs.PC.Lsb());
        ctx.AdvanceState(States.InstRTSimp5);
    }

    public void RtsImp5(Context ctx)
    {
        // pull PCH
        PullFromStack(ctx, ctx.Regs.PC.Msb());
        ctx.AdvanceState(States.InstRTSimp6);
    }

    public void RtsImp6(Context ctx)
    {
        // NOTE: the address pushed on the stack was the address of the second operand and
        // not the address of the next instruction.  The increment points it to the next instruction.
        if (ctx.GetSubStep() == P2LASTSUBSTEP)
            ctx.Regs.PC.Inc();
        ctx.AdvanceState(States.Fetch);
    }
}
