using System.Diagnostics;

namespace W65C02S.Engine.Types;

/// <summary>
/// Unsigned 16-bit integer with bitwise and arithmetic utilities.
/// </summary>
/// <remarks>
/// An immutable value: every operation returns a new value and leaves this one unchanged, so a
/// result must be assigned back (e.g. <c>regs.PC = regs.PC.Inc()</c>). Construction from an int
/// keeps the low 16 bits.
/// </remarks>
public readonly struct UInt16 : IEquatable<UInt16>
{
    private readonly ushort _value;

    public const int Max = 65535;

    public const int MaxBit = 16;

    public UInt16(int value) => _value = (ushort)value;

    public UInt16(UInt8 lsb, UInt8 msb) => _value = (ushort)((msb.ToInt() << 8) | lsb.ToInt());

    public UInt16(UInt8 lsb) => _value = (ushort)lsb.ToInt();

    public static implicit operator UInt16(ushort value) => new(value);

    public static implicit operator ushort(UInt16 value) => value._value;

    public UInt16 Copy() => this;

    public bool Equals(UInt16 other) => _value == other._value;

    public override bool Equals(object? o) => o is UInt16 that && Equals(that);

    public override int GetHashCode() => _value;

    public override string ToString() => $"0x{_value:X4}";

    public int ToInt() => _value;

    public bool EqualsZero() => _value == 0;

    public bool IsBitSet(int bitIndex)
    {
        Debug.Assert(bitIndex is >= 0 and < MaxBit);

        return (_value & (1 << bitIndex)) != 0;
    }

    public UInt16 SetBit(int bitIndex)
    {
        Debug.Assert(bitIndex is >= 0 and < MaxBit);

        return new UInt16(_value | (1 << bitIndex));
    }

    public UInt16 ClearBit(int bitIndex)
    {
        Debug.Assert(bitIndex is >= 0 and < MaxBit);

        return new UInt16(_value & ~(1 << bitIndex));
    }

    public int GetBitValue(int bitIndex) => IsBitSet(bitIndex) ? 1 : 0;

    public UInt16 SetBitValue(int bitIndex, int toValue)
    {
        Debug.Assert(toValue is 0 or 1);

        return toValue == 0 ? ClearBit(bitIndex) : SetBit(bitIndex);
    }

    public UInt8 Lsb() => new(_value);

    public UInt8 Msb() => new(_value >> 8);

    public UInt16 WithLsb(UInt8 lsb) => new((_value & 0xFF00) | lsb.ToInt());

    public UInt16 WithMsb(UInt8 msb) => new((msb.ToInt() << 8) | (_value & 0x00FF));

    public UInt16 Inc() => new(_value + 1);

    public UInt16 Dec() => new(_value - 1);

    public UInt16 AddUnsigned(UInt8 amount) => new(_value + amount.ToInt());

    public UInt16 AddSigned(UInt8 amount) => new(_value + (sbyte)amount.ToInt());
}
