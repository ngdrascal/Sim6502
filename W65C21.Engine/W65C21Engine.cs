namespace W65C21.Engine;

/// <summary>
/// Pin-level W65C21 PIA. The host updates <see cref="Pins"/> inputs and calls
/// <see cref="Evaluate"/> after any change; the engine detects PHI2 edges, control-line
/// transitions and RESB itself and updates the outputs.
/// </summary>
public class W65C21Engine
{
    private const int PortARegister = 0;
    private const int CraRegister = 1;
    private const int PortBRegister = 2;

    private readonly PiaSide _a = new();
    private readonly PiaSide _b = new();

    private bool _prevPhi2;
    private bool _cycleSelected;
    private bool _cycleRead;
    private int _cycleRegister;
    private bool _driveData;
    private byte _dataOut;

    public W65C21Engine()
    {
        _prevPhi2 = Pins.PHI2;
        Reset();
        UpdateOutputs();
    }

    public W65C21Pins Pins { get; } = new();

    internal byte CRA => _a.Cr;
    internal byte CRB => _b.Cr;
    internal byte DDRA => _a.Ddr;
    internal byte DDRB => _b.Ddr;
    internal byte ORA => _a.Or;
    internal byte ORB => _b.Or;

    public void Evaluate()
    {
        if (!Pins.RESB)
        {
            Reset();
        }
        else
        {
            if (Pins.PHI2 && !_prevPhi2)
                OnPhi2Rise();
            else if (!Pins.PHI2 && _prevPhi2)
                OnPhi2Fall();

            _a.SampleControlLines(Pins.CA1, Pins.CA2In);
            _b.SampleControlLines(Pins.CB1, Pins.CB2In);
        }

        _prevPhi2 = Pins.PHI2;
        UpdateOutputs();
    }

    private void Reset()
    {
        _a.Reset(Pins.CA1, Pins.CA2In);
        _b.Reset(Pins.CB1, Pins.CB2In);
        _cycleSelected = false;
        _driveData = false;
    }

    private void OnPhi2Rise()
    {
        _b.ClockStrobe();

        _cycleSelected = Pins.CS0 && Pins.CS1 && !Pins.CS2B;
        _cycleRead = Pins.RWB;
        _cycleRegister = (Pins.RS1 ? 2 : 0) | (Pins.RS0 ? 1 : 0);

        if (!_cycleSelected || !_cycleRead)
            return;

        _dataOut = ReadRegister(_cycleRegister);
        _driveData = true;
    }

    private void OnPhi2Fall()
    {
        _driveData = false;

        if (_cycleSelected && !_cycleRead)
            WriteRegister(_cycleRegister, Pins.DataIn);

        _cycleSelected = false;
        _a.ClockStrobe();
    }

    private byte ReadRegister(int register)
    {
        switch (register)
        {
            case PortARegister:
                if (!_a.DataSelected)
                    return _a.Ddr;
                _a.ClearFlags();
                _a.StartStrobe();
                return Pins.PAIn;
            case CraRegister:
                return _a.Cr;
            case PortBRegister:
                if (!_b.DataSelected)
                    return _b.Ddr;
                _b.ClearFlags();
                return (byte)((_b.Or & _b.Ddr) | (Pins.PBIn & ~_b.Ddr));
            default:
                return _b.Cr;
        }
    }

    private void WriteRegister(int register, byte value)
    {
        switch (register)
        {
            case PortARegister:
                if (_a.DataSelected)
                    _a.Or = value;
                else
                    _a.Ddr = value;
                break;
            case CraRegister:
                _a.WriteCr(value);
                break;
            case PortBRegister:
                if (_b.DataSelected)
                {
                    _b.Or = value;
                    _b.StartStrobe();
                }
                else
                {
                    _b.Ddr = value;
                }
                break;
            default:
                _b.WriteCr(value);
                break;
        }
    }

    private void UpdateOutputs()
    {
        Pins.DataOut = _dataOut;
        Pins.DataDrive = _driveData ? (byte)0xFF : (byte)0x00;
        Pins.PAOut = _a.Or;
        Pins.PADrive = _a.Ddr;
        Pins.PBOut = _b.Or;
        Pins.PBDrive = _b.Ddr;
        Pins.CA2Out = _a.C2Level;
        Pins.CA2Drive = _a.C2IsOutput;
        Pins.CB2Out = _b.C2Level;
        Pins.CB2Drive = _b.C2IsOutput;
        Pins.IRQABDrive = _a.IrqAsserted;
        Pins.IRQBBDrive = _b.IrqAsserted;
    }
}
