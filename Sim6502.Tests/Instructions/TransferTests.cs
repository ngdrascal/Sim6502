// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class TransferTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // TAX
    // -------------------------------------------------------------------------
    private void ExecuteTAXimp(UInt8 aValue, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.TAXimp.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstTAXimp - transfer the value in the A reg to the X reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(aValue, Regs.X);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestTAXimpWith0()
    {
        ExecuteTAXimp(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestTAXimpWithNeg()
    {
        ExecuteTAXimp(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestTAXimpWithNotZeroNotNeg()
    {
        ExecuteTAXimp(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // TAY
    // -------------------------------------------------------------------------
    private void ExecuteTAYimp(UInt8 aValue, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.TAYimp.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstTAYimp - transfer the value in the A reg to the Y reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(aValue, Regs.Y);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestTAYimpWith0()
    {
        ExecuteTAYimp(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestTAYimpWithNeg()
    {
        ExecuteTAYimp(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestTAYimpWithNotZeroNotNeg()
    {
        ExecuteTAYimp(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // TSX
    // -------------------------------------------------------------------------
    private void ExecuteTSXimp(UInt8 sValue, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.TSXimp.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.S.UpdateValue(sValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstTSXimp - transfer the value in the S reg to the X reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(sValue, Regs.X);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestTSXimpWith0()
    {
        ExecuteTSXimp(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestTSXimpWithNeg()
    {
        ExecuteTSXimp(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestTSXimpWithNotZeroNotNeg()
    {
        ExecuteTSXimp(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // TXA
    // -------------------------------------------------------------------------
    private void ExecuteTXAimp(UInt8 xValue, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.TXAimp.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstTXAimp - transfer the value in the X reg to the A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(xValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestTXAimpWith0()
    {
        ExecuteTXAimp(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestTXAimpWithNeg()
    {
        ExecuteTXAimp(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestTXAimpWithNotZeroNotNeg()
    {
        ExecuteTXAimp(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // TXS
    // -------------------------------------------------------------------------
    private void ExecuteTXSimp(UInt8 xValue, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.TXSimp.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstTXSimp - transfer the value in the X reg to the S reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(xValue, Regs.S);
    }

    [Fact]
    public void TestTXSimpWith0()
    {
        ExecuteTXSimp(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestTXSimpWithNeg()
    {
        ExecuteTXSimp(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestTXSimpWithNotZeroNotNeg()
    {
        ExecuteTXSimp(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // TYA
    // -------------------------------------------------------------------------
    private void ExecuteTYAimp(UInt8 yValue, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        var opCode = OpCodes.TYAimp.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstTYAimp - transfer the value in the Y reg to the A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(yValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestTYAimpWith0()
    {
        ExecuteTYAimp(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestTYAimpWithNeg()
    {
        ExecuteTYAimp(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestTYAimpWithNotZeroNotNeg()
    {
        ExecuteTYAimp(new UInt8(1), Low, Low);
    }
}
