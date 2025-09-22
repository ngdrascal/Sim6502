using Sim6502.Instructions;
using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502;

public class W65C02SEngine : IT2Registry, IStateRegistry
{
    private const byte Low = Constants.Low;
    private const byte High = Constants.High;
    private const byte Read = Constants.High;
    private const byte P1MiddleStep = Constants.P1MiddleStep;
    private const byte LastSubstep = Constants.P2LastSubstep;

    private readonly Context _ctx;

    private readonly States[] _t2Map;

    private readonly Action<Context>[] _stateMethodMap;

    private byte _lastClock = Constants.High;

    private long _instCount;
    private long _cycleCount;
    private int _rstClockCount;
    private byte _lastNmiState = 1;
    private bool _nmiAsserted;
    private bool _irqAsserted;

    public W65C02SEngine(Context ctx)
    {
        _ctx = ctx;
        ctx.InitState(States.WarmUp0);
        ctx.ForwardToLastSubStep();

        _t2Map = new States[256];

        _stateMethodMap = new Action<Context>[Enum.GetValues(typeof(States)).Length];
        InitStateMethodMap(_stateMethodMap);

        _instCount = 0;
        _cycleCount = 0;
        _rstClockCount = 0;
        _nmiAsserted = false;
        _irqAsserted = false;

        // Instantiate all instruction classes
        _ = new InstADC().RegisterT2State(this).RegisterStates(this);
        _ = new InstAND().RegisterT2State(this).RegisterStates(this);
        _ = new InstASL().RegisterT2State(this).RegisterStates(this);
        _ = new InstBIT().RegisterT2State(this).RegisterStates(this);
        _ = new InstBranch().RegisterT2State(this).RegisterStates(this);
        _ = new InstBRK().RegisterT2State(this).RegisterStates(this);
        _ = new InstCMP().RegisterT2State(this).RegisterStates(this);
        _ = new InstControl().RegisterT2State(this).RegisterStates(this);
        _ = new InstCPX().RegisterT2State(this).RegisterStates(this);
        _ = new InstCPY().RegisterT2State(this).RegisterStates(this);
        _ = new InstDecrement().RegisterT2State(this).RegisterStates(this);
        _ = new InstEOR().RegisterT2State(this).RegisterStates(this);
        _ = new InstFlags().RegisterT2State(this).RegisterStates(this);
        _ = new InstIncrement().RegisterT2State(this).RegisterStates(this);
        _ = new InstLDA().RegisterT2State(this).RegisterStates(this);
        _ = new InstLDX().RegisterT2State(this).RegisterStates(this);
        _ = new InstLDY().RegisterT2State(this).RegisterStates(this);
        _ = new InstLSR().RegisterT2State(this).RegisterStates(this);
        _ = new InstNOP().RegisterT2State(this).RegisterStates(this);
        _ = new InstORA().RegisterT2State(this).RegisterStates(this);
        _ = new InstROL().RegisterT2State(this).RegisterStates(this);
        _ = new InstROR().RegisterT2State(this).RegisterStates(this);
        _ = new InstSBC().RegisterT2State(this).RegisterStates(this);
        _ = new InstSTA().RegisterT2State(this).RegisterStates(this);
        _ = new InstStack().RegisterT2State(this).RegisterStates(this);
        _ = new InstSTP().RegisterT2State(this).RegisterStates(this);
        _ = new InstSTX().RegisterT2State(this).RegisterStates(this);
        _ = new InstSTY().RegisterT2State(this).RegisterStates(this);
        _ = new InstSTZ().RegisterT2State(this).RegisterStates(this);
        _ = new InstTransfer().RegisterT2State(this).RegisterStates(this);
        _ = new InstTRB().RegisterT2State(this).RegisterStates(this);
        _ = new InstTSB().RegisterT2State(this).RegisterStates(this);
        _ = new InstWAI().RegisterT2State(this).RegisterStates(this);
        _ = new Interrupts().RegisterT2State(this).RegisterStates(this);
    }

    public event EventHandler<SubstepChangingEventArgs>? SubstepChanging;

    private void OnStateChanging(SubstepChangingEventArgs args)
    {
        SubstepChanging?.Invoke(this, args);
    }

    public long InstCount => _instCount;

    public long CycleCount => _cycleCount;

    public void Map(OpCodes opCode, States state)
    {
        _t2Map[opCode.ToInt()] = state;
    }

    public void Map(States state, Action<Context> method)
    {
        _stateMethodMap[(int)state] = method;
    }

    private void InitStateMethodMap(Action<Context>[] map)
    {
        map[(int)States.WarmUp0] = WarmUp0;
        map[(int)States.WarmUp1] = WarmUp1;
        map[(int)States.WarmUp2] = WarmUp2;

        map[(int)States.Boot1] = Boot1;
        map[(int)States.Boot2] = Boot2;

        map[(int)States.NotReady] = NotReady;

        map[(int)States.Stop] = Stop;

        map[(int)States.Fetch] = Fetch;
    }

    public void Step()
    {
        if (_ctx.Pins.GetPHI2() == _lastClock)
            return;

        _lastClock = _ctx.Pins.GetPHI2();

        var substep = _ctx.GetSubStep();

        if (substep == 1)
        {
            if (_ctx.Pins.GetRDY() == Low)
            {
                // this condition is nested instead of ANDs because we don't want
                // the interrupts handled until RDY is high
                if (_ctx.State != States.NotReady)
                {
                    _ctx.ReadyState = _ctx.State;
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
        else if (substep == Constants.P1LastStep)
        {
            _ctx.Pins.SetPHI1O(Low);
            _ctx.Pins.SetPHI2O(High);
        }

        if (_ctx.State == States.Fetch)
            if (_ctx.NmiFlag || _ctx.IrqFlag)
                _ctx.InitState(States.Interrupt1);

        OnStateChanging(new SubstepChangingEventArgs(_ctx.State, substep));

        var method = _stateMethodMap[(int)_ctx.State];
        method(_ctx);

        if (substep == LastSubstep)
        {
            _ctx.Pins.SetPHI1O(High);
            _ctx.Pins.SetPHI1O(Low);
            CheckForReset(_ctx);
            CheckForNmi(_ctx);
            CheckForIrq(_ctx);
            _ctx.CaptureRegs();
        }

        _ctx.IncSubStep();

        _ctx.Pins.SetDBGSUBSTEP((byte)_ctx.GetSubStep());
    }

    private void CheckForReset(Context ctx)
    {
        // if the reset pin is asserted
        if (ctx.Pins.GetRESB() == 0)
            _rstClockCount++;
        // else if the reset pin is no longer asserted AND
        // it was asserted for 2 or more clock cycles
        else if (ctx.Pins.GetRESB() == 1 && _rstClockCount >= 2)
            ctx.AdvanceState(States.Boot1);
        else
            _rstClockCount = 0;
    }

    private void CheckForNmi(Context ctx)
    {
        var currentNmiState = ctx.Pins.GetNMIB();
        if (_lastNmiState == High && currentNmiState == Low)
            _nmiAsserted = true;

        _lastNmiState = currentNmiState;
    }

    private void CheckForIrq(Context ctx)
    {
        _irqAsserted = ctx.Regs.P.IRQDisabled.IsCleared() && ctx.Pins.GetIRQB() == 0;
    }

    private void WarmUp0(Context ctx)
    {
        // start by syncing with the first clock HIGH signal
        if (ctx.Pins.GetPHI2() == High)
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
        if (ctx.GetSubStep() == LastSubstep)
            ResetFlags(ctx);
        ctx.AdvanceState(States.Boot1);
    }

    private void ResetPins(Context ctx)
    {
        ctx.Pins.SetPHI1O(High);
        ctx.Pins.SetPHI1O(High);
        ctx.Pins.SetPHI1O(High);
        ctx.Pins.SetPHI1O(Low);
        ctx.Pins.SetAddrBusMode(AddrBusMode.Output);
        ctx.Pins.AddrBus = new UInt16(0xFFFF);
        ctx.Pins.SetDataBusMode(DataBusMode.Input);
        ctx.Pins.SetRWB(Read);
    }

    private void ResetFlags(Context ctx)
    {
        ctx.Regs.P.SetFlags(new UInt8(0b00110100));
    }

    private void Boot1(Context ctx)
    {
        if (ctx.GetSubStep() == 1)
        {
            ResetPins(ctx);
            _rstClockCount = 0;
        }
        else if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.SetAddrBusMode(AddrBusMode.Output);
            ctx.Pins.SetDataBusMode(DataBusMode.Input);
            ctx.Pins.AddrBus = new UInt16(0xFFFC);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.PC.Lsb().UpdateValue(data);
        }
        ctx.AdvanceState(States.Boot2);
    }

    private void Boot2(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.AddrBus = new UInt16(0xFFFD);
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.PC.Msb().UpdateValue(data);
        }
        ctx.AdvanceState(States.Fetch);
    }

    private void Fetch(Context ctx)
    {
        if (ctx.GetSubStep() == 1)
        {
            ctx.Pins.SetPHI1O(High);
            ctx.Pins.SetRWB(Read);
            ctx.Pins.SetDataBusMode(DataBusMode.Input);
        }
        else if (ctx.GetSubStep() == P1MiddleStep)
        {
            var pc = ctx.Regs.PC.Copy();
            ctx.DbgPC = pc;
            ctx.Pins.AddrBus = pc;
            ctx.Regs.PC.Inc();
            ctx.Pins.SetRWB(Read);
        }
        else if (ctx.GetSubStep() == LastSubstep)
        {
            ctx.Regs.Inst.UpdateValue(ctx.Pins.DataBus);
            ctx.Pins.SetDBGINST(ctx.Regs.Inst);
            ctx.Pins.SetPHI1O(Low);
            _instCount++;
            var opCode = OpCodesExtensions.FromValue(ctx.Regs.Inst);
            ctx.DbgOpCode = opCode;
        }
        ctx.AdvanceState(_t2Map[ctx.Regs.Inst.ToInt()]);
    }

    private void NotReady(Context ctx)
    {
        if (ctx.Pins.GetPHI2() == High)
            ctx.AdvanceState(ctx.ReadyState);
    }

    private void Stop(Context ctx)
    {
        // nothing to do here but wait for a reset
    }
}
