// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using UInt8 = W65C02S.Engine.Types.UInt8;
using UInt16 = W65C02S.Engine.Types.UInt16;

namespace W65C02S.Engine.Tests;

// Dummy reads of the indexing cycles, per the SingleStepTests 65x02 wdc65c02 vectors and the W65C02S
// datasheet Table 7-1 ("extra read of last instruction byte" on a page cross).
[ExcludeFromCodeCoverage]
public class IndexedBusActivityTests : UnitTestBase
{
    // runs one instruction at $1000 against memory with X = Y = index and A = a; $40/$41 point at $12F0
    private List<MemoryCycle> Execute(OpCodes opCode, int[] operands, int index, int cycles, int a = 0)
    {
        Memory[0x1000] = (byte)opCode.ToUInt8().ToInt();
        for (var i = 0; i < operands.Length; i++)
            Memory[0x1001 + i] = (byte)operands[i];
        Memory[0x0040] = 0xF0;
        Memory[0x0041] = 0x12;
        BootToAddress(new UInt16(0x1000));
        Regs.X = new UInt8(index);
        Regs.Y = new UInt8(index);
        Regs.A = new UInt8(a);

        return Enumerable.Range(0, cycles).Select(_ => ExecuteCycleWithMemory()).ToList();
    }

    /*
      TITLE: The zp,X / zp,Y / (zp,X) indexing cycle reads the zero page base address
      GIVEN: a CPU at $1000 with the instruction and operand $40, and X = Y = 5
      WHEN: the instruction runs its first 4 cycles
      THEN: cycle 3 reads $0040 (the base) and cycle 4 addresses $0045 (base + index)
    */
    [Theory]
    // zp,X / zp,Y
    [InlineData(OpCodes.ORAzpgx)]
    [InlineData(OpCodes.ASLzpgx)]
    [InlineData(OpCodes.BITzpgx)]
    [InlineData(OpCodes.ANDzpgx)]
    [InlineData(OpCodes.ROLzpgx)]
    [InlineData(OpCodes.NOP54)]
    [InlineData(OpCodes.EORzpgx)]
    [InlineData(OpCodes.LSRzpgx)]
    [InlineData(OpCodes.STZzpgx)]
    [InlineData(OpCodes.ADCzpgx)]
    [InlineData(OpCodes.RORzpgx)]
    [InlineData(OpCodes.STYzpgx)]
    [InlineData(OpCodes.STAzpgx)]
    [InlineData(OpCodes.STXzpgy)]
    [InlineData(OpCodes.LDYzpgx)]
    [InlineData(OpCodes.LDAzpgx)]
    [InlineData(OpCodes.LDXzpgy)]
    [InlineData(OpCodes.NOPD4)]
    [InlineData(OpCodes.CMPzpgx)]
    [InlineData(OpCodes.DECzpgx)]
    [InlineData(OpCodes.NOPF4)]
    [InlineData(OpCodes.SBCzpgx)]
    [InlineData(OpCodes.INCzpgx)]
    // (zp,X)
    [InlineData(OpCodes.ORAindx)]
    [InlineData(OpCodes.ANDindx)]
    [InlineData(OpCodes.EORindx)]
    [InlineData(OpCodes.ADCindx)]
    [InlineData(OpCodes.STAindx)]
    [InlineData(OpCodes.LDAindx)]
    [InlineData(OpCodes.CMPindx)]
    [InlineData(OpCodes.SBCindx)]
    public void TestZpIndexingCycleReadsBase(OpCodes opCode)
    {
        // ARRANGE:

        // ACT:
        var cycles = Execute(opCode, [0x40], 5, 4);

        // ASSERT:
        Assert.Equal(new MemoryCycle(0x0040, Memory[0x0040], false), cycles[2]);
        Assert.Equal(0x0045, cycles[3].Addr);
    }

    /*
      TITLE: abs,X / abs,Y / (zp),Y reads re-read the last instruction byte only on a page cross
      GIVEN: a CPU at $1000 with the instruction and its operands ($12F0 as the base, directly or
             through $40/$41), and X = Y = the given index
      WHEN: the instruction runs through its indexing cycle and the cycle after it
      THEN: with a page cross the indexing cycle reads the last instruction byte and the next cycle
            reads EA; without one the indexing cycle reads EA
    */
    [Theory]
    // abs,X / abs,Y, page cross: dummy read of $1002
    [InlineData(OpCodes.ORAabsy, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.ORAabsx, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.ANDabsy, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.BITabsx, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.ANDabsx, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.EORabsy, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.EORabsx, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.ADCabsy, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.ADCabsx, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.LDAabsy, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.LDYabsx, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.LDAabsx, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.LDXabsy, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.CMPabsy, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.CMPabsx, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.SBCabsy, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    [InlineData(OpCodes.SBCabsx, new[] { 0xF0, 0x12 }, 0x20, 3, 0x1002, 0x1310)]
    // (zp),Y, page cross: dummy read of $1001
    [InlineData(OpCodes.ORAindy, new[] { 0x40 }, 0x20, 4, 0x1001, 0x1310)]
    [InlineData(OpCodes.ANDindy, new[] { 0x40 }, 0x20, 4, 0x1001, 0x1310)]
    [InlineData(OpCodes.EORindy, new[] { 0x40 }, 0x20, 4, 0x1001, 0x1310)]
    [InlineData(OpCodes.ADCindy, new[] { 0x40 }, 0x20, 4, 0x1001, 0x1310)]
    [InlineData(OpCodes.LDAindy, new[] { 0x40 }, 0x20, 4, 0x1001, 0x1310)]
    [InlineData(OpCodes.CMPindy, new[] { 0x40 }, 0x20, 4, 0x1001, 0x1310)]
    [InlineData(OpCodes.SBCindy, new[] { 0x40 }, 0x20, 4, 0x1001, 0x1310)]
    // no page cross: the indexing cycle reads EA
    [InlineData(OpCodes.LDAabsx, new[] { 0xF0, 0x12 }, 0x05, 3, 0x12F5, 0x12F5)]
    [InlineData(OpCodes.LDAabsy, new[] { 0xF0, 0x12 }, 0x05, 3, 0x12F5, 0x12F5)]
    [InlineData(OpCodes.LDAindy, new[] { 0x40 }, 0x05, 4, 0x12F5, 0x12F5)]
    public void TestPageCrossReadsLastInstructionByte(OpCodes opCode, int[] operands, int index,
                                                      int indexCycle, int indexAddr, int ea)
    {
        // ARRANGE:
        var crossed = indexAddr != ea;

        // ACT:
        var cycles = Execute(opCode, operands, index, indexCycle + 2);

        // ASSERT:
        Assert.Equal(new MemoryCycle(indexAddr, Memory[indexAddr], false), cycles[indexCycle]);
        if (crossed)
            Assert.Equal(new MemoryCycle(ea, Memory[ea], false), cycles[indexCycle + 1]);
        else
            Assert.Equal(0x1000 + operands.Length + 1, cycles[indexCycle + 1].Addr);
    }

    /*
      TITLE: STA (zp),Y always re-reads its operand byte while Y is added
      GIVEN: a CPU at $1000 with STA ($40),Y, $40/$41 pointing at $12F0, A = $A5, and Y as given
      WHEN: STA (zp),Y runs its 6 cycles
      THEN: cycle 5 reads $1001 and cycle 6 writes $A5 to $12F0 + Y, with or without a page cross
    */
    [Theory]
    [InlineData(0x05)]
    [InlineData(0x20)]
    public void TestSTAindyReadsOperandByte(int y)
    {
        // ARRANGE:
        const int a = 0xA5;

        // ACT:
        var cycles = Execute(OpCodes.STAindy, [0x40], y, 6, a);

        // ASSERT:
        Assert.Equal(new MemoryCycle(0x1001, 0x40, false), cycles[4]);
        Assert.Equal(new MemoryCycle(0x12F0 + y, 0xA5, true), cycles[5]);
    }
}
