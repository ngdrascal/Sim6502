using UInt16 = Sim6502.types.UInt16;

namespace Sim6502.Instructions;

public class Interrupts : InstBase
{
    public Interrupts(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.BRKimp, States.InstBRKimp2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.Interrupt1, Interrupt1);

        stateRegistry.Map(States.InstBRKimp2, BrkImp2);
        stateRegistry.Map(States.InstBRKimp3, BrkImp3);
        stateRegistry.Map(States.InstBRKimp4, BrkImp4);
        stateRegistry.Map(States.InstBRKimp5, BrkImp5);
        stateRegistry.Map(States.InstBRKimp6, BrkImp6);
        stateRegistry.Map(States.InstBRKimp7, BrkImp7);
    }

    private void LoadTempFromPcDontAdvancePc(Context ctx)
    {
        if (ctx.GetSubStep() == P1Middlestep)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.PC);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    // Interrupt (both NMI and IRQ)
    /////////////////////////////////////////////////////////////////////////////
    public void Interrupt1(Context ctx)
    {
        // NOTE: The BRK instruction, the non-maskable and maskable interrupts
        //       share some code (steps 2 - 7).  This state is a replacement for
        //       the FETCH state.  It differs by discarding the read op-code
        //       and NOT increment the program counter.  This differentiates the
        //       interrupts (NMI and IRQ) from the break instruction.

        // load operand into the temp reg., then ignore it, don't advance the PC
        LoadTempFromPcDontAdvancePc(ctx);

        ctx.AdvanceState(States.InstBRKimp2);
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
    public void BrkImp2(Context ctx)
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

    public void BrkImp3(Context ctx)
    {
        // push PCH
        PushOnStack(ctx, ctx.Regs.PC.Msb());

        ctx.AdvanceState(States.InstBRKimp4);
    }

    public void BrkImp4(Context ctx)
    {
        // push PCL
        PushOnStack(ctx, ctx.Regs.PC.Lsb());

        ctx.AdvanceState(States.InstBRKimp5);
    }

    public void BrkImp5(Context ctx)
    {
        // push the status register
        PushOnStack(ctx, ctx.Regs.P.GetFlags());

        if (ctx.GetSubStep() == P2Lastsubstep)
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

    public void BrkImp6(Context ctx)
    {
        // read 0xFFFE into the PCL
        if (ctx.GetSubStep() == P1Middlestep)
        {
            ctx.Pins.SetAddrBusMode(AddrBusMode.Output);
            ctx.Pins.SetDataBusMode(DataBusMode.Input);
            if (ctx.NmiFlag)
                ctx.Pins.SetAddrBusPins(new UInt16(0xFFFA));
            else
                ctx.Pins.SetAddrBusPins(new UInt16(0xFFFE));
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.PC.Lsb().UpdateValue(data);
        }

        ctx.AdvanceState(States.InstBRKimp7);
    }

    public void BrkImp7(Context ctx)
    {
        // read 0xFFFF into the PCH
        if (ctx.GetSubStep() == P1Middlestep)
        {
            if (ctx.NmiFlag)
                ctx.Pins.SetAddrBusPins(new UInt16(0xFFFB));
            else
                ctx.Pins.SetAddrBusPins(new UInt16(0xFFFF));
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2Lastsubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.PC.Msb().UpdateValue(data);
        }

        ctx.AdvanceState(States.Fetch);
    }
}
