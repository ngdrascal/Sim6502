// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine.Types;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class TSBTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // TSBzpg
    // -------------------------------------------------------------------------
    private void ExecuteTSBzpg(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag)
    {
        var opCode = OpCodes.TSBzpg.ToUInt8();
        var operand1 = new UInt8(0x12);
        var expectedAddr = new UInt16(operand1);
        var expectedValue = aValue.Copy().Or(memValue.Copy());

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A = aValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // TSBzpg2 - fetch address into EA

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // TSBzpg3 - fetch value from memory

        ExecuteClockCycles(1); // TSBzpg4 - shift value and update flags

        ExecuteClockCycles(1); // TSBzpg5 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(aValue, Regs.A);
    }

    /*
      TITLE: TSB zpg sets Z when A and M are both zero
      GIVEN: A = $00 and memory = $00
      WHEN: TSB zpg executes
      THEN: memory is written with A OR M = $00, Z is set (A AND M = $00)
    */
    [Fact]
    public void TestTSBzpgZero()
    {
        ExecuteTSBzpg(new UInt8(0x00), new UInt8(0x00), High);
    }

    /*
      TITLE: TSB zpg clears Z when A AND M is non-zero
      GIVEN: A = $F0 and memory = $A1
      WHEN: TSB zpg executes
      THEN: memory is written with A OR M = $F1, Z is clear (A AND M = $A0)
    */
    [Fact]
    public void TestTSBzpgNotZero()
    {
        ExecuteTSBzpg(new UInt8(0xF0), new UInt8(0xA1), Low);
    }

    /*
      TITLE: TSB zpg sets Z from A AND M, not from the value written back
      GIVEN: A = $0F and memory = $F0
      WHEN: TSB zpg executes
      THEN: memory is written with A OR M = $FF, but Z is set because A AND M = $00
    */
    [Fact]
    public void TestTSBzpgNonZeroResultTestZero()
    {
        ExecuteTSBzpg(new UInt8(0x0F), new UInt8(0xF0), High);
    }

    // -------------------------------------------------------------------------
    // TSBabs
    // -------------------------------------------------------------------------
    private void ExecuteTSBabs(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag)
    {
        var opCode = OpCodes.TSBabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2);
        var expectedValue = new UInt8((aValue.ToInt() | memValue.ToInt()) & 0x000000FF);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A = aValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // TSBabs2 - fetch operand1 into EA low

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // TSBabs3 - fetch operand2 into EA high

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // TSBabs4 - fetch value from memory

        ExecuteClockCycles(1); // TSBabs5 - shift value and update flags

        ExecuteClockCycles(1); // TSBabs6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(aValue, Regs.A);
    }

    /*
      TITLE: TSB abs sets Z when A and M are both zero
      GIVEN: A = $00 and memory = $00
      WHEN: TSB abs executes
      THEN: memory is written with A OR M = $00, Z is set (A AND M = $00)
    */
    [Fact]
    public void TestTSBabsZero()
    {
        ExecuteTSBabs(new UInt8(0x00), new UInt8(0x00), High);
    }

    /*
      TITLE: TSB abs clears Z when A AND M is non-zero
      GIVEN: A = $F0 and memory = $A1
      WHEN: TSB abs executes
      THEN: memory is written with A OR M = $F1, Z is clear (A AND M = $A0)
    */
    [Fact]
    public void TestTSBabsNotZero()
    {
        ExecuteTSBabs(new UInt8(0xF0), new UInt8(0xA1), Low);
    }

    /*
      TITLE: TSB abs sets Z from A AND M, not from the value written back
      GIVEN: A = $0F and memory = $F0
      WHEN: TSB abs executes
      THEN: memory is written with A OR M = $FF, but Z is set because A AND M = $00
    */
    [Fact]
    public void TestTSBabsNonZeroResultTestZero()
    {
        ExecuteTSBabs(new UInt8(0x0F), new UInt8(0xF0), High);
    }
}
