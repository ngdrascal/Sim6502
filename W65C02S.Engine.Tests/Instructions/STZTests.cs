// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class STZTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // STZ zeropage
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTZzpg()
    {
        var opCode = OpCodes.STZzpg.ToUInt8();
        var operand1 = new UInt8(0x34);
        var expectedAddr = new UInt16(operand1);

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstSTZzpg2 - fetch the second byte of the op-code (zpg-value)

        ExecuteClockCycles(1); // InstSTZzpg3 - store the value in Y at address zpg-value

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt8(0), Pins.DataBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.RWB);
    }

    // -------------------------------------------------------------------------
    // STZ zeropage,X
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTZzpgx()
    {
        var opCode = OpCodes.STZzpgx.ToUInt8();
        var operand1 = new UInt8(0x34);
        var xValue = new UInt8(0x56);
        var expectedAddr = new UInt16(operand1).AddUnsigned(xValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X = xValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstSTZzpgx2 - fetch the second byte of the op-code (zpg-value)

        ExecuteClockCycles(1); // InstSTZzpgx3 - EA = zpg-value + X reg

        ExecuteClockCycles(1); // InstSTZzpgx4 - store the zero at EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt8(0), Pins.DataBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.RWB);
    }

    // -------------------------------------------------------------------------
    // STZ absolute
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTZabs()
    {
        var opCode = OpCodes.STZabs.ToUInt8();
        var operand1 = new UInt8(0x34);
        var operand2 = new UInt8(0x12);
        var expectedAddr = new UInt16(operand1, operand2);

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstSTZabs2 - fetch the first byte of the addr

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstSTZabs3 - fetch the second byte of the addr, EA = op2,op1

        ExecuteClockCycles(1); // InstSTZabs4 - store the zero at EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt8(0), Pins.DataBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.RWB);
    }

    // -------------------------------------------------------------------------
    // STZ absolute,X
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTZabsx()
    {
        ExecuteSTZabsx(new UInt8(0x10));
    }

    /*
       TITLE: STZ absolute,X carries the index into the high byte of the address
       GIVEN: a booted CPU with X=$F0
       WHEN: STZ $4321,X executes ($21 + $F0 crosses into page $44)
       THEN: zero is written to $4411
     */
    [Fact]
    public void TestSTZabsxPageChange()
    {
        ExecuteSTZabsx(new UInt8(0xF0));
    }

    private void ExecuteSTZabsx(UInt8 xValue)
    {
        var opCode = OpCodes.STZabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X = xValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstSTZabsx2 - fetch the first byte of the addr

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstSTZabsx3 - fetch the second byte of the addr, EA = op2,op1

        ExecuteClockCycles(1); // InstSTZabsx4 - add X reg to EA

        ExecuteClockCycles(1); // InstSTZabsx5 - store zero at EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt8(0), Pins.DataBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.RWB);
    }
}
