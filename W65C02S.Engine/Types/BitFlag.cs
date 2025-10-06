#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
namespace W65C02S.Engine.Types;

/// <summary>
/// Represents a single bit flag with utility methods.
/// </summary>
public class BitFlag
{
    private bool _value;
    private static readonly BitFlag FlagSetObject = new(true);
    private static readonly BitFlag FlagClearedObject = new(false);

    public static BitFlag High() => FlagSetObject;
    public static BitFlag Low() => FlagClearedObject;

    public BitFlag(bool value)
    {
        _value = value;
    }

    public override bool Equals(object? o)
    {
        if (ReferenceEquals(this, o))
            return true;

        if (o is not BitFlag that)
            return false;

        return _value == that._value;
    }

    public override string ToString() => _value.ToString();

    public BitFlag Copy() => new(_value);

    public void Set() => _value = true;

    public void Clear() => _value = false;

    public bool IsSet() => _value;

    public bool IsCleared() => !_value;

    public bool GetValue() => _value;

    public void UpdateValue(bool toValue) => _value = toValue;

    public void UpdateValue(BitFlag toValue) => _value = toValue._value;

    public BitFlag Not()
    {
        _value = !_value;
        return this;
    }

    public int ToInt() => _value ? 1 : 0;
}
