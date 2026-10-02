using System.Diagnostics.CodeAnalysis;
using Digisim.Engine;

namespace DigisimPlugin.TestHelpers;

/// <summary>
/// 64K x 8 memory on a 6502 bus: drives D with the addressed byte while RWB is high and stores D
/// while RWB is low and PHI2 is high. Floats D when A or RWB is high-Z, or when
/// <see cref="Decode"/> rejects the address (leaving it to another device on the bus).
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class FakeMemory : LogicNode
{
    private readonly byte[] _bytes = new byte[0x10000];
    private long _address;
    private bool _busValid, _read, _phi2;
    private long _data;
    private bool _dataValid;

    public FakeMemory()
        : base(Guid.NewGuid(), "MEM", "mem")
    {
        A = AddInputPin("A", 1, 16);
        RWB = AddInputPin("RWB", 2, 1);
        PHI2 = AddInputPin("PHI2", 3, 1);
        D = AddInOutPin("D", 4, 8);
        D.Mode = PinModes.Input;
    }

    public InputPin A { get; }

    public InputPin RWB { get; }

    public InputPin PHI2 { get; }

    public InOutPin D { get; }

    /// <summary>True for the addresses this memory answers; all of them by default.</summary>
    public Func<int, bool> Decode { get; init; } = _ => true;

    public byte this[int address]
    {
        get => _bytes[address];
        set => _bytes[address] = value;
    }

    public void Load(int address, params byte[] bytes) => bytes.CopyTo(_bytes, address);

    public override void ReadInputs()
    {
        A.UpdateFromDrivers();
        RWB.UpdateFromDrivers();
        PHI2.UpdateFromDrivers();

        _busValid = !A.IsHighZ && !RWB.IsHighZ && Decode((int)(A.Value & 0xFFFF));
        _address = A.Value & 0xFFFF;
        _read = RWB.GetBool();
        _phi2 = PHI2.GetBool();

        if (D.Mode == PinModes.Input)
        {
            D.UpdateFromDrivers();
            _data = D.Value;
            _dataValid = !D.IsHighZ;
        }
    }

    public override void WriteOutputs()
    {
        if (_busValid && _read)
        {
            D.DriveValue(_bytes[_address]);
            return;
        }

        D.Mode = PinModes.Input;
        if (_busValid && _phi2 && _dataValid)
            _bytes[_address] = (byte)(_data & 0xFF);
    }
}
