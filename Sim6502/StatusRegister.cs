// Converted from StatusRegister.java
// Represents the status register for W65c02s
using Us.Retrocpu.Shared;

public class StatusRegister
{
    private readonly BitFlag _n;
    private readonly BitFlag _v;
    private readonly BitFlag _b;
    private readonly BitFlag _d;
    private readonly BitFlag _i;
    private readonly BitFlag _z;
    private readonly BitFlag _c;

    public StatusRegister()
    {
        _n = new BitFlag(false);
        _v = new BitFlag(false);
        _b = new BitFlag(true);
        _d = new BitFlag(false);
        _i = new BitFlag(true);
        _z = new BitFlag(false);
        _c = new BitFlag(false);
    }

    public override string ToString()
    {
        string result = _n.IsSet() ? "N" : "n";
        result += _v.IsSet() ? "V" : "v";
        result += "1";
        result += _b.IsSet() ? "B" : "b";
        result += _d.IsSet() ? "D" : "d";
        result += _i.IsSet() ? "I" : "i";
        result += _z.IsSet() ? "Z" : "z";
        result += _c.IsSet() ? "C" : "c";
        return result;
    }

    public UInt8 ToUInt8()
    {
        int result = 0;
        if (_n.IsSet()) result |= 0b10000000;
        if (_v.IsSet()) result |= 0b01000000;
        result |= 0b00100000;
        if (_b.IsSet()) result |= 0b00010000;
        if (_d.IsSet()) result |= 0b00001000;
        if (_i.IsSet()) result |= 0b00000100;
        if (_z.IsSet()) result |= 0b00000010;
        if (_c.IsSet()) result |= 0b00000001;
        return new UInt8(result);
    }

    public UInt8 GetFlags()
    {
        UInt8 result = new UInt8(0);
        if (_n.IsSet()) result.SetBit(7);
        if (_v.IsSet()) result.SetBit(6);
        result.SetBit(5);
        if (_b.IsSet()) result.SetBit(4);
        if (_d.IsSet()) result.SetBit(3);
        if (_i.IsSet()) result.SetBit(2);
        if (_z.IsSet()) result.SetBit(1);
        if (_c.IsSet()) result.SetBit(0);
        return result;
    }

    public void SetFlags(UInt8 value)
    {
        _n.UpdateValue(value.IsBitSet(7));
        _v.UpdateValue(value.IsBitSet(6));
        _b.UpdateValue(value.IsBitSet(4));
        _d.UpdateValue(value.IsBitSet(3));
        _i.UpdateValue(value.IsBitSet(2));
        _z.UpdateValue(value.IsBitSet(1));
        _c.UpdateValue(value.IsBitSet(0));
    }

    public BitFlag GetCarry() => _c;
    public void SetCarry() => _c.Set();
    public void ClearCarry() => _c.Clear();

    public BitFlag GetZero() => _z;
    public void SetZero() => _z.Set();
    public void ClearZero() => _z.Clear();

    public BitFlag GetIRQDisabled() => _i;
    public void SetIRQDisabled() => _i.Set();
    public void ClearIRQDisabled() => _i.Clear();

    public BitFlag GetDecimal() => _d;
    public void SetDecimal() => _d.Set();
    public void ClearDecimal() => _d.Clear();

    public BitFlag GetBreak() => _b;
    public void SetBreak() => _b.Set();
    public void ClearBreak() => _b.Clear();

    public BitFlag GetOverflow() => _v;
    public void SetOverflow() => _v.Set();
    public void ClearOverflow() => _v.Clear();

    public BitFlag GetNegative() => _n;
    public void SetNegative() => _n.Set();
    public void ClearNegative() => _n.Clear();
}
