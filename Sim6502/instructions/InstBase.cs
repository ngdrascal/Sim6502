using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;

namespace Us.Retrocpu.W65c02s.Instructions
{
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

        protected InstBase(T2RegistryIntf t2Registry, StateRegistryIntf stateRegistry)
        {
            RegisterT2State(t2Registry);
            RegisterStates(stateRegistry);
            // _logger = LoggerFactory.GetLogger("W65c02s.stack");
        }

        protected virtual void RegisterT2State(T2RegistryIntf registry) { }
        protected virtual void RegisterStates(StateRegistryIntf registry) { }

        /////////////////////////////////////////////////////////////////////////////
        // common routines
        /////////////////////////////////////////////////////////////////////////////
        protected void Imm2SetAddrBus(Context ctx)
        {
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                ctx.Pins.AddrBusPins = ctx.Regs.PC;
                ctx.Regs.PC.Inc();
                ctx.Pins.RWB = READ;
            }
        }

        protected void FetchEffAddrLow(Context ctx)
        {
            var pins = ctx.Pins;
            var regs = ctx.Regs;
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                var addr = regs.PC.Copy();
                regs.PC.Inc();
                pins.AddrBusPins = addr;
                pins.RWB = READ;
            }
            else if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = pins.DataBusPins;
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
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                var addr = regs.PC.Copy();
                regs.PC.Inc();
                pins.AddrBusPins = addr;
                pins.RWB = READ;
            }
            else if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = pins.DataBusPins;
                regs.EA.Msb().UpdateValue(data);
                ctx.DbgOperand2 = data;
            }
        }

        protected void FetchEA2LowIndirect(Context ctx)
        {
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                ctx.Pins.AddrBusPins = ctx.Regs.EA;
                ctx.Pins.RWB = READ;
            }
            else if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = ctx.Pins.DataBusPins;
                ctx.Regs.EA2.Lsb().UpdateValue(data);
                ctx.Regs.EA2.Msb().Zero();
            }
        }

        protected void FetchEA2HighIndirect(Context ctx)
        {
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                var eaPlus1 = ctx.Regs.EA.Copy();
                eaPlus1.Inc();
                ctx.Pins.AddrBusPins = eaPlus1;
                ctx.Pins.RWB = READ;
            }
            else if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = ctx.Pins.DataBusPins;
                ctx.Regs.EA2.Msb().UpdateValue(data);
            }
        }

        protected void ReadAndDiscard(Context ctx)
        {
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                var addr = ctx.Regs.PC;
                ctx.Pins.AddrBusPins = addr;
                ctx.Pins.RWB = READ;
            }
            else if (ctx.SubStep == P2LASTSUBSTEP)
            {
                ctx.Pins.DataBusPins;
            }
        }

        protected void LoadTempFromEffAddr(Context ctx)
        {
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                ctx.Pins.AddrBusPins = ctx.Regs.EA;
                ctx.Pins.RWB = READ;
            }
            else if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = ctx.Pins.DataBusPins;
                ctx.Regs.Temp.UpdateValue(data);
            }
        }

        protected void StoreTempToEffAddr(Context ctx)
        {
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                ctx.Pins.AddrBusPins = ctx.Regs.EA;
                ctx.Pins.RWB = WRITE;
                ctx.Pins.DataBusMode = DataBusMode.Output;
            }
            else if (ctx.SubStep == P2MIDDLESTEP)
            {
                ctx.Pins.DataBusPins = ctx.Regs.Temp;
            }
        }

        protected void LoadTempFromPC(Context ctx)
        {
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                ctx.Pins.AddrBusPins = ctx.Regs.PC;
                ctx.Pins.RWB = READ;
            }
            else if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = ctx.Pins.DataBusPins;
                ctx.Regs.Temp.UpdateValue(data);
                ctx.DbgOperand1 = data;
                ctx.Regs.PC.Inc();
            }
        }

        protected void PrepareStackWrite(Context ctx)
        {
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
                ctx.Pins.AddrBusPins = sp;
                ctx.Pins.RWB = WRITE;
            }
            if (ctx.SubStep == P2MIDDLESTEP)
            {
                ctx.Pins.DataBusMode = DataBusMode.Output;
            }
        }

        protected void PrepareStackRead(Context ctx)
        {
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
                ctx.Pins.AddrBusPins = sp;
                ctx.Pins.DataBusMode = DataBusMode.Input;
                ctx.Pins.RWB = READ;
            }
        }

        protected void PushOnStack(Context ctx, UInt8 value)
        {
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
                ctx.Pins.AddrBusPins = sp;
                ctx.Pins.DataBusMode = DataBusMode.Output;
                ctx.Pins.RWB = WRITE;
            }
            else if (ctx.SubStep == P2MIDDLESTEP)
            {
                ctx.Pins.DataBusPins = value;
                var addr = ctx.Regs.S;
                // _logger.Debug($"JSR|BRK: push {value.ToInt():X2} to 0x1{addr.ToInt():X2}");
            }
            else if (ctx.SubStep == P2LASTSUBSTEP)
            {
                ctx.Regs.S.Dec();
            }
        }

        protected void PullFromStack(Context ctx, UInt8 value)
        {
            if (ctx.SubStep == P1MIDDLESTEP)
            {
                ctx.Regs.S.Inc();
                var sp = new UInt16(0x100).AddUnsigned(ctx.Regs.S);
                ctx.Pins.AddrBusPins = sp;
                ctx.Pins.DataBusMode = DataBusMode.Input;
                ctx.Pins.RWB = READ;
            }
            else if (ctx.SubStep == P2LASTSUBSTEP)
            {
                var data = ctx.Pins.DataBusPins;
                value.UpdateValue(data);
                var addr = ctx.Regs.S;
                // _logger.Debug($"RTS|RTI: pull {value.ToInt():X2} from 0x1{addr.ToInt():X2}");
            }
        }
    }
}
