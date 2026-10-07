// Represents the status register for W65c02s

using W65C02S.Engine.Types;

namespace W65C02S.Engine;

public class StatusRegister
{
    public StatusRegister()
    {
        Break = BitFlag.High();
        IRQDisabled = BitFlag.High();
    }

    public override string ToString()
    {
        var result = Negative.IsSet() ? "N" : "n";
        result += Overflow.IsSet() ? "V" : "v";
        result += "1";
        result += Break.IsSet() ? "B" : "b";
        result += Decimal.IsSet() ? "D" : "d";
        result += IRQDisabled.IsSet() ? "I" : "i";
        result += Zero.IsSet() ? "Z" : "z";
        result += Carry.IsSet() ? "C" : "c";
        return result;
    }

    public UInt8 ToUInt8()
    {
        var result = 0;
        if (Negative.IsSet()) result |= 0b10000000;
        if (Overflow.IsSet()) result |= 0b01000000;
        result |= 0b00100000;
        if (Break.IsSet()) result |= 0b00010000;
        if (Decimal.IsSet()) result |= 0b00001000;
        if (IRQDisabled.IsSet()) result |= 0b00000100;
        if (Zero.IsSet()) result |= 0b00000010;
        if (Carry.IsSet()) result |= 0b00000001;
        return new UInt8(result);
    }

    public void ResetFlags()
    {
        // n v 1 B d I z c
        ClearNegative();
        ClearOverflow();
        SetBreak();
        ClearDecimal();
        SetIRQDisabled();
        ClearZero();
        ClearCarry();
    }

    public void SetFlags(UInt8 value)
    {
        Negative = value.GetBitFlag(7);
        Overflow = value.GetBitFlag(6);
        // NOTE: Bit 5 is unused and always set to 1
        // NOTE: Bit 4 is the Break flag.  It is not affected by this method.
        Decimal = value.GetBitFlag(3);
        IRQDisabled = value.GetBitFlag(2);
        Zero = value.GetBitFlag(1);
        Carry = value.GetBitFlag(0);
    }

    public StatusRegister Copy()
    {
        return new StatusRegister
        {
            Negative = Negative,
            Overflow = Overflow,
            Break = Break,
            Decimal = Decimal,
            IRQDisabled = IRQDisabled,
            Zero = Zero,
            Carry = Carry
        };
    }

    public BitFlag Carry { get; set; }

    public void SetCarry() => Carry = BitFlag.High();
    public void ClearCarry() => Carry = BitFlag.Low();

    public BitFlag Zero { get; set; }

    public void SetZero() => Zero = BitFlag.High();
    public void ClearZero() => Zero = BitFlag.Low();

    public BitFlag IRQDisabled { get; set; }

    public void SetIRQDisabled() => IRQDisabled = BitFlag.High();
    public void ClearIRQDisabled() => IRQDisabled = BitFlag.Low();

    public BitFlag Decimal { get; set; }

    public void SetDecimal() => Decimal = BitFlag.High();
    public void ClearDecimal() => Decimal = BitFlag.Low();

    public BitFlag Break { get; set; }

    public void SetBreak() => Break = BitFlag.High();
    public void ClearBreak() => Break = BitFlag.Low();

    public BitFlag Overflow { get; set; }

    public void SetOverflow() => Overflow = BitFlag.High();
    public void ClearOverflow() => Overflow = BitFlag.Low();

    public BitFlag Negative { get; set; }

    public void SetNegative() => Negative = BitFlag.High();
    public void ClearNegative() => Negative = BitFlag.Low();
}
