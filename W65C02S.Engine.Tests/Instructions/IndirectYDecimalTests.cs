// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using UInt8 = W65C02S.Engine.Types.UInt8;
using UInt16 = W65C02S.Engine.Types.UInt16;

namespace W65C02S.Engine.Tests;

// ADC/SBC (zp),Y in decimal mode with a page cross take 7 cycles: the page cross and the decimal
// cycle each add one.
[ExcludeFromCodeCoverage]
public class IndirectYDecimalTests : UnitTestBase
{
    /*
      TITLE: ADC/SBC (zp),Y in decimal mode with a page cross take 7 cycles
      GIVEN: a CPU booted to $1000 holding the instruction with operand $40; $40/$41 point at
             $12F0, Y = $20 (EA $1310 holds $28), D set, and A and C as given
      WHEN: the instruction runs against memory
      THEN: the cycles read PC, PC+1, $40, $41, EA, EA, EA; A holds the decimal result and the
            next cycle is an opcode fetch at $1002
    */
    [Theory]
    [InlineData(OpCodes.ADCindy, 0x19, false, 0x47)]
    [InlineData(OpCodes.SBCindy, 0x47, true, 0x19)]
    public void TestDecimalPageCrossTakes7Cycles(OpCodes opCode, int a, bool carry, int expectedA)
    {
        // ARRANGE:
        var op = opCode.ToUInt8().ToInt();
        Memory[0x1000] = (byte)op;
        Memory[0x1001] = 0x40;
        Memory[0x0040] = 0xF0;
        Memory[0x0041] = 0x12;
        Memory[0x1310] = 0x28;
        var expected = new[]
        {
            new MemoryCycle(0x1000, op, false),
            new MemoryCycle(0x1001, 0x40, false),
            new MemoryCycle(0x0040, 0xF0, false),
            new MemoryCycle(0x0041, 0x12, false),
            new MemoryCycle(0x1310, 0x28, false),
            new MemoryCycle(0x1310, 0x28, false),
            new MemoryCycle(0x1310, 0x28, false)
        };

        BootToAddress(BootAddr);
        Regs.A = new UInt8(a);
        Regs.Y = new UInt8(0x20);
        Regs.P.Decimal = true;
        Regs.P.Carry = carry;

        // ACT:
        var cycles = expected.Select(_ => ExecuteCycleWithMemory()).ToList();
        var next = ExecuteCycleWithMemory();

        // ASSERT:
        Assert.Equal(expected, cycles);
        Assert.Equal(new UInt8(expectedA), Regs.A);
        Assert.Equal(0x1002, next.Addr);
        Assert.Equal((byte)High.ToInt(), Pins.SYNC);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
    }
}
