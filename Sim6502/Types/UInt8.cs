namespace Sim6502.types;

/// <summary>
/// Unsigned 8-bit integer with bitwise and arithmetic utilities.
/// </summary>
public class UInt8
{
    private const int MaxBit = 8;
    private int _value;
    public const int Max = 256;

    public UInt8() => UpdateValue(0);

    public UInt8(int value) => UpdateValue(value);

    public UInt8 Copy() => new UInt8(_value);

    public override bool Equals(object? o)
    {
        if (ReferenceEquals(this, o))
            return true;

        if (o is not UInt8 that)
            return false;

        return _value == that._value;
    }

    public override int GetHashCode() => ToInt();
    
    public override string ToString() => $"0x{_value:X2}";
    
    public int ToInt() => _value;

    public void UpdateValue(int toValue)
    {
        if (toValue < 0 || toValue > 255)
            throw new ArgumentOutOfRangeException(nameof(toValue));
        _value = toValue;
    }

    public void UpdateValue(UInt8 toValue) => _value = toValue._value;
    
    public bool EqualsZero() => _value == 0;

    public bool IsBitSet(int bitIndex)
    {
        if (bitIndex < 0 || bitIndex >= MaxBit)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        int mask = 1 << bitIndex;
        return (_value & mask) > 0;
    }

    public void SetBit(int bitIndex)
    {
        if (bitIndex < 0 || bitIndex >= MaxBit)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        int mask = 1 << bitIndex;
        _value |= mask;
    }

    public void ClearBit(int bitIndex)
    {
        if (bitIndex < 0 || bitIndex >= MaxBit)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        int mask = ~(1 << bitIndex);
        _value &= mask;
    }

    public int GetBitValue(int bitIndex)
    {
        if (bitIndex < 0 || bitIndex >= MaxBit)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        return IsBitSet(bitIndex) ? 1 : 0;
    }

    public BitFlag GetBitFlag(int bitIndex)
    {
        if (bitIndex < 0 || bitIndex >= MaxBit)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        return IsBitSet(bitIndex) ? BitFlag.High() : BitFlag.Low();
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

    public UInt8 Zero()
    {
        _value = 0;
        return this;
    }

    public UInt8 Inc()
    {
        _value = (_value == 255) ? 0 : _value + 1;
        return this;
    }

    public UInt8 Dec()
    {
        _value = (_value == 0) ? 255 : _value - 1;
        return this;
    }

    public UInt8 Negate()
    {
        _value = -_value & 0xFF;
        return this;
    }

    public UInt8 Shl()
    {
        _value = (_value << 1) & 0xFF;
        return this;
    }

    public UInt8 Shl(BitFlag newBitValue)
    {
        Shl();
        if (newBitValue.IsCleared())
            _value &= 0xFE;
        else
            _value |= 0x01;
        return this;
    }

    public UInt8 Shr()
    {
        _value = _value >> 1;
        return this;
    }

    public UInt8 Shr(BitFlag newBitValue)
    {
        _value = _value >> 1;
        if (newBitValue.IsCleared())
            _value &= 0x7F;
        else
            _value |= 0x80;
        return this;
    }

    public UInt8 And(UInt8 operand2)
    {
        _value &= operand2._value;
        return this;
    }

    public UInt8 Or(UInt8 operand2)
    {
        _value |= operand2._value;
        return this;
    }

    public UInt8 Xor(UInt8 operand2)
    {
        _value ^= operand2._value;
        return this;
    }

    public UInt8 Not()
    {
        _value = ~_value & 0xFF;
        return this;
    }

    public int Parity()
    {
        var parity = 0;
        var temp = _value;
        while (temp != 0)
        {
            parity ^= temp & 1;
            temp >>= 1;
        }
        return parity;
    }

    public UInt8 AddWithWrapAround(UInt8 operand2)
    {
        _value = (_value + operand2._value) % Max;
        return this;
    }

    private MathResult AdcDecimal(UInt8 operand, BitFlag carryIn)
    {
        int right = operand._value;
        int halfCarry = 0;
        int carryOut = 0;

        int lowNibble = (_value & 0x0F) + (right & 0x0F) + (carryIn.ToInt());
        if (lowNibble >= 10)
        {
            lowNibble = (lowNibble + 6) & 0x0F;
            halfCarry = 1;
        }

        int highNibble = ((_value & 0xF0) >> 4) + ((right & 0xF0) >> 4) + halfCarry;
        if (highNibble >= 10)
        {
            highNibble = (highNibble + 6) & 0x0F;
            carryOut = 1;
        }

        BitFlag vFlag = CalcADCOverflow(_value, right, carryIn.ToInt());

        int result = (highNibble << 4) | (lowNibble);
        _value = result & 0xFF;

        BitFlag cFlag = new BitFlag(carryOut == 1);
        BitFlag zFlag = new BitFlag(result == 0);
        BitFlag nFlag = new BitFlag((result & 0x80) > 0);

        return new MathResult(new UInt8(_value), nFlag, vFlag, zFlag, cFlag);
    }

    private BitFlag CalcADCOverflow(int left, int right, int carryIn)
    {
        var lowNibble = (left & 0x0F) + (right & 0x0F) + carryIn;
        if (lowNibble >= 10)
            lowNibble = ((lowNibble + 6) & 0x0F) + 0x10;

        var result = (sbyte)(left & 0xF0) + (sbyte)(right & 0xF0) + (sbyte)lowNibble;

        return new BitFlag(result < -128 || result > 127);
    }

    private MathResult AdcBinary(UInt8 operand, BitFlag carryIn)
    {
        var right = operand._value;
        var carry = carryIn.ToInt();

        var result = _value + right + carry;

        var cFlag = new BitFlag(result > 255);

        var rightSign = (right & 0x80) != 0 ? 1 : 0;
        var valueSign = (_value & 0x80) != 0 ? 1 : 0;
        var resultSign = (result & 0x80) != 0 ? 1 : 0;
        var vFlag = new BitFlag(((rightSign ^ resultSign) & (valueSign ^ resultSign)) != 0);

        var zFlag = new BitFlag(result == 0);
        var nFlag = new BitFlag((result & 0x80) > 0);

        _value = result & 0x00FF;

        return new MathResult(this, nFlag, vFlag, zFlag, cFlag);
    }

    public MathResult Adc(UInt8 operand, BitFlag carryIn, BitFlag decimalMode)
    {
        if (decimalMode.IsSet())
            return AdcDecimal(operand, carryIn);
        else
            return AdcBinary(operand, carryIn);
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
        var N1 = (sbyte)_value;
        var N2 = (sbyte)operand.ToInt();

        // N2L = N2 & $0F
        var N2L = (sbyte)(N2 & 0x0F);

        var N2H = new sbyte[2];

        // N2H = N2 & $F0
        N2H[0] = (sbyte)(N2 & 0xF0);

        // N2H+1 = (N2 & $F0) + $0F
        N2H[1] = (sbyte)((N2 & 0xF0) + 0x0F);

        // N1L = N1 & $0F
        var N1L = (sbyte)(N1 & 0x0F);

        // N1H = N1 & $F0
        var N1H = (sbyte)(N1 & 0xF0);

        int C;
        int A;
        int X;
        int oldA;

        C = carryIn.ToInt();
        A = N1L;

        oldA = (A & 0xFF);
        A = A - N2L - (1 - C);
        C = CalcCarry(oldA, N2L, C);
        X = 0;

        if (C == 0)
        {
            X++;
            A &= 0x0F;
            C = 0;
        }

        A |= (N1H);

        oldA = (A & 0xFF);
        A = A - N2H[X] - (1 - C);
        C = CalcCarry(oldA, N2H[X], C);

        if (C == 0)
        {
            oldA = (A & 0xFF);
            A = A - 0x5F - (1 - C);
            C = CalcCarry(oldA, 0x5F, C);
        }

        if (X != 0)
        {
            C = 1;
            oldA = (A & 0xFF);
            A = A - 0x06 - (1 - C);
            C = CalcCarry(oldA, 0x06, C);
        }

        A &= 0xFF;
        _value = A;

        var cFlag = CalcCarry(N1, N2, carryIn.ToInt()) == 1;
        var vFlag = CalcSBCOverflow(N1, N2, carryIn.ToInt());
        var zFlag = _value == 0;
        var nFlag = (_value & 0x80) == 0x80;

        return new MathResult(new UInt8(_value), new BitFlag(nFlag), new BitFlag(vFlag),
              new BitFlag(zFlag), new BitFlag(cFlag));
    }

    private int CalcCarry(int n1, int n2, int carry)
    {
        int binaryResult = (n1 & 0xFF) + ((n2 & 0x0FF) ^ 0xFF) + carry;
        return binaryResult > 0xFF ? 1 : 0;
    }

    private bool CalcSBCOverflow(sbyte n1, sbyte n2, int carry)
    {
        int binaryResult = n1 - n2 - (1 - carry);
        return binaryResult < -128 || binaryResult > 127;
    }

    private MathResult SbcBinary(UInt8 operand, BitFlag carryIn)
    {
        int diff;
        int data = operand.Copy().Not().ToInt();

        diff = (_value) + (data) + (carryIn.ToInt());

        bool vFlag = (~(_value ^ data) & (_value ^ diff) & 0x80) == 0x80;
        bool cFlag = diff > 0xFF;
        bool zFlag = (diff & 0xFF) == 0;
        bool nFlag = (diff & 0x80) == 0x80;

        _value = diff & 0xFF;

        return new MathResult(new UInt8(_value), new BitFlag(nFlag), new BitFlag(vFlag),
              new BitFlag(zFlag), new BitFlag(cFlag));
    }

    public MathResult Sbc(UInt8 operand, BitFlag carryIn, BitFlag decimalMode)
    {
        if (decimalMode.IsSet())
            return SbcDecimal(operand, carryIn);
        else
            return SbcBinary(operand, carryIn);
    }
}
