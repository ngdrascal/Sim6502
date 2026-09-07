using System.Diagnostics.CodeAnalysis;

namespace W65C02S.ValidationSuite;

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