// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using UInt8 = W65C02S.Engine.Types.UInt8;
using UInt16 = W65C02S.Engine.Types.UInt16;

namespace W65C02S.Engine.Tests;

// Dummy reads of implied, accumulator, pull and return instructions, per the SingleStepTests 65x02
// wdc65c02 vectors. JSR and JMP (abs,X) are in ControlTests.
[ExcludeFromCodeCoverage]
public class ImpliedStackBusActivityTests : UnitTestBase
{
    // runs one instruction at $1000 against memory with S = $80; $1001 holds $5A
    private List<MemoryCycle> Execute(OpCodes opCode, int cycles)
    {
        Memory[0x1000] = (byte)opCode.ToUInt8().ToInt();
        Memory[0x1001] = 0x5A;
        BootToAddress(new UInt16(0x1000));
        Regs.S = new UInt8(0x80);

        return Enumerable.Range(0, cycles).Select(_ => ExecuteCycleWithMemory()).ToList();
    }

    private static MemoryCycle ReadOf(int addr, int data)
    {
        return new MemoryCycle(addr, data, false);
    }

    /*
      TITLE: Implied and accumulator instructions read the byte after the opcode in their second cycle
      GIVEN: a CPU at $1000 with the instruction and $5A at $1001
      WHEN: the instruction runs its 2 cycles and the next cycle
      THEN: cycle 2 reads $1001, and the next cycle fetches the opcode at $1001
    */
    [Theory]
    [InlineData(OpCodes.ASLacc)]
    [InlineData(OpCodes.INCacc)]
    [InlineData(OpCodes.ROLacc)]
    [InlineData(OpCodes.DECacc)]
    [InlineData(OpCodes.LSRacc)]
    [InlineData(OpCodes.RORacc)]
    [InlineData(OpCodes.CLCimp)]
    [InlineData(OpCodes.SECimp)]
    [InlineData(OpCodes.CLIimp)]
    [InlineData(OpCodes.SEIimp)]
    [InlineData(OpCodes.DEYimp)]
    [InlineData(OpCodes.TXAimp)]
    [InlineData(OpCodes.TYAimp)]
    [InlineData(OpCodes.TXSimp)]
    [InlineData(OpCodes.TAYimp)]
    [InlineData(OpCodes.TAXimp)]
    [InlineData(OpCodes.CLVimp)]
    [InlineData(OpCodes.TSXimp)]
    [InlineData(OpCodes.INYimp)]
    [InlineData(OpCodes.DEXimp)]
    [InlineData(OpCodes.CLDimp)]
    [InlineData(OpCodes.INXimp)]
    [InlineData(OpCodes.NOP)]
    [InlineData(OpCodes.SEDimp)]
    public void TestImpliedReadsNextByte(OpCodes opCode)
    {
        // ARRANGE:

        // ACT:
        var cycles = Execute(opCode, 3);

        // ASSERT:
        Assert.Equal(ReadOf(0x1001, 0x5A), cycles[1]);
        Assert.Equal(0x1001, cycles[2].Addr);
        Assert.Equal((byte)High.ToInt(), Pins.SYNC);
    }

    /*
      TITLE: Pulls read the byte after the opcode, then the stack at S, then pull from S + 1
      GIVEN: a CPU at $1000 with the pull instruction, S = $80, and $33 at $0181
      WHEN: the pull runs its 4 cycles
      THEN: the cycles read $1000, $1001, $0180 and $0181, and S is $81
    */
    [Theory]
    [InlineData(OpCodes.PLAimp)]
    [InlineData(OpCodes.PLPimp)]
    [InlineData(OpCodes.PLXimp)]
    [InlineData(OpCodes.PLYimp)]
    public void TestPullReadsStackAtS(OpCodes opCode)
    {
        // ARRANGE:
        Memory[0x0181] = 0x33;
        var expected = new[]
        {
            ReadOf(0x1000, opCode.ToUInt8().ToInt()), ReadOf(0x1001, 0x5A), ReadOf(0x0180, 0x00),
            ReadOf(0x0181, 0x33)
        };

        // ACT:
        var cycles = Execute(opCode, 4);

        // ASSERT:
        Assert.Equal(expected, cycles);
        Assert.Equal(new UInt8(0x81), Regs.S);
    }

    /*
      TITLE: RTS reads the stack at S and the pulled address before adding 1
      GIVEN: a CPU at $1000 with RTS, S = $80, and $2002 pushed at $0181/$0182
      WHEN: RTS runs its 6 cycles
      THEN: the cycles read $1000, $1001, $0180, $0181, $0182 and $2002; PC is $2003
    */
    [Fact]
    public void TestRTSBusActivity()
    {
        // ARRANGE:
        Memory[0x0181] = 0x02;
        Memory[0x0182] = 0x20;
        var expected = new[]
        {
            ReadOf(0x1000, OpCodes.RTSimp.ToUInt8().ToInt()), ReadOf(0x1001, 0x5A), ReadOf(0x0180, 0x00),
            ReadOf(0x0181, 0x02), ReadOf(0x0182, 0x20), ReadOf(0x2002, 0x00)
        };

        // ACT:
        var cycles = Execute(OpCodes.RTSimp, 6);

        // ASSERT:
        Assert.Equal(expected, cycles);
        Assert.Equal(new UInt16(0x2003), Regs.PC);
        Assert.Equal(new UInt8(0x82), Regs.S);
    }

    /*
      TITLE: RTI reads the byte after the opcode and the stack at S before pulling P and PC
      GIVEN: a CPU at $1000 with RTI, S = $80, and P = $C3, PC = $2002 pushed at $0181..$0183
      WHEN: RTI runs its 6 cycles
      THEN: the cycles read $1000, $1001, $0180, $0181, $0182 and $0183; PC is $2002
    */
    [Fact]
    public void TestRTIBusActivity()
    {
        // ARRANGE:
        Memory[0x0181] = 0xC3;
        Memory[0x0182] = 0x02;
        Memory[0x0183] = 0x20;
        var expected = new[]
        {
            ReadOf(0x1000, OpCodes.RTIimp.ToUInt8().ToInt()), ReadOf(0x1001, 0x5A), ReadOf(0x0180, 0x00),
            ReadOf(0x0181, 0xC3), ReadOf(0x0182, 0x02), ReadOf(0x0183, 0x20)
        };

        // ACT:
        var cycles = Execute(OpCodes.RTIimp, 6);

        // ASSERT:
        Assert.Equal(expected, cycles);
        Assert.Equal(new UInt16(0x2002), Regs.PC);
        Assert.Equal(new UInt8(0x83), Regs.S);
    }
}
