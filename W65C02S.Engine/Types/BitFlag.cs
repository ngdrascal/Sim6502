namespace W65C02S.Engine.Types;

/// <summary>
/// Represents a single bit flag with utility methods.
/// </summary>
/// <remarks>
/// An immutable value: <see cref="Not"/> returns a new flag, so a result must be assigned back.
/// </remarks>
public readonly struct BitFlag : IEquatable<BitFlag>
{
    private readonly bool _value;

    public static BitFlag High() => new(true);

    public static BitFlag Low() => new(false);

    public BitFlag(bool value)
    {
        _value = value;
    }

    public static implicit operator BitFlag(bool value) => new(value);

    public bool Equals(BitFlag other) => _value == other._value;

    public override bool Equals(object? o) => o is BitFlag that && Equals(that);

    public override int GetHashCode() => _value ? 1 : 0;

    public override string ToString() => _value.ToString();

    public BitFlag Copy() => this;

    public bool IsSet() => _value;

    public bool IsCleared() => !_value;

    public bool GetValue() => _value;

    public BitFlag Not() => new(!_value);

    public int ToInt() => _value ? 1 : 0;
}
