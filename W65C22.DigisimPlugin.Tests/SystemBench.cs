using System.Diagnostics.CodeAnalysis;
using Digisim.Engine;
using DigisimPlugin.TestHelpers;
using W65C02S.DigisimPlugin;

namespace W65C22.DigisimPlugin.Tests;

/// <summary>
/// A W65C02S, a 64K memory and a W65C22 at $8000-$800F sharing one clock and bus in a real Digisim
/// <see cref="SimulationModel"/>. The VIA's IRQB drives the CPU's IRQB; CS1 and RESB of the VIA
/// float (read high). The reset vector points at <see cref="ProgramStart"/>.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class SystemBench
{
    public const int ProgramStart = 0x0200;
    public const int ViaBase = 0x8000;

    private const int MaxCycles = 500;

    private readonly SimulationModel _model = new(new OscillationDetector());

    public SystemBench()
    {
        Cpu = new W65C02SCpu(Guid.NewGuid(), "cpu");
        Via = new W65C22Via(Guid.NewGuid(), "via");
        Memory = new FakeMemory { Decode = address => !IsVia(address) };
        Decoder = new ViaDecoder();
        Clock = new SignalSource("clk", 1, 0);
        Ca1 = new SignalSource("ca1", 1, 1);

        Clock.Y.Connect(Cpu.PHI2);
        Clock.Y.Connect(Memory.PHI2);
        Clock.Y.Connect(Via.PHI2);
        Cpu.A.Connect(Memory.A);
        Cpu.A.Connect(Decoder.A);
        Cpu.RWB.Connect(Memory.RWB);
        Cpu.RWB.Connect(Via.RWB);
        Cpu.D.Connect(Memory.D);
        Cpu.D.Connect(Via.D);
        Decoder.CS2B.Connect(Via.CS2B);
        Decoder.RS0.Connect(Via.RS0);
        Decoder.RS1.Connect(Via.RS1);
        Decoder.RS2.Connect(Via.RS2);
        Decoder.RS3.Connect(Via.RS3);
        Via.IRQB.Connect(Cpu.IRQB);
        Ca1.Y.Connect(Via.CA1);

        _model.AddNodes(Cpu, Via, Memory, Decoder, Clock, Ca1);

        SetVector(0xFFFC, ProgramStart);
    }

    public W65C02SCpu Cpu { get; }

    public W65C22Via Via { get; }

    public FakeMemory Memory { get; }

    public ViaDecoder Decoder { get; }

    public SignalSource Clock { get; }

    public SignalSource Ca1 { get; }

    /// <summary>Called after every half cycle, once the model settled.</summary>
    public Action? OnHalfCycle { get; set; }

    public void Attach(SignalSource source, Action<OutputPin> connect)
    {
        connect(source.Y);
        _model.AddNode(source);
    }

    public void SetVector(int vectorAddress, int target)
    {
        Memory.Load(vectorAddress, (byte)(target & 0xFF), (byte)(target >> 8));
    }

    public void Start()
    {
        _model.Initialize();
        _model.Step();
    }

    /// <summary>Settles the model without a clock edge, after a source changed.</summary>
    public void Settle() => _model.Step();

    public void Cycle()
    {
        HalfCycle(1);
        HalfCycle(0);
    }

    public void RunCycles(int count)
    {
        for (var i = 0; i < count; i++)
            Cycle();
    }

    public void RunUntil(Func<bool> condition)
    {
        for (var i = 0; i < MaxCycles; i++)
        {
            if (condition())
                return;

            Cycle();
        }

        throw new TimeoutException($"Condition not met within {MaxCycles} cycles.");
    }

    private void HalfCycle(long phi2)
    {
        Clock.Value = phi2;
        _model.Step();
        OnHalfCycle?.Invoke();
    }

    private static bool IsVia(int address) => (address & 0xFFF0) == ViaBase;
}

/// <summary>Selects the VIA (CS2B low) for $8000-$800F and passes A0-A3 to RS0-RS3.</summary>
[ExcludeFromCodeCoverage]
internal sealed class ViaDecoder : LogicNode
{
    private long _address;
    private bool _valid;

    public ViaDecoder()
        : base(Guid.NewGuid(), "DEC", "dec")
    {
        A = AddInputPin("A", 1, 16);
        CS2B = AddOutputPin("CS2B", 2, 1);
        RS0 = AddOutputPin("RS0", 3, 1);
        RS1 = AddOutputPin("RS1", 4, 1);
        RS2 = AddOutputPin("RS2", 5, 1);
        RS3 = AddOutputPin("RS3", 6, 1);
    }

    public InputPin A { get; }

    public OutputPin CS2B { get; }

    public OutputPin RS0 { get; }

    public OutputPin RS1 { get; }

    public OutputPin RS2 { get; }

    public OutputPin RS3 { get; }

    public override void ReadInputs()
    {
        A.UpdateFromDrivers();
        _valid = !A.IsHighZ;
        _address = A.Value & 0xFFFF;
    }

    public override void WriteOutputs()
    {
        var selected = _valid && (_address & 0xFFF0) == SystemBench.ViaBase;
        CS2B.DriveValue(selected ? 0 : 1);
        RS0.DriveValue(_address & 1);
        RS1.DriveValue((_address >> 1) & 1);
        RS2.DriveValue((_address >> 2) & 1);
        RS3.DriveValue((_address >> 3) & 1);
    }
}
