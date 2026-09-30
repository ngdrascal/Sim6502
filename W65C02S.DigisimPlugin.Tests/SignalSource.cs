using System.Diagnostics.CodeAnalysis;
using Digisim.Engine;

namespace W65C02S.DigisimPlugin.Tests;

/// <summary>A test driver whose output is set directly: a value, or high-Z.</summary>
[ExcludeFromCodeCoverage]
internal sealed class SignalSource : LogicNode
{
    public SignalSource(string label, int bits, long value)
        : base(Guid.NewGuid(), "SRC", label)
    {
        Y = AddOutputPin("Y", 1, bits);
        Value = value;
    }

    public OutputPin Y { get; }

    public long Value { get; set; }

    public bool HighZ { get; set; }

    public override void ReadInputs()
    {
    }

    public override void WriteOutputs()
    {
        if (HighZ)
            Y.SetToHighZ();
        else
            Y.DriveValue(Value);
    }
}
