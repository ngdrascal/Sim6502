using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;
using Us.Retrocpu.W65c02s.Types;

namespace Us.Retrocpu.W65c02s.Instructions
{
    /// <summary>
    /// Implements STP (Stop the processor) instruction for W65c02s.
    /// </summary>
    public class InstSTP : InstBase
    {
        public InstSTP(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.STP, States.InstSTP2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstSTP2, ctx => Stp2(ctx));
        }

        private void Stp2(Context ctx)
        {
            // Stop the processor (implementation may vary)
            ctx.GetRegs().GetStopped().UpdateValue(true);
        }
    }
}