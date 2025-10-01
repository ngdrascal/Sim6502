// Represents the status register for W65c02s

using W65C02S.Engine.Types;

namespace W65C02S.Engine;

public class StatusRegister
{
    public StatusRegister()
    {
        Negative = new BitFlag(false);
        Overflow = new BitFlag(false);
        Break = new BitFlag(true);
        Decimal = new BitFlag(false);
        IRQDisabled = new BitFlag(true);
        Zero = new BitFlag(false);
        Carry = new BitFlag(false);
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
        Negative.UpdateValue(value.IsBitSet(7));
        Overflow.UpdateValue(value.IsBitSet(6));
        // NOTE: Bit 5 is unused and always set to 1
        Decimal.UpdateValue(value.IsBitSet(3));
        IRQDisabled.UpdateValue(value.IsBitSet(2));
        Zero.UpdateValue(value.IsBitSet(1));
        Carry.UpdateValue(value.IsBitSet(0));
    }

    public BitFlag Carry { get; }

    public void SetCarry() => Carry.Set();
    public void ClearCarry() => Carry.Clear();

    public BitFlag Zero { get; }

    public void SetZero() => Zero.Set();
    public void ClearZero() => Zero.Clear();

    public BitFlag IRQDisabled { get; }

    public void SetIRQDisabled() => IRQDisabled.Set();
    public void ClearIRQDisabled() => IRQDisabled.Clear();

    public BitFlag Decimal { get; }

    public void SetDecimal() => Decimal.Set();
    public void ClearDecimal() => Decimal.Clear();

    public BitFlag Break { get; }

    public void SetBreak() => Break.Set();
    public void ClearBreak() => Break.Clear();

    public BitFlag Overflow { get; }

    public void SetOverflow() => Overflow.Set();
    public void ClearOverflow() => Overflow.Clear();

    public BitFlag Negative { get; }

    public void SetNegative() => Negative.Set();
    public void ClearNegative() => Negative.Clear();
}