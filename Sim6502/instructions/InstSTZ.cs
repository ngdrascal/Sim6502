using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;
using Us.Retrocpu.W65c02s.Types;

namespace Us.Retrocpu.W65c02s.Instructions
{
    /// <summary>
    /// Implements STZ (Store Zero) instruction for W65c02s.
    /// </summary>
    public class InstSTZ : InstBase
    {
        public InstSTZ(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.STZzpg, States.InstSTZzpg2);
            registry.Map(OpCodes.STZzpgx, States.InstSTZzpgx2);
            registry.Map(OpCodes.STZabs, States.InstSTZabs2);
            registry.Map(OpCodes.STZabsx, States.InstSTZabsx2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstSTZzpg2, ctx => Zpg2(ctx));
            stateRegistry.Map(States.InstSTZzpg3, ctx => Zpg3(ctx));

            stateRegistry.Map(States.InstSTZzpgx2, ctx => Zpgx2(ctx));
            stateRegistry.Map(States.InstSTZzpgx3, ctx => Zpgx3(ctx));
            stateRegistry.Map(States.InstSTZzpgx4, ctx => Zpgx4(ctx));

            stateRegistry.Map(States.InstSTZabs2, ctx => Abs2(ctx));
            stateRegistry.Map(States.InstSTZabs3, ctx => Abs3(ctx));
            stateRegistry.Map(States.InstSTZabs4, ctx => Abs4(ctx));

            stateRegistry.Map(States.InstSTZabsx2, ctx => Absx2(ctx));
            stateRegistry.Map(States.InstSTZabsx3, ctx => Absx3(ctx));
            stateRegistry.Map(States.InstSTZabsx4, ctx => Absx4(ctx));
            stateRegistry.Map(States.InstSTZabsx5, ctx => Absx5(ctx));
        }

        private void StzToMemory(Context ctx)
        {
            ctx.GetRegs().GetTemp().UpdateValue(new UInt8(0));
        }

        // ...existing code for all state methods (Zpg2, Zpg3, Abs2, etc.)...
    }
}