using System;
using us.retrocpu.shared;
using us.retrocpu.w65c02s;

namespace us.retrocpu.w65c02s.instructions
{
    // Branch instructions for 65C02S
    public class InstBranch : InstBase
    {
        public InstBranch(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry) : base(t2Registry, stateRegistry) { }

        protected override void RegisterT2State(T2RegistryIntf registry)
        {
            registry.Map(OpCodes.BRArel, States.InstBRArel2);
            registry.Map(OpCodes.BCCrel, States.InstBCCrel2);
            registry.Map(OpCodes.BCSrel, States.InstBCSrel2);
            registry.Map(OpCodes.BEQrel, States.InstBEQrel2);
            registry.Map(OpCodes.BMIrel, States.InstBMIrel2);
            registry.Map(OpCodes.BNErel, States.InstBNErel2);
            registry.Map(OpCodes.BPLrel, States.InstBPLrel2);
            registry.Map(OpCodes.BVCrel, States.InstBVCrel2);
            registry.Map(OpCodes.BVSrel, States.InstBVSrel2);
        }

        protected override void RegisterStates(StateRegistryIntf stateRegistry)
        {
            stateRegistry.Map(States.InstBRArel2, ctx => BraRel2(ctx));
            stateRegistry.Map(States.InstBRArel3, ctx => BraRel3(ctx));
            stateRegistry.Map(States.InstBRArel4, ctx => BraRel4(ctx));

            stateRegistry.Map(States.InstBCCrel2, ctx => BccRel2(ctx));
            stateRegistry.Map(States.InstBCCrel3, ctx => BccRel3(ctx));
            stateRegistry.Map(States.InstBCCrel4, ctx => BccRel4(ctx));

            stateRegistry.Map(States.InstBCSrel2, ctx => BcsRel2(ctx));
            stateRegistry.Map(States.InstBCSrel3, ctx => BcsRel3(ctx));
            stateRegistry.Map(States.InstBCSrel4, ctx => BcsRel4(ctx));

            stateRegistry.Map(States.InstBEQrel2, ctx => BeqRel2(ctx));
            stateRegistry.Map(States.InstBEQrel3, ctx => BeqRel3(ctx));
            stateRegistry.Map(States.InstBEQrel4, ctx => BeqRel4(ctx));

            stateRegistry.Map(States.InstBMIrel2, ctx => BmiRel2(ctx));
            stateRegistry.Map(States.InstBMIrel3, ctx => BmiRel3(ctx));
            stateRegistry.Map(States.InstBMIrel4, ctx => BmiRel4(ctx));

            stateRegistry.Map(States.InstBNErel2, ctx => BneRel2(ctx));
            stateRegistry.Map(States.InstBNErel3, ctx => BneRel3(ctx));
            stateRegistry.Map(States.InstBNErel4, ctx => BneRel4(ctx));

            stateRegistry.Map(States.InstBPLrel2, ctx => BplRel2(ctx));
            stateRegistry.Map(States.InstBPLrel3, ctx => BplRel3(ctx));
            stateRegistry.Map(States.InstBPLrel4, ctx => BplRel4(ctx));

            stateRegistry.Map(States.InstBVCrel2, ctx => BvcRel2(ctx));
            stateRegistry.Map(States.InstBVCrel3, ctx => BvcRel3(ctx));
            stateRegistry.Map(States.InstBVCrel4, ctx => BvcRel4(ctx));

            stateRegistry.Map(States.InstBVSrel2, ctx => BvsRel2(ctx));
            stateRegistry.Map(States.InstBVSrel3, ctx => BvsRel3(ctx));
            stateRegistry.Map(States.InstBVSrel4, ctx => BvsRel4(ctx));
        }

        private void BranchCleared2(Context ctx, BitFlag flag, States nextState)
        {
            LoadTempFromPC(ctx);
            if (flag.IsCleared())
                ctx.AdvanceState(nextState);
            else
                ctx.AdvanceState(States.Fetch);
        }

        private void BranchSet2(Context ctx, BitFlag flag, States nextState)
        {
            LoadTempFromPC(ctx);
            if (flag.IsSet())
                ctx.AdvanceState(nextState);
            else
                ctx.AdvanceState(States.Fetch);
        }

        private void Branch3(Context ctx, States nextState)
        {
            if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var regs = ctx.Regs;
                var beforePage = regs.PC.Msb().Copy();
                regs.PC.AddSigned(regs.Temp);
                var afterPage = regs.PC.Msb().Copy();
                if (afterPage.Equals(beforePage))
                {
                    ctx.Pins.AddrBusPins = ctx.Regs.PC;
                    ctx.AdvanceState(States.Fetch);
                }
                else
                {
                    ctx.AdvanceState(nextState);
                }
            }
        }

        public void Branch4(Context ctx)
        {
            if (ctx.SubStep == P2LASTSUBSTEP)
                ctx.Pins.AddrBusPins = ctx.Regs.PC;
            ctx.AdvanceState(States.Fetch);
        }

        // BRA - Branch Always
        public void BraRel2(Context ctx)
        {
            LoadTempFromPC(ctx);
            ctx.AdvanceState(States.InstBRArel3);
        }

        public void BraRel3(Context ctx)
        {
            Branch3(ctx, States.InstBRArel4);
        }

        public void BraRel4(Context ctx)
        {
            Branch4(ctx);
        }

        // BCC - Branch on Carry Clear
        public void BccRel2(Context ctx)
        {
            BranchCleared2(ctx, ctx.Regs.P.Carry, States.InstBCCrel3);
        }

        public void BccRel3(Context ctx)
        {
            Branch3(ctx, States.InstBCCrel4);
        }

        public void BccRel4(Context ctx)
        {
            Branch4(ctx);
        }

        // BCS - Branch on Carry Set
        public void BcsRel2(Context ctx)
        {
            BranchSet2(ctx, ctx.Regs.P.Carry, States.InstBCSrel3);
        }

        public void BcsRel3(Context ctx)
        {
            Branch3(ctx, States.InstBCSrel4);
        }

        public void BcsRel4(Context ctx)
        {
            Branch4(ctx);
        }

        // BEQ - Branch on Result Zero
        public void BeqRel2(Context ctx)
        {
            BranchSet2(ctx, ctx.Regs.P.Zero, States.InstBEQrel3);
        }

        public void BeqRel3(Context ctx)
        {
            Branch3(ctx, States.InstBEQrel4);
        }

        public void BeqRel4(Context ctx)
        {
            Branch4(ctx);
        }

        // BMI - Branch on Result Minus
        public void BmiRel2(Context ctx)
        {
            BranchSet2(ctx, ctx.Regs.P.Negative, States.InstBMIrel3);
        }

        public void BmiRel3(Context ctx)
        {
            Branch3(ctx, States.InstBMIrel4);
        }

        public void BmiRel4(Context ctx)
        {
            Branch4(ctx);
        }

        // BNE - Branch on Result Not Zero
        public void BneRel2(Context ctx)
        {
            BranchCleared2(ctx, ctx.Regs.P.Zero, States.InstBNErel3);
        }

        public void BneRel3(Context ctx)
        {
            Branch3(ctx, States.InstBNErel4);
        }

        public void BneRel4(Context ctx)
        {
            Branch4(ctx);
        }

        // BPL - Branch on Result Plus
        public void BplRel2(Context ctx)
        {
            BranchCleared2(ctx, ctx.Regs.P.Negative, States.InstBPLrel3);
        }

        public void BplRel3(Context ctx)
        {
            Branch3(ctx, States.InstBPLrel4);
        }

        public void BplRel4(Context ctx)
        {
            Branch4(ctx);
        }

        // BVC - Branch on Overflow Clear
        public void BvcRel2(Context ctx)
        {
            BranchCleared2(ctx, ctx.Regs.P.Overflow, States.InstBVCrel3);
        }

        public void BvcRel3(Context ctx)
        {
            Branch3(ctx, States.InstBVCrel4);
        }

        public void BvcRel4(Context ctx)
        {
            Branch4(ctx);
        }

        // BVS - Branch on Overflow Set
        public void BvsRel2(Context ctx)
        {
            BranchSet2(ctx, ctx.Regs.P.Overflow, States.InstBVSrel3);
        }

        public void BvsRel3(Context ctx)
        {
            Branch3(ctx, States.InstBVSrel4);
        }

        public void BvsRel4(Context ctx)
        {
            Branch4(ctx);
        }
    }
}
