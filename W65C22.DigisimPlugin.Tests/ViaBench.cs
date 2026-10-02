using System.Diagnostics.CodeAnalysis;
using Digisim.Engine;
using DigisimPlugin.TestHelpers;
using Microsoft.Extensions.Logging;

namespace W65C22.DigisimPlugin.Tests;

/// <summary>
/// A W65C22 in a real Digisim <see cref="SimulationModel"/> with sources on every bus-side input
/// and on CA1. Port and CA2/CB1/CB2 sources are added per test with <see cref="Attach"/> before
/// <see cref="Start"/>. Bus helpers run whole PHI2 cycles the way a 6502 would.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class ViaBench
{
    public const int Orb = 0x0;
    public const int Ora = 0x1;
    public const int Ddrb = 0x2;
    public const int Ddra = 0x3;
    public const int Sr = 0xA;
    public const int Acr = 0xB;
    public const int Pcr = 0xC;
    public const int Ifr = 0xD;
    public const int Ier = 0xE;

    private readonly SimulationModel _model = new(new OscillationDetector());

    public ViaBench(bool connectCs2B = true, ILogger? logger = null)
    {
        Via = logger is null ? new W65C22Via(Guid.NewGuid(), "via") : new W65C22Via(Guid.NewGuid(), "via", logger);
        _model.AddNode(Via);

        Phi2 = Input("phi2", Via.PHI2, 0);
        Resb = Input("resb", Via.RESB, 1);
        Cs1 = Input("cs1", Via.CS1, 1);
        Cs2B = new SignalSource("cs2b", 1, 1);
        if (connectCs2B)
            Attach(Cs2B, y => y.Connect(Via.CS2B));
        Rs = [Input("rs0", Via.RS0, 0), Input("rs1", Via.RS1, 0), Input("rs2", Via.RS2, 0), Input("rs3", Via.RS3, 0)];
        Rwb = Input("rwb", Via.RWB, 1);
        Ca1 = Input("ca1", Via.CA1, 1);

        Data = new SignalSource("d", 8, 0) { HighZ = true };
        Attach(Data, y => y.Connect(Via.D));
    }

    public W65C22Via Via { get; }

    public SignalSource Phi2 { get; }

    public SignalSource Resb { get; }

    public SignalSource Cs1 { get; }

    public SignalSource Cs2B { get; }

    public SignalSource[] Rs { get; }

    public SignalSource Rwb { get; }

    public SignalSource Ca1 { get; }

    /// <summary>The CPU side of D: drives during writes, high-Z otherwise.</summary>
    public SignalSource Data { get; }

    public void Attach(SignalSource source, Action<OutputPin> connect)
    {
        connect(source.Y);
        _model.AddNode(source);
    }

    public void Start()
    {
        _model.Initialize();
        _model.Step();
    }

    /// <summary>Settles the model after a source changed.</summary>
    public void Settle() => _model.Step();

    public void Rise()
    {
        Phi2.Value = 1;
        Settle();
    }

    public void Fall()
    {
        Phi2.Value = 0;
        Settle();
    }

    public void Cycle()
    {
        Rise();
        Fall();
    }

    public void Cycles(int count)
    {
        for (var i = 0; i < count; i++)
            Cycle();
    }

    public void Address(int register, bool read)
    {
        Cs2B.Value = 0;
        for (var bit = 0; bit < Rs.Length; bit++)
            Rs[bit].Value = (register >> bit) & 1;
        Rwb.Value = read ? 1 : 0;
        Settle();
    }

    public void Deselect()
    {
        Cs2B.Value = 1;
        Rwb.Value = 1;
        Data.HighZ = true;
        Settle();
    }

    public void Write(int register, byte value)
    {
        Address(register, read: false);
        Rise();
        Data.Value = value;
        Data.HighZ = false;
        Settle();
        Fall();
        Deselect();
    }

    /// <summary>A read cycle; returns what the VIA drove on D while PHI2 was high, or null if nothing.</summary>
    public byte? Read(int register)
    {
        Address(register, read: true);
        Rise();
        byte? value = Via.D.IsDriving ? (byte)Via.D.Value : null;
        Fall();
        Deselect();
        return value;
    }

    private SignalSource Input(string label, InputPin pin, long value)
    {
        var source = new SignalSource(label, 1, value);
        Attach(source, y => y.Connect(pin));
        return source;
    }
}
