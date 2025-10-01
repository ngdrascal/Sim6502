namespace W65C02S.Engine.Types;

/// <summary>
/// Represents a single bit flag with utility methods.
/// </summary>
public class BitFlag
{
    private bool _value;
    private static readonly BitFlag _flagSetObject = new BitFlag(true);
    private static readonly BitFlag _flagClearedObject = new BitFlag(false);

    public static BitFlag High() => _flagSetObject;
    public static BitFlag Low() => _flagClearedObject;

    public BitFlag(bool value)
    {
        _value = value;
    }

    public override bool Equals(object? o)
    {
        if (ReferenceEquals(this, o)) return true;
        if (o is not BitFlag that) return false;
        return _value == that._value;
    }

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public BitFlag Copy() => new BitFlag(_value);

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
