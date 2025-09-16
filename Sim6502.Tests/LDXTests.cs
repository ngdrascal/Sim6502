// ReSharper disable InconsistentNaming
using Sim6502.types;
using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502.Tests;

public class LDXTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // LDX immediate
    // -------------------------------------------------------------------------
    private void ExecuteLDXimm(UInt8 expectedX, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.LDXimm.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(expectedX);
        ExecuteClockCycles(1); // InstLDXabs2 - fetch the first operand into X reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedX, Regs.X);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDXimmWithZero()
    {
        ExecuteLDXimm(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDXimmWithNeg()
    {
        ExecuteLDXimm(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDXimmWithNotZeroNotNeg()
    {
        ExecuteLDXimm(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDX zeropage
    // -------------------------------------------------------------------------
    private void ExecuteLDXzpg(UInt8 expectedX, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.LDXzpg.ToUInt8();
        var operand = new UInt8(0xF0);

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstLDXzpg2 - fetch the second byte of the op-code (zpg-value)

        Pins.SetDataBusPins(expectedX);
        ExecuteClockCycles(1); // InstLDXzpg3 - fetch the value at address zpg-value

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(operand), Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedX, Regs.X);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDXzpgWithZero()
    {
        ExecuteLDXzpg(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDXzpgWithNeg()
    {
        ExecuteLDXzpg(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDXzpgWithNotZeroNotNeg()
    {
        ExecuteLDXzpg(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDX zeropage,Y
    // -------------------------------------------------------------------------
    private void ExecuteLDXzpgy(UInt8 expectedX, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.LDXzpgy.ToUInt8();
        var operand = new UInt8(0xE0);
        var yValue = new UInt8(0x10);
        var finalAddr = new UInt16(operand.Copy().AddWithWrapAround(yValue));

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstLDXzpgy2 - fetch the second byte of the op-code (zpg-value)

        ExecuteClockCycles(1); // InstLDXzpgy3 - effective address = zpg-value + Y

        Pins.SetDataBusPins(expectedX);
        ExecuteClockCycles(1); // InstLDXzpgy4 - fetch the value at address the effect address

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedX, Regs.X);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDXzpgyWithZero()
    {
        ExecuteLDXzpgy(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDXzpgyWithNeg()
    {
        ExecuteLDXzpgy(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDXzpgyWithNotZeroNotNeg()
    {
        ExecuteLDXzpgy(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDX absolute
    // -------------------------------------------------------------------------
    private void ExecuteLDXabs(UInt8 expectedX, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.LDXabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var finalAddr = new UInt16(operand1, operand2);

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstLDXabs2 - fetch the second byte of the op-code (EA low)

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstLDXabs3 - fetch the thrid byte of the op-code (EA high)

        Pins.SetDataBusPins(expectedX);
        ExecuteClockCycles(1); // InstLDXabs4 - fetch the value at effective address

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedX, Regs.X);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDXabsWith0()
    {
        ExecuteLDXabs(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDXabsWithNeg()
    {
        ExecuteLDXabs(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDXabsWithNotZeroNotNeg()
    {
        ExecuteLDXabs(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDX absolute,Y
    // -------------------------------------------------------------------------
    private void ExecuteLDXabsy(UInt8 expectedX, UInt8 yValue, BitFlag expectedZFlag,
        BitFlag expectedNFlag)
    {
        var opCode = OpCodes.LDXabsy.ToUInt8();
        var operand1 = new UInt8(0x80);
        var operand2 = new UInt8(0x04);
        var finalAddr = new UInt16(operand1, operand2).AddUnsigned(yValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstLDXabsy2 - fetch the second byte of the op-code (EA low)

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstLDXabsy3 - fetch the thrid byte of the op-code (EA high)

        Pins.SetDataBusPins(expectedX);
        ExecuteClockCycles(1); // InstLDXabsy4 - fetch the value at address zpg-value + Y

        // if adding the Y reg cause the page to change
        if (!finalAddr.Msb().Equals(operand2))
        {
            Pins.SetDataBusPins(expectedX);
            ExecuteClockCycles(1); // InstLDXabsy5 - fetch the value at address zpg-value + Y       
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedX, Regs.X);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDXabsyWith0()
    {
        ExecuteLDXabsy(new UInt8(0), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestLDXabsyWithNeg()
    {
        ExecuteLDXabsy(new UInt8(0xFF), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestLDXabsyWithNotZeroNotNeg()
    {
        ExecuteLDXabsy(new UInt8(1), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestLDXabsyWith0PageChange()
    {
        ExecuteLDXabsy(new UInt8(0), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestLDXabsyWithNegPageChange()
    {
        ExecuteLDXabsy(new UInt8(0xFF), new UInt8(0x80), Low, High);
    }

    [Fact]
    public void TestLDXabsyWithNotZeroNotNegPageChange()
    {
        ExecuteLDXabsy(new UInt8(1), new UInt8(0x80), Low, Low);
    }
}
