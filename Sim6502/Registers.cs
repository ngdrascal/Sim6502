// Converted from Registers.java
// Represents the registers for W65c02s
using Us.Retrocpu.Shared;
using Us.Retrocpu.W65c02s;

public class Registers
{
    private readonly UInt8 _a;
    private readonly UInt8 _x;
    private readonly UInt8 _y;
    private readonly UInt16 _pc;
    private readonly UInt8 _s;
    private readonly StatusRegister _p;
    private readonly UInt8 _inst;
    private readonly UInt16 _ea;
    private readonly UInt16 _ea2;
    private readonly UInt8 _temp;

    public Registers(StatusRegister statusReg)
    {
        _p = statusReg;
        _a = new UInt8(0);
        _x = new UInt8(0);
        _y = new UInt8(0);
        _pc = new UInt16(0);
        _s = new UInt8(0xFD);
        _inst = OpCodes.Nop.ToByte();
        _ea = new UInt16(0);
        _ea2 = new UInt16(0);
        _temp = new UInt8(0);
    }

    public UInt8 GetA() => _a;

    public void UpdateAUpdateFlags(UInt8 value)
    {
        GetA().UpdateValue(value);
        if (value.EqualsZero())
        {
            _p.SetZero();
            _p.ClearNegative();
        }
        else if (value.IsBitSet(7))
        {
            _p.ClearZero();
            _p.SetNegative();
        }
        else
        {
            _p.ClearZero();
            _p.ClearNegative();
        }
    }

    public UInt8 GetX() => _x;

    public void SetXUpdateFlags(UInt8 value)
    {
        GetX().UpdateValue(value);
        if (value.EqualsZero())
        {
            _p.SetZero();
            _p.ClearNegative();
        }
        else if (value.IsBitSet(7))
        {
            _p.ClearZero();
            _p.SetNegative();
        }
        else
        {
            _p.ClearZero();
            _p.ClearNegative();
        }
    }

    public UInt8 GetY() => _y;

    public void SetYUpdateFlags(UInt8 value)
    {
        GetY().UpdateValue(value);
        if (value.EqualsZero())
        {
            _p.SetZero();
            _p.ClearNegative();
        }
        else if (value.IsBitSet(7))
        {
            _p.ClearZero();
            _p.SetNegative();
        }
        else
        {
            _p.ClearZero();
            _p.ClearNegative();
        }
    }

    public UInt16 GetPC() => _pc;

    public UInt8 GetS() => _s;

    public StatusRegister GetP() => _p;

    public UInt8 GetInst() => _inst;

    public void IncEALWithX() => _ea.Lsb().AddWithWrapAround(_x);

    public void IncEALWithY() => _ea.Lsb().AddWithWrapAround(_y);

    public UInt16 GetEA() => _ea;

    public void IncEAWithX() => _ea.AddUnsigned(_x);

    public void IncEAWithY() => _ea.AddUnsigned(_y);

    public UInt16 GetEA2() => _ea2;

    public void CopyEA2ToEA() => _ea.UpdateValue(_ea2);

    public UInt8 GetTemp() => _temp;
}
