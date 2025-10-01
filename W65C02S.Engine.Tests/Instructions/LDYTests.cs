// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine;
using W65C02S.Engine.Types;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class LDYTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // LDY immediate
    // -------------------------------------------------------------------------
    private void ExecuteLDYimm(UInt8 expectedY, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.LDYimm.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (expectedY);
        ExecuteClockCycles(1); // InstLDYabs2 - fetch the first operand into X reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedY, Regs.Y);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDYimmWithZero()
    {
        ExecuteLDYimm(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDYimmWithNeg()
    {
        ExecuteLDYimm(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDYimmWithNotZeroNotNeg()
    {
        ExecuteLDYimm(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDY zeropage
    // -------------------------------------------------------------------------
    private void ExecuteLDYzpg(UInt8 expectedY, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.LDYzpg.ToUInt8();
        var operand = new UInt8(0xF0);
        var finalAddr = new UInt16(operand);

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstLDYzpg2 - fetch the second byte of the op-code (zpg-value)

        Pins.DataBus = (expectedY);
        ExecuteClockCycles(1); // InstLDYzpg3 - fetch the value at address zpg-value

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(finalAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedY, Regs.Y);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDYzpgWithZero()
    {
        ExecuteLDYzpg(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDYzpgWithNeg()
    {
        ExecuteLDYzpg(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDYzpgWithNotZeroNotNeg()
    {
        ExecuteLDYzpg(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDY zeropage,X
    // -------------------------------------------------------------------------
    private void ExecuteLDYzpgx(UInt8 expectedY, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.LDYzpgx.ToUInt8();
        var operand = new UInt8(0xE0);
        var xValue = new UInt8(0x10);
        var finalAddr = new UInt16(operand.Copy().AddWithWrapAround(xValue));

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstLDYzpgx2 - fetch the second byte of the op-code (zpg-value)

        ExecuteClockCycles(1); // InstLDYzpgx3 - effective address = zpg-value + Y

        Pins.DataBus = (expectedY);
        ExecuteClockCycles(1); // InstLDYzpgx4 - fetch the value at address the effect address

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(finalAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedY, Regs.Y);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDYzpgxWithZero()
    {
        ExecuteLDYzpgx(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDYzpgxWithNeg()
    {
        ExecuteLDYzpgx(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDYzpgxWithNotZeroNotNeg()
    {
        ExecuteLDYzpgx(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDY absolute
    // -------------------------------------------------------------------------
    private void ExecuteLDYabs(UInt8 expectedY, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.LDYabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var finalAddr = new UInt16(operand1, operand2);

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstLDYabs2 - fetch the second byte of the op-code (EA low)

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstLDYabs3 - fetch the third byte of the op-code (EA high)

        Pins.DataBus = (expectedY);
        ExecuteClockCycles(1); // InstLDYabs4 - fetch the value at effective address

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(finalAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedY, Regs.Y);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDYabsWith0()
    {
        ExecuteLDYabs(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDYabsWithNeg()
    {
        ExecuteLDYabs(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDYabsWithNotZeroNotNeg()
    {
        ExecuteLDYabs(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDY absolute,X
    // -------------------------------------------------------------------------
    private void ExecuteLDYabsx(UInt8 expectedY, UInt8 xValue, BitFlag expectedZFlag,
        BitFlag expectedNFlag)
    {
        var opCode = OpCodes.LDYabsx.ToUInt8();
        var operand1 = new UInt8(0x80);
        var operand2 = new UInt8(0x04);
        var absAddr = new UInt16(operand1, operand2);
        var finalAddr = absAddr.Copy().AddUnsigned(xValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstLDYabsx2 - fetch the second byte of the op-code (EA low)

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstLDYabsx3 - fetch the third byte of the op-code (EA high)

        Pins.DataBus = (expectedY);
        ExecuteClockCycles(1); // InstLDYabsx4 - fetch the value at address zpg-value + Y

        // if adding the Y reg cause the page to change
        if (!finalAddr.Msb().Equals(operand2))
        {
            Pins.DataBus = (expectedY);
            ExecuteClockCycles(1); // InstLDYabsy5 - fetch the value at address zpg-value + Y       
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(finalAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedY, Regs.Y);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDYabsxWith0()
    {
        ExecuteLDYabsx(new UInt8(0), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestLDYabsxWithNeg()
    {
        ExecuteLDYabsx(new UInt8(0xFF), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestLDYabsxWithNotZeroNotNeg()
    {
        ExecuteLDYabsx(new UInt8(1), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestLDYabsxWith0PageChange()
    {
        ExecuteLDYabsx(new UInt8(0), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestLDYabsxWithNegPageChange()
    {
        ExecuteLDYabsx(new UInt8(0xFF), new UInt8(0x80), Low, High);
    }

    [Fact]
    public void TestLDYabsxWithNotZeroNotNegPageChange()
    {
        ExecuteLDYabsx(new UInt8(1), new UInt8(0x80), Low, Low);
    }
}
