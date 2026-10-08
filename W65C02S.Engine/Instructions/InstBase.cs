using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine;

internal class InstBase
{
    // Logger stub (replace with your logging framework if needed)
    // private readonly Logger _logger;

    protected const byte Low = Constants.Low;
    protected const byte High = Constants.High;
    protected const byte Write = Low;
    protected const byte Read = High;
    protected static readonly byte P1MiddleStep = Constants.P1MiddleStep;
    protected static readonly byte P2MiddleStep = Constants.P2MiddleStep;
    protected static readonly byte P2LastSubstep = Constants.P2LastSubstep;

    protected InstBase()
    {
        // _logger = LoggerFactory.GetLogger("W65c02s.stack");
    }

    /////////////////////////////////////////////////////////////////////////////
    // common routines
    /////////////////////////////////////////////////////////////////////////////
    protected void Imm2SetAddrBus(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.PC;
            ctx.Regs.PC = ctx.Regs.PC.Inc();
            ctx.Pins.RWB = Read;
        }
    }

    protected void FetchEffAddrLow(Context ctx)
    {
        var pins = ctx.Pins;
        var regs = ctx.Regs;
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var addr = regs.PC.Copy();
            regs.PC = regs.PC.Inc();
            pins.AddrBus = addr;
            pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = pins.DataBus;
            regs.EA = regs.EA.WithLsb(data);
            regs.EA = regs.EA.WithMsb(0);
            ctx.DbgOperand1 = data;
            ctx.DbgOperand2 = new UInt8(0);
        }
    }

    protected void FetchEffAddrHigh(Context ctx)
    {
        var pins = ctx.Pins;
        var regs = ctx.Regs;
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var addr = regs.PC.Copy();
            regs.PC = regs.PC.Inc();
            pins.AddrBus = addr;
            pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = pins.DataBus;
            regs.EA = regs.EA.WithMsb(data);
            ctx.DbgOperand2 = data;
        }
    }

    protected void FetchEA2LowIndirect(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.EA;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.EA2 = ctx.Regs.EA2.WithLsb(data);
            ctx.Regs.EA2 = ctx.Regs.EA2.WithMsb(0);
        }
    }

    protected void FetchEA2HighIndirect(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var eaPlus1 = ctx.Regs.EA.Copy();
            eaPlus1 = eaPlus1.Inc();
            ctx.Pins.AddrBus = eaPlus1;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.EA2 = ctx.Regs.EA2.WithMsb(data);
        }
    }

    protected void ReadAndDiscard(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var addr = ctx.Regs.PC;
            ctx.Pins.AddrBus = addr;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            _ = ctx.Pins.DataBus;
        }
    }

    protected void LoadTempFromEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.EA;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp = data;
        }
    }

    protected void StoreTempToEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.EA;
            ctx.Pins.RWB = Write;
            ctx.Pins.DataBusMode = DataBusMode.Output;
        }
        else if (ctx.GetSubStep() == P2MiddleStep)
        {
            ctx.Pins.DataBus = ctx.Regs.Temp;
        }
    }

    protected void LoadTempFromPC(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.PC;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp = data;
            ctx.DbgOperand1 = data;
            ctx.Regs.PC = ctx.Regs.PC.Inc();
        }
    }

    // NOTE: The BRK instruction, the non-maskable and maskable interrupts
    //       share some code (steps 2 - 7).  This state is a replacement for
    //       the FETCH state.  It differs by discarding the read op-code
    //       and NOT increment the program counter.  This differentiates the
    //       interrupts (NMI and IRQ) from the break instruction.
    protected void LoadTempFromPcDontAdvancePc(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.PC;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.Temp = data;
        }
    }

    protected void PrepareStackWrite(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
            ctx.Pins.AddrBus = sp;
            ctx.Pins.RWB = Write;
        }
        if (ctx.GetSubStep() == P2MiddleStep)
        {
            ctx.Pins.DataBusMode = DataBusMode.Output;
        }
    }

    protected void PrepareStackRead(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
            ctx.Pins.AddrBus = sp;
            ctx.Pins.DataBusMode = DataBusMode.Input;
            ctx.Pins.RWB = Read;
        }
    }

    protected void PushOnStack(Context ctx, UInt8 value)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
            ctx.Pins.AddrBus = sp;
            ctx.Pins.DataBusMode = DataBusMode.Output;
            ctx.Pins.RWB = Write;
        }
        else if (ctx.GetSubStep() == P2MiddleStep)
        {
            ctx.Pins.DataBus = value;
            // var addr = ctx.Regs.S;
            // _logger.Debug($"JSR|BRK: push {value.ToInt():X2} to 0x1{addr.ToInt():X2}");
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            ctx.Regs.S = ctx.Regs.S.Dec();
        }
    }

    // returns true, with the pulled byte in value, on the substep that latches it
    protected bool PullFromStack(Context ctx, out UInt8 value)
    {
        value = default;
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Regs.S = ctx.Regs.S.Inc();
            var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
            ctx.Pins.AddrBus = sp;
            ctx.Pins.DataBusMode = DataBusMode.Input;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            value = data;
            // var addr = ctx.Regs.S;
            // _logger.Debug($"RTS|RTI: pull {value.ToInt():X2} from 0x1{addr.ToInt():X2}");

            return true;
        }

        return false;
    }

    // Branch bus activity. The W65C02S datasheet has no per-cycle table; this follows the
    // SingleStepTests 65x02 wdc65c02 vectors (and TomHarte/CLK, which generated them):
    //   taken cycle:      dummy read of PC, the next instruction's address
    //   page cross cycle: Bxx/BRA dummy read of old PCH : new PCL;
    //                     BBR/BBS dummy read of the next instruction's address again
    // MAME (ow65c02.lst bbr_zpb/bbs_zpb) reads old PCH : new PCL for BBR/BBS too.

    // branch taken: add the signed offset in Temp to PCL; a page change costs one more cycle
    protected void Branch3(Context ctx, States nextState)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.PC;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var regs = ctx.Regs;
            var target = regs.PC.AddSigned(regs.Temp);
            if (target.Msb().Equals(regs.PC.Msb()))
            {
                regs.PC = target;
                ctx.AdvanceState(States.Fetch);
            }
            else
            {
                // PCH is fixed in the page cross cycle
                regs.PC = regs.PC.WithLsb(target.Lsb());
                ctx.AdvanceState(nextState);
            }
        }
    }

    // page cross: PC holds old PCH : new PCL; carry or borrow into PCH
    protected void Branch4(Context ctx, bool rereadNext)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var pc = ctx.Regs.PC;
            ctx.Pins.AddrBus = rereadNext
                ? pc.WithLsb(new UInt8(pc.Lsb().ToInt() - ctx.Regs.Temp.ToInt()))
                : pc;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var pageDelta = ctx.Regs.Temp.IsBitSet(7) ? -0x100 : 0x100;
            ctx.Regs.PC = new UInt16(ctx.Regs.PC.ToInt() + pageDelta);
        }

        ctx.AdvanceState(States.Fetch);
    }
}
