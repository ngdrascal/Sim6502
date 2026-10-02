namespace W65C22.Engine;

/// <summary>
/// Pin-level W65C22 VIA. The host updates <see cref="Pins"/> inputs and calls
/// <see cref="Evaluate"/> after any change; the engine detects PHI2 edges, control-line
/// transitions and RESB itself and updates the outputs.
/// </summary>
public class W65C22Engine
{
    private const int OrbRegister = 0x0;
    private const int OraRegister = 0x1;
    private const int DdrbRegister = 0x2;
    private const int DdraRegister = 0x3;
    private const int T1CounterLowRegister = 0x4;
    private const int T1CounterHighRegister = 0x5;
    private const int T1LatchLowRegister = 0x6;
    private const int T1LatchHighRegister = 0x7;
    private const int T2LowRegister = 0x8;
    private const int T2HighRegister = 0x9;
    private const int SrRegister = 0xA;
    private const int AcrRegister = 0xB;
    private const int PcrRegister = 0xC;
    private const int IfrRegister = 0xD;
    private const int IerRegister = 0xE;
    private const int OraNoHandshakeRegister = 0xF;

    // IFR / IER bits.
    private const byte Ca2Flag = 0x01;
    private const byte Ca1Flag = 0x02;
    private const byte SrFlag = 0x04;
    private const byte Cb2Flag = 0x08;
    private const byte Cb1Flag = 0x10;
    private const byte T2Flag = 0x20;
    private const byte T1Flag = 0x40;
    private const byte IrqBit = 0x80;
    private const byte FlagMask = 0x7F;

    // ACR bits.
    private const byte PaLatchEnable = 0x01;
    private const byte PbLatchEnable = 0x02;
    private const byte SrModeMask = 0x1C;
    private const byte T2PulseCounting = 0x20;
    private const byte T1FreeRun = 0x40;
    private const byte T1Pb7Output = 0x80;

    private const byte Pb6 = 0x40;
    private const byte Pb7 = 0x80;

    private static readonly string[] ReadNames =
    [
        "IRB", "IRA", "DDRB", "DDRA", "T1C-L", "T1C-H", "T1L-L", "T1L-H",
        "T2C-L", "T2C-H", "SR", "ACR", "PCR", "IFR", "IER", "IRA-NH"
    ];

    private static readonly string[] WriteNames =
    [
        "ORB", "ORA", "DDRB", "DDRA", "T1L-L", "T1C-H", "T1L-L", "T1L-H",
        "T2L-L", "T2C-H", "SR", "ACR", "PCR", "IFR", "IER", "ORA-NH"
    ];

    private readonly ControlLines _a = new();
    private readonly ControlLines _b = new();
    private readonly Timer1 _t1 = new();
    private readonly Timer2 _t2 = new();
    private readonly ShiftRegister _sr = new();

    private bool _prevPhi2;
    private bool _cycleSelected;
    private bool _cycleRead;
    private int _cycleRegister;
    private bool _driveData;
    private byte _dataOut;

    private byte _ira;
    private byte _irb;
    private bool _paLatched;
    private bool _pbLatched;

    public W65C22Engine()
    {
        _prevPhi2 = Pins.PHI2;
        Reset();
        UpdateOutputs();
    }

    public W65C22Pins Pins { get; } = new();

    public event Action<RegisterAccess>? RegisterAccessed;

    internal byte ORA { get; private set; }
    internal byte ORB { get; private set; }
    internal byte DDRA { get; private set; }
    internal byte DDRB { get; private set; }
    internal byte ACR { get; private set; }
    internal byte PCR { get; private set; }

    /// <summary>The Interrupt Flags (bits 0-6); bit 7 is computed on read.</summary>
    internal byte IFR { get; private set; }

    /// <summary>The Interrupt Enable bits (bits 0-6).</summary>
    internal byte IER { get; private set; }

    internal ushort T1Counter => _t1.Counter;
    internal ushort T1Latch => _t1.Latch;
    internal ushort T2Counter => _t2.Counter;
    internal byte T2LatchLow => _t2.LatchLow;
    internal byte SR => _sr.Value;

    private bool IrqActive => (IFR & IER & FlagMask) != 0;

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

            SampleControlLines();
        }

        _prevPhi2 = Pins.PHI2;
        UpdateOutputs();
    }

    private void Reset()
    {
        ORA = 0;
        ORB = 0;
        DDRA = 0;
        DDRB = 0;
        ACR = 0;
        PCR = 0;
        IFR = 0;
        IER = 0;
        _paLatched = false;
        _pbLatched = false;
        _a.Reset(Pins.CA1, Pins.CA2In);
        _b.Reset(Pins.CB1In, Pins.CB2In);
        _t1.Reset();
        _t2.Reset();
        _sr.Reset();
        _cycleSelected = false;
        _driveData = false;
    }

    private void SampleControlLines()
    {
        var (ca1, ca2) = _a.Sample(Pins.CA1, Pins.CA2In);
        if (ca1)
        {
            IFR |= Ca1Flag;
            if ((ACR & PaLatchEnable) != 0)
            {
                _ira = Pins.PAIn;
                _paLatched = true;
            }
        }

        if (ca2)
            IFR |= Ca2Flag;

        var (cb1, cb2) = _b.Sample(Pins.CB1In, Pins.CB2In);
        if (cb1)
        {
            IFR |= Cb1Flag;
            if ((ACR & PbLatchEnable) != 0)
            {
                _irb = Pins.PBIn;
                _pbLatched = true;
            }
        }

        if (cb2)
            IFR |= Cb2Flag;

        if (_sr.SampleCb1(Pins.CB1In))
            IFR |= SrFlag;
    }

    private void OnPhi2Rise()
    {
        if (_t1.ClockRise((ACR & T1FreeRun) != 0))
            IFR |= T1Flag;
        if (_t2.ClockRise((ACR & T2PulseCounting) != 0, (Pins.PBIn & Pb6) != 0))
            IFR |= T2Flag;
        if (_sr.ClockRise(Pins.CB2In))
            IFR |= SrFlag;

        _a.ClockStrobe(fall: false);
        _b.ClockStrobe(fall: false);

        _cycleSelected = Pins.CS1 && !Pins.CS2B;
        _cycleRead = Pins.RWB;
        _cycleRegister = (Pins.RS3 ? 8 : 0) | (Pins.RS2 ? 4 : 0) | (Pins.RS1 ? 2 : 0) | (Pins.RS0 ? 1 : 0);

        if (!_cycleSelected || !_cycleRead)
            return;

        _dataOut = ReadRegister(_cycleRegister);
        _driveData = true;
        RegisterAccessed?.Invoke(new RegisterAccess(ReadNames[_cycleRegister], false, _dataOut));
    }

    private void OnPhi2Fall()
    {
        _driveData = false;

        // Timers count before a write so a counter load at this fall is not decremented. While a
        // shift mode uses T2, its low byte is the shift clock and the timer itself is held.
        _t1.ClockFall();
        var t2Underflow = _sr.ClocksFromT2 && _t2.ClockShiftFall();
        if ((ACR & T2PulseCounting) == 0 && !_sr.UsesT2)
            _t2.ClockFall();
        if (_sr.ClockFall(t2Underflow))
            IFR |= SrFlag;

        if (_cycleSelected && !_cycleRead)
        {
            RegisterAccessed?.Invoke(new RegisterAccess(WriteNames[_cycleRegister], true, Pins.DataIn));
            WriteRegister(_cycleRegister, Pins.DataIn);
        }

        _cycleSelected = false;
        _a.ClockStrobe(fall: true);
        _b.ClockStrobe(fall: true);
    }

    private byte ReadRegister(int register)
    {
        switch (register)
        {
            case OrbRegister:
                ClearPortBFlags();
                return ReadPortB();
            case OraRegister:
                ClearPortAFlags();
                _a.StartStrobe(onFall: true);
                return ReadPortA();
            case OraNoHandshakeRegister:
                return ReadPortA();
            case DdrbRegister:
                return DDRB;
            case DdraRegister:
                return DDRA;
            case T1CounterLowRegister:
                IFR = (byte)(IFR & ~T1Flag);
                return (byte)_t1.Counter;
            case T1CounterHighRegister:
                return (byte)(_t1.Counter >> 8);
            case T1LatchLowRegister:
                return (byte)_t1.Latch;
            case T1LatchHighRegister:
                return (byte)(_t1.Latch >> 8);
            case T2LowRegister:
                IFR = (byte)(IFR & ~T2Flag);
                return (byte)_t2.Counter;
            case T2HighRegister:
                return (byte)(_t2.Counter >> 8);
            case SrRegister:
                var value = _sr.Value;
                AccessShiftRegister();
                return value;
            case AcrRegister:
                return ACR;
            case PcrRegister:
                return PCR;
            case IfrRegister:
                return (byte)(IFR | (IrqActive ? IrqBit : 0));
            default:
                // IER ($E), the only register left.
                return (byte)(IER | IrqBit);
        }
    }

    private void WriteRegister(int register, byte value)
    {
        switch (register)
        {
            case OrbRegister:
                ClearPortBFlags();
                ORB = value;
                _b.StartStrobe(onFall: false);
                break;
            case OraRegister:
                ClearPortAFlags();
                ORA = value;
                _a.StartStrobe(onFall: false);
                break;
            case OraNoHandshakeRegister:
                ORA = value;
                break;
            case DdrbRegister:
                DDRB = value;
                break;
            case DdraRegister:
                DDRA = value;
                break;
            case T1CounterLowRegister:
            case T1LatchLowRegister:
                _t1.WriteLatchLow(value);
                break;
            case T1CounterHighRegister:
                _t1.WriteLatchHigh(value);
                _t1.Load();
                IFR = (byte)(IFR & ~T1Flag);
                break;
            case T1LatchHighRegister:
                _t1.WriteLatchHigh(value);
                IFR = (byte)(IFR & ~T1Flag);
                break;
            case T2LowRegister:
                _t2.LatchLow = value;
                break;
            case T2HighRegister:
                _t2.Load(value);
                IFR = (byte)(IFR & ~T2Flag);
                break;
            case SrRegister:
                _sr.Value = value;
                AccessShiftRegister();
                break;
            case AcrRegister:
                WriteAcr(value);
                break;
            case PcrRegister:
                PCR = value;
                _a.WriteControl((byte)(value & 0x0F));
                _b.WriteControl((byte)(value >> 4));
                break;
            case IfrRegister:
                IFR = (byte)(IFR & ~value & FlagMask);
                break;
            case IerRegister:
                IER = (value & IrqBit) != 0
                    ? (byte)((IER | value) & FlagMask)
                    : (byte)(IER & ~value & FlagMask);
                break;
        }
    }

    private void WriteAcr(byte value)
    {
        ACR = value;
        _sr.SetMode((ACR & SrModeMask) >> 2);
        if (!_sr.IsEnabled)
            IFR = (byte)(IFR & ~SrFlag);
        if ((ACR & PaLatchEnable) == 0)
            _paLatched = false;
        if ((ACR & PbLatchEnable) == 0)
            _pbLatched = false;
    }

    // IRA: the PA pin levels, or the levels latched at the last CA1 active transition until read.
    private byte ReadPortA()
    {
        var value = _paLatched ? _ira : Pins.PAIn;
        _paLatched = false;
        return value;
    }

    // IRB: ORB (PB7 from T1 when ACR7 is set) on output bits; pin levels (or the CB1 latch
    // until read) on input bits.
    private byte ReadPortB()
    {
        var inputs = _pbLatched ? _irb : Pins.PBIn;
        _pbLatched = false;
        return (byte)((PortBOutput & PortBDrive) | (inputs & ~PortBDrive));
    }

    private byte PortBDrive => (ACR & T1Pb7Output) != 0 ? (byte)(DDRB | Pb7) : DDRB;

    private byte PortBOutput => (ACR & T1Pb7Output) == 0
        ? ORB
        : (byte)((ORB & ~Pb7) | (_t1.Pb7 ? Pb7 : 0));

    private void AccessShiftRegister()
    {
        IFR = (byte)(IFR & ~SrFlag);
        _sr.Access();
        if (_sr.UsesT2)
            _t2.RestartShiftClock();
    }

    private void ClearPortAFlags()
    {
        IFR = (byte)(IFR & ~(_a.C2IsIndependent ? Ca1Flag : Ca1Flag | Ca2Flag));
    }

    private void ClearPortBFlags()
    {
        IFR = (byte)(IFR & ~(_b.C2IsIndependent ? Cb1Flag : Cb1Flag | Cb2Flag));
    }

    private void UpdateOutputs()
    {
        Pins.DataOut = _dataOut;
        Pins.DataDrive = _driveData ? (byte)0xFF : (byte)0x00;
        Pins.PAOut = ORA;
        Pins.PADrive = DDRA;
        Pins.PBOut = PortBOutput;
        Pins.PBDrive = PortBDrive;
        Pins.CA2Out = _a.C2Level;
        Pins.CA2Drive = _a.C2IsOutput;
        Pins.CB1Out = _sr.Cb1Level;
        Pins.CB1Drive = _sr.DrivesCb1;
        Pins.CB2Out = _sr.DrivesCb2 ? _sr.Cb2Level : _b.C2Level;
        Pins.CB2Drive = _sr.IsEnabled ? _sr.DrivesCb2 : _b.C2IsOutput;
        Pins.IRQB = !IrqActive;
    }
}
