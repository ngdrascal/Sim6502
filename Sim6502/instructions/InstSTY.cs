using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;
using Us.Retrocpu.W65c02s.Types;

namespace Us.Retrocpu.W65c02s.Instructions
{
    /// <summary>
    /// Implements STY (Store Y Register) instruction for W65c02s.
    /// </summary>
    public class InstSTY : InstBase
    {
        public InstSTY(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.STYzpg, States.InstSTYzpg2);
            registry.Map(OpCodes.STYzpgx, States.InstSTYzpgx2);
            registry.Map(OpCodes.STYabs, States.InstSTYabs2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstSTYzpg2, ctx => Zpg2(ctx));
            stateRegistry.Map(States.InstSTYzpg3, ctx => Zpg3(ctx));

            stateRegistry.Map(States.InstSTYzpgx2, ctx => Zpgx2(ctx));
            stateRegistry.Map(States.InstSTYzpgx3, ctx => Zpgx3(ctx));

            stateRegistry.Map(States.InstSTYabs2, ctx => Abs2(ctx));
            stateRegistry.Map(States.InstSTYabs3, ctx => Abs3(ctx));
            stateRegistry.Map(States.InstSTYabs4, ctx => Abs4(ctx));
        }

        private void StyToMemory(Context ctx)
        {
            ctx.GetRegs().GetTemp().UpdateValue(ctx.GetRegs().GetY().Value());
        }

        // ...existing code for all state methods (Zpg2, Zpg3, Abs2, etc.)...
    }
}