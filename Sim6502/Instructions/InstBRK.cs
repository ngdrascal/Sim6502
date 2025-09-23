using UInt16 = Sim6502.types.UInt16;

namespace Sim6502.Instructions;

public class InstBRK : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.BRKimp, States.InstBRKimp2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstBRKimp2, BrkImp2);
        stateRegistry.Map(States.InstBRKimp3, BrkImp3);
        stateRegistry.Map(States.InstBRKimp4, BrkImp4);
        stateRegistry.Map(States.InstBRKimp5, BrkImp5);
        stateRegistry.Map(States.InstBRKimp6, BrkImp6);
        stateRegistry.Map(States.InstBRKimp7, BrkImp7);

        return this;
    }

    /////////////////////////////////////////////////////////////////////////////
    // BRK - Break command
    // push PC+2, push SR
    // N V B D I Z C
    // - - 1 0 1 - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        BRK           00      1      7
    /////////////////////////////////////////////////////////////////////////////
    private void BrkImp2(Context ctx)
    {
        // if handling a hardware interrupt
        if (ctx.NmiFlag || ctx.IrqFlag)
            // load operand into the temp reg., then ignore it, don't advance the PC
            LoadTempFromPcDontAdvancePc(ctx);
        else
            // load operand into the temp reg., then ignore it
            LoadTempFromPC(ctx);

        ctx.AdvanceState(States.InstBRKimp3);
    }

    private void BrkImp3(Context ctx)
    {
        // push PCH
        PushOnStack(ctx, ctx.Regs.PC.Msb());

        ctx.AdvanceState(States.InstBRKimp4);
    }

    private void BrkImp4(Context ctx)
    {
        // push PCL
        PushOnStack(ctx, ctx.Regs.PC.Lsb());

        ctx.AdvanceState(States.InstBRKimp5);
    }

    private void BrkImp5(Context ctx)
    {
        // push the status register
        PushOnStack(ctx, ctx.Regs.P.GetFlags());

        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var p = ctx.Regs.P;
            if (ctx.NmiFlag || ctx.IrqFlag)
                p.ClearBreak();
            else
                p.SetBreak();
            p.ClearDecimal();
            p.SetIRQDisabled();
        }

        ctx.AdvanceState(States.InstBRKimp6);
    }

    private void BrkImp6(Context ctx)
    {
        // read 0xFFFE into the PCL
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.AddrBusMode = AddrBusMode.Output;
            ctx.Pins.DataBusMode = DataBusMode.Input;
            if (ctx.NmiFlag)
                ctx.Pins.AddrBus = new UInt16(0xFFFA);
            else
                ctx.Pins.AddrBus = new UInt16(0xFFFE);
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.PC.Lsb().UpdateValue(data);
        }

        ctx.AdvanceState(States.InstBRKimp7);
    }

    private void BrkImp7(Context ctx)
    {
        // read 0xFFFF into the PCH
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            if (ctx.NmiFlag)
                ctx.Pins.AddrBus = new UInt16(0xFFFB);
            else
                ctx.Pins.AddrBus = new UInt16(0xFFFF);
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.PC.Msb().UpdateValue(data);
        }

        ctx.AdvanceState(States.Fetch);
    }
}
