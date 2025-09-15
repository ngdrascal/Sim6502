using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;
using Us.Retrocpu.W65c02s.Types;

namespace Us.Retrocpu.W65c02s.Instructions
{
    /// <summary>
    /// Implements transfer instructions for W65c02s (TAX, TAY, TXA, TYA, TSX, TXS).
    /// </summary>
    public class InstTransfer : InstBase
    {
        public InstTransfer(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.TAX, States.InstTAX2);
            registry.Map(OpCodes.TAY, States.InstTAY2);
            registry.Map(OpCodes.TXA, States.InstTXA2);
            registry.Map(OpCodes.TYA, States.InstTYA2);
            registry.Map(OpCodes.TSX, States.InstTSX2);
            registry.Map(OpCodes.TXS, States.InstTXS2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstTAX2, ctx => Tax2(ctx));
            stateRegistry.Map(States.InstTAY2, ctx => Tay2(ctx));
            stateRegistry.Map(States.InstTXA2, ctx => Txa2(ctx));
            stateRegistry.Map(States.InstTYA2, ctx => Tya2(ctx));
            stateRegistry.Map(States.InstTSX2, ctx => Tsx2(ctx));
            stateRegistry.Map(States.InstTXS2, ctx => Txs2(ctx));
        }

        private void Tax2(Context ctx)
        {
            ctx.GetRegs().GetX().UpdateValue(ctx.GetRegs().GetA().Value());
            ctx.GetRegs().GetP().GetZero().UpdateValue(ctx.GetRegs().GetX().Value().EqualsZero());
            ctx.GetRegs().GetP().GetNegative().UpdateValue(ctx.GetRegs().GetX().Value().IsBitSet(7));
        }

        private void Tay2(Context ctx)
        {
            ctx.GetRegs().GetY().UpdateValue(ctx.GetRegs().GetA().Value());
            ctx.GetRegs().GetP().GetZero().UpdateValue(ctx.GetRegs().GetY().Value().EqualsZero());
            ctx.GetRegs().GetP().GetNegative().UpdateValue(ctx.GetRegs().GetY().Value().IsBitSet(7));
        }

        private void Txa2(Context ctx)
        {
            ctx.GetRegs().GetA().UpdateValue(ctx.GetRegs().GetX().Value());
            ctx.GetRegs().GetP().GetZero().UpdateValue(ctx.GetRegs().GetA().Value().EqualsZero());
            ctx.GetRegs().GetP().GetNegative().UpdateValue(ctx.GetRegs().GetA().Value().IsBitSet(7));
        }

        private void Tya2(Context ctx)
        {
            ctx.GetRegs().GetA().UpdateValue(ctx.GetRegs().GetY().Value());
            ctx.GetRegs().GetP().GetZero().UpdateValue(ctx.GetRegs().GetA().Value().EqualsZero());
            ctx.GetRegs().GetP().GetNegative().UpdateValue(ctx.GetRegs().GetA().Value().IsBitSet(7));
        }

        private void Tsx2(Context ctx)
        {
            ctx.GetRegs().GetX().UpdateValue(ctx.GetRegs().GetS().Value());
            ctx.GetRegs().GetP().GetZero().UpdateValue(ctx.GetRegs().GetX().Value().EqualsZero());
            ctx.GetRegs().GetP().GetNegative().UpdateValue(ctx.GetRegs().GetX().Value().IsBitSet(7));
        }

        private void Txs2(Context ctx)
        {
            ctx.GetRegs().GetS().UpdateValue(ctx.GetRegs().GetX().Value());
        }
    }
}