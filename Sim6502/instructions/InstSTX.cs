using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;
using Us.Retrocpu.W65c02s.Types;

namespace Us.Retrocpu.W65c02s.Instructions
{
    /// <summary>
    /// Implements STX (Store X Register) instruction for W65c02s.
    /// </summary>
    public class InstSTX : InstBase
    {
        public InstSTX(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.STXzpg, States.InstSTXzpg2);
            registry.Map(OpCodes.STXzpgy, States.InstSTXzpgy2);
            registry.Map(OpCodes.STXabs, States.InstSTXabs2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstSTXzpg2, ctx => Zpg2(ctx));
            stateRegistry.Map(States.InstSTXzpg3, ctx => Zpg3(ctx));

            stateRegistry.Map(States.InstSTXzpgy2, ctx => Zpgy2(ctx));
            stateRegistry.Map(States.InstSTXzpgy3, ctx => Zpgy3(ctx));

            stateRegistry.Map(States.InstSTXabs2, ctx => Abs2(ctx));
            stateRegistry.Map(States.InstSTXabs3, ctx => Abs3(ctx));
            stateRegistry.Map(States.InstSTXabs4, ctx => Abs4(ctx));
        }

        private void StxToMemory(Context ctx)
        {
            ctx.GetRegs().GetTemp().UpdateValue(ctx.GetRegs().GetX().Value());
        }

        // ...existing code for all state methods (Zpg2, Zpg3, Zpgy2, etc.)...
    }
}