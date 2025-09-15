using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;
using Us.Retrocpu.W65c02s.Types;

namespace Us.Retrocpu.W65c02s.Instructions
{
    /// <summary>
    /// Implements WAI (Wait for Interrupt) instruction for W65c02s.
    /// </summary>
    public class InstWAI : InstBase
    {
        public InstWAI(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.WAI, States.InstWAI2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstWAI2, ctx => Wai2(ctx));
        }

        private void Wai2(Context ctx)
        {
            // Wait for interrupt (implementation may vary)
            ctx.GetRegs().GetWaiting().UpdateValue(true);
        }
    }
}