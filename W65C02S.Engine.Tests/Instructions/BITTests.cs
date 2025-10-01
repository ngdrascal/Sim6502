// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine;
using W65C02S.Engine.Types;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class BITTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // BIT immediate
    // -------------------------------------------------------------------------
    private void ExecuteBITimm(UInt8 aValue, UInt8 operand, BitFlag expectedZFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.BITimm.ToUInt8();
        var expectedNFlag = Regs.P.Negative;
        var expectedVFlag = Regs.P.Overflow;

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstBITimm2 - AND A reg. with operand, update flags

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(aValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedVFlag, Regs.P.Overflow);
    }

    [Fact]
    public void TestBITimmPosNoV()
    {
        ExecuteBITimm(new UInt8(0b00000111), new UInt8(0b00000011), Low);
    }

    [Fact]
    public void TestBITimmPosWithV()
    {
        ExecuteBITimm(new UInt8(0b01000111), new UInt8(0b01000011), Low);
    }

    [Fact]
    public void TestBITimmZero()
    {
        ExecuteBITimm(new UInt8(0b11110000), new UInt8(0b00001111), High);
    }

    [Fact]
    public void TestBITimmNegNoV()
    {
        ExecuteBITimm(new UInt8(0b10000000), new UInt8(0b10000000), Low);
    }

    [Fact]
    public void TestBITimmNegWithV()
    {
        ExecuteBITimm(new UInt8(0b11000000), new UInt8(0b11000000), Low);
    }

    // -------------------------------------------------------------------------
    // BIT zeropage
    // -------------------------------------------------------------------------
    private void ExecuteBITzpg(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag, BitFlag expectedVFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.BITzpg.ToUInt8();
        var operand = new UInt8(0x12);
        var expectedAddr = new UInt16(operand);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstBITzpg2 - fetch the operand into EA reg

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstBITzpg3 - fetch the memory value at EA then BIT it with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(aValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedVFlag, Regs.P.Overflow);
    }

    [Fact]
    public void TestBITzpgPosNoV()
    {
        ExecuteBITzpg(new UInt8(0b00000111), new UInt8(0b00000011), Low, Low, Low);
    }

    [Fact]
    public void TestBITzpgPosWithV()
    {
        ExecuteBITzpg(new UInt8(0b01000111), new UInt8(0b01000011), Low, Low, High);
    }

    [Fact]
    public void TestBITzpgZero()
    {
        ExecuteBITzpg(new UInt8(0b11110000), new UInt8(0b00001111), High, Low, Low);
    }

    [Fact]
    public void TestBITzpgNegNoV()
    {
        ExecuteBITzpg(new UInt8(0b10000000), new UInt8(0b10000000), Low, High, Low);
    }

    [Fact]
    public void TestBITzpgNegWithV()
    {
        ExecuteBITzpg(new UInt8(0b11000000), new UInt8(0b11000000), Low, High, High);
    }

    // -------------------------------------------------------------------------
    // BIT zeropage,X
    // -------------------------------------------------------------------------
    private void ExecuteBITzpgx(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag, BitFlag expectedVFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.BITzpgx.ToUInt8();
        var operand = new UInt8(0x12);
        var xValue = new UInt8(0x10);
        var expectedAddr = new UInt16(operand.Copy().AddWithWrapAround(xValue));

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstBITzpgx2 - fetch the operand into EA reg

        ExecuteClockCycles(1); // InstBITzpgx3 - EA = EA + X

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstBITzpgx4 - fetch the memory value at EA into temp then BIT Temp reg with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(aValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedVFlag, Regs.P.Overflow);
    }

    [Fact]
    public void TestBITzpgxPosNoV()
    {
        ExecuteBITzpgx(new UInt8(0b00000111), new UInt8(0b00000011), Low, Low, Low);
    }

    [Fact]
    public void TestBITzpgxPosWithV()
    {
        ExecuteBITzpgx(new UInt8(0b01000111), new UInt8(0b01000011), Low, Low, High);
    }

    [Fact]
    public void TestBITzpgxZero()
    {
        ExecuteBITzpgx(new UInt8(0b11110000), new UInt8(0b00001111), High, Low, Low);
    }

    [Fact]
    public void TestBITzpgxNegNoV()
    {
        ExecuteBITzpgx(new UInt8(0b10000000), new UInt8(0b10000000), Low, High, Low);
    }

    [Fact]
    public void TestBITzpgxNegWithV()
    {
        ExecuteBITzpgx(new UInt8(0b11000000), new UInt8(0b11000000), Low, High, High);
    }

    // -------------------------------------------------------------------------
    // BIT absolute
    // -------------------------------------------------------------------------
    private void ExecuteBITabs(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag, BitFlag expectedVFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.BITabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstBITabs2 - fetch the operand into EAL reg

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstBITabs3 - fetch the operand into EAH reg

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstBITabs4 - fetch the memory value at EA into temp then BIT Temp reg with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(aValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedVFlag, Regs.P.Overflow);
    }

    [Fact]
    public void TestBITabsPosNoV()
    {
        ExecuteBITabs(new UInt8(0b00000111), new UInt8(0b00000011), Low, Low, Low);
    }

    [Fact]
    public void TestBITabsPosWithV()
    {
        ExecuteBITabs(new UInt8(0b01000111), new UInt8(0b01000011), Low, Low, High);
    }

    [Fact]
    public void TestBITabsZero()
    {
        ExecuteBITabs(new UInt8(0b11110000), new UInt8(0b00001111), High, Low, Low);
    }

    [Fact]
    public void TestBITabsNegNoV()
    {
        ExecuteBITabs(new UInt8(0b10000000), new UInt8(0b10000000), Low, High, Low);
    }

    [Fact]
    public void TestBITabsNegWithV()
    {
        ExecuteBITabs(new UInt8(0b11000000), new UInt8(0b11000000), Low, High, High);
    }

    // -------------------------------------------------------------------------
    // BIT absolute,X
    // -------------------------------------------------------------------------
    private void ExecuteBITabsx(UInt8 aValue, UInt8 memValue, UInt8 xValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag, BitFlag expectedVFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.BITabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstBITabsx2 - fetch the operand into EAL reg

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstBITabsx3 - fetch the operand into EAH reg

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstBITabsx4 - fetch the memory value at EA into temp then BIT Temp reg with A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.DataBus = (memValue);
            ExecuteClockCycles(1); // InstADDabsx5 - fetch the value at address EA + Y
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(aValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedVFlag, Regs.P.Overflow);
    }

    [Fact]
    public void TestBITabsxPosNoV()
    {
        ExecuteBITabsx(new UInt8(0b00000111), new UInt8(0b00000011), new UInt8(0x10), Low, Low, Low);
    }

    [Fact]
    public void TestBITabsxPosWithV()
    {
        ExecuteBITabsx(new UInt8(0b01000111), new UInt8(0b01000011), new UInt8(0x10), Low, Low, High);
    }

    [Fact]
    public void TestBITabsxZero()
    {
        ExecuteBITabsx(new UInt8(0b11110000), new UInt8(0b00001111), new UInt8(0x10), High, Low, Low);
    }

    [Fact]
    public void TestBITabsxNegNoV()
    {
        ExecuteBITabsx(new UInt8(0b10000000), new UInt8(0b10000000), new UInt8(0x10), Low, High, Low);
    }

    [Fact]
    public void TestBITabsxNegWithV()
    {
        ExecuteBITabsx(new UInt8(0b11000000), new UInt8(0b11000000), new UInt8(0x10), Low, High, High);
    }

    [Fact]
    public void TestBITabsxPosNoVPageChanged()
    {
        ExecuteBITabsx(new UInt8(0b00000111), new UInt8(0b00000011), new UInt8(0x80), Low, Low, Low);
    }

    [Fact]
    public void TestBITabsxPosWithVPageChanged()
    {
        ExecuteBITabsx(new UInt8(0b01000111), new UInt8(0b01000011), new UInt8(0x80), Low, Low, High);
    }

    [Fact]
    public void TestBITabsxZeroPageChanged()
    {
        ExecuteBITabsx(new UInt8(0b11110000), new UInt8(0b00001111), new UInt8(0x80), High, Low, Low);
    }

    [Fact]
    public void TestBITabsxNegNoVPageChanged()
    {
        ExecuteBITabsx(new UInt8(0b10000000), new UInt8(0b10000000), new UInt8(0x80), Low, High, Low);
    }

    [Fact]
    public void TestBITabsxNegWithVPageChanged()
    {
        ExecuteBITabsx(new UInt8(0b11000000), new UInt8(0b11000000), new UInt8(0x80), Low, High, High);
    }
}
