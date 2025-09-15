using System;
using us.retrocpu.shared;
using us.retrocpu.w65c02s;

namespace us.retrocpu.w65c02s.instructions
{
    // AND - "AND" Memory with Accumulator
    // A AND M -> A
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // immediate      AND #oper     29      2      2
    // zeropage       AND oper      25      2      3
    // zeropage,X     AND oper,X    35      2      4
    // absolute       AND oper      2D      3      4
    // absolute,X     AND oper,X    3D      3      4*
    // absolute,Y     AND oper,Y    39      3      4*
    // (indirect,X)   AND (oper,X)  21      2      6
    // (indirect),Y   AND (oper),Y  31      2      5*
    // (indirect)     AND (oper)    32      2      5
    public class InstAND : InstBase
    {
        public InstAND(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry) : base(t2Registry, stateRegistry) { }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.ANDimm, States.InstANDimm2);
            registry.Map(OpCodes.ANDzpg, States.InstANDzpg2);
            registry.Map(OpCodes.ANDzpgx, States.InstANDzpgx2);
            registry.Map(OpCodes.ANDabs, States.InstANDabs2);
            registry.Map(OpCodes.ANDabsx, States.InstANDabsx2);
            registry.Map(OpCodes.ANDabsy, States.InstANDabsy2);
            registry.Map(OpCodes.ANDindx, States.InstANDindx2);
            registry.Map(OpCodes.ANDindy, States.InstANDindy2);
            registry.Map(OpCodes.ANDind, States.InstANDind2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstANDimm2, ctx => Imm2(ctx));

            stateRegistry.Map(States.InstANDzpg2, ctx => Zpg2(ctx));
            stateRegistry.Map(States.InstANDzpg3, ctx => Zpg3(ctx));

            stateRegistry.Map(States.InstANDzpgx2, ctx => Zpgx2(ctx));
            stateRegistry.Map(States.InstANDzpgx3, ctx => Zpgx3(ctx));
            stateRegistry.Map(States.InstANDzpgx4, ctx => Zpgx4(ctx));

            stateRegistry.Map(States.InstANDabs2, ctx => Abs2(ctx));
            stateRegistry.Map(States.InstANDabs3, ctx => Abs3(ctx));
            stateRegistry.Map(States.InstANDabs4, ctx => Abs4(ctx));

            stateRegistry.Map(States.InstANDabsx2, ctx => Absx2(ctx));
            stateRegistry.Map(States.InstANDabsx3, ctx => Absx3(ctx));
            stateRegistry.Map(States.InstANDabsx4, ctx => Absx4(ctx));
            stateRegistry.Map(States.InstANDabsx5, ctx => Absx5(ctx));

            stateRegistry.Map(States.InstANDabsy2, ctx => Absy2(ctx));
            stateRegistry.Map(States.InstANDabsy3, ctx => Absy3(ctx));
            stateRegistry.Map(States.InstANDabsy4, ctx => Absy4(ctx));
            stateRegistry.Map(States.InstANDabsy5, ctx => Absy5(ctx));

            stateRegistry.Map(States.InstANDindx2, ctx => Indx2(ctx));
            stateRegistry.Map(States.InstANDindx3, ctx => Indx3(ctx));
            stateRegistry.Map(States.InstANDindx4, ctx => Indx4(ctx));
            stateRegistry.Map(States.InstANDindx5, ctx => Indx5(ctx));
            stateRegistry.Map(States.InstANDindx6, ctx => Indx6(ctx));

            stateRegistry.Map(States.InstANDindy2, ctx => Indy2(ctx));
            stateRegistry.Map(States.InstANDindy3, ctx => Indy3(ctx));
            stateRegistry.Map(States.InstANDindy4, ctx => Indy4(ctx));
            stateRegistry.Map(States.InstANDindy5, ctx => Indy5(ctx));
            stateRegistry.Map(States.InstANDindy6, ctx => Indy6(ctx));

            stateRegistry.Map(States.InstANDind2, ctx => Ind2(ctx));
            stateRegistry.Map(States.InstANDind3, ctx => Ind3(ctx));
            stateRegistry.Map(States.InstANDind4, ctx => Ind4(ctx));
            stateRegistry.Map(States.InstANDind5, ctx => Ind5(ctx));
        }

        protected void AndAWithTemp(Context ctx)
        {
            var memValue = ctx.Regs.Temp;
            var result = ctx.Regs.A.Copy().And(memValue);
            ctx.Regs.UpdateAUpdateFlags(result);
        }

        // [0x29] AND immediate
        public void Imm2(Context ctx)
        {
            Imm2SetAddrBus(ctx);
            if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = ctx.Pins.DataBusPins;
                var result = ctx.Regs.A.Copy().And(data);
                ctx.Regs.UpdateAUpdateFlags(result);
                ctx.DbgOperand1 = data;
            }
            ctx.AdvanceState(States.Fetch);
        }

        // [0x25] AND zeropage
        public void Zpg2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstANDzpg3);
        }

        public void Zpg3(Context ctx)
        {
            LoadTempFromEffAddr(ctx);
            if (ctx.SubStep == P2LASTSUBSTEP)
                AndAWithTemp(ctx);
            ctx.AdvanceState(States.Fetch);
        }

        // [0x35] AND zeropage,X
        public void Zpgx2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstANDzpgx3);
        }

        public void Zpgx3(Context ctx)
        {
            if (ctx.SubStep == P2LASTSUBSTEP)
                ctx.Regs.IncEALWithX();
            ctx.AdvanceState(States.InstANDzpgx4);
        }

        public void Zpgx4(Context ctx)
        {
            LoadTempFromEffAddr(ctx);
            if (ctx.SubStep == P2LASTSUBSTEP)
                AndAWithTemp(ctx);
            ctx.AdvanceState(States.Fetch);
        }

        // [0x2D] AND absolute
        public void Abs2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstANDabs3);
        }

        public void Abs3(Context ctx)
        {
            FetchEffAddrHigh(ctx);
            ctx.AdvanceState(States.InstANDabs4);
        }

        public void Abs4(Context ctx)
        {
            LoadTempFromEffAddr(ctx);
            if (ctx.SubStep == P2LASTSUBSTEP)
                AndAWithTemp(ctx);
            ctx.AdvanceState(States.Fetch);
        }

        // [0x3D] AND absolute,X
        public void Absx2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstANDabsx3);
        }

        public void Absx3(Context ctx)
        {
            FetchEffAddrHigh(ctx);
            ctx.AdvanceState(States.InstANDabsx4);
        }

        public void Absx4(Context ctx)
        {
            var nextState = States.Fetch;
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                var beforePage = ctx.Regs.EA.Msb().Copy();
                ctx.Regs.IncEAWithX();
                var afterPage = ctx.Regs.EA.Msb().Copy();
                ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
                ctx.Pins.AddrBusPins = ctx.Regs.EA;
                ctx.Pins.RWB = READ;
            }
            else if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = ctx.Pins.DataBusPins;
                ctx.Regs.Temp.UpdateValue(data);
                if (!ctx.CrossedPageBoundary)
                {
                    AndAWithTemp(ctx);
                }
                else
                {
                    nextState = States.InstANDabsx5;
                }
            }
            ctx.AdvanceState(nextState);
        }

        public void Absx5(Context ctx)
        {
            if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = ctx.Pins.DataBusPins;
                ctx.Regs.Temp.UpdateValue(data);
                AndAWithTemp(ctx);
            }
            ctx.AdvanceState(States.Fetch);
        }

        // [0x39] AND absolute,Y
        public void Absy2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstANDabsy3);
        }

        public void Absy3(Context ctx)
        {
            FetchEffAddrHigh(ctx);
            ctx.AdvanceState(States.InstANDabsy4);
        }

        public void Absy4(Context ctx)
        {
            var nextState = States.Fetch;
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                var beforePage = ctx.Regs.EA.Msb().Copy();
                ctx.Regs.IncEAWithY();
                var afterPage = ctx.Regs.EA.Msb().Copy();
                ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
                ctx.Pins.AddrBusPins = ctx.Regs.EA;
                ctx.Pins.RWB = READ;
            }
            else if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = ctx.Pins.DataBusPins;
                ctx.Regs.Temp.UpdateValue(data);
                if (!ctx.CrossedPageBoundary)
                {
                    AndAWithTemp(ctx);
                }
                else
                {
                    nextState = States.InstANDabsy5;
                }
            }
            ctx.AdvanceState(nextState);
        }

        public void Absy5(Context ctx)
        {
            if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = ctx.Pins.DataBusPins;
                ctx.Regs.Temp.UpdateValue(data);
                AndAWithTemp(ctx);
            }
            ctx.AdvanceState(States.Fetch);
        }

        // [0x21] AND (indirect,X)
        public void Indx2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstANDindx3);
        }

        public void Indx3(Context ctx)
        {
            if (ctx.SubStep == P2LASTSUBSTEP)
            {
                ctx.Regs.EA.Lsb().AddWithWrapAround(ctx.Regs.X);
                ctx.Regs.EA.Msb().Zero();
            }
            ctx.AdvanceState(States.InstANDindx4);
        }

        public void Indx4(Context ctx)
        {
            FetchEA2LowIndirect(ctx);
            ctx.AdvanceState(States.InstANDindx5);
        }

        public void Indx5(Context ctx)
        {
            FetchEA2HighIndirect(ctx);
            if (ctx.SubStep == P2LASTSUBSTEP)
                ctx.Regs.CopyEA2ToEA();
            ctx.AdvanceState(States.InstANDindx6);
        }

        public void Indx6(Context ctx)
        {
            LoadTempFromEffAddr(ctx);
            if (ctx.SubStep == P2LASTSUBSTEP)
                AndAWithTemp(ctx);
            ctx.AdvanceState(States.Fetch);
        }

        // [0x31] AND (indirect),Y
        public void Indy2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstANDindy3);
        }

        public void Indy3(Context ctx)
        {
            FetchEA2LowIndirect(ctx);
            ctx.AdvanceState(States.InstANDindy4);
        }

        public void Indy4(Context ctx)
        {
            FetchEA2HighIndirect(ctx);
            if (ctx.SubStep == P2LASTSUBSTEP)
                ctx.Regs.CopyEA2ToEA();
            ctx.AdvanceState(States.InstANDindy5);
        }

        public void Indy5(Context ctx)
        {
            var nextState = States.Fetch;
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                var beforePage = ctx.Regs.EA.Msb().Copy();
                ctx.Regs.IncEAWithY();
                var afterPage = ctx.Regs.EA.Msb().Copy();
                ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
                ctx.Pins.AddrBusPins = ctx.Regs.EA;
                ctx.Pins.RWB = READ;
            }
            else if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = ctx.Pins.DataBusPins;
                ctx.Regs.Temp.UpdateValue(data);
                if (!ctx.CrossedPageBoundary)
                {
                    AndAWithTemp(ctx);
                }
                else
                {
                    nextState = States.InstANDindy6;
                }
            }
            ctx.AdvanceState(nextState);
        }

        public void Indy6(Context ctx)
        {
            LoadTempFromEffAddr(ctx);
            if (ctx.SubStep == P2LASTSUBSTEP)
                AndAWithTemp(ctx);
            ctx.AdvanceState(States.Fetch);
        }

        // [0x32] AND (indirect)
        public void Ind2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstANDind3);
        }

        public void Ind3(Context ctx)
        {
            FetchEA2LowIndirect(ctx);
            ctx.AdvanceState(States.InstANDind4);
        }

        public void Ind4(Context ctx)
        {
            FetchEA2HighIndirect(ctx);
            if (ctx.SubStep == P2LASTSUBSTEP)
                ctx.Regs.CopyEA2ToEA();
            ctx.AdvanceState(States.InstANDind5);
        }

        public void Ind5(Context ctx)
        {
            LoadTempFromEffAddr(ctx);
            if (ctx.SubStep == P2LASTSUBSTEP)
                AndAWithTemp(ctx);
            ctx.AdvanceState(States.Fetch);
        }
    }
}
