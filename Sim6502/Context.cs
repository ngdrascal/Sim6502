// Converted from Context.java
// Represents the CPU context for W65c02s
using System;

namespace Us.Retrocpu.W65c02s {
    public class Context {
        private readonly PinsInternalIntf _pins;
        private readonly Registers _registers;
        private bool _crossedPageBoundary;
        private States _currentState;
        private int _subStep;
        private bool _nmiFlag;
        private bool _irqFlag;
        private States _readyState;
        private UInt16 _dbgPC;
        private OpCodes _dbgOpCode;
        private UInt8 _dbgOperand1;
        private UInt8 _dbgOperand2;
        // Logger omitted for brevity

        public Context(PinsInternalIntf pins, Registers registers) {
            _pins = pins;
            _registers = registers;
            _nmiFlag = false;
            _irqFlag = false;
        }

        public PinsInternalIntf Pins => _pins;

        public Registers Regs => _registers;

        public bool CrossedPageBoundary {
            get => _crossedPageBoundary;
            set => _crossedPageBoundary = value;
        }

        public States State {
            get => _currentState;
            private set => _currentState = value;
        }

        public int SubStep => _subStep;

        public void IncSubStep() {
            _subStep++;
            if (_subStep > Constants.P2LastSubStep)
                _subStep = 1;
        }

        public void ForwardToLastSubStep() {
            _subStep = Constants.P2LastSubStep;
        }

        public void AdvanceState(States nextState) {
            if (_subStep == Constants.P2LastSubStep) {
                InitState(nextState);
                // Logging omitted
            }
        }

        public void InitState(States value) {
            State = value;
            Pins.SetDBGSTATE((byte)State);
        }

        public bool NmiFlag => _nmiFlag;

        public void SetNmiFlag() => _nmiFlag = true;

        public void ClearNmiFlag() => _nmiFlag = false;

        public bool IrqFlag => _irqFlag;

        public void SetIrqFlag() => _irqFlag = true;

        public void ClearIrqFlag() => _irqFlag = false;

        public States ReadyState {
            get => _readyState;
            set => _readyState = value;
        }

        public UInt16 DbgPC {
            get => _dbgPC;
            set => _dbgPC = value;
        }

        public OpCodes DbgOpCode {
            get => _dbgOpCode;
            set => _dbgOpCode = value;
        }

        public UInt8 DbgOperand1 {
            get => _dbgOperand1;
            set => _dbgOperand1 = value;
        }

        public UInt8 DbgOperand2 {
            get => _dbgOperand2;
            set => _dbgOperand2 = value;
        }
    }
}
