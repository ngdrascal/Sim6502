using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using UInt8 = W65C02S.Engine.Types.UInt8;
using UInt16 = W65C02S.Engine.Types.UInt16;

namespace W65C02S.Engine.Tests;

// Runs the vendored subset of the SingleStepTests 65x02 wdc65c02 vectors (Harte/v1, see
// Harte/README.md) and compares every cycle's address, data and read/write, the cycle count, and the
// final registers and RAM.
[ExcludeFromCodeCoverage]
public class HarteConformanceTests
{
    private record OptOut(string Reason, Func<HarteCase, bool>? Applies = null);

    private static bool DecimalMode(HarteCase c) => (c.Initial.P & 0x08) != 0;

    // Known deviations from the vectors. An entry whose cases all pass fails the test, so remove it.
    // Background: Docs/Plan-Harte-Conformance.md.
    private static readonly Dictionary<int, OptOut> OptOuts = new()
    {
        // WDC datasheet Table 7-1 gives 5C 8 cycles; the vectors have 4
        [0x5C] = new("WDC datasheet: 8 cycles, vectors: 4"),
        // the vectors put an extra decimal-mode cycle at $7F/$00; no other source
        [0x69] = new("decimal mode extra cycle address unverified", DecimalMode),
        [0xE9] = new("decimal mode extra cycle address unverified", DecimalMode),
    };

    private static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "Harte", "v1");

    public static TheoryData<string> Opcodes()
    {
        var data = new TheoryData<string>();
        foreach (var file in Directory.GetFiles(DataDir, "*.json").Order())
            data.Add(Path.GetFileNameWithoutExtension(file));

        return data;
    }

    /*
      TITLE: Every opcode matches the SingleStepTests wdc65c02 vectors cycle for cycle
      GIVEN: the vendored cases for one opcode and the opt-out list of known deviations
      WHEN: each case runs from its initial registers and RAM on a fresh engine
      THEN: each cycle's address, data and read/write, the cycle count, and the final registers and
            RAM match; opted-out cases are skipped, and fail the test if they all pass
    */
    [Theory]
    [MemberData(nameof(Opcodes))]
    public void TestOpcodeMatchesVectors(string opcode)
    {
        // ARRANGE:
        var json = File.ReadAllText(Path.Combine(DataDir, opcode + ".json"));
        var cases = JsonSerializer.Deserialize<HarteCase[]>(json, JsonOptions)!;
        OptOuts.TryGetValue(Convert.ToInt32(opcode, 16), out var optOut);
        bool OptedOut(HarteCase c) => optOut != null && (optOut.Applies == null || optOut.Applies(c));

        // ACT:
        var failures = new List<string>();
        var optedOutFailures = 0;
        var optedOutCases = 0;
        foreach (var testCase in cases)
        {
            var failure = new CaseRunner().Run(testCase);
            if (OptedOut(testCase))
            {
                optedOutCases++;
                if (failure != null)
                    optedOutFailures++;
            }
            else if (failure != null)
            {
                failures.Add(failure);
            }
        }

        // ASSERT:
        Assert.True(failures.Count == 0,
            $"{failures.Count} of {cases.Length} cases differ from the vectors:\n{string.Join("\n", failures.Take(3))}");
        if (optOut == null)
            return;

        Assert.True(optedOutCases > 0, $"opt-out for {opcode} matches no case; remove it");
        Assert.True(optedOutFailures > 0, $"opted-out cases of {opcode} all pass; remove it from the opt-out list");
        Assert.Skip($"{optedOutFailures} of {optedOutCases} cases opted out: {optOut.Reason}");
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // ReSharper disable once ClassNeverInstantiated.Global
    public record HarteState(int Pc, int S, int A, int X, int Y, int P, int[][] Ram);

    // ReSharper disable once ClassNeverInstantiated.Global
    public record HarteCase(string Name, HarteState Initial, HarteState Final, JsonElement[][] Cycles);

    // a fresh engine per case
    private sealed class CaseRunner : UnitTestBase
    {
        // P without B (bit 4) and bit 5, which the engine does not hold as real bits
        private const int PMask = 0xCF;

        // returns null when the case matches, otherwise a description of the first difference
        public string? Run(HarteCase testCase)
        {
            var initial = testCase.Initial;
            BootToAddress(new UInt16(initial.Pc));
            foreach (var cell in initial.Ram)
                Memory[cell[0]] = (byte)cell[1];
            Regs.PC = new UInt16(initial.Pc);
            Regs.A = new UInt8(initial.A);
            Regs.X = new UInt8(initial.X);
            Regs.Y = new UInt8(initial.Y);
            Regs.S = new UInt8(initial.S);
            Regs.P.SetFlags(new UInt8(initial.P));

            var expected = testCase.Cycles
                .Select(c => new MemoryCycle(c[0].GetInt32(), c[1].GetInt32(), c[2].GetString() == "write"))
                .ToList();
            var actual = expected.Select(_ => ExecuteCycleWithMemory()).ToList();
            var differs = expected.Zip(actual).Select((pair, i) => (pair, i)).FirstOrDefault(x => x.pair.First != x.pair.Second);
            if (differs != default)
                return Describe(testCase, $"cycle {differs.i} differs", expected, actual);

            var final = testCase.Final;
            var registers = new[]
            {
                ("PC", final.Pc, Regs.PC.ToInt()), ("A", final.A, Regs.A.ToInt()), ("X", final.X, Regs.X.ToInt()),
                ("Y", final.Y, Regs.Y.ToInt()), ("S", final.S, Regs.S.ToInt()),
                ("P", final.P & PMask, Regs.P.ToUInt8().ToInt() & PMask)
            };
            foreach (var (name, want, got) in registers)
            {
                if (want != got)
                    return Describe(testCase, $"final {name} ${want:X2} expected, ${got:X2} actual", expected, actual);
            }

            foreach (var cell in final.Ram)
            {
                if (Memory[cell[0]] != cell[1])
                    return Describe(testCase, $"final RAM ${cell[0]:X4} ${cell[1]:X2} expected, ${Memory[cell[0]]:X2} actual",
                        expected, actual);
            }

            // the instruction must end here: the next cycle is an opcode fetch
            ExecuteCycleWithMemory();
            if (Pins.SYNC != (byte)High.ToInt())
                return Describe(testCase, $"takes more than {expected.Count} cycles", expected, actual);

            return null;
        }

        private static string Describe(HarteCase testCase, string what, List<MemoryCycle> expected,
                                       List<MemoryCycle> actual)
        {
            static string Format(MemoryCycle c) => $"{(c.IsWrite ? "W" : "R")} {c.Addr:X4} {c.Data:X2}";

            var text = new StringBuilder($"case '{testCase.Name}': {what}\n");
            for (var i = 0; i < expected.Count; i++)
            {
                var mark = expected[i] == actual[i] ? " " : "*";
                text.AppendLine($"   {i}{mark} expected {Format(expected[i])}   actual {Format(actual[i])}");
            }

            return text.ToString();
        }
    }
}
