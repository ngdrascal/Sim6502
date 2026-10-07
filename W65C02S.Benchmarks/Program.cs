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

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }

    // Runs each benchmark workload once and prints the outcome and effective clock.
    private static void Verify()
    {
        var dormann = new DormannBenchmark();
        dormann.Setup();
        var sw = Stopwatch.StartNew();
        var cycles = dormann.RunToSuccess();
        sw.Stop();
        Console.WriteLine($"Dormann: PASS, {cycles:N0} cycles, {sw.Elapsed.TotalMilliseconds:N0} ms, " +
                          $"{cycles / sw.Elapsed.TotalSeconds / 1e6:N2} MHz");
    }
}
