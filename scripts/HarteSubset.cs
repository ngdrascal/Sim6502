// Builds the vendored subset of the SingleStepTests 65x02 wdc65c02 vectors used by
// W65C02S.Engine.Tests/HarteConformanceTests.
//
//   dotnet run scripts/HarteSubset.cs -- <cacheDir> [outDir]
//
// Downloads each opcode file (about 4.7 MB, 10,000 cases) into cacheDir when it is missing, then
// writes outDir/xx.json (default W65C02S.Engine.Tests/Harte/v1) holding:
//   - up to 2 cases for each distinct signature: cycle count, decimal flag, and zero page pointer
//     wrap (a read of $00FF followed by a read of $0000), so page cross, branch taken, decimal
//     penalty and pointer wrap cases are kept whenever the opcode has them
//   - 12 more cases chosen at random with a fixed seed
// Cases keep their upstream order and format.

using System.Text.Json;
using System.Text.Json.Nodes;

const string SourceUrl = "https://raw.githubusercontent.com/SingleStepTests/65x02/main/wdc65c02/v1/";
const int PerSignature = 2;
const int RandomCases = 12;

if (args.Length < 1)
{
    Console.Error.WriteLine("usage: dotnet run scripts/HarteSubset.cs -- <cacheDir> [outDir]");
    return 1;
}

var cacheDir = Directory.CreateDirectory(args[0]).FullName;
var outDir = Directory.CreateDirectory(args.Length > 1 ? args[1] : "W65C02S.Engine.Tests/Harte/v1").FullName;

using var http = new HttpClient();
var opcodes = Enumerable.Range(0, 256).Select(op => $"{op:x2}").ToArray();

await Parallel.ForEachAsync(opcodes, new ParallelOptions { MaxDegreeOfParallelism = 8 }, async (name, ct) =>
{
    var path = Path.Combine(cacheDir, name + ".json");
    if (File.Exists(path))
        return;

    using var response = await http.GetAsync(SourceUrl + name + ".json", ct);
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"{name}: no upstream file ({(int)response.StatusCode})");
        return;
    }

    var tmp = path + ".tmp";
    await File.WriteAllBytesAsync(tmp, await response.Content.ReadAsByteArrayAsync(ct), ct);
    File.Move(tmp, path, true);
});

var totalCases = 0;
foreach (var name in opcodes)
{
    var path = Path.Combine(cacheDir, name + ".json");
    if (!File.Exists(path) || new FileInfo(path).Length == 0)
    {
        // upstream has no cases for CB (WAI) and DB (STP)
        Console.WriteLine($"{name}: no cases");
        continue;
    }

    var cases = JsonNode.Parse(File.ReadAllText(path))!.AsArray();
    var picked = new SortedSet<int>();

    var bySignature = new Dictionary<string, int>();
    for (var i = 0; i < cases.Count; i++)
    {
        var signature = Signature(cases[i]!);
        bySignature.TryGetValue(signature, out var count);
        if (count < PerSignature)
        {
            picked.Add(i);
            bySignature[signature] = count + 1;
        }
    }

    var random = new Random(6502);
    var target = picked.Count + Math.Min(RandomCases, cases.Count - picked.Count);
    while (picked.Count < target)
        picked.Add(random.Next(cases.Count));

    var subset = new JsonArray(picked.Select(i => cases[i]!.DeepClone()).ToArray());
    File.WriteAllText(Path.Combine(outDir, name + ".json"), subset.ToJsonString(new JsonSerializerOptions()));
    totalCases += subset.Count;
    Console.WriteLine($"{name}: {subset.Count} cases, {bySignature.Count} signatures");
}

Console.WriteLine($"total: {totalCases} cases");
return 0;

static string Signature(JsonNode testCase)
{
    var cycles = testCase["cycles"]!.AsArray();
    var decimalMode = (testCase["initial"]!["p"]!.GetValue<int>() & 0x08) != 0;
    var pointerWrap = false;
    for (var i = 1; i < cycles.Count; i++)
    {
        if (cycles[i - 1]![0]!.GetValue<int>() == 0x00FF && cycles[i]![0]!.GetValue<int>() == 0x0000)
            pointerWrap = true;
    }

    return $"{cycles.Count}/{decimalMode}/{pointerWrap}";
}
