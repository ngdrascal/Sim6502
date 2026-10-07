using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;

namespace W65C02S.Benchmarks;

/// <summary>
/// Runs 1M CPU cycles of <see cref="MixedLoopProgram"/> on the engine alone.
/// Effective clock in MHz = 1 / (mean time in seconds).
/// </summary>
[ExcludeFromCodeCoverage]
[MemoryDiagnoser]
public class MixedLoopBenchmark
{
    private const long Cycles = 1_000_000;

    private EngineHarness _harness = null!;

    [GlobalSetup]
    public void Setup()
    {
        _harness = new EngineHarness(MixedLoopProgram.Bytes, MixedLoopProgram.LoadAddr, MixedLoopProgram.LoadAddr);
        _harness.Memory[MixedLoopProgram.PointerAddr] = 0x00;
        _harness.Memory[MixedLoopProgram.PointerAddr + 1] = 0x04;
    }

    internal byte Peek(int address) => _harness.Memory[address];

    [Benchmark]
    public void Run1MCycles()
    {
        _harness.RunCycles(Cycles);
    }
}
