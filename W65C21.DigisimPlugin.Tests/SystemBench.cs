using System.Diagnostics.CodeAnalysis;
using Digisim.Engine;
using DigisimPlugin.TestHelpers;
using W65C02S.DigisimPlugin;

namespace W65C21.DigisimPlugin.Tests;

/// <summary>
/// A W65C02S, a 64K memory and a W65C21 at $8000-$8003 sharing one clock and bus in a real Digisim
/// <see cref="SimulationModel"/>. IRQAB drives the CPU's IRQB; CS0, CS1 and RESB of the PIA float
/// (read high). The reset vector points at <see cref="ProgramStart"/>.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class SystemBench
{
    public const int ProgramStart = 0x0200;
    public const int PiaBase = 0x8000;

    private const int MaxCycles = 500;

    private readonly SimulationModel _model = new(new OscillationDetector());

    public SystemBench()
    {
        Cpu = new W65C02SCpu(Guid.NewGuid(), "cpu");
        Pia = new W65C21Pia(Guid.NewGuid(), "pia");
        Memory = new FakeMemory { Decode = address => !IsPia(address) };
        Decoder = new PiaDecoder();
        Clock = new SignalSource("clk", 1, 0);
        Ca1 = new SignalSource("ca1", 1, 1);

        Clock.Y.Connect(Cpu.PHI2);
        Clock.Y.Connect(Memory.PHI2);
        Clock.Y.Connect(Pia.PHI2);
        Cpu.A.Connect(Memory.A);
        Cpu.A.Connect(Decoder.A);
        Cpu.RWB.Connect(Memory.RWB);
        Cpu.RWB.Connect(Pia.RWB);
        Cpu.D.Connect(Memory.D);
        Cpu.D.Connect(Pia.D);
        Decoder.CS2B.Connect(Pia.CS2B);
        Decoder.RS0.Connect(Pia.RS0);
        Decoder.RS1.Connect(Pia.RS1);
        Pia.IRQAB.Connect(Cpu.IRQB);
        Ca1.Y.Connect(Pia.CA1);

        _model.AddNodes(Cpu, Pia, Memory, Decoder, Clock, Ca1);

        SetVector(0xFFFC, ProgramStart);
    }

    public W65C02SCpu Cpu { get; }

    public W65C21Pia Pia { get; }

    public FakeMemory Memory { get; }

    public PiaDecoder Decoder { get; }

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

    private static bool IsPia(int address) => (address & 0xFFFC) == PiaBase;
}

/// <summary>Selects the PIA (CS2B low) for $8000-$8003 and passes A0/A1 to RS0/RS1.</summary>
[ExcludeFromCodeCoverage]
internal sealed class PiaDecoder : LogicNode
{
    private long _address;
    private bool _valid;

    public PiaDecoder()
        : base(Guid.NewGuid(), "DEC", "dec")
    {
        A = AddInputPin("A", 1, 16);
        CS2B = AddOutputPin("CS2B", 2, 1);
        RS0 = AddOutputPin("RS0", 3, 1);
        RS1 = AddOutputPin("RS1", 4, 1);
    }

    public InputPin A { get; }

    public OutputPin CS2B { get; }

    public OutputPin RS0 { get; }

    public OutputPin RS1 { get; }

    public override void ReadInputs()
    {
        A.UpdateFromDrivers();
        _valid = !A.IsHighZ;
        _address = A.Value & 0xFFFF;
    }

    public override void WriteOutputs()
    {
        var selected = _valid && (_address & 0xFFFC) == SystemBench.PiaBase;
        CS2B.DriveValue(selected ? 0 : 1);
        RS0.DriveValue(_address & 1);
        RS1.DriveValue((_address >> 1) & 1);
    }
}
