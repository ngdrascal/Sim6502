using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace W65C02S.Benchmarks;

/// <summary>
/// Runs the Klaus Dormann functional test from reset to its success trap with tracing off.
/// Long-running, so it uses a few iterations instead of the default statistics.
/// </summary>
[ExcludeFromCodeCoverage]
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Monitoring, warmupCount: 0, iterationCount: 2)]
public class DormannBenchmark
{
    // "jmp *" at the success label in 6502_functional_test.lst
    private const int SuccessAddr = 0x3469;
    private const long MaxCycles = 500_000_000;

    private byte[] _program = [];

    [GlobalSetup]
    public void Setup()
    {
        _program = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Tests", "6502_functional_test.bin"));
    }

    [Benchmark]
    public long RunToSuccess()
    {
        var harness = new EngineHarness(_program, 0x0000, 0x0400);
        var cycles = harness.RunUntilTrapped(MaxCycles);
        if (harness.TrapAddress != SuccessAddr)
        {
            throw new InvalidOperationException(
                $"Dormann test trapped at ${harness.TrapAddress:X4} after {cycles} cycles, expected ${SuccessAddr:X4}");
        }

        return cycles;
    }
}
