// Converted from W65c02sEngine.java
// Main engine for W65c02s
using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;

public class W65c02sEngine : T2RegistryIntf, StateRegistryIntf
{
    // ...fields and constants...
    private readonly Context _ctx;
    private readonly States[] _t2Map;
    private readonly InstMethodIntf[] _stateMethodMap;
    private readonly byte LOW = Constants.LOW;
    private readonly byte HIGH = Constants.HIGH;
    private readonly byte READ = Constants.HIGH;
    private readonly byte P1MIDDLESTEP = Constants.P1MIDDLESTEP;
    private readonly byte LASTSUBSTEP = Constants.P2LASTSUBSTEP;

    private byte _lastClock = Constants.HIGH;
    private long _instCount = 0;
    private long _cycleCount = 0;
    private int _rstClockCount = 0;
    private byte _lastNmiState = 1;
    private bool _nmiAsserted = false;
    private bool _irqAsserted = false;

    // Logger stubs (replace with your logging framework if needed)
    // private readonly Logger _instLogger;
    // private readonly Logger _stateLogger;
    private string _debugRegisters;

    public W65c02sEngine(PinsInternalIntf pins, Registers regs, Context ctx)
    {
        _ctx = ctx;
        _ctx.InitState(States.WarmUp0);
        _ctx.ForwardToLastSubStep();
        _t2Map = new States[256];
        _stateMethodMap = new InstMethodIntf[Enum.GetValues(typeof(States)).Length];
        InitStateMethodMap(_stateMethodMap);
        // Instantiate instruction classes (replace with actual implementations)
        // new InstLDA(this, this);
        // ... instantiate other instructions ...
        // new Interrupts(this, this);
    }

    public long GetInstCount() => _instCount;
    public long GetCycleCount() => _cycleCount;

    public void Map(OpCodes opCode, States state) => _t2Map[(int)opCode] = state;
    public void Map(States state, InstMethodIntf method) => _stateMethodMap[(int)state] = method;

    private void InitStateMethodMap(InstMethodIntf[] map)
    {
        map[(int)States.WarmUp0] = (ctx) => WarmUp0(ctx);
        map[(int)States.WarmUp1] = (ctx) => WarmUp1(ctx);
        map[(int)States.WarmUp2] = (ctx) => WarmUp2(ctx);
        map[(int)States.Boot1] = (ctx) => Boot1(ctx);
        map[(int)States.Boot2] = (ctx) => Boot2(ctx);
        map[(int)States.NotReady] = (ctx) => NotReady(ctx);
        map[(int)States.Stop] = (ctx) => Stop(ctx);
        map[(int)States.Fetch] = (ctx) => Fetch(ctx);
    }

    public void Step()
    {
        if (_ctx.Pins.PHI2 == _lastClock)
            return;

        _lastClock = _ctx.Pins.PHI2;
        int subStep = _ctx.SubStep;

        if (subStep == 1)
        {
            if (_ctx.State == States.Fetch && _ctx.DbgOpCode != null)
            {
                // _instLogger.Debug($"{Dissasembler.Dissasemble(_ctx),-24} {_debugRegisters}");
            }
            // _stateLogger.Debug("----------------------------------------");
            if (_ctx.Pins.RDY == LOW)
            {
                if (_ctx.State != States.NotReady)
                {
                    _ctx.SetReadyState(_ctx.State);
                    _ctx.InitState(States.NotReady);
                }
            }
            else if (_nmiAsserted)
            {
                _nmiAsserted = false;
                _ctx.SetNmiFlag();
            }
            else if (_irqAsserted)
            {
                _irqAsserted = false;
                _ctx.SetIrqFlag();
            }
            _cycleCount++;
        }
        else if (subStep == Constants.P1LASTSTEP)
        {
            _ctx.Pins.PHI1O = LOW;
            _ctx.Pins.PHI2O = HIGH;
        }

        if (_ctx.State == States.Fetch)
        {
            if (_ctx.NmiFlag || _ctx.IrqFlag)
                _ctx.InitState(States.Interrupt1);
        }

        int stateOrd = (int)_ctx.State;
        InstMethodIntf method = _stateMethodMap[stateOrd];
        // _stateLogger.Trace($"Executing state: {_ctx.State}, substep: {_ctx.SubStep}");
        method.Execute(_ctx);

        if (subStep == LASTSUBSTEP)
        {
            _ctx.Pins.PHI1O = HIGH;
            _ctx.Pins.PHI2O = LOW;
            CheckForReset(_ctx);
            CheckForNmi(_ctx);
            CheckForIrq(_ctx);
            Registers regs = _ctx.Regs;
            _debugRegisters = $"A:{regs.A.ToInt():X2} P:{regs.P} X:{regs.X.ToInt():X2} Y:{regs.Y.ToInt():X2} S:{regs.S.ToInt():X2} PC:{regs.PC.ToInt():X4}";
        }
        _ctx.IncSubStep();
        _ctx.Pins.DBGSUBSTEP = (byte)_ctx.SubStep;
    }

    private void CheckForReset(Context ctx)
    {
        if (ctx.Pins.RESB == 0)
            _rstClockCount++;
        else if (ctx.Pins.RESB == 1 && _rstClockCount >= 2)
            ctx.AdvanceState(States.Boot1);
        else
            _rstClockCount = 0;
    }

    private void CheckForNmi(Context ctx)
    {
        byte currentNmiState = ctx.Pins.NMIB;
        if (_lastNmiState == HIGH && currentNmiState == LOW)
            _nmiAsserted = true;
        _lastNmiState = currentNmiState;
    }

    private void CheckForIrq(Context ctx)
    {
        _irqAsserted = ctx.Regs.P.IRQDisabled.IsCleared() && ctx.Pins.IRQB == 0;
    }

    private void WarmUp0(Context ctx)
    {
        if (ctx.Pins.PHI2 == HIGH)
        {
            ctx.ForwardToLastSubStep();
            ctx.InitState(States.WarmUp1);
        }
    }

    private void WarmUp1(Context ctx)
    {
        ctx.AdvanceState(States.WarmUp2);
    }

    private void WarmUp2(Context ctx)
    {
        if (ctx.SubStep == LASTSUBSTEP)
            ResetFlags(ctx);
        ctx.AdvanceState(States.Boot1);
    }

    private void ResetPins(Context ctx)
    {
        ctx.Pins.VPB = HIGH;
        ctx.Pins.PHI1O = HIGH;
        ctx.Pins.MLB = HIGH;
        ctx.Pins.SYNC = LOW;
        ctx.Pins.AddrBusMode = AddrBusMode.Output;
        ctx.Pins.AddrBusPins = new UInt16(0xFFFF);
        ctx.Pins.DataBusMode = DataBusMode.Input;
        ctx.Pins.RWB = READ;
    }

    private void ResetFlags(Context ctx)
    {
        ctx.Regs.P.SetFlags(new UInt8(0b00110100));
    }

    private void Boot1(Context ctx)
    {
        if (ctx.SubStep == 1)
        {
            ResetPins(ctx);
            _rstClockCount = 0;
        }
        else if (ctx.SubStep == P1MIDDLESTEP)
        {
            ctx.Pins.AddrBusMode = AddrBusMode.Output;
            ctx.Pins.DataBusMode = DataBusMode.Input;
            ctx.Pins.AddrBusPins = new UInt16(0xFFFC);
            ctx.Pins.RWB = READ;
        }
        else if (ctx.SubStep == LASTSUBSTEP)
        {
            UInt8 data = ctx.Pins.DataBusPins;
            ctx.Regs.PC.Lsb().UpdateValue(data);
        }
        ctx.AdvanceState(States.Boot2);
    }

    private void Boot2(Context ctx)
    {
        if (ctx.SubStep == P1MIDDLESTEP)
        {
            ctx.Pins.AddrBusPins = new UInt16(0xFFFD);
            ctx.Pins.RWB = READ;
        }
        else if (ctx.SubStep == LASTSUBSTEP)
        {
            UInt8 data = ctx.Pins.DataBusPins;
            ctx.Regs.PC.Msb().UpdateValue(data);
        }
        ctx.AdvanceState(States.Fetch);
    }

    private void Fetch(Context ctx)
    {
        if (ctx.SubStep == 1)
        {
            ctx.Pins.SYNC = HIGH;
            ctx.Pins.RWB = READ;
            ctx.Pins.DataBusMode = DataBusMode.Input;
        }
        else if (ctx.SubStep == P1MIDDLESTEP)
        {
            UInt16 pc = ctx.Regs.PC.Copy();
            ctx.DbgPC = pc;
            ctx.Pins.AddrBusPins = pc;
            ctx.Regs.PC.Inc();
            ctx.Pins.RWB = READ;
        }
        else if (ctx.SubStep == LASTSUBSTEP)
        {
            ctx.Regs.Inst.UpdateValue(ctx.Pins.DataBusPins);
            ctx.Pins.SetDbgInst(ctx.Regs.Inst);
            ctx.Pins.SYNC = LOW;
            _instCount++;
            OpCodes opCode = OpCodes.FromValue(ctx.Regs.Inst);
            ctx.DbgOpCode = opCode;
        }
        ctx.AdvanceState(_t2Map[ctx.Regs.Inst.ToInt()]);
    }

    private void NotReady(Context ctx)
    {
        if (ctx.Pins.RDY == HIGH)
            ctx.AdvanceState(ctx.ReadyState);
    }

    private void Stop(Context ctx)
    {
        // nothing to do here but wait for a reset
    }
}
