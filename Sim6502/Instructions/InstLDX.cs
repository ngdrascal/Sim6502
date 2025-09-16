namespace Sim6502.Instructions
{
    public class InstLDX : InstBase
    {
        public InstLDX(IT2Registry t2Registry, IStateRegistry stateRegistry)
            : base(t2Registry, stateRegistry)
        {
        }

        protected override void RegisterT2State(IT2Registry registry)
        {
            registry.Map(OpCodes.LDXimm, States.InstLDXimm2);
            registry.Map(OpCodes.LDXzpg, States.InstLDXzpg2);
            registry.Map(OpCodes.LDXzpgy, States.InstLDXzpgy2);
            registry.Map(OpCodes.LDXabs, States.InstLDXabs2);
            registry.Map(OpCodes.LDXabsy, States.InstLDXabsy2);
        }

        protected override void RegisterStates(IStateRegistry registry)
        {
            registry.Map(States.InstLDXimm2, ctx => Imm2(ctx));
            registry.Map(States.InstLDXzpg2, ctx => Zpg2(ctx));
            registry.Map(States.InstLDXzpg3, ctx => Zpg3(ctx));
            registry.Map(States.InstLDXzpgy2, ctx => Zpgy2(ctx));
            registry.Map(States.InstLDXzpgy3, ctx => Zpgy3(ctx));
            registry.Map(States.InstLDXzpgy4, ctx => Zpgy4(ctx));
            registry.Map(States.InstLDXabs2, ctx => Abs2(ctx));
            registry.Map(States.InstLDXabs3, ctx => Abs3(ctx));
            registry.Map(States.InstLDXabs4, ctx => Abs4(ctx));
            registry.Map(States.InstLDXabsy2, ctx => Absy2(ctx));
            registry.Map(States.InstLDXabsy3, ctx => Absy3(ctx));
            registry.Map(States.InstLDXabsy4, ctx => Absy4(ctx));
            registry.Map(States.InstLDXabsy5, ctx => Absy5(ctx));
        }

        private void LoadXFromEffAddr(Context ctx)
        {
            if (ctx.GetSubStep() == P1Middlestep)
            {
                ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
                ctx.Pins.SetRWB(Read);
            }
            else if (ctx.GetSubStep() == P2Lastsubstep)
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

        // [0xA2] LDX immediate
        public void Imm2(Context ctx)
        {
            Imm2SetAddrBus(ctx);
            if (ctx.GetSubStep() == P2Lastsubstep)
            {
                var data = ctx.Pins.GetDataBusPins();
                ctx.Regs.SetXUpdateFlags(data);
                ctx.DbgOperand1 = data;
            }
            ctx.AdvanceState(States.Fetch);
        }

        // [0xA6] LDX zeropage
        public void Zpg2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstLDXzpg3);
        }
        public void Zpg3(Context ctx)
        {
            LoadXFromEffAddr(ctx);
            ctx.AdvanceState(States.Fetch);
        }

        // [0xB6] LDX zeropage,Y
        public void Zpgy2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstLDXzpgy3);
        }
        public void Zpgy3(Context ctx)
        {
            if (ctx.GetSubStep() == P2Lastsubstep)
                ctx.Regs.IncEALWithY();
            ctx.AdvanceState(States.InstLDXzpgy4);
        }
        public void Zpgy4(Context ctx)
        {
            LoadXFromEffAddr(ctx);
            ctx.AdvanceState(States.Fetch);
        }

        // [0xAE] LDX absolute
        public void Abs2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstLDXabs3);
        }
        public void Abs3(Context ctx)
        {
            FetchEffAddrHigh(ctx);
            ctx.AdvanceState(States.InstLDXabs4);
        }
        public void Abs4(Context ctx)
        {
            LoadXFromEffAddr(ctx);
            ctx.AdvanceState(States.Fetch);
        }

        // [0xBE] LDX absolute,Y
        public void Absy2(Context ctx)
        {
            FetchEffAddrLow(ctx);
            ctx.AdvanceState(States.InstLDXabsy3);
        }
        public void Absy3(Context ctx)
        {
            FetchEffAddrHigh(ctx);
            ctx.AdvanceState(States.InstLDXabsy4);
        }
        public void Absy4(Context ctx)
        {
            var nextState = States.Fetch;
            if (ctx.GetSubStep() == P1Middlestep)
            {
                var beforePage = ctx.Regs.EA.Msb().Copy();
                ctx.Regs.IncEAWithY();
                var afterPage = ctx.Regs.EA.Msb().Copy();
                ctx.CrossedPageBoundary = !afterPage.Equals(beforePage);
                ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
                ctx.Pins.SetRWB(Read);
            }
            else if (ctx.GetSubStep() == P2Lastsubstep)
            {
                var data = ctx.Pins.GetDataBusPins();
                if (!ctx.CrossedPageBoundary)
                    ctx.Regs.SetXUpdateFlags(data);
                else
                    nextState = States.InstLDXabsy5;
            }
            ctx.AdvanceState(nextState);
        }
        public void Absy5(Context ctx)
        {
            LoadXFromEffAddr(ctx);
            ctx.AdvanceState(States.Fetch);
        }
    }
}
