using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502;

public class Context
{
    private int _subStep;
    private Registers _debugCapture;

    public Context(IPinsInternal pins, Registers registers)
    {
        Pins = pins;
        Regs = registers;

        NmiFlag = false;
        IrqFlag = false;
        DbgPC = new UInt16(0);
        DbgOperand1 = new UInt8(0);
        DbgOperand2 = new UInt8(0);
    }

    public event EventHandler<StateChangingEventArgs>? StateChanging;

    private void OnStateChanging(StateChangingEventArgs args)
    {
        StateChanging?.Invoke(this, args);
    }

    public IPinsInternal Pins { get; }

    public Registers Regs { get; }

    public bool CrossedPageBoundary { get; set; }

    public States State { get; private set; }

    public int GetSubStep() => _subStep;

    public void IncSubStep()
    {
        _subStep++;
        if (_subStep > Constants.P2LastSubstep)
            _subStep = 1;
    }

    public void ForwardToLastSubStep() => _subStep = Constants.P2LastSubstep;

    public void AdvanceState(States nextState)
    {
        if (_subStep == Constants.P2LastSubstep)
        {
            OnStateChanging(new StateChangingEventArgs(State, nextState));

            InitState(nextState);
        }
    }

    public void InitState(States value)
    {
        State = value;
        Pins.SetDBGSTATE((byte)State);
    }

    public bool NmiFlag { get; private set; }

    public void SetNmiFlag() => NmiFlag = true;

    public void ClearNmiFlag() => NmiFlag = false;

    public bool IrqFlag { get; private set; }

    public void SetIrqFlag() => IrqFlag = true;

    public void ClearIrqFlag() => IrqFlag = false;

    public States ReadyState { get; set; }

    public UInt16 DbgPC { get; set; }

    public OpCodes? DbgOpCode { get; set; }

    public UInt8 DbgOperand1 { get; set; }

    public UInt8 DbgOperand2 { get; set; }

    public void CaptureRegs()
    {
        _debugCapture = new Registers(Regs);
    }

    public Registers DebugCapture => _debugCapture;
}
