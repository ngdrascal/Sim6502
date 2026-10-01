using System.Diagnostics.CodeAnalysis;
using Digisim.Engine;
using DigisimPlugin.TestHelpers;
using Microsoft.Extensions.Logging;

namespace W65C02S.DigisimPlugin.Tests;

/// <summary>
/// A W65C02S wired to a clock, a 64K memory and (optionally) sources on RESB, IRQB, NMIB, RDY, BE
/// and SOB, all in a real Digisim <see cref="SimulationModel"/>. The reset vector points at
/// <see cref="ProgramStart"/>.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class CpuBench
{
    public const int ProgramStart = 0x0200;

    private const int MaxCycles = 500;

    private readonly SimulationModel _model = new(new OscillationDetector());

    public CpuBench(bool connectControls = true, ILogger? logger = null)
    {
        Cpu = logger is null ? new W65C02SCpu(Guid.NewGuid(), "cpu") : new W65C02SCpu(Guid.NewGuid(), "cpu", logger);
        Clock = new SignalSource("clk", 1, 0);
        Memory = new FakeMemory();

        Clock.Y.Connect(Cpu.PHI2);
        Clock.Y.Connect(Memory.PHI2);
        Cpu.A.Connect(Memory.A);
        Cpu.RWB.Connect(Memory.RWB);
        Cpu.D.Connect(Memory.D);
        _model.AddNodes(Cpu, Clock, Memory);

        Resb = Control("resb", Cpu.RESB, connectControls);
        Irqb = Control("irqb", Cpu.IRQB, connectControls);
        Nmib = Control("nmib", Cpu.NMIB, connectControls);
        Rdy = Control("rdy", Cpu.RDY, connectControls);
        Be = Control("be", Cpu.BE, connectControls);
        Sob = Control("sob", Cpu.SOB, connectControls);

        SetVector(0xFFFC, ProgramStart);
    }

    /// <summary>Points the vector at <paramref name="vectorAddress"/> to <paramref name="target"/>.</summary>
    public void SetVector(int vectorAddress, int target)
    {
        Memory.Load(vectorAddress, (byte)(target & 0xFF), (byte)(target >> 8));
    }

    public W65C02SCpu Cpu { get; }

    public SignalSource Clock { get; }

    public FakeMemory Memory { get; }

    public SignalSource Resb { get; }

    public SignalSource Irqb { get; }

    public SignalSource Nmib { get; }

    public SignalSource Rdy { get; }

    public SignalSource Be { get; }

    public SignalSource Sob { get; }

    /// <summary>Every half cycle run so far, sampled after the model settled.</summary>
    public List<BusSample> Samples { get; } = [];

    public void Start()
    {
        _model.Initialize();
        _model.Step();
    }

    /// <summary>Settles the model without a clock edge, after a control source changed.</summary>
    public void Settle() => _model.Step();

    public void HalfCycle(bool phi2)
    {
        Clock.Value = phi2 ? 1 : 0;
        _model.Step();
        Samples.Add(new BusSample(phi2, Cpu.A.Value, Cpu.RWB.Value, Cpu.D.IsDriving,
            Cpu.SYNC.Value, Cpu.VPB.Value, Cpu.MLB.Value));
    }

    /// <summary>PHI2 high then low: the cycle ends on the falling edge that starts the next one.</summary>
    public void Cycle()
    {
        HalfCycle(true);
        HalfCycle(false);
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

        var recent = string.Join(" ", Samples.TakeLast(40).Where(s => !s.Phi2).Select(s => $"{s.Address:X4}{(s.Rwb == 0 ? "w" : "r")}"));
        throw new TimeoutException($"Condition not met within {MaxCycles} cycles. Last addresses: {recent}");
    }

    private SignalSource Control(string label, InputPin pin, bool connect)
    {
        var source = new SignalSource(label, 1, 1);
        if (connect)
        {
            source.Y.Connect(pin);
            _model.AddNode(source);
        }

        return source;
    }
}

internal readonly record struct BusSample(bool Phi2, long Address, long Rwb, bool DataDriven, long Sync, long Vpb, long Mlb);
