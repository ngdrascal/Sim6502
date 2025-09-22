namespace Sim6502.Instructions
{
    public class InstLDX : InstBase, IInstruction
    {
        public IInstruction RegisterT2State(IT2Registry registry)
        {
            registry.Map(OpCodes.LDXimm, States.InstLDXimm2);
            registry.Map(OpCodes.LDXzpg, States.InstLDXzpg2);
            registry.Map(OpCodes.LDXzpgy, States.InstLDXzpgy2);
            registry.Map(OpCodes.LDXabs, States.InstLDXabs2);
            registry.Map(OpCodes.LDXabsy, States.InstLDXabsy2);

            return this;
        }

        public IInstruction RegisterStates(IStateRegistry registry)
        {
            registry.Map(States.InstLDXimm2, Imm2);
            registry.Map(States.InstLDXzpg2, Zpg2);
            registry.Map(States.InstLDXzpg3, Zpg3);
            registry.Map(States.InstLDXzpgy2, Zpgy2);
            registry.Map(States.InstLDXzpgy3, Zpgy3);
            registry.Map(States.InstLDXzpgy4, Zpgy4);
            registry.Map(States.InstLDXabs2, Abs2);
            registry.Map(States.InstLDXabs3, Abs3);
            registry.Map(States.InstLDXabs4, Abs4);
            registry.Map(States.InstLDXabsy2, Absy2);
            registry.Map(States.InstLDXabsy3, Absy3);
            registry.Map(States.InstLDXabsy4, Absy4);
            registry.Map(States.InstLDXabsy5, Absy5);

            return this;
        }

        private void LoadXFromEffAddr(Context ctx)
        {
            if (ctx.GetSubStep() == P1MiddleStep)
            {
                ctx.Pins.AddrBus = ctx.Regs.EA;
                ctx.Pins.SetRWB(Read);
            }
            else if (ctx.GetSubStep() == P2LastSubstep)
            {
                var x = ctx.Pins.GetDataBusPins();
                ctx.Regs.SetXUpdateFlags(x);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////
        // LDX - Load Index X with Memory
        // M -> X
        // N V B D I Z C
        // + - - - - + -
        //
        // addressing     assembler     opc   bytes  cycles
        // ------------------------------------------------
        // immediate      LDX #oper     A2      2      2  
        // zeropage       LDX oper      A6      2      3  
        // zeropage,Y     LDX oper,Y    B6      2      4  
        // absolute       LDX oper      AE      3      4  
        // absolute,Y     LDX oper,Y    BE      3      4* 
        ///////////////////////////////////////////////////////////////////////////////

        // -------------------------------------------------------------------------
        // [0xA2] LDX immediate
        // -------------------------------------------------------------------------
        private void Imm2(Context ctx)
        {
            Imm2SetAddrBus(ctx);
            if (ctx.GetSubStep() == P2LastSubstep)
            {
                var data = ctx.Pins.GetDataBusPins();
                ctx.Regs.SetXUpdateFlags(data);
                ctx.DbgOperand1 = data;
            }

            ctx.AdvanceState(States.Fetch);
        }

        // -------------------------------------------------------------------------
        // [0xA6] LDX zeropage
        // -------------------------------------------------------------------------
        private void Zpg2(Context ctx)
        {
            FetchEffAddrLow(ctx);

            ctx.AdvanceState(States.InstLDXzpg3);
        }

        private void Zpg3(Context ctx)
        {
            LoadXFromEffAddr(ctx);

            ctx.AdvanceState(States.Fetch);
        }

        // -------------------------------------------------------------------------
        // [0xB6] LDX zeropage,Y
        // -------------------------------------------------------------------------
        private void Zpgy2(Context ctx)
        {
            FetchEffAddrLow(ctx);

            ctx.AdvanceState(States.InstLDXzpgy3);
        }

        private void Zpgy3(Context ctx)
        {
            if (ctx.GetSubStep() == P2LastSubstep)
                ctx.Regs.IncEALWithY();

            ctx.AdvanceState(States.InstLDXzpgy4);
        }

        private void Zpgy4(Context ctx)
        {
            LoadXFromEffAddr(ctx);

            ctx.AdvanceState(States.Fetch);
        }

        // -------------------------------------------------------------------------
        // [0xAE] LDX absolute
        // -------------------------------------------------------------------------
        private void Abs2(Context ctx)
        {
            FetchEffAddrLow(ctx);

            ctx.AdvanceState(States.InstLDXabs3);
        }

        private void Abs3(Context ctx)
        {
            FetchEffAddrHigh(ctx);

            ctx.AdvanceState(States.InstLDXabs4);
        }

        private void Abs4(Context ctx)
        {
            LoadXFromEffAddr(ctx);

            ctx.AdvanceState(States.Fetch);
        }

        // -------------------------------------------------------------------------
        // [0xBE] LDX absolute,Y
        // -------------------------------------------------------------------------
        private void Absy2(Context ctx)
        {
            FetchEffAddrLow(ctx);

            ctx.AdvanceState(States.InstLDXabsy3);
        }

        private void Absy3(Context ctx)
        {
            FetchEffAddrHigh(ctx);

            ctx.AdvanceState(States.InstLDXabsy4);
        }

        private void Absy4(Context ctx)
        {
            var nextState = States.Fetch;
            if (ctx.GetSubStep() == P1MiddleStep)
            {
                var beforePage = ctx.Regs.EA.Msb().Copy();
                ctx.Regs.IncEAWithY();
                var afterPage = ctx.Regs.EA.Msb().Copy();
                ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
                ctx.Pins.AddrBus = ctx.Regs.EA;
                ctx.Pins.SetRWB(Read);
            }
            else if (ctx.GetSubStep() == P2LastSubstep)
            {
                var data = ctx.Pins.GetDataBusPins();
                if (!ctx.CrossedPageBoundary)
                    ctx.Regs.SetXUpdateFlags(data);
                else
                    nextState = States.InstLDXabsy5;
            }

            ctx.AdvanceState(nextState);
        }

        private void Absy5(Context ctx)
        {
            LoadXFromEffAddr(ctx);

            ctx.AdvanceState(States.Fetch);
        }
    }
}
