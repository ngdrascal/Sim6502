using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;
using Us.Retrocpu.W65c02s.Types;

namespace Us.Retrocpu.W65c02s.Instructions
{
    /// <summary>
    /// Implements interrupt handling for W65c02s (BRK, IRQ, NMI, RESET).
    /// </summary>
    public class Interrupts : InstBase
    {
        public Interrupts(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.BRK, States.InstBRK2);
            registry.Map(OpCodes.IRQ, States.InstIRQ2);
            registry.Map(OpCodes.NMI, States.InstNMI2);
            registry.Map(OpCodes.RESET, States.InstRESET2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstBRK2, ctx => Brk2(ctx));
            stateRegistry.Map(States.InstIRQ2, ctx => Irq2(ctx));
            stateRegistry.Map(States.InstNMI2, ctx => Nmi2(ctx));
            stateRegistry.Map(States.InstRESET2, ctx => Reset2(ctx));
        }

        private void Brk2(Context ctx)
        {
            // Handle BRK interrupt
            ctx.GetRegs().GetInterrupt().UpdateValue(true);
        }

        private void Irq2(Context ctx)
        {
            // Handle IRQ interrupt
            ctx.GetRegs().GetInterrupt().UpdateValue(true);
        }

        private void Nmi2(Context ctx)
        {
            // Handle NMI interrupt
            ctx.GetRegs().GetInterrupt().UpdateValue(true);
        }

        private void Reset2(Context ctx)
        {
            // Handle RESET interrupt
            ctx.GetRegs().GetReset().UpdateValue(true);
        }
    }
}