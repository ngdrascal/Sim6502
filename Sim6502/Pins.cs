using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502;

public class Pins : IPinsInternal, IPinsExternal
{
    private readonly byte[] _pinValues;
    private readonly UInt8 _dbgInstReg = new();

    public Pins()
    {
        _pinValues = new byte[40];
        for (var i = 0; i < 40; i++)
        {
            _pinValues[i] = 0;
        }
    }

    private byte ReadPin(int pinNumber)
    {
        if (pinNumber < 1 || pinNumber > _pinValues.Length)
            throw new IndexOutOfRangeException("pinNumber");
        return _pinValues[pinNumber - 1];
    }

    private void WritePin(int pinNumber, byte value)
    {
        if (pinNumber < 1 || pinNumber > _pinValues.Length)
            throw new IndexOutOfRangeException("pinNumber");
        _pinValues[pinNumber - 1] = value;
    }

    public byte VPB
    {
        get => ReadPin(PinMap.VPB);
        set => WritePin(PinMap.VPB, value);
    }

    public RdyPinMode RdyPinMode { get; } = RdyPinMode.Input;

    public byte RDY
    {
        get => ReadPin(PinMap.RDY);
        set => WritePin(PinMap.RDY, value);
    }

    public byte PHI1O
    {
        get => ReadPin(PinMap.PHI1O);
        set => WritePin(PinMap.PHI1O, value);
    }

    public byte IRQB
    {
        get => ReadPin(PinMap.IRQB);
        set => WritePin(PinMap.IRQB, value);
    }

    public byte MLB
    {
        get => ReadPin(PinMap.MLB);
        set => WritePin(PinMap.MLB, value);
    }

    public byte NMIB
    {
        get => ReadPin(PinMap.NMIB);
        set => WritePin(PinMap.NMIB, value);
    }

    public byte SYNC
    {
        get => ReadPin(PinMap.SYNC);
        set => WritePin(PinMap.SYNC, value);
    }

    public AddrBusMode AddrBusMode { get; set; } = AddrBusMode.HighZ;

    public byte A0 => ReadPin(PinMap.VPB);

    public byte A1 => ReadPin(PinMap.A1);

    public byte A2 => ReadPin(PinMap.A2);

    public byte A3 => ReadPin(PinMap.A3);

    public byte A4 => ReadPin(PinMap.A4);

    public byte A5 => ReadPin(PinMap.A5);

    public byte A6 => ReadPin(PinMap.A6);

    public byte A7 => ReadPin(PinMap.A7);

    public byte A8 => ReadPin(PinMap.A8);

    public byte A9 => ReadPin(PinMap.A9);

    public byte A10 => ReadPin(PinMap.A10);

    public byte A11 => ReadPin(PinMap.A11);

    public byte A12 => ReadPin(PinMap.A12);

    public byte A13 => ReadPin(PinMap.A13);

    public byte A14 => ReadPin(PinMap.A14);

    public byte A15 => ReadPin(PinMap.A15);

    public UInt16 AddrBus
    {
        get
        {
            var addr = 0;
            for (var i = 0; i < PinMap.AddrPins.Length; i++)
            {
                var pv = ReadPin(PinMap.AddrPins[i]) << i;
                addr |= pv;
            }

            return new UInt16(addr);
        }
        set
        {
            var rawValue = value.ToInt();
            foreach (var pin in PinMap.AddrPins)
            {
                var pinValue = (byte)(rawValue & 0x01);
                WritePin(pin, pinValue);
                rawValue >>= 1;
            }
        }
    }

    public DataBusMode DataBusMode { get; set; } = DataBusMode.Input;

    public UInt8 DataBus
    {
        get
        {
            var data = 0;
            data |= D0;
            data |= D1 << 1;
            data |= D2 << 2;
            data |= D3 << 3;
            data |= D4 << 4;
            data |= D5 << 5;
            data |= D6 << 6;
            data |= D7 << 7;
            return new UInt8(data);
        }
        set
        {
            var rawValue = (byte)value.ToInt();
            WritePin(PinMap.D0, (byte)(rawValue & 0x01)); rawValue >>= 1;
            WritePin(PinMap.D1, (byte)(rawValue & 0x01)); rawValue >>= 1;
            WritePin(PinMap.D2, (byte)(rawValue & 0x01)); rawValue >>= 1;
            WritePin(PinMap.D3, (byte)(rawValue & 0x01)); rawValue >>= 1;
            WritePin(PinMap.D4, (byte)(rawValue & 0x01)); rawValue >>= 1;
            WritePin(PinMap.D5, (byte)(rawValue & 0x01)); rawValue >>= 1;
            WritePin(PinMap.D6, (byte)(rawValue & 0x01)); rawValue >>= 1;
            WritePin(PinMap.D7, (byte)(rawValue & 0x01));
        }
    }

    public byte D7
    {
        get => ReadPin(PinMap.D7);
        set => WritePin(PinMap.D7, value);
    }

    public byte D6
    {
        get => ReadPin(PinMap.D6);
        set => WritePin(PinMap.D6, value);
    }

    public byte D5
    {
        get => ReadPin(PinMap.D5);
        set => WritePin(PinMap.D5, value);
    }

    public byte D4
    {
        get => ReadPin(PinMap.D4);
        set => WritePin(PinMap.D4, value);
    }

    public byte D3
    {
        get => ReadPin(PinMap.D3);
        set => WritePin(PinMap.D3, value);
    }

    public byte D2
    {
        get => ReadPin(PinMap.D2);
        set => WritePin(PinMap.D2, value);
    }

    public byte D1
    {
        get => ReadPin(PinMap.D1);
        set => WritePin(PinMap.D1, value);
    }

    public byte D0
    {
        get => ReadPin(PinMap.D0);
        set => WritePin(PinMap.D0, value);
    }

    public byte RWB
    {
        get => ReadPin(PinMap.RWB);
        set => WritePin(PinMap.RWB, value);
    }

    public byte BE
    {
        get => ReadPin(PinMap.BE);
        set => WritePin(PinMap.BE, value);
    }

    public byte PHI2
    {
        get => ReadPin(PinMap.PHI2);
        set => WritePin(PinMap.PHI2, value);
    }

    public byte SOB
    {
        get => ReadPin(PinMap.SOB);
        set => WritePin(PinMap.SOB, value);
    }

    public byte PHI2O
    {
        get => ReadPin(PinMap.PHI2O);
        set => WritePin(PinMap.PHI2O, value);
    }

    public byte RESB
    {
        get => ReadPin(PinMap.RESB);
        set => WritePin(PinMap.RESB, value);
    }

    public byte DBGSTATE { get; set; }

    public byte DBGSUBSTEP { get; set; }

    public UInt8 DBGINST
    {
        get => _dbgInstReg;
        set => _dbgInstReg.UpdateValue(value);
    }
}
