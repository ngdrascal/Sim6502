// Represents the status register for W65c02s

using Sim6502.types;

namespace Sim6502;

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

    public StatusRegister(StatusRegister source)
    {
        Negative = new BitFlag(source.Negative.IsSet());
        Overflow = new BitFlag(source.Overflow.IsSet());
        Break = new BitFlag(source.Break.IsSet());
        Decimal = new BitFlag(source.Decimal.IsSet());
        IRQDisabled = new BitFlag(source.IRQDisabled.IsSet());
        Zero = new BitFlag(source.Zero.IsSet());
        Carry = new BitFlag(source.Carry.IsSet());
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

    public UInt8 GetFlags()
    {
        var result = new UInt8(0);
        if (Negative.IsSet()) result.SetBit(7);
        if (Overflow.IsSet()) result.SetBit(6);
        result.SetBit(5);
        if (Break.IsSet()) result.SetBit(4);
        if (Decimal.IsSet()) result.SetBit(3);
        if (IRQDisabled.IsSet()) result.SetBit(2);
        if (Zero.IsSet()) result.SetBit(1);
        if (Carry.IsSet()) result.SetBit(0);
        return result;
    }

    public void SetFlags(UInt8 value)
    {
        Negative.UpdateValue(value.IsBitSet(7));
        Overflow.UpdateValue(value.IsBitSet(6));
        Break.UpdateValue(value.IsBitSet(4));
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