using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;
using Us.Retrocpu.W65c02s.Types;

namespace Us.Retrocpu.W65c02s.Instructions
{
    /// <summary>
    /// Implements STA (Store Accumulator) instruction for W65c02s.
    /// </summary>
    public class InstSTA : InstBase
    {
        public InstSTA(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.STAzpg, States.InstSTAzpg2);
            registry.Map(OpCodes.STAzpgx, States.InstSTAzpgx2);
            registry.Map(OpCodes.STAabs, States.InstSTAabs2);
            registry.Map(OpCodes.STAabsx, States.InstSTAabsx2);
            registry.Map(OpCodes.STAabsy, States.InstSTAabsy2);
            registry.Map(OpCodes.STAindx, States.InstSTAindx2);
            registry.Map(OpCodes.STAindy, States.InstSTAindy2);
            registry.Map(OpCodes.STAind, States.InstSTAind2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstSTAzpg2, ctx => Zpg2(ctx));
            stateRegistry.Map(States.InstSTAzpg3, ctx => Zpg3(ctx));

            stateRegistry.Map(States.InstSTAzpgx2, ctx => Zpgx2(ctx));
            stateRegistry.Map(States.InstSTAzpgx3, ctx => Zpgx3(ctx));
            stateRegistry.Map(States.InstSTAzpgx4, ctx => Zpgx4(ctx));

            stateRegistry.Map(States.InstSTAabs2, ctx => Abs2(ctx));
            stateRegistry.Map(States.InstSTAabs3, ctx => Abs3(ctx));
            stateRegistry.Map(States.InstSTAabs4, ctx => Abs4(ctx));

            stateRegistry.Map(States.InstSTAabsx2, ctx => Absx2(ctx));
            stateRegistry.Map(States.InstSTAabsx3, ctx => Absx3(ctx));
            stateRegistry.Map(States.InstSTAabsx4, ctx => Absx4(ctx));
            stateRegistry.Map(States.InstSTAabsx5, ctx => Absx5(ctx));

            stateRegistry.Map(States.InstSTAabsy2, ctx => Absy2(ctx));
            stateRegistry.Map(States.InstSTAabsy3, ctx => Absy3(ctx));
            stateRegistry.Map(States.InstSTAabsy4, ctx => Absy4(ctx));
            stateRegistry.Map(States.InstSTAabsy5, ctx => Absy5(ctx));

            stateRegistry.Map(States.InstSTAindx2, ctx => Indx2(ctx));
            stateRegistry.Map(States.InstSTAindx3, ctx => Indx3(ctx));
            stateRegistry.Map(States.InstSTAindx4, ctx => Indx4(ctx));
            stateRegistry.Map(States.InstSTAindx5, ctx => Indx5(ctx));
            stateRegistry.Map(States.InstSTAindx6, ctx => Indx6(ctx));

            stateRegistry.Map(States.InstSTAindy2, ctx => Indy2(ctx));
            stateRegistry.Map(States.InstSTAindy3, ctx => Indy3(ctx));
            stateRegistry.Map(States.InstSTAindy4, ctx => Indy4(ctx));
            stateRegistry.Map(States.InstSTAindy5, ctx => Indy5(ctx));
            stateRegistry.Map(States.InstSTAindy6, ctx => Indy6(ctx));

            stateRegistry.Map(States.InstSTAind2, ctx => Ind2(ctx));
            stateRegistry.Map(States.InstSTAind3, ctx => Ind3(ctx));
            stateRegistry.Map(States.InstSTAind4, ctx => Ind4(ctx));
            stateRegistry.Map(States.InstSTAind5, ctx => Ind5(ctx));
            stateRegistry.Map(States.InstSTAind6, ctx => Ind6(ctx));
        }

        private void StaToMemory(Context ctx)
        {
            ctx.GetRegs().GetTemp().UpdateValue(ctx.GetRegs().GetA().Value());
        }

        // ...existing code for all state methods (Zpg2, Zpg3, Abs2, etc.)...
    }
}