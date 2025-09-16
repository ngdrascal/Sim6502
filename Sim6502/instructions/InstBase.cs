using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502.Instructions;

public class InstBase
{
    // Logger stub (replace with your logging framework if needed)
    // private readonly Logger _logger;

    protected static readonly byte LOW = Constants.LOW;
    protected static readonly byte HIGH = Constants.HIGH;
    protected static readonly byte WRITE = LOW;
    protected static readonly byte READ = HIGH;
    protected static readonly byte P1MIDDLESTEP = Constants.P1MIDDLESTEP;
    protected static readonly byte P2MIDDLESTEP = Constants.P2MIDDLESTEP;
    protected static readonly byte P2LASTSUBSTEP = Constants.P2LASTSUBSTEP;

    protected InstBase(IT2Registry it2Registry, IStateRegistry stateRegistry)
    {
        RegisterT2State(it2Registry);
        RegisterStates(stateRegistry);
        // _logger = LoggerFactory.GetLogger("W65c02s.stack");
    }

    protected virtual void RegisterT2State(IT2Registry registry) { }
    protected virtual void RegisterStates(IStateRegistry registry) { }

    /////////////////////////////////////////////////////////////////////////////
    // common routines
    /////////////////////////////////////////////////////////////////////////////
    protected void Imm2SetAddrBus(Context ctx)
    {
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.PC);
            ctx.Regs.PC.Inc();
            ctx.Pins.SetRWB(READ);
        }
    }

    protected void FetchEffAddrLow(Context ctx)
    {
        var pins = ctx.Pins;
        var regs = ctx.Regs;
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            var addr = regs.PC.Copy();
            regs.PC.Inc();
            pins.SetAddrBusPins(addr);
            pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == P2LASTSUBSTEP)
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
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            var addr = regs.PC.Copy();
            regs.PC.Inc();
            pins.SetAddrBusPins(addr);
            pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = pins.GetDataBusPins();
            regs.EA.Msb().UpdateValue(data);
            ctx.DbgOperand2 = data;
        }
    }

    protected void FetchEA2LowIndirect(Context ctx)
    {
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.EA2.Lsb().UpdateValue(data);
            ctx.Regs.EA2.Msb().Zero();
        }
    }

    protected void FetchEA2HighIndirect(Context ctx)
    {
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            var eaPlus1 = ctx.Regs.EA.Copy();
            eaPlus1.Inc();
            ctx.Pins.SetAddrBusPins(eaPlus1);
            ctx.Pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.EA2.Msb().UpdateValue(data);
        }
    }

    protected void ReadAndDiscard(Context ctx)
    {
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            var addr = ctx.Regs.PC;
            ctx.Pins.SetAddrBusPins(addr);
            ctx.Pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            _ = ctx.Pins.GetDataBusPins();
        }
    }

    protected void LoadTempFromEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
        }
    }

    protected void StoreTempToEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(WRITE);
            ctx.Pins.SetDataBusMode(DataBusMode.Output);
        }
        else if (ctx.GetSubStep() == P2MIDDLESTEP)
        {
            ctx.Pins.SetDataBusPins(ctx.Regs.Temp);
        }
    }

    protected void LoadTempFromPC(Context ctx)
    {
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.PC);
            ctx.Pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.Temp.UpdateValue(data);
            ctx.DbgOperand1 = data;
            ctx.Regs.PC.Inc();
        }
    }

    protected void PrepareStackWrite(Context ctx)
    {
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
            ctx.Pins.SetAddrBusPins(sp);
            ctx.Pins.SetRWB(WRITE);
        }
        if (ctx.GetSubStep() == P2MIDDLESTEP)
        {
            ctx.Pins.SetDataBusMode(DataBusMode.Output);
        }
    }

    protected void PrepareStackRead(Context ctx)
    {
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
            ctx.Pins.SetAddrBusPins(sp);
            ctx.Pins.SetDataBusMode(DataBusMode.Input);
            ctx.Pins.SetRWB(READ);
        }
    }

    protected void PushOnStack(Context ctx, UInt8 value)
    {
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
            ctx.Pins.SetAddrBusPins(sp);
            ctx.Pins.SetDataBusMode(DataBusMode.Output);
            ctx.Pins.SetRWB(WRITE);
        }
        else if (ctx.GetSubStep() == P2MIDDLESTEP)
        {
            ctx.Pins.SetDataBusPins(value);
            var addr = ctx.Regs.S;
            // _logger.Debug($"JSR|BRK: push {value.ToInt():X2} to 0x1{addr.ToInt():X2}");
        }
        else if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            _ = ctx.Regs.S.Dec();
        }
    }

    protected void PullFromStack(Context ctx, UInt8 value)
    {
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            ctx.Regs.S.Inc();
            var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
            ctx.Pins.SetAddrBusPins(sp);
            ctx.Pins.SetDataBusMode(DataBusMode.Input);
            ctx.Pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == P2LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            value.UpdateValue(data);
            var addr = ctx.Regs.S;
            // _logger.Debug($"RTS|RTI: pull {value.ToInt():X2} from 0x1{addr.ToInt():X2}");
        }
    }
}
