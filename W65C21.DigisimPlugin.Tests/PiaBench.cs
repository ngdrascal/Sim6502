using System.Diagnostics.CodeAnalysis;
using Digisim.Engine;
using DigisimPlugin.TestHelpers;
using Microsoft.Extensions.Logging;

namespace W65C21.DigisimPlugin.Tests;

/// <summary>
/// A W65C21 in a real Digisim <see cref="SimulationModel"/> with sources on every bus-side input
/// and on CA1/CB1. Port and CA2/CB2 sources are added per test with <see cref="Attach"/> before
/// <see cref="Start"/>. Bus helpers run whole PHI2 cycles the way a 6502 would.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class PiaBench
{
    public const int PortA = 0;
    public const int Cra = 1;
    public const int PortB = 2;
    public const int Crb = 3;

    private readonly SimulationModel _model = new(new OscillationDetector());

    public PiaBench(bool connectCs2B = true, ILogger? logger = null)
    {
        Pia = logger is null ? new W65C21Pia(Guid.NewGuid(), "pia") : new W65C21Pia(Guid.NewGuid(), "pia", logger);
        _model.AddNode(Pia);

        Phi2 = Input("phi2", Pia.PHI2, 0);
        Resb = Input("resb", Pia.RESB, 1);
        Cs0 = Input("cs0", Pia.CS0, 1);
        Cs1 = Input("cs1", Pia.CS1, 1);
        Cs2B = new SignalSource("cs2b", 1, 1);
        if (connectCs2B)
            Attach(Cs2B, y => y.Connect(Pia.CS2B));
        Rs0 = Input("rs0", Pia.RS0, 0);
        Rs1 = Input("rs1", Pia.RS1, 0);
        Rwb = Input("rwb", Pia.RWB, 1);
        Ca1 = Input("ca1", Pia.CA1, 1);
        Cb1 = Input("cb1", Pia.CB1, 1);

        Data = new SignalSource("d", 8, 0) { HighZ = true };
        Attach(Data, y => y.Connect(Pia.D));
    }

    public W65C21Pia Pia { get; }

    public SignalSource Phi2 { get; }

    public SignalSource Resb { get; }

    public SignalSource Cs0 { get; }

    public SignalSource Cs1 { get; }

    public SignalSource Cs2B { get; }

    public SignalSource Rs0 { get; }

    public SignalSource Rs1 { get; }

    public SignalSource Rwb { get; }

    public SignalSource Ca1 { get; }

    public SignalSource Cb1 { get; }

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

    public void Address(int register, bool read)
    {
        Cs2B.Value = 0;
        Rs0.Value = register & 1;
        Rs1.Value = (register >> 1) & 1;
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

    /// <summary>A read cycle; returns what the PIA drove on D while PHI2 was high, or null if nothing.</summary>
    public byte? Read(int register)
    {
        Address(register, read: true);
        Rise();
        byte? value = Pia.D.IsDriving ? (byte)Pia.D.Value : null;
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
