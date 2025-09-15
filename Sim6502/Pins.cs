// Converted from Pins.java
// Represents the pin logic for W65c02s
using System;
using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;

public class Pins : PinsInternalIntf, PinsExternalIntf
{
    private readonly byte[] _pinValues;
    private AddrBusMode _addrBusMode = AddrBusMode.Highz;
    private DataBusMode _dataBusMode = DataBusMode.Input;
    private RdyPinMode _rdyPinMode = RdyPinMode.Input;
    private readonly UInt8 _dbgInstReg = new UInt8();
    private byte _dbgState;
    private byte _dbgSubStep;

    public Pins()
    {
        _pinValues = new byte[40];
        for (int i = 0; i < 40; i++)
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

    public byte GetVPB() => ReadPin(PinMap.Vpb);

    public void SetVPB(byte value) => WritePin(PinMap.Vpb, value);

    public RdyPinMode GetRdyPinMode() => _rdyPinMode;

    public byte GetRDY() => ReadPin(PinMap.Rdy);

    public void SetRDY(byte value) => WritePin(PinMap.Rdy, value);

    public byte GetPHI1O() => ReadPin(PinMap.Phi1O);

    public void SetPHI1O(byte value) => WritePin(PinMap.Phi1O, value);

    public byte GetIRQB() => ReadPin(PinMap.Irqb);

    public void SetIRQB(byte value) => WritePin(PinMap.Irqb, value);

    public byte GetMLB() => ReadPin(PinMap.Mlb);

    public void SetMLB(byte value) => WritePin(PinMap.Mlb, value);

    public byte GetNMIB() => ReadPin(PinMap.Nmib);

    public void SetNMIB(byte value) => WritePin(PinMap.Nmib, value);

    public byte GetSYNC() => ReadPin(PinMap.Sync);

    public void SetSYNC(byte value) => WritePin(PinMap.Sync, value);

    public AddrBusMode GetAddrBusMode() => _addrBusMode;

    public void SetAddrBusMode(AddrBusMode mode) => _addrBusMode = mode;

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

    public UInt16 GetAddrBusPins()
    {
        int addr = 0;
        for (int i = 0; i < PinMap.AddrPins.Length; i++)
        {
            int pv = ReadPin(PinMap.AddrPins[i]) << i;
            addr |= pv;
        }
        return new UInt16(addr);
    }

    public void SetAddrBusPins(UInt16 value)
    {
        int rawValue = value.ToInt();
        for (int i = 0; i < PinMap.AddrPins.Length; i++)
        {
            byte pinValue = (byte)(rawValue & 0x01);
            WritePin(PinMap.AddrPins[i], pinValue);
            rawValue >>= 1;
        }
    }

    public DataBusMode GetDataBusMode() => _dataBusMode;

    public void SetDataBusMode(DataBusMode mode) => _dataBusMode = mode;

    public UInt8 GetDataBusPins()
    {
        int data = 0;
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

    public void SetDataBusPins(UInt8 value)
    {
        byte rawValue = (byte)value.ToInt();
        WritePin(PinMap.D0, (byte)(rawValue & 0x01)); rawValue >>= 1;
        WritePin(PinMap.D1, (byte)(rawValue & 0x01)); rawValue >>= 1;
        WritePin(PinMap.D2, (byte)(rawValue & 0x01)); rawValue >>= 1;
        WritePin(PinMap.D3, (byte)(rawValue & 0x01)); rawValue >>= 1;
        WritePin(PinMap.D4, (byte)(rawValue & 0x01)); rawValue >>= 1;
        WritePin(PinMap.D5, (byte)(rawValue & 0x01)); rawValue >>= 1;
        WritePin(PinMap.D6, (byte)(rawValue & 0x01)); rawValue >>= 1;
        WritePin(PinMap.D7, (byte)(rawValue & 0x01));
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

    public byte GetRWB() => ReadPin(PinMap.Rwb);
    public void SetRWB(byte value) => WritePin(PinMap.Rwb, value);
    public byte GetBE() => ReadPin(PinMap.Be);
    public void SetBE(byte value) => WritePin(PinMap.Be, value);
    public byte GetPHI2() => ReadPin(PinMap.Phi2);
    public void SetPHI2(byte value) => WritePin(PinMap.Phi2, value);
    public byte GetSOB() => ReadPin(PinMap.Sob);
    public void SetSOB(byte value) => WritePin(PinMap.Sob, value);
    public byte GetPHI2O() => ReadPin(PinMap.Phi2O);
    public void SetPHI2O(byte value) => WritePin(PinMap.Phi2O, value);
    public byte GetRESB() => ReadPin(PinMap.Resb);
    public void SetRESB(byte value) => WritePin(PinMap.Resb, value);

    public byte GetDBGSTATE() => _dbgState;
    public void SetDBGSTATE(byte value) => _dbgState = value;
    public byte GetDBGSUBSTEP() => _dbgSubStep;
    public void SetDBGSUBSTEP(byte value) => _dbgSubStep = value;
    public UInt8 GetDBGINST() => _dbgInstReg;
    public void SetDBGINST(UInt8 value) => _dbgInstReg.UpdateValue(value);
}
