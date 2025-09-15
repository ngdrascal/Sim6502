using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;
using Us.Retrocpu.W65c02s.Types;

namespace Us.Retrocpu.W65c02s.Instructions
{
    /// <summary>
    /// Implements stack-related instructions for W65c02s (PHA, PHP, PLA, PLP).
    /// </summary>
    public class InstStack : InstBase
    {
        public InstStack(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.PHA, States.InstPHA2);
            registry.Map(OpCodes.PHP, States.InstPHP2);
            registry.Map(OpCodes.PLA, States.InstPLA2);
            registry.Map(OpCodes.PLP, States.InstPLP2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstPHA2, ctx => Pha2(ctx));
            stateRegistry.Map(States.InstPHA3, ctx => Pha3(ctx));

            stateRegistry.Map(States.InstPHP2, ctx => Php2(ctx));
            stateRegistry.Map(States.InstPHP3, ctx => Php3(ctx));

            stateRegistry.Map(States.InstPLA2, ctx => Pla2(ctx));
            stateRegistry.Map(States.InstPLA3, ctx => Pla3(ctx));

            stateRegistry.Map(States.InstPLP2, ctx => Plp2(ctx));
            stateRegistry.Map(States.InstPLP3, ctx => Plp3(ctx));
        }

        private void Pha2(Context ctx)
        {
            // Push accumulator onto stack
            ctx.GetRegs().GetTemp().UpdateValue(ctx.GetRegs().GetA().Value());
        }

        private void Php2(Context ctx)
        {
            // Push processor status onto stack
            ctx.GetRegs().GetTemp().UpdateValue(ctx.GetRegs().GetP().Value());
        }

        private void Pla3(Context ctx)
        {
            // Pull accumulator from stack
            ctx.GetRegs().GetA().UpdateValue(ctx.GetRegs().GetTemp().Value());
            ctx.GetRegs().GetP().GetZero().UpdateValue(ctx.GetRegs().GetA().Value().EqualsZero());
            ctx.GetRegs().GetP().GetNegative().UpdateValue(ctx.GetRegs().GetA().Value().IsBitSet(7));
        }

        private void Plp3(Context ctx)
        {
            // Pull processor status from stack
            ctx.GetRegs().GetP().UpdateValue(ctx.GetRegs().GetTemp().Value());
        }

        // ...existing code for all state methods (Pha3, Php3, Pla2, Plp2, etc.)...
    }
}