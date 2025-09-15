using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;

namespace Us.Retrocpu.W65c02s.Instructions
{
    /// <summary>
    /// Implements control instructions (JMP, JSR, RTI, RTS) for W65c02s.
    /// </summary>
    public class InstControl : InstBase
    {
        public InstControl(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.JMPind, States.InstJMPind2);
            registry.Map(OpCodes.JMPabs, States.InstJMPabs2);
            registry.Map(OpCodes.JSRabs, States.InstJSRabs2);
            registry.Map(OpCodes.RTIimp, States.InstRTIimp2);
            registry.Map(OpCodes.RTSimp, States.InstRTSimp2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
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

        // JMP - Jump to New Location
        // (PC+1) -> PCL, (PC+2) -> PCH
        // N V B D I Z C
        // - - - - - - -
        // addressing     assembler     opc   bytes  cycles
        // ------------------------------------------------
        // absolute       JMP oper      4C      3      3
        // indirect       JMP (oper)    6C      3      5

        // JMP absolute
        public void JmpAbs2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstJMPabs3);
        }

        public void JmpAbs3(Context ctx)
        {
            FetchEffAddrHigh(ctx);
            if (ctx.GetSubStep() == P2LASTSUBSTEP)
                ctx.GetRegs().GetPC().UpdateValue(ctx.GetRegs().GetEA());
            ctx.AdvanceState(States.Fetch);
        }

        // JMP indirect
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
            // ...existing code...
        }

        public void JmpInd5(Context ctx)
        {
            // ...existing code...
        }

        // JSR, RTI, RTS methods would follow similar conversion, preserving logic and comments
        public void JsrAbs2(Context ctx) { /* ... */ }
        public void JsrAbs3(Context ctx) { /* ... */ }
        public void JsrAbs4(Context ctx) { /* ... */ }
        public void JsrAbs5(Context ctx) { /* ... */ }
        public void JsrAbs6(Context ctx) { /* ... */ }

        public void RtiImp2(Context ctx) { /* ... */ }
        public void RtiImp3(Context ctx) { /* ... */ }
        public void RtiImp4(Context ctx) { /* ... */ }
        public void RtiImp5(Context ctx) { /* ... */ }
        public void RtiImp6(Context ctx) { /* ... */ }

        public void RtsImp2(Context ctx) { /* ... */ }
        public void RtsImp3(Context ctx) { /* ... */ }
        public void RtsImp4(Context ctx) { /* ... */ }
        public void RtsImp5(Context ctx) { /* ... */ }
        public void RtsImp6(Context ctx) { /* ... */ }
    }
}