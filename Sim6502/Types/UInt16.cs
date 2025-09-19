namespace Sim6502.types;

/// <summary>
/// Unsigned 16-bit integer with bitwise and arithmetic utilities.
/// </summary>
public class UInt16
{
    private readonly UInt8 _lsb = new UInt8();
    private readonly UInt8 _msb = new UInt8();

    public const int Max = 65535;
    public const int MaxBit = 16;

    public UInt16() { }
    public UInt16(int value) => UpdateValue(value);
    public UInt16(UInt8 lsb, UInt8 msb) => UpdateValue((msb.ToInt() << 8) | lsb.ToInt());
    public UInt16(UInt8 lsb) => UpdateValue(lsb.ToInt());
    public UInt16 Copy() => new UInt16(_lsb, _msb);

    public override bool Equals(object? o)
    {
        if (ReferenceEquals(this, o)) return true;
        if (o is not UInt16 that) return false;
        return ToInt() == that.ToInt();
    }

    public override int GetHashCode() => ToInt();
    public override string ToString() => $"0x{ToInt():X4}";
    public int ToInt() => (_msb.ToInt() << 8) + _lsb.ToInt();

    public void UpdateValue(int toValue)
    {
        if (toValue < 0 || toValue > Max)
            throw new ArgumentOutOfRangeException(nameof(toValue));
        _lsb.UpdateValue(toValue & 0xFF);
        _msb.UpdateValue((toValue & 0xFF00) >> 8);
    }

    public void UpdateValue(UInt16 toValue) => UpdateValue(toValue.ToInt());
    public bool EqualsZero() => ToInt() == 0;

    public bool IsBitSet(int bitIndex)
    {
        if (bitIndex < 0 || bitIndex >= MaxBit)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        int mask = 1 << bitIndex;
        return (ToInt() & mask) > 0;
    }

    public void SetBit(int bitIndex)
    {
        if (bitIndex < 0 || bitIndex >= MaxBit)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        int mask = 1 << bitIndex;
        UpdateValue(ToInt() | mask);
    }

    public void ClearBit(int bitIndex)
    {
        if (bitIndex < 0 || bitIndex >= MaxBit)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        int mask = ~(1 << bitIndex);
        UpdateValue(ToInt() & mask);
    }

    public int GetBitValue(int bitIndex)
    {
        if (bitIndex < 0 || bitIndex >= MaxBit)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        return IsBitSet(bitIndex) ? 1 : 0;
    }

    public void SetBitValue(int bitIndex, int toValue)
    {
        if (bitIndex < 0 || bitIndex >= MaxBit)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        if (toValue < 0 || toValue > 1)
            throw new ArgumentOutOfRangeException(nameof(toValue));
        if (toValue == 0)
            ClearBit(bitIndex);
        else
            SetBit(bitIndex);
    }

    public UInt8 Lsb() => _lsb;
    public UInt8 Msb() => _msb;

    public UInt16 Inc()
    {
        UpdateValue(ToInt() == Max ? 0 : ToInt() + 1);
        return this;
    }

    public UInt16 Dec()
    {
        UpdateValue(ToInt() == 0 ? Max : ToInt() - 1);
        return this;
    }

    public UInt16 AddUnsigned(UInt8 amount)
    {
        UpdateValue((ToInt() + amount.ToInt()) % Max);
        return this;
    }

    public UInt16 AddSigned(UInt8 amount)
    {
        var left = ToInt();
        var right = (sbyte)amount.ToInt();
        UpdateValue(left + right);
        return this;
    }
}
