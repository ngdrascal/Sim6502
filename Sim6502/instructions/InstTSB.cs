using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;
using Us.Retrocpu.W65c02s.Types;

namespace Us.Retrocpu.W65c02s.Instructions
{
    /// <summary>
    /// Implements TSB (Test and Set Bits) instruction for W65c02s.
    /// </summary>
    public class InstTSB : InstBase
    {
        public InstTSB(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.TSBzpg, States.InstTSBzpg2);
            registry.Map(OpCodes.TSBabs, States.InstTSBabs2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstTSBzpg2, ctx => Zpg2(ctx));
            stateRegistry.Map(States.InstTSBzpg3, ctx => Zpg3(ctx));

            stateRegistry.Map(States.InstTSBabs2, ctx => Abs2(ctx));
            stateRegistry.Map(States.InstTSBabs3, ctx => Abs3(ctx));
        }

        private void TsbToMemory(Context ctx)
        {
            UInt8 mem = ctx.GetRegs().GetTemp();
            UInt8 acc = ctx.GetRegs().GetA().Value();
            ctx.GetRegs().GetTemp().UpdateValue((UInt8)(mem | acc));
            ctx.GetRegs().GetP().GetZero().UpdateValue((mem & acc).EqualsZero());
        }

        // ...existing code for all state methods (Zpg2, Zpg3, Abs2, Abs3)...
    }
}