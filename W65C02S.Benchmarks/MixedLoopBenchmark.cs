using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;

namespace W65C02S.Benchmarks;

/// <summary>
/// Runs 1M CPU cycles of a tight loop that mixes addressing modes, ALU ops, stack,
/// subroutine and branch instructions. Effective clock in MHz = 1 / (mean time in seconds).
/// </summary>
[ExcludeFromCodeCoverage]
[MemoryDiagnoser]
public class MixedLoopBenchmark
{
    private const long Cycles = 1_000_000;

    private const ushort LoadAddr = 0x0200;

    // 0200 A2 10     start: LDX #$10
    // 0202 B5 10     loop:  LDA $10,X
    // 0204 69 01            ADC #$01
    // 0206 9D 00 03         STA $0300,X
    // 0209 E6 20            INC $20
    // 020B 48               PHA
    // 020C 68               PLA
    // 020D B1 30            LDA ($30),Y
    // 020F 20 18 02         JSR sub
    // 0212 CA               DEX
    // 0213 D0 ED            BNE loop
    // 0215 4C 00 02         JMP start
    // 0218 2A        sub:   ROL A
    // 0219 24 20            BIT $20
    // 021B 60               RTS
    private static readonly byte[] Program =
    [
        0xA2, 0x10,
        0xB5, 0x10,
        0x69, 0x01,
        0x9D, 0x00, 0x03,
        0xE6, 0x20,
        0x48,
        0x68,
        0xB1, 0x30,
        0x20, 0x18, 0x02,
        0xCA,
        0xD0, 0xED,
        0x4C, 0x00, 0x02,
        0x2A,
        0x24, 0x20,
        0x60
    ];

    private EngineHarness _harness = null!;

    [GlobalSetup]
    public void Setup()
    {
        _harness = new EngineHarness(Program, LoadAddr, LoadAddr);
        _harness.Memory[0x30] = 0x00;
        _harness.Memory[0x31] = 0x04;
    }

    [Benchmark]
    public void Run1MCycles()
    {
        _harness.RunCycles(Cycles);
    }
}
