using System.Diagnostics.CodeAnalysis;
using Digisim.Engine;

namespace DigisimPlugin.TestHelpers;

/// <summary>
/// A test driver whose output is set directly: a value, high-Z, or a value with some bits high-Z.
/// A weak source acts as a pull-up/pull-down that any normal driver overrides.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SignalSource : LogicNode
{
    public SignalSource(string label, int bits, long value)
        : base(Guid.NewGuid(), "SRC", label)
    {
        Y = AddOutputPin("Y", 1, bits);
        Value = value;
    }

    public SignalSource(string label, int bits, long value, DriveStrength strength)
        : base(Guid.NewGuid(), "SRC", label)
    {
        Y = AddOutputPin("Y", 1, bits, strength);
        Value = value;
    }

    public OutputPin Y { get; }

    public long Value { get; set; }

    public bool HighZ { get; set; }

    /// <summary>Bits set here are high-Z while the rest drive <see cref="Value"/>.</summary>
    public long HighZMask { get; set; }

    public override void ReadInputs()
    {
    }

    public override void WriteOutputs()
    {
        if (HighZ)
            Y.SetToHighZ();
        else
            Y.Drive(Value, HighZMask);
    }
}
