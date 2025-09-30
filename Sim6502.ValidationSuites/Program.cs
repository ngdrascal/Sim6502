namespace Sim6502.ValidationSuites
{
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
}
