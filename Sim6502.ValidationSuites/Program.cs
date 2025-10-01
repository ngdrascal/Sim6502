using System.Diagnostics.CodeAnalysis;

namespace Sim6502.ValidationSuites;

[ExcludeFromCodeCoverage]
internal static class Program
{
    static void Main()
    {
        var testSuite = new ValidationTests();
        // testSuite.RunBruceClarkBcdTest();
        testSuite.RunKlausDormannTest();
        // testSuite.RunExtendedOpCodeTest();
    }
}