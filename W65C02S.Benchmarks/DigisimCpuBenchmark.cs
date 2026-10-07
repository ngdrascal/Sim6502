using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using Digisim.Engine;
using DigisimPlugin.TestHelpers;
using W65C02S.DigisimPlugin;

namespace W65C02S.Benchmarks;

/// <summary>
/// Runs 100K CPU cycles of <see cref="MixedLoopProgram"/> on a <see cref="W65C02SCpu"/> wired to a
/// clock and a 64K memory in a real Digisim model. Effective clock in MHz = 0.1 / (mean seconds).
/// Includes the Digisim scheduler and memory model, so it is an end-to-end number.
/// </summary>
[ExcludeFromCodeCoverage]
[MemoryDiagnoser]
public class DigisimCpuBenchmark
{
    private const int Cycles = 100_000;

    private SimulationModel _model = null!;
    private SignalSource _clock = null!;

    internal FakeMemory Memory { get; private set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        _model = new SimulationModel(new OscillationDetector());
        var cpu = new W65C02SCpu(Guid.NewGuid(), "cpu");
        _clock = new SignalSource("clk", 1, 0);
        var memory = new FakeMemory();
        Memory = memory;

        _clock.Y.Connect(cpu.PHI2);
        _clock.Y.Connect(memory.PHI2);
        cpu.A.Connect(memory.A);
        cpu.RWB.Connect(memory.RWB);
        cpu.D.Connect(memory.D);
        _model.AddNodes(cpu, _clock, memory);

        memory.Load(MixedLoopProgram.LoadAddr, MixedLoopProgram.Bytes);
        memory.Load(MixedLoopProgram.PointerAddr, 0x00, 0x04);
        memory.Load(0xFFFC, MixedLoopProgram.LoadAddr & 0xFF, MixedLoopProgram.LoadAddr >> 8);

        _model.Initialize();
        _model.Step();
    }

    [Benchmark]
    public void Run100KCycles()
    {
        for (var i = 0; i < Cycles; i++)
        {
            _clock.Value = 1;
            _model.Step();
            _clock.Value = 0;
            _model.Step();
        }
    }
}
