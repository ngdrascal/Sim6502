namespace W65C02S.Engine.Types;

public readonly struct MathResult
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

    private static BitFlag FlagValue(string nvzc, char flag)
    {
        if (string.IsNullOrEmpty(nvzc) || nvzc.ToLower() != "nvzc")
            throw new ArgumentException(
                "Flags string must be in the format 'NVZC' with uppercase for set flags and lowercase for cleared flags.");

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
}
