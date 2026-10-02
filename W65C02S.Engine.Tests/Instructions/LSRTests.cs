// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine.Types;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class LSRTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // LSRacc
    // -------------------------------------------------------------------------
    private void ExecuteLSRacc(UInt8 input, BitFlag expectedZFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.LSRacc.ToUInt8();
        var expectedValue = input.Copy().Shr();
        var expectedNFlag = new BitFlag(false);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A.UpdateValue(input);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // LSRacc - shift value and update flags

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(0x1000), Pins.AddrBus);
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestLSRaccZeroNoCarry()
    {
        ExecuteLSRacc(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLSRaccZeroCarry()
    {
        ExecuteLSRacc(new UInt8(0x01), High, High);
    }

    [Fact]
    public void TestLSRaccNegNoCarry()
    {
        ExecuteLSRacc(new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestLSRaccNegCarry()
    {
        ExecuteLSRacc(new UInt8(0x11), Low, High);
    }

    // -------------------------------------------------------------------------
    // LSRzpg
    // -------------------------------------------------------------------------
    private void ExecuteLSRzpg(UInt8 memValue, BitFlag expectedZFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.LSRzpg.ToUInt8();
        var operand = new UInt8(0x12);
        var expectedValue = memValue.Copy().Shr();
        var finalAddr = new UInt16(operand);
        var expectedNFlag = new BitFlag(false);

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // LSRzpg2 - fetch address into EA

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // LSRzpg3 - fetch value from memory

        ExecuteClockCycles(1); // LSRzpg4 - shift value and update flags

        ExecuteClockCycles(1); // LSRzpg5 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(finalAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestLSRzpgZeroNoCarry()
    {
        ExecuteLSRzpg(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLSRzpgZeroCarry()
    {
        ExecuteLSRzpg(new UInt8(0x01), High, High);
    }

    [Fact]
    public void TestLSRzpgNegNoCarry()
    {
        ExecuteLSRzpg(new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestLSRzpgNegCarry()
    {
        ExecuteLSRzpg(new UInt8(0x11), Low, High);
    }

    // -------------------------------------------------------------------------
    // LSRzpgx
    // -------------------------------------------------------------------------
    private void ExecuteLSRzpgx(UInt8 memValue, BitFlag expectedZFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.LSRzpgx.ToUInt8();
        var operand = new UInt8(0x21);
        var xValue = new UInt8(0x10);
        var finalAddr = new UInt16(operand.Copy().AddWithWrapAround(xValue));
        var expectedValue = memValue.Copy().Shr();
        var expectedNFlag = new BitFlag(false);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // LSRzpgx2 - fetch operand1 into EA low

        ExecuteClockCycles(1); // LSRzpgx3 - add X reg to EA

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // LSRzpgx4 - fetch value from memory

        ExecuteClockCycles(1); // LSRzpgx5 - shift value and update flags

        ExecuteClockCycles(1); // LSRzpgx6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(finalAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestLSRzpgxZeroNoCarry()
    {
        ExecuteLSRzpgx(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLSRzpgxZeroCarry()
    {
        ExecuteLSRzpgx(new UInt8(0x01), High, High);
    }

    [Fact]
    public void TestLSRzpgxNegNoCarry()
    {
        ExecuteLSRzpgx(new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestLSRzpgxNegCarry()
    {
        ExecuteLSRzpgx(new UInt8(0x11), Low, High);
    }

    // -------------------------------------------------------------------------
    // LSRabs
    // -------------------------------------------------------------------------
    private void ExecuteLSRabs(UInt8 memValue, BitFlag expectedZFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.LSRabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var finalAddr = new UInt16(operand1, operand2);
        var expectedValue = memValue.Copy().Shr();
        var expectedNFlag = new BitFlag(false);

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // LSRabs2 - fetch operand1 into EA low

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // LSRabs3 - fetch operand2 into EA high

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // LSRabs4 - fetch value from memory

        ExecuteClockCycles(1); // LSRabs5 - shift value and update flags

        ExecuteClockCycles(1); // LSRabs6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(finalAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestLSRabsZeroNoCarry()
    {
        ExecuteLSRabs(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLSRabsZeroCarry()
    {
        ExecuteLSRabs(new UInt8(1), High, High);
    }

    [Fact]
    public void TestLSRabsNegNoCarry()
    {
        ExecuteLSRabs(new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestLSRabsNegCarry()
    {
        ExecuteLSRabs(new UInt8(0x11), Low, High);
    }

    // -------------------------------------------------------------------------
    // LSRabsx
    // -------------------------------------------------------------------------
    private void ExecuteLSRabsx(UInt8 memValue, BitFlag expectedZFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.LSRabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var xValue = new UInt8(0x10);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);
        var expectedValue = memValue.Copy().Shr();
        var expectedNFlag = new BitFlag(false);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // LSRabsx2 - fetch operand1 into EA low

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // LSRabsx3 - fetch operand2 into EA high

        ExecuteClockCycles(1); // LSRabsx4 - add X to the EA reg

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // LSRabsx5 - fetch value from memory

        ExecuteClockCycles(1); // LSRabsx6 - shift value and update flags

        ExecuteClockCycles(1); // LSRabsx7 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestLSRabsxZeroNoCarry()
    {
        ExecuteLSRabsx(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLSRabsxZeroCarry()
    {
        ExecuteLSRabsx(new UInt8(1), High, High);
    }

    [Fact]
    public void TestLSRabsxNegNoCarry()
    {
        ExecuteLSRabsx(new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestLSRabsxNegCarry()
    {
        ExecuteLSRabsx(new UInt8(0x11), Low, High);
    }
}
