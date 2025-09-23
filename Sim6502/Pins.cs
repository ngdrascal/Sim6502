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

    public byte GetA0() => ReadPin(PinMap.A0);
    public byte GetA1() => ReadPin(PinMap.A1);
    public byte GetA2() => ReadPin(PinMap.A2);
    public byte GetA3() => ReadPin(PinMap.A3);
    public byte GetA4() => ReadPin(PinMap.A4);
    public byte GetA5() => ReadPin(PinMap.A5);
    public byte GetA6() => ReadPin(PinMap.A6);
    public byte GetA7() => ReadPin(PinMap.A7);
    public byte GetA8() => ReadPin(PinMap.A8);
    public byte GetA9() => ReadPin(PinMap.A9);
    public byte GetA10() => ReadPin(PinMap.A10);
    public byte GetA11() => ReadPin(PinMap.A11);
    public byte GetA12() => ReadPin(PinMap.A12);
    public byte GetA13() => ReadPin(PinMap.A13);
    public byte GetA14() => ReadPin(PinMap.A14);
    public byte GetA15() => ReadPin(PinMap.A15);

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
            data |= GetD0();
            data |= GetD1() << 1;
            data |= GetD2() << 2;
            data |= GetD3() << 3;
            data |= GetD4() << 4;
            data |= GetD5() << 5;
            data |= GetD6() << 6;
            data |= GetD7() << 7;
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

    public byte GetD7() => ReadPin(PinMap.D7);
    public void SetD7(byte value) => WritePin(PinMap.D7, value);
    public byte GetD6() => ReadPin(PinMap.D6);
    public void SetD6(byte value) => WritePin(PinMap.D6, value);
    public byte GetD5() => ReadPin(PinMap.D5);
    public void SetD5(byte value) => WritePin(PinMap.D5, value);
    public byte GetD4() => ReadPin(PinMap.D4);
    public void SetD4(byte value) => WritePin(PinMap.D4, value);
    public byte GetD3() => ReadPin(PinMap.D3);
    public void SetD3(byte value) => WritePin(PinMap.D3, value);
    public byte GetD2() => ReadPin(PinMap.D2);
    public void SetD2(byte value) => WritePin(PinMap.D2, value);
    public byte GetD1() => ReadPin(PinMap.D1);
    public void SetD1(byte value) => WritePin(PinMap.D1, value);
    public byte GetD0() => ReadPin(PinMap.D0);
    public void SetD0(byte value) => WritePin(PinMap.D0, value);

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
