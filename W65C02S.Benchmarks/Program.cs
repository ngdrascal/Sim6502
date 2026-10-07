using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Running;

namespace W65C02S.Benchmarks;

[ExcludeFromCodeCoverage]
internal static class Program
{
    private static void Main(string[] args)
    {
        if (args is ["--verify"])
        {
            Verify();
            return;
        }

        // long steady runs to attach a profiler to (e.g. dotnet-trace collect -- <exe> --profile engine)
        if (args is ["--profile", "digisim"])
        {
            var digisim = new DigisimCpuBenchmark();
            digisim.Setup();
            for (var i = 0; i < 50; i++)
                digisim.Run100KCycles();
            return;
        }

        if (args is ["--profile", "engine"])
        {
            var mixed = new MixedLoopBenchmark();
            mixed.Setup();
            for (var i = 0; i < 50; i++)
                mixed.Run1MCycles();
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }

    // Runs each benchmark workload once and prints the outcome and effective clock.
    private static void Verify()
    {
        // INC $20 runs 16 times per outer pass of the mixed loop, so a changed $20 shows the loop ran
        var mixed = new MixedLoopBenchmark();
        mixed.Setup();
        mixed.Run1MCycles();
        Console.WriteLine($"Mixed loop: {(mixed.Peek(0x20) != 0 ? "PASS" : "FAIL")}, $20 = ${mixed.Peek(0x20):X2}");

        var digisim = new DigisimCpuBenchmark();
        digisim.Setup();
        var sw = Stopwatch.StartNew();
        digisim.Run100KCycles();
        sw.Stop();
        Console.WriteLine($"Digisim mixed loop: {(digisim.Memory[0x20] != 0 ? "PASS" : "FAIL")}, " +
                          $"$20 = ${digisim.Memory[0x20]:X2}, {sw.Elapsed.TotalMilliseconds:N0} ms (cold)");

        var dormann = new DormannBenchmark();
        dormann.Setup();
        sw.Restart();
        var cycles = dormann.RunToSuccess();
        sw.Stop();
        Console.WriteLine($"Dormann: PASS, {cycles:N0} cycles, {sw.Elapsed.TotalMilliseconds:N0} ms, " +
                          $"{cycles / sw.Elapsed.TotalSeconds / 1e6:N2} MHz");

        // ends in BRK; the ERROR byte at $01B0 is 0 when every case matched
        var bcd = new EngineHarness(LoadTest("BruceClarkBCDTest.bin"), 0x0000, 0x0000) { StopOnBrkVector = true };
        var bcdCycles = bcd.RunUntilTrapped(500_000_000);
        var bcdPass = bcd.TrapAddress == EngineHarness.BrkVector && bcd.Memory[0x01B0] == 0;
        Console.WriteLine($"Bruce Clark BCD: {(bcdPass ? "PASS" : "FAIL")}, stopped at ${bcd.TrapAddress:X4} " +
                          $"after {bcdCycles:N0} cycles, ERROR = ${bcd.Memory[0x01B0]:X2}");
    }

    private static byte[] LoadTest(string fileName)
    {
        return File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Tests", fileName));
    }
}
