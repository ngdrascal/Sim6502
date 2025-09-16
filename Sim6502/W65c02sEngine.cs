using Microsoft.Extensions.Logging;
using Sim6502.Instructions;
using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502;

public class W65c02sEngine : IT2Registry, IStateRegistry
{
    private readonly byte LOW = Constants.LOW;
    private readonly byte HIGH = Constants.HIGH;
    private readonly byte READ = Constants.HIGH;
    private readonly byte P1MIDDLESTEP = Constants.P1MIDDLESTEP;
    private readonly byte LASTSUBSTEP = Constants.P2LASTSUBSTEP;

    private readonly Context _ctx;
    private readonly States[] _t2Map;
    private readonly Action<Context>[] _stateMethodMap;
    private byte _lastClock = Constants.HIGH;
    private long _instCount = 0;
    private long _cycleCount = 0;
    private int _rstClockCount = 0;
    private byte _lastNmiState = 1;
    private bool _nmiAsserted = false;
    private bool _irqAsserted = false;
    private readonly ILogger _instLogger;
    private readonly ILogger _stateLogger;
    private string _debugRegisters;

    public W65c02sEngine(IPinsInternal pins, Registers regs, Context ctx, ILoggerFactory loggerFactory)
    {
        _ctx = ctx;
        ctx.InitState(States.WarmUp0);
        ctx.ForwardToLastSubStep();
        _t2Map = new States[256];
        _stateMethodMap = new Action<Context>[Enum.GetValues(typeof(States)).Length];
        InitStateMethodMap(_stateMethodMap);
        _instLogger = loggerFactory.CreateLogger("W65c02s.inst_");
        _stateLogger = loggerFactory.CreateLogger("W65c02s.state");
        // Instantiate all instruction classes
        new InstLDA(this, this);
        new InstLDX(this, this);
        new InstLDY(this, this);
        new InstSTA(this, this);
        new InstSTX(this, this);
        new InstSTY(this, this);
        new InstSTZ(this, this);
        new InstTransfer(this, this);
        new InstStack(this, this);
        new InstASL(this, this);
        new InstLSR(this, this);
        new InstROL(this, this);
        new InstROR(this, this);
        new InstAND(this, this);
        new InstORA(this, this);
        new InstEOR(this, this);
        new InstBIT(this, this);
        new InstTRB(this, this);
        new InstTSB(this, this);
        new InstFlags(this, this);
        new InstBranch(this, this);
        new InstControl(this, this);
        new InstDecrement(this, this);
        new InstIncrement(this, this);
        new InstADC(this, this);
        new InstSBC(this, this);
        new InstCMP(this, this);
        new InstCPX(this, this);
        new InstCPY(this, this);
        new InstSTP(this, this);
        new InstWAI(this, this);
        new InstNOP(this, this);
        new Interrupts(this, this);
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
        map[(int)States.WarmUp0] = ctx => WarmUp0(ctx);
        map[(int)States.WarmUp1] = ctx => WarmUp1(ctx);
        map[(int)States.WarmUp2] = ctx => WarmUp2(ctx);
        map[(int)States.Boot1] = ctx => Boot1(ctx);
        map[(int)States.Boot2] = ctx => Boot2(ctx);
        map[(int)States.NotReady] = ctx => NotReady(ctx);
        map[(int)States.Stop] = ctx => Stop(ctx);
        map[(int)States.Fetch] = ctx => Fetch(ctx);
    }

    public void Step()
    {
        if (_ctx.Pins.GetPHI2() == _lastClock)
            return;
        _lastClock = _ctx.Pins.GetPHI2();
        var subStep = _ctx.GetSubStep();
        if (subStep == 1)
        {
            if (_ctx.State == States.Fetch && _ctx.DbgOpCode != null)
                _instLogger.LogDebug($"{Dissasembler.Dissasemble(_ctx),-24} {_debugRegisters}");
            _stateLogger.LogDebug("----------------------------------------");
            if (_ctx.Pins.GetRDY() == LOW)
            {
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
        else if (subStep == Constants.P1LASTSTEP)
        {
            _ctx.Pins.SetPHI1O(LOW);
            _ctx.Pins.SetPHI2O(HIGH);
        }

        if (_ctx.State == States.Fetch)
            if (_ctx.NmiFlag || _ctx.IrqFlag)
                _ctx.InitState(States.Interrupt1);
        var stateOrd = (int)_ctx.State;
        var method = _stateMethodMap[stateOrd];
        _stateLogger.LogTrace($"Executing state: {_ctx.State}, substep: {subStep}");
        method.BeginInvoke(_ctx, null, null);
        if (subStep == LASTSUBSTEP)
        {
            _ctx.Pins.SetPHI1O(HIGH);
            _ctx.Pins.SetPHI1O(LOW);
            CheckForReset(_ctx);
            CheckForNmi(_ctx);
            CheckForIrq(_ctx);
            Registers regs = _ctx.Regs;
            _debugRegisters = $"A:{regs.A.ToInt():X2} P:{regs.P} X:{regs.X.ToInt():X2} Y:{regs.Y.ToInt():X2} S:{regs.S.ToInt():X2} PC:{regs.PC.ToInt():X4}";
        }
        _ctx.IncSubStep();
        _ctx.Pins.SetDBGSUBSTEP((byte)_ctx.GetSubStep());
    }

    private void CheckForReset(Context ctx)
    {
        if (ctx.Pins.GetPHI2() == 0)
            _rstClockCount++;
        else if (ctx.Pins.GetPHI2() == 1 && _rstClockCount >= 2)
            ctx.AdvanceState(States.Boot1);
        else
            _rstClockCount = 0;
    }

    private void CheckForNmi(Context ctx)
    {
        byte currentNmiState = ctx.Pins.GetPHI2();
        if (_lastNmiState == HIGH && currentNmiState == LOW)
            _nmiAsserted = true;
        _lastNmiState = currentNmiState;
    }

    private void CheckForIrq(Context ctx)
    {
        _irqAsserted = ctx.Regs.P.IRQDisabled.IsCleared() && ctx.Pins.GetPHI2() == 0;
    }

    private void WarmUp0(Context ctx)
    {
        if (ctx.Pins.GetPHI2() == HIGH)
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
        if (ctx.GetSubStep() == LASTSUBSTEP)
            ResetFlags(ctx);
        ctx.AdvanceState(States.Boot1);
    }

    private void ResetPins(Context ctx)
    {
        ctx.Pins.SetPHI1O(HIGH);
        ctx.Pins.SetPHI1O(HIGH);
        ctx.Pins.SetPHI1O(HIGH);
        ctx.Pins.SetPHI1O(LOW);
        ctx.Pins.SetAddrBusMode(AddrBusMode.Output);
        ctx.Pins.SetAddrBusPins(new UInt16(0xFFFF));
        ctx.Pins.SetDataBusMode(DataBusMode.Input);
        ctx.Pins.SetRWB(READ);
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
        else if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            ctx.Pins.SetAddrBusMode(AddrBusMode.Output);
            ctx.Pins.SetDataBusMode(DataBusMode.Input);
            ctx.Pins.SetAddrBusPins(new UInt16(0xFFFC));
            ctx.Pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.PC.Lsb().UpdateValue(data);
        }
        ctx.AdvanceState(States.Boot2);
    }

    private void Boot2(Context ctx)
    {
        if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            ctx.Pins.SetAddrBusPins(new UInt16(0xFFFD));
            ctx.Pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == LASTSUBSTEP)
        {
            var data = ctx.Pins.GetDataBusPins();
            ctx.Regs.PC.Msb().UpdateValue(data);
        }
        ctx.AdvanceState(States.Fetch);
    }

    private void Fetch(Context ctx)
    {
        if (ctx.GetSubStep() == 1)
        {
            ctx.Pins.SetPHI1O(HIGH);
            ctx.Pins.SetRWB(READ);
            ctx.Pins.SetDataBusMode(DataBusMode.Input);
        }
        else if (ctx.GetSubStep() == P1MIDDLESTEP)
        {
            var pc = ctx.Regs.PC.Copy();
            ctx.DbgPC = pc;
            ctx.Pins.SetAddrBusPins(pc);
            ctx.Regs.PC.Inc();
            ctx.Pins.SetRWB(READ);
        }
        else if (ctx.GetSubStep() == LASTSUBSTEP)
        {
            ctx.Regs.Inst.UpdateValue(ctx.Pins.GetDataBusPins());
            ctx.Pins.SetDBGINST(ctx.Regs.Inst);
            ctx.Pins.SetPHI1O(LOW);
            _instCount++;
            var opCode = OpCodesExtensions.FromValue(ctx.Regs.Inst);
            ctx.DbgOpCode = opCode;
        }
        ctx.AdvanceState(_t2Map[ctx.Regs.Inst.ToInt()]);
    }

    private void NotReady(Context ctx)
    {
        if (ctx.Pins.GetPHI2() == HIGH)
            ctx.AdvanceState(ctx.ReadyState);
    }

    private void Stop(Context ctx)
    {
        // nothing to do here but wait for a reset
    }
}
