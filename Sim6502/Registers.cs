using System.Diagnostics.CodeAnalysis;
using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502;

[SuppressMessage("ReSharper", "InconsistentNaming")]
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
        _inst = OpCodes.NOP.ToUInt8();
        _ea = new UInt16(0);
        _ea2 = new UInt16(0);
        _temp = new UInt8(0);
    }

    public UInt8 A => _a;

    public void UpdateAUpdateFlags(UInt8 value)
    {
        A.UpdateValue(value);
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

    public UInt8 X => _x;

    public void SetXUpdateFlags(UInt8 value)
    {
        X.UpdateValue(value);
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

    public UInt8 Y => _y;

    public void SetYUpdateFlags(UInt8 value)
    {
        Y.UpdateValue(value);
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

    public UInt16 PC => _pc;

    public UInt8 S => _s;

    public StatusRegister P => _p;

    public UInt8 Inst => _inst;

    public void IncEALWithX() => _ea.Lsb().AddWithWrapAround(_x);

    public void IncEALWithY() => _ea.Lsb().AddWithWrapAround(_y);

    public UInt16 EA => _ea;

    public void IncEAWithX() => _ea.AddUnsigned(_x);

    public void IncEAWithY() => _ea.AddUnsigned(_y);

    public UInt16 EA2 => _ea2;

    public void CopyEA2ToEA() => _ea.UpdateValue(_ea2);

    public UInt8 Temp => _temp;

    public override string ToString()
    {
        return $"A:{A.ToInt():X2} P:{P} X:{X.ToInt():X2} Y:{Y.ToInt():X2} S:{S.ToInt():X2} PC:{PC.ToInt():X4}";
    }
}