using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine;

/// <summary>
/// The 40 pins of the W65C02S. Single pins hold 0 or 1; the address and data buses are held as
/// whole values and the individual A0-A15 and D0-D7 pins are views onto their bits.
/// </summary>
public class Pins : IPinsInternal, IPinsExternal
{
    private readonly UInt8 _dbgInstReg = new();

    private int _addr;
    private int _data;

    public byte VPB { get; set; }

    public RdyPinMode RdyPinMode { get; } = RdyPinMode.Input;

    public byte RDY { get; set; }

    public byte PHI1O { get; set; }

    public byte IRQB { get; set; }

    public byte MLB { get; set; }

    public byte NMIB { get; set; }

    public byte SYNC { get; set; }

    public AddrBusMode AddrBusMode { get; set; } = AddrBusMode.HighZ;

    public byte A0 => AddrBit(0);

    public byte A1 => AddrBit(1);

    public byte A2 => AddrBit(2);

    public byte A3 => AddrBit(3);

    public byte A4 => AddrBit(4);

    public byte A5 => AddrBit(5);

    public byte A6 => AddrBit(6);

    public byte A7 => AddrBit(7);

    public byte A8 => AddrBit(8);

    public byte A9 => AddrBit(9);

    public byte A10 => AddrBit(10);

    public byte A11 => AddrBit(11);

    public byte A12 => AddrBit(12);

    public byte A13 => AddrBit(13);

    public byte A14 => AddrBit(14);

    public byte A15 => AddrBit(15);

    public UInt16 AddrBus
    {
        get => new(_addr);
        set => _addr = value.ToInt();
    }

    public DataBusMode DataBusMode { get; set; } = DataBusMode.Input;

    public UInt8 DataBus
    {
        get => new(_data);
        set => _data = value.ToInt();
    }

    public byte D7
    {
        get => DataBit(7);
        set => SetDataBit(7, value);
    }

    public byte D6
    {
        get => DataBit(6);
        set => SetDataBit(6, value);
    }

    public byte D5
    {
        get => DataBit(5);
        set => SetDataBit(5, value);
    }

    public byte D4
    {
        get => DataBit(4);
        set => SetDataBit(4, value);
    }

    public byte D3
    {
        get => DataBit(3);
        set => SetDataBit(3, value);
    }

    public byte D2
    {
        get => DataBit(2);
        set => SetDataBit(2, value);
    }

    public byte D1
    {
        get => DataBit(1);
        set => SetDataBit(1, value);
    }

    public byte D0
    {
        get => DataBit(0);
        set => SetDataBit(0, value);
    }

    public byte RWB { get; set; }

    public byte BE { get; set; }

    public byte PHI2 { get; set; }

    public byte SOB { get; set; }

    public byte PHI2O { get; set; }

    public byte RESB { get; set; }

    public byte DBGSTATE { get; set; }

    public byte DBGSUBSTEP { get; set; }

    public UInt8 DBGINST
    {
        get => _dbgInstReg;
        set => _dbgInstReg.UpdateValue(value);
    }

    private byte AddrBit(int bit) => (byte)((_addr >> bit) & 1);

    private byte DataBit(int bit) => (byte)((_data >> bit) & 1);

    private void SetDataBit(int bit, byte value)
    {
        _data = (_data & ~(1 << bit)) | ((value & 1) << bit);
    }
}
