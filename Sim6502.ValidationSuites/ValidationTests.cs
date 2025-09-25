using static System.Console;

namespace Sim6502.ValidationSuites;

public class ValidationTests
{
    public void RunBruceClarkBcdTest()
    {
        // ARRANGE:
        // ReSharper disable InconsistentNaming
        const int AR = 0x01AC;
        const int CF = 0x01AD;
        const int DA = 0x01AE;
        const int DNVZC = 0x01AF;
        const int ERROR = 0x01B0;
        const int HA = 0x01B1;
        const int HNVZC = 0x01B2;
        const int N1 = 0x01B3;
        const int N1H = 0x01B4;
        const int N1L = 0x01B5;
        const int N2 = 0x01B6;
        const int N2L = 0x01B7;
        const int NF = 0x01B8;
        const int VF = 0x01B9;
        const int ZF = 0x01BA;
        const int N2H = 0x01BB;
        // ReSharper restore InconsistentNaming

        var program = LoadBinaryFromFile(@".\BruceClarkBCDTest.bin");

        var simulator = new Simulator();
        simulator.LoadAndRunProgram(program, 0, 0);

        var expectedFlags = BuildNvzcString(simulator.Peek(NF), simulator.Peek(VF),
                                            simulator.Peek(ZF), simulator.Peek(CF));

        WriteLine($"ERROR: {simulator.Peek(ERROR):X2}");
        WriteLine("Inputs");
        WriteLine($"   N1    : {simulator.Peek(N1):X2}");
        WriteLine($"     N1H : {simulator.Peek(N1H):X2}");
        WriteLine($"     N1L : {simulator.Peek(N1L):X2}");
        WriteLine($"   N2    : {simulator.Peek(N2):X2}");
        WriteLine($"     N2L : {simulator.Peek(N2L):X2}");
        WriteLine($"     N2H : {simulator.Peek(N2H):X2}");
        WriteLine("Expected values");
        WriteLine($"   A reg : {simulator.Peek(AR):X2}");
        WriteLine($"   flags : {expectedFlags}");
        WriteLine("Actual Decimal");
        WriteLine($"   A     : {simulator.Peek(DA):X2}");
        WriteLine($"   flags : {BuildNvzcString(simulator.Peek(DNVZC))}");
        WriteLine("Actual Binary");
        WriteLine($"   A     : {simulator.Peek(HA):X2}");
        WriteLine($"   flags : {BuildNvzcString(simulator.Peek(HNVZC))}");
        WriteLine($"Instruction count: {simulator.InstCount}");

        WriteLine("BruceClarkBCDTest: " + (simulator.Peek(ERROR) == 0x00 ? "PASS" : "FAIL"));
    }

    private string BuildNvzcString(byte nValue, byte vValue, byte zValue, byte cValue)
    {
        String result = "";
        result += (nValue & 0b10000000) == 0 ? 'n' : 'N';
        result += (vValue & 0b01000000) == 0 ? 'v' : 'V';
        result += (zValue & 0b00000010) == 0 ? 'z' : 'Z';
        result += (cValue & 0b00000001) == 0 ? 'c' : 'C';

        return result;
    }

    private string BuildNvzcString(byte flags)
    {
        return BuildNvzcString(flags, flags, flags, flags);
    }

    public void RunKlausDormannTest()
    {
        var program = LoadBinaryFromFile(@".\6502_functional_test.bin");
        var simulator = new Simulator();
        simulator.LoadAndRunProgram(program, 0x0000, 0x0400);
    }

    private byte[] LoadBinaryFromFile(string fileName)
    {
        try
        {
            return File.ReadAllBytes(fileName);
        }
        catch (Exception e)
        {
            WriteLine($"Error reading file {fileName}: {e.Message}");
            WriteLine(e.StackTrace);
            return [];
        }
    }
}
