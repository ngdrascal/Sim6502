using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;
using Us.Retrocpu.W65c02s.Types;

namespace Us.Retrocpu.W65c02s.Instructions
{
    /// <summary>
    /// Implements SBC (Subtract with Borrow) instruction for W65c02s.
    /// </summary>
    public class InstSBC : InstBase
    {
        public InstSBC(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.SBCimm, States.InstSBCimm2);
            registry.Map(OpCodes.SBCzpg, States.InstSBCzpg2);
            registry.Map(OpCodes.SBCzpgx, States.InstSBCzpgx2);
            registry.Map(OpCodes.SBCabs, States.InstSBCabs2);
            registry.Map(OpCodes.SBCabsx, States.InstSBCabsx2);
            registry.Map(OpCodes.SBCabsy, States.InstSBCabsy2);
            registry.Map(OpCodes.SBCindx, States.InstSBCindx2);
            registry.Map(OpCodes.SBCindy, States.InstSBCindy2);
            registry.Map(OpCodes.SBCind, States.InstSBCind2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstSBCimm2, ctx => Imm2(ctx));
            stateRegistry.Map(States.InstSBCimm3, ctx => Imm3(ctx));

            stateRegistry.Map(States.InstSBCzpg2, ctx => Zpg2(ctx));
            stateRegistry.Map(States.InstSBCzpg3, ctx => Zpg3(ctx));
            stateRegistry.Map(States.InstSBCzpg4, ctx => Zpg4(ctx));

            stateRegistry.Map(States.InstSBCzpgx2, ctx => Zpgx2(ctx));
            stateRegistry.Map(States.InstSBCzpgx3, ctx => Zpgx3(ctx));
            stateRegistry.Map(States.InstSBCzpgx4, ctx => Zpgx4(ctx));
            stateRegistry.Map(States.InstSBCzpgx5, ctx => Zpgx5(ctx));

            stateRegistry.Map(States.InstSBCabs2, ctx => Abs2(ctx));
            stateRegistry.Map(States.InstSBCabs3, ctx => Abs3(ctx));
            stateRegistry.Map(States.InstSBCabs4, ctx => Abs4(ctx));
            stateRegistry.Map(States.InstSBCabs5, ctx => Abs5(ctx));

            stateRegistry.Map(States.InstSBCabsx2, ctx => Absx2(ctx));
            stateRegistry.Map(States.InstSBCabsx3, ctx => Absx3(ctx));
            stateRegistry.Map(States.InstSBCabsx4, ctx => Absx4(ctx));
            stateRegistry.Map(States.InstSBCabsx5, ctx => Absx5(ctx));
            stateRegistry.Map(States.InstSBCabsx6, ctx => Absx6(ctx));

            stateRegistry.Map(States.InstSBCabsy2, ctx => Absy2(ctx));
            stateRegistry.Map(States.InstSBCabsy3, ctx => Absy3(ctx));
            stateRegistry.Map(States.InstSBCabsy4, ctx => Absy4(ctx));
            stateRegistry.Map(States.InstSBCabsy5, ctx => Absy5(ctx));
            stateRegistry.Map(States.InstSBCabsy6, ctx => Absy6(ctx));

            stateRegistry.Map(States.InstSBCindx2, ctx => Indx2(ctx));
            stateRegistry.Map(States.InstSBCindx3, ctx => Indx3(ctx));
            stateRegistry.Map(States.InstSBCindx4, ctx => Indx4(ctx));
            stateRegistry.Map(States.InstSBCindx5, ctx => Indx5(ctx));
            stateRegistry.Map(States.InstSBCindx6, ctx => Indx6(ctx));
            stateRegistry.Map(States.InstSBCindx7, ctx => Indx7(ctx));

            stateRegistry.Map(States.InstSBCindy2, ctx => Indy2(ctx));
            stateRegistry.Map(States.InstSBCindy3, ctx => Indy3(ctx));
            stateRegistry.Map(States.InstSBCindy4, ctx => Indy4(ctx));
            stateRegistry.Map(States.InstSBCindy5, ctx => Indy5(ctx));
            stateRegistry.Map(States.InstSBCindy6, ctx => Indy6(ctx));
            stateRegistry.Map(States.InstSBCindy7, ctx => Indy7(ctx));

            stateRegistry.Map(States.InstSBCind2, ctx => Ind2(ctx));
            stateRegistry.Map(States.InstSBCind3, ctx => Ind3(ctx));
            stateRegistry.Map(States.InstSBCind4, ctx => Ind4(ctx));
            stateRegistry.Map(States.InstSBCind5, ctx => Ind5(ctx));
            stateRegistry.Map(States.InstSBCind6, ctx => Ind6(ctx));
        }

        private void SbcThenUpdateFlags(Context ctx)
        {
            StatusRegister p = ctx.GetRegs().GetP();
            UInt8 operand = ctx.GetRegs().GetTemp();
            MathResult result = ctx.GetRegs().GetA().Sbc(operand, p.GetCarry(), p.GetDecimal());
            ctx.GetRegs().GetA().UpdateValue(result.Value());
            p.GetNegative().UpdateValue(result.Value().IsBitSet(7));
            p.GetOverflow().UpdateValue(result.Overflow());
            p.GetZero().UpdateValue(result.Value().EqualsZero());
            p.GetCarry().UpdateValue(result.Carry());
        }

        // ...existing code for all state methods (Imm2, Imm3, Zpg2, etc.)...
    }
}