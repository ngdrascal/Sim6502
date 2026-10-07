using System.Diagnostics;

namespace W65C02S.Engine.Types;

/// <summary>
/// Unsigned 8-bit integer with bitwise and arithmetic utilities.
/// </summary>
/// <remarks>
/// An immutable value: every operation returns a new value and leaves this one unchanged, so a
/// result must be assigned back (e.g. <c>regs.S = regs.S.Inc()</c>). Construction from an int keeps
/// the low 8 bits.
/// </remarks>
public readonly struct UInt8 : IEquatable<UInt8>
{
    private readonly byte _value;

    public const int Max = 256;

    public const int MaxBit = 8;

    public UInt8(int value) => _value = (byte)value;

    public static implicit operator UInt8(byte value) => new(value);

    public static implicit operator byte(UInt8 value) => value._value;

    public UInt8 Copy() => this;

    public bool Equals(UInt8 other) => _value == other._value;

    public override bool Equals(object? o) => o is UInt8 that && Equals(that);

    public override int GetHashCode() => _value;

    public override string ToString() => $"0x{_value:X2}";

    public int ToInt() => _value;

    public bool EqualsZero() => _value == 0;

    public bool IsBitSet(int bitIndex)
    {
        Debug.Assert(bitIndex is >= 0 and < MaxBit);

        return (_value & (1 << bitIndex)) != 0;
    }

    public UInt8 SetBit(int bitIndex)
    {
        Debug.Assert(bitIndex is >= 0 and < MaxBit);

        return new UInt8(_value | (1 << bitIndex));
    }

    public UInt8 ClearBit(int bitIndex)
    {
        Debug.Assert(bitIndex is >= 0 and < MaxBit);

        return new UInt8(_value & ~(1 << bitIndex));
    }

    public int GetBitValue(int bitIndex) => IsBitSet(bitIndex) ? 1 : 0;

    public BitFlag GetBitFlag(int bitIndex) => new(IsBitSet(bitIndex));

    public UInt8 SetBitValue(int bitIndex, int toValue)
    {
        Debug.Assert(toValue is 0 or 1);

        return toValue == 0 ? ClearBit(bitIndex) : SetBit(bitIndex);
    }

    public UInt8 Zero() => default;

    public UInt8 Inc() => new(_value + 1);

    public UInt8 Dec() => new(_value - 1);

    public UInt8 Shl() => new(_value << 1);

    public UInt8 Shl(BitFlag newBitValue) => new((_value << 1) | newBitValue.ToInt());

    public UInt8 Shr() => new(_value >> 1);

    public UInt8 Shr(BitFlag newBitValue) => new((_value >> 1) | (newBitValue.ToInt() << 7));

    public UInt8 And(UInt8 operand2) => new(_value & operand2._value);

    public UInt8 Or(UInt8 operand2) => new(_value | operand2._value);

    public UInt8 Xor(UInt8 operand2) => new(_value ^ operand2._value);

    public UInt8 Not() => new(~_value);

    public int Parity()
    {
        var parity = 0;
        var temp = (int)_value;
        while (temp != 0)
        {
            parity ^= temp & 1;
            temp >>= 1;
        }

        return parity;
    }

    public UInt8 AddWithWrapAround(UInt8 operand2) => new(_value + operand2._value);

    private MathResult AdcDecimal(UInt8 operand, BitFlag carryIn)
    {
        var left = (int)_value;
        var right = (int)operand._value;
        var halfCarry = 0;
        var carryOut = 0;

        var lowNibble = (left & 0x0F) + (right & 0x0F) + (carryIn.ToInt());
        if (lowNibble >= 10)
        {
            lowNibble = (lowNibble + 6) & 0x0F;
            halfCarry = 1;
        }

        var highNibble = ((left & 0xF0) >> 4) + ((right & 0xF0) >> 4) + halfCarry;
        if (highNibble >= 10)
        {
            highNibble = (highNibble + 6) & 0x0F;
            carryOut = 1;
        }

        var vFlag = CalcADCOverflow(left, right, carryIn.ToInt());

        var result = (highNibble << 4) | (lowNibble);

        var cFlag = new BitFlag(carryOut == 1);
        var zFlag = new BitFlag(result == 0);
        var nFlag = new BitFlag((result & 0x80) > 0);

        return new MathResult(new UInt8(result), nFlag, vFlag, zFlag, cFlag);
    }

    private static BitFlag CalcADCOverflow(int left, int right, int carryIn)
    {
        var lowNibble = (left & 0x0F) + (right & 0x0F) + carryIn;
        if (lowNibble >= 10)
            lowNibble = ((lowNibble + 6) & 0x0F) + 0x10;

        var result = (sbyte)(left & 0xF0) + (sbyte)(right & 0xF0) + (sbyte)lowNibble;

        return new BitFlag(result is < -128 or > 127);
    }

    private MathResult AdcBinary(UInt8 operand, BitFlag carryIn)
    {
        var left = (int)_value;
        var right = (int)operand._value;
        var carry = carryIn.ToInt();

        var result = left + right + carry;

        var cFlag = new BitFlag(result > 255);

        var rightSign = (right & 0x80) != 0 ? 1 : 0;
        var valueSign = (left & 0x80) != 0 ? 1 : 0;
        var resultSign = (result & 0x80) != 0 ? 1 : 0;
        var vFlag = new BitFlag(((rightSign ^ resultSign) & (valueSign ^ resultSign)) != 0);

        var zFlag = new BitFlag(result == 0);
        var nFlag = new BitFlag((result & 0x80) > 0);

        return new MathResult(new UInt8(result), nFlag, vFlag, zFlag, cFlag);
    }

    public MathResult Adc(UInt8 operand, BitFlag carryIn, BitFlag decimalMode)
    {
        return decimalMode.IsSet() ? AdcDecimal(operand, carryIn) : AdcBinary(operand, carryIn);
    }

    private MathResult SbcDecimal(UInt8 operand, BitFlag carryIn)
    {
        /*
         * The rules for binary subtraction are:
         * 0 ? 0 = 0
         * 0 ? 1 = 0 Carry-1
         * 1 - 0 = 1
         * 1 ? 1 = 0
         *
         *
         * SUB2 CPY #1 ; set carry if Y = 1, clear carry if Y = 0
         * LDA N1L
         * SBC N2L
         * LDX #0 ; point to the lower nibble of N2H
         * BCS S21 ; C == 0 means the A reg < memory (branch if diff >= 0)
         * INX ; point to the upper nibble of N2H
         * AND #$0F ; zero out the upper nibble of N1
         * CLC ;
         * S21 ORA N1H
         * ;
         * ; if (N1L - N2L >= 0) then subtract N2 & $F0
         * ; if (N1L - N2L < 0) then subtract (N2 & $F0) + $0F + 1 (carry is clear)
         * ;
         * SBC N2H,X
         * BCS S22
         * SBC #$5F ; subtract $60 (carry is clear)
         * S22 CPX #0
         * BEQ S23
         * SBC #6
         * S23 STA AR ; predicted accumulator result
         * RTS
         */
        // ReSharper disable InconsistentNaming
        var N1 = (sbyte)_value;

        var N2 = (sbyte)operand.ToInt();

        // N2L = N2 & $0F
        var N2L = (sbyte)(N2 & 0x0F);

        // N2H = N2 & $F0
        var N2H0 = (sbyte)(N2 & 0xF0);

        // N2H+1 = (N2 & $F0) + $0F
        var N2H1 = (sbyte)((N2 & 0xF0) + 0x0F);

        // N1L = N1 & $0F
        var N1L = (sbyte)(N1 & 0x0F);

        // N1H = N1 & $F0
        var N1H = (sbyte)(N1 & 0xF0);

        // ReSharper restore InconsistentNaming
        var c = carryIn.ToInt();
        int a = N1L;

        var oldA = (a & 0xFF);
        a = a - N2L - (1 - c);
        c = CalcCarry(oldA, N2L, c);
        var x = 0;

        if (c == 0)
        {
            x++;
            a &= 0x0F;
            c = 0;
        }

        a |= (byte)(N1H);

        var n2H = x == 0 ? N2H0 : N2H1;
        oldA = (a & 0xFF);
        a = a - n2H - (1 - c);
        c = CalcCarry(oldA, n2H, c);

        if (c == 0)
        {
            a = a - 0x5F - 1;
        }

        if (x != 0)
        {
            a = a - 0x06;
        }

        a &= 0xFF;

        var cFlag = CalcCarry(N1, N2, carryIn.ToInt()) == 1;
        var vFlag = CalcSBCOverflow(N1, N2, carryIn.ToInt());
        var zFlag = a == 0;
        var nFlag = (a & 0x80) == 0x80;

        return new MathResult(new UInt8(a), new BitFlag(nFlag), new BitFlag(vFlag),
              new BitFlag(zFlag), new BitFlag(cFlag));
    }

    private static int CalcCarry(int n1, int n2, int carry)
    {
        var binaryResult = (n1 & 0xFF) + ((n2 & 0x0FF) ^ 0xFF) + carry;
        return binaryResult > 0xFF ? 1 : 0;
    }

    private static bool CalcSBCOverflow(sbyte n1, sbyte n2, int carry)
    {
        var binaryResult = n1 - n2 - (1 - carry);
        return binaryResult is < -128 or > 127;
    }

    private MathResult SbcBinary(UInt8 operand, BitFlag carryIn)
    {
        var left = (int)_value;
        var data = operand.Not().ToInt();

        var diff = left + data + carryIn.ToInt();

        var vFlag = (~(left ^ data) & (left ^ diff) & 0x80) == 0x80;
        var cFlag = diff > 0xFF;
        var zFlag = (diff & 0xFF) == 0;
        var nFlag = (diff & 0x80) == 0x80;

        return new MathResult(new UInt8(diff), new BitFlag(nFlag), new BitFlag(vFlag),
              new BitFlag(zFlag), new BitFlag(cFlag));
    }

    public MathResult Sbc(UInt8 operand, BitFlag carryIn, BitFlag decimalMode)
    {
        return decimalMode.IsSet() ? SbcDecimal(operand, carryIn) : SbcBinary(operand, carryIn);
    }
}
