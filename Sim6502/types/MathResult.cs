using Us.Retrocpu.Shared;

namespace Us.Retrocpu.W65c02s.Types
{
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

        protected BitFlag FlagValue(string nvzc, char flag)
        {
            switch (flag)
            {
                case 'N':
                case 'n':
                    return nvzc.ToCharArray()[0] == 'N' ? BitFlag.High() : BitFlag.Low();
                case 'V':
                case 'v':
                    return nvzc.ToCharArray()[1] == 'V' ? BitFlag.High() : BitFlag.Low();
                case 'Z':
                case 'z':
                    return nvzc.ToCharArray()[2] == 'Z' ? BitFlag.High() : BitFlag.Low();
                case 'C':
                case 'c':
                    return nvzc.ToCharArray()[3] == 'C' ? BitFlag.High() : BitFlag.Low();
                default:
                    return BitFlag.Low();
            }
        }

        public override string ToString()
        {
            string flags = "";
            flags += _negative.IsSet() ? "N" : "n";
            flags += _overflow.IsSet() ? "V" : "v";
            flags += _zero.IsSet() ? "Z" : "z";
            flags += _carry.IsSet() ? "C" : "c";
            return string.Format("{0:X2} {1}", _value.ToInt(), flags);
        }

        public UInt8 Value() => _value;
        public BitFlag Negative() => _negative;
        public BitFlag Overflow() => _overflow;
        public BitFlag Zero() => _zero;
        public BitFlag Carry() => _carry;
    }
}
