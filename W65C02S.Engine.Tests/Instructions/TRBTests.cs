// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine.Types;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class TRBTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // TRBzpg
    // -------------------------------------------------------------------------
    private void ExecuteTRBzpg(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag)
    {
        var opCode = OpCodes.TRBzpg.ToUInt8();
        var operand1 = new UInt8(0x12);
        var expectedAddr = new UInt16(operand1);
        var expectedValue = aValue.Copy().Not().And(memValue.Copy());

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A = aValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // TRBzpg2 - fetch address into EA

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // TRBzpg3 - fetch value from memory

        ExecuteClockCycles(1); // TRBzpg4 - shift value and update flags

        ExecuteClockCycles(1); // TRBzpg5 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(aValue, Regs.A);
    }

    /*
      TITLE: TRB zpg clears Z when A AND M is non-zero
      GIVEN: A = $F0 and memory = $A1
      WHEN: TRB zpg executes
      THEN: memory is written with ~A AND M = $01, Z is clear (A AND M = $A0), A is unchanged
    */
    [Fact]
    public void TestTRBzpgNotZero()
    {
        ExecuteTRBzpg(new UInt8(0xF0), new UInt8(0xA1), Low);
    }

    /*
      TITLE: TRB zpg sets Z from A AND M, not from the value written back
      GIVEN: A = $F0 and memory = $A0
      WHEN: TRB zpg executes
      THEN: memory is written with ~A AND M = $00, but Z is clear because A AND M = $A0
    */
    [Fact]
    public void TestTRBzpgZeroResultTestNotZero()
    {
        ExecuteTRBzpg(new UInt8(0xF0), new UInt8(0xA0), Low);
    }

    /*
      TITLE: TRB zpg clears Z when all bits are reset even though A has bits clear
      GIVEN: A = $FF and memory = $0F
      WHEN: TRB zpg executes
      THEN: memory is written with ~A AND M = $00, Z is clear because A AND M = $0F
    */
    [Fact]
    public void TestTRBzpgAllBitsReset()
    {
        ExecuteTRBzpg(new UInt8(0xFF), new UInt8(0x0F), Low);
    }

    /*
      TITLE: TRB zpg sets Z when A AND M is zero even though the written value is not
      GIVEN: A = $F0 and memory = $0F
      WHEN: TRB zpg executes
      THEN: memory is written with ~A AND M = $0F, Z is set because A AND M = $00
    */
    [Fact]
    public void TestTRBzpgTestZero()
    {
        ExecuteTRBzpg(new UInt8(0xF0), new UInt8(0x0F), High);
    }

    // -------------------------------------------------------------------------
    // TRBabs
    // -------------------------------------------------------------------------
    private void ExecuteTRBabs(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag)
    {
        var opCode = OpCodes.TRBabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2);
        var expectedValue = aValue.Copy().Not().And(memValue.Copy());

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A = aValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // TRBabs2 - fetch operand1 into EA low

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // TRBabs3 - fetch operand2 into EA high

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // TRBabs4 - fetch value from memory

        ExecuteClockCycles(1); // TRBabs5 - shift value and update flags

        ExecuteClockCycles(1); // TRBabs6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(aValue, Regs.A);
    }

    /*
      TITLE: TRB abs clears Z when A AND M is non-zero
      GIVEN: A = $F0 and memory = $A1
      WHEN: TRB abs executes
      THEN: memory is written with ~A AND M = $01, Z is clear (A AND M = $A0), A is unchanged
    */
    [Fact]
    public void TestTRBabsNotZero()
    {
        ExecuteTRBabs(new UInt8(0xF0), new UInt8(0xA1), Low);
    }

    /*
      TITLE: TRB abs sets Z from A AND M, not from the value written back
      GIVEN: A = $F0 and memory = $A0
      WHEN: TRB abs executes
      THEN: memory is written with ~A AND M = $00, but Z is clear because A AND M = $A0
    */
    [Fact]
    public void TestTRBabsZeroResultTestNotZero()
    {
        ExecuteTRBabs(new UInt8(0xF0), new UInt8(0xA0), Low);
    }

    /*
      TITLE: TRB abs clears Z when all bits are reset even though A has bits clear
      GIVEN: A = $FF and memory = $0F
      WHEN: TRB abs executes
      THEN: memory is written with ~A AND M = $00, Z is clear because A AND M = $0F
    */
    [Fact]
    public void TestTRBabsAllBitsReset()
    {
        ExecuteTRBabs(new UInt8(0xFF), new UInt8(0x0F), Low);
    }

    /*
      TITLE: TRB abs sets Z when A AND M is zero even though the written value is not
      GIVEN: A = $F0 and memory = $0F
      WHEN: TRB abs executes
      THEN: memory is written with ~A AND M = $0F, Z is set because A AND M = $00
    */
    [Fact]
    public void TestTRBabsTestZero()
    {
        ExecuteTRBabs(new UInt8(0xF0), new UInt8(0x0F), High);
    }
}
