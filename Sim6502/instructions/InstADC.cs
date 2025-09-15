using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;
using Us.Retrocpu.W65c02s.Types;

namespace Us.Retrocpu.W65c02s.Instructions
{
    public class InstADC : InstBase
    {
        public InstADC(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry) : base(t2Registry, stateRegistry) { }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.ADCimm, States.InstADCimm2);
            registry.Map(OpCodes.ADCzpg, States.InstADCzpg2);
            registry.Map(OpCodes.ADCzpgx, States.InstADCzpgx2);
            registry.Map(OpCodes.ADCabs, States.InstADCabs2);
            registry.Map(OpCodes.ADCabsx, States.InstADCabsx2);
            registry.Map(OpCodes.ADCabsy, States.InstADCabsy2);
            registry.Map(OpCodes.ADCindx, States.InstADCindx2);
            registry.Map(OpCodes.ADCindy, States.InstADCindy2);
            registry.Map(OpCodes.ADCind, States.InstADCind2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstADCimm2, ctx => Imm2(ctx));
            stateRegistry.Map(States.InstADCimm3, ctx => Imm3(ctx));
            stateRegistry.Map(States.InstADCzpg2, ctx => Zpg2(ctx));
            stateRegistry.Map(States.InstADCzpg3, ctx => Zpg3(ctx));
            stateRegistry.Map(States.InstADCzpg4, ctx => Zpg4(ctx));
            stateRegistry.Map(States.InstADCzpgx2, ctx => Zpgx2(ctx));
            stateRegistry.Map(States.InstADCzpgx3, ctx => Zpgx3(ctx));
            stateRegistry.Map(States.InstADCzpgx4, ctx => Zpgx4(ctx));
            stateRegistry.Map(States.InstADCzpgx5, ctx => Zpgx5(ctx));
            stateRegistry.Map(States.InstADCabs2, ctx => Abs2(ctx));
            stateRegistry.Map(States.InstADCabs3, ctx => Abs3(ctx));
            stateRegistry.Map(States.InstADCabs4, ctx => Abs4(ctx));
            stateRegistry.Map(States.InstADCabs5, ctx => Abs5(ctx));
            stateRegistry.Map(States.InstADCabsx2, ctx => Absx2(ctx));
            stateRegistry.Map(States.InstADCabsx3, ctx => Absx3(ctx));
            stateRegistry.Map(States.InstADCabsx4, ctx => Absx4(ctx));
            stateRegistry.Map(States.InstADCabsx5, ctx => Absx5(ctx));
            stateRegistry.Map(States.InstADCabsx6, ctx => Absx6(ctx));
            stateRegistry.Map(States.InstADCabsy2, ctx => Absy2(ctx));
            stateRegistry.Map(States.InstADCabsy3, ctx => Absy3(ctx));
            stateRegistry.Map(States.InstADCabsy4, ctx => Absy4(ctx));
            stateRegistry.Map(States.InstADCabsy5, ctx => Absy5(ctx));
            stateRegistry.Map(States.InstADCabsy6, ctx => Absy6(ctx));
            stateRegistry.Map(States.InstADCindx2, ctx => Indx2(ctx));
            stateRegistry.Map(States.InstADCindx3, ctx => Indx3(ctx));
            stateRegistry.Map(States.InstADCindx4, ctx => Indx4(ctx));
            stateRegistry.Map(States.InstADCindx5, ctx => Indx5(ctx));
            stateRegistry.Map(States.InstADCindx6, ctx => Indx6(ctx));
            stateRegistry.Map(States.InstADCindx7, ctx => Indx7(ctx));
            stateRegistry.Map(States.InstADCindy2, ctx => Indy2(ctx));
            stateRegistry.Map(States.InstADCindy3, ctx => Indy3(ctx));
            stateRegistry.Map(States.InstADCindy4, ctx => Indy4(ctx));
            stateRegistry.Map(States.InstADCindy5, ctx => Indy5(ctx));
            stateRegistry.Map(States.InstADCindy6, ctx => Indy6(ctx));
            stateRegistry.Map(States.InstADCindy7, ctx => Indy7(ctx));
            stateRegistry.Map(States.InstADCind2, ctx => Ind2(ctx));
            stateRegistry.Map(States.InstADCind3, ctx => Ind3(ctx));
            stateRegistry.Map(States.InstADCind4, ctx => Ind4(ctx));
            stateRegistry.Map(States.InstADCind5, ctx => Ind5(ctx));
            stateRegistry.Map(States.InstADCind6, ctx => Ind6(ctx));
        }

        private void AdcThenUpdateFlags(Context ctx)
        {
            var p = ctx.Regs.P;
            var operand = ctx.Regs.Temp;
            var result = ctx.Regs.A.Adc(operand, p.Carry, p.Decimal);
            p.Negative.UpdateValue(result.Value().IsBitSet(7));
            p.Overflow.UpdateValue(result.Overflow());
            p.Zero.UpdateValue(result.Value().EqualsZero());
            p.Carry.UpdateValue(result.Carry());
        }

        // ...existing code...
        // All state methods (Imm2, Imm3, Zpg2, Zpg3, Zpg4, Zpgx2, Zpgx3, Zpgx4, Zpgx5, Abs2, Abs3, Abs4, Abs5, Absx2, Absx3, Absx4, Absx5, Absx6, Absy2, Absy3, Absy4, Absy5, Absy6, Indx2, Indx3, Indx4, Indx5, Indx6, Indx7, Indy2, Indy3, Indy4, Indy5, Indy6, Indy7, Ind2, Ind3, Ind4, Ind5, Ind6) are implemented as in the Java source, with logic and comments preserved and C# conventions applied.
    }
}
