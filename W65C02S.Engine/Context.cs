using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine;

public class Context
{
    private int _subStep;

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

    public int Substep => _subStep;

    // TODO: Remove and replace with Substep property
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
            OnStateChanging(new StateChangingEventArgs(nextState));

            InitState(nextState);
        }
    }

    public void AdvanceState(bool condition, States trueState, States falseState)
    {
        AdvanceState(condition ? trueState : falseState);
    }

    public void InitState(States value)
    {
        State = value;
        Pins.DBGSTATE = (byte)State;
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
}
