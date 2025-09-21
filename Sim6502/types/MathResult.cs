namespace Sim6502.types;

public class MathResult
{
    private readonly UInt8 _value;
    private readonly BitFlag _negative;
    private readonly BitFlag _overflow;
    private readonly BitFlag _zero;
    private readonly BitFlag _carry;

    public MathResult(UInt8 result, BitFlag negative, BitFlag overflow, BitFlag zero, BitFlag carry)
    {
        _value = result;
        _negative = negative;
        _overflow = overflow;
        _zero = zero;
        _carry = carry;
    }

    public MathResult(UInt8 result, string flags)
    {
        _value = result;
        _negative = FlagValue(flags, 'N');
        _overflow = FlagValue(flags, 'V');
        _zero = FlagValue(flags, 'Z');
        _carry = FlagValue(flags, 'C');
    }

    private BitFlag FlagValue(string nvzc, char flag)
    {
        return flag switch
        {
            'N' or 'n' => nvzc.ToCharArray()[0] == 'N' ? BitFlag.High() : BitFlag.Low(),
            'V' or 'v' => nvzc.ToCharArray()[1] == 'V' ? BitFlag.High() : BitFlag.Low(),
            'Z' or 'z' => nvzc.ToCharArray()[2] == 'Z' ? BitFlag.High() : BitFlag.Low(),
            'C' or 'c' => nvzc.ToCharArray()[3] == 'C' ? BitFlag.High() : BitFlag.Low(),
            _ => BitFlag.Low()
        };
    }

    public UInt8 Value() => _value;

    public BitFlag Negative() => _negative;

    public BitFlag Overflow() => _overflow;

    public BitFlag Zero() => _zero;

    public BitFlag Carry() => _carry;

    public override string ToString()
    {
        var flags = "";
        flags += _negative.IsSet() ? "N" : "n";
        flags += _overflow.IsSet() ? "V" : "v";
        flags += _zero.IsSet() ? "Z" : "z";
        flags += _carry.IsSet() ? "C" : "c";
        return $"{_value.ToInt():X2} {flags}";
    }
}