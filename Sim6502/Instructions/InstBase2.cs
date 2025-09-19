using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502.Instructions;

public class InstBase2
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

    protected InstBase2()
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
            ctx.Pins.SetAddrBusPins(ctx.Regs.PC);
            ctx.Regs.PC.Inc();
            ctx.Pins.SetRWB(Read);
        }
    }

    protected void FetchEffAddrLow(Context ctx)
    {
        var pins = ctx.Pins;
        var regs = ctx.Regs;
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var addr = regs.PC.Copy();
            regs.PC.Inc();
            pins.SetAddrBusPins(addr);
            pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = pins.GetDataBusPins();
            regs.EA.Lsb().UpdateValue(data);
            regs.EA.Msb().Zero();
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
            regs.PC.Inc();
            pins.SetAddrBusPins(addr);
            pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = pins.GetDataBusPins();
            regs.EA.Msb().UpdateValue(data);
            ctx.DbgOperand2 = data;
        }
    }

    protected void FetchEA2LowIndirect(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.EA2.Lsb().UpdateValue(data);
            ctx.Regs.EA2.Msb().Zero();
        }
    }

    protected void FetchEA2HighIndirect(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var eaPlus1 = ctx.Regs.EA.Copy();
            eaPlus1.Inc();
            ctx.Pins.SetAddrBusPins(eaPlus1);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.EA2.Msb().UpdateValue(data);
        }
    }

    protected void ReadAndDiscard(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var addr = ctx.Regs.PC;
            ctx.Pins.SetAddrBusPins(addr);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            _ = ctx.Pins.GetDataBusPins();
        }
    }

    protected void LoadTempFromEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
        }
    }

    protected void StoreTempToEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Write);
            ctx.Pins.SetDataBusMode(DataBusMode.Output);
        }
        else if (ctx.GetSubStep() == P2MiddleStep)
        {
            ctx.Pins.SetDataBusPins(ctx.Regs.Temp);
        }
    }

    protected void LoadTempFromPC(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.PC);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            ctx.DbgOperand1 = data;
            ctx.Regs.PC.Inc();
        }
    }

    protected void PrepareStackWrite(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
            ctx.Pins.SetAddrBusPins(sp);
            ctx.Pins.SetRWB(Write);
        }
        if (ctx.GetSubStep() == P2MiddleStep)
        {
            ctx.Pins.SetDataBusMode(DataBusMode.Output);
        }
    }

    protected void PrepareStackRead(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
            ctx.Pins.SetAddrBusPins(sp);
            ctx.Pins.SetDataBusMode(DataBusMode.Input);
            ctx.Pins.SetRWB(Read);
        }
    }

    protected void PushOnStack(Context ctx, UInt8 value)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
            ctx.Pins.SetAddrBusPins(sp);
            ctx.Pins.SetDataBusMode(DataBusMode.Output);
            ctx.Pins.SetRWB(Write);
        }
        else if (ctx.GetSubStep() == P2MiddleStep)
        {
            ctx.Pins.SetDataBusPins(value);
            // var addr = ctx.Regs.S;
            // _logger.Debug($"JSR|BRK: push {value.ToInt():X2} to 0x1{addr.ToInt():X2}");
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            _ = ctx.Regs.S.Dec();
        }
    }

    protected void PullFromStack(Context ctx, UInt8 value)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Regs.S.Inc();
            var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
            ctx.Pins.SetAddrBusPins(sp);
            ctx.Pins.SetDataBusMode(DataBusMode.Input);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.GetDataBusPins();
            value.UpdateValue(data);
            // var addr = ctx.Regs.S;
            // _logger.Debug($"RTS|RTI: pull {value.ToInt():X2} from 0x1{addr.ToInt():X2}");
        }
    }
}
