// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using Sim6502.Tests.Types;
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class ASLTests : UnitTestBase
{
    protected readonly UInt8 Zero = new(0);

    // -------------------------------------------------------------------------
    // ASLacc
    // -------------------------------------------------------------------------
    private void ExecuteASLacc(UInt8 accValue, BitFlag expectedZFlag, BitFlag expectedNFlag,
                               BitFlag expectedCFlag)
    {
        var opCode = OpCodes.ASLacc.ToUInt8();
        var expectedValue = accValue.Copy().Shl();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A.UpdateValue(accValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // ASLacc - shift value and update flags

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(BootAddr.AddUnsigned(new UInt8(1)), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestASLaccZeroNoCarry()
    {
        ExecuteASLacc(Zero, High, Low, Low);
    }

    [Fact]
    public void TestASLaccZeroCarry()
    {
        ExecuteASLacc(new UInt8(0x80), High, Low, High);
    }

    [Fact]
    public void TestASLaccNegNoCarry()
    {
        ExecuteASLacc(new UInt8(0x40), Low, High, Low);
    }

    [Fact]
    public void TestASLaccNegCarry()
    {
        ExecuteASLacc(new UInt8(0xC0), Low, High, High);
    }

    [Fact]
    public void TestASLaccNotZeroNotNegNoCarry()
    {
        ExecuteASLacc(new UInt8(0x01), Low, Low, Low);
    }

    [Fact]
    public void TestASLaccNotZeroNotNegCarry()
    {
        ExecuteASLacc(new UInt8(0x81), Low, Low, High);
    }

    // -------------------------------------------------------------------------
    // ASLzpg
    // -------------------------------------------------------------------------
    private void ExecuteASLzpg(UInt8 memValue, BitFlag expectedZFlag, BitFlag expectedNFlag,
                               BitFlag expectedCFlag)
    {
        var opCode = OpCodes.ASLzpg.ToUInt8();
        var operand1 = new UInt8(0x12);
        var expectedValue = memValue.Copy().Shl();

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // ASLzpg2 - fetch address into EA

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // ASLzpg3 - fetch value from memory

        ExecuteClockCycles(1); // ASLzpg4 - shift value and update flags

        ExecuteClockCycles(1); // ASLzpg5 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(operand1.ToInt(), Pins.GetAddrBusPins().ToInt());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(BootAddr.AddUnsigned(new UInt8(2)), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestASLzpgZeroNoCarry()
    {
        ExecuteASLzpg(Zero, High, Low, Low);
    }

    [Fact]
    public void TestASLzpgZeroCarry()
    {
        ExecuteASLzpg(new UInt8(0x80), High, Low, High);
    }

    [Fact]
    public void TestASLzpgNegNoCarry()
    {
        ExecuteASLzpg(new UInt8(0x40), Low, High, Low);
    }

    [Fact]
    public void TestASLzpgNegCarry()
    {
        ExecuteASLzpg(new UInt8(0xC0), Low, High, High);
    }

    [Fact]
    public void TestASLzpgNotZeroNotNegNoCarry()
    {
        ExecuteASLzpg(new UInt8(0x01), Low, Low, Low);
    }

    [Fact]
    public void TestASLzpgNotZeroNotNegCarry()
    {
        ExecuteASLzpg(new UInt8(0x81), Low, Low, High);
    }

    // -------------------------------------------------------------------------
    // ASLzpgx
    // -------------------------------------------------------------------------
    private void ExecuteASLzpgx(UInt8 memValue, BitFlag expectedZFlag, BitFlag expectedNFlag,
                                BitFlag expectedCFlag)
    {
        var opCode = OpCodes.ASLzpgx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var xValue = new UInt8(0x10);
        var expectedAddr = new UInt16(operand1.Copy().AddWithWrapAround(xValue));
        var expectedValue = memValue.Copy().Shl();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // ASLzpgx2 - fetch operand1 into EA low

        ExecuteClockCycles(1); // ASLzpgx3 - add X reg to EA

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // ASLzpgx4 - fetch value from memory

        ExecuteClockCycles(1); // ASLzpgx5 - shift value and update flags

        ExecuteClockCycles(1); // ASLzpgx6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(BootAddr.AddUnsigned(new UInt8(2)), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestASLzpgxZeroNoCarry()
    {
        ExecuteASLzpgx(new UInt8(0), High, Low, Low);
    }

    [Fact]
    public void TestASLzpgxZeroCarry()
    {
        ExecuteASLzpgx(new UInt8(0x80), High, Low, High);
    }

    [Fact]
    public void TestASLzpgxNegNoCarry()
    {
        ExecuteASLzpgx(new UInt8(0x40), Low, High, Low);
    }

    [Fact]
    public void TestASLzpgxNegCarry()
    {
        ExecuteASLzpgx(new UInt8(0xC0), Low, High, High);
    }

    [Fact]
    public void TestASLzpgxNotZeroNotNegNoCarry()
    {
        ExecuteASLzpgx(new UInt8(0x01), Low, Low, Low);
    }

    [Fact]
    public void TestASLzpgxNotZeroNotNegCarry()
    {
        ExecuteASLzpgx(new UInt8(0x81), Low, Low, High);
    }

    // -------------------------------------------------------------------------
    // ASLabs
    // -------------------------------------------------------------------------
    private void ExecuteASLabs(UInt8 memValue, BitFlag expectedZFlag, BitFlag expectedNFlag,
                               BitFlag expectedCFlag)
    {
        var opCode = OpCodes.ASLabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2);
        var expectedValue = memValue.Copy().Shl();

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.SetDataBusPins(OpCodes.ASLabs.ToUInt8());
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // ASLabs2 - fetch operand1 into EA low

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // ASLabs3 - fetch operand2 into EA high

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // ASLabs4 - fetch value from memory

        ExecuteClockCycles(1); // ASLabs5 - shift value and update flags

        ExecuteClockCycles(1); // ASLabs6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(BootAddr.AddUnsigned(new UInt8(3)), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestASLabsZeroNoCarry()
    {
        ExecuteASLabs(Zero, High, Low, Low);
    }

    [Fact]
    public void TestASLabsZeroCarry()
    {
        ExecuteASLabs(new UInt8(0x80), High, Low, High);
    }

    [Fact]
    public void TestASLabsNegNoCarry()
    {
        ExecuteASLabs(new UInt8(0x40), Low, High, Low);
    }

    [Fact]
    public void TestASLabsNegCarry()
    {
        ExecuteASLabs(new UInt8(0xC0), Low, High, High);
    }

    [Fact]
    public void TestASLabsNotZeroNotNegNoCarry()
    {
        ExecuteASLabs(new UInt8(0x01), Low, Low, Low);
    }

    [Fact]
    public void TestASLabsNotZeroNotNegCarry()
    {
        ExecuteASLabs(new UInt8(0x81), Low, Low, High);
    }

    // -------------------------------------------------------------------------
    // ASLabsx
    // -------------------------------------------------------------------------
    private void ExecuteASLabsx(UInt8 memValue, BitFlag expectedZFlag, BitFlag expectedNFlag,
                                BitFlag expectedCFlag)
    {
        var opCode = OpCodes.ASLabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var xValue = new UInt8(0x10);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);
        var expectedValue = memValue.Copy().Shl();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // ASLabsx2 - fetch operand1 into EA low

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // ASLabsx3 - fetch operand2 into EA high

        ExecuteClockCycles(1); // ASLabsx4 - add X to the EA reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // ASLabsx5 - fetch value from memory

        ExecuteClockCycles(1); // ASLabsx6 - shift value and update flags

        ExecuteClockCycles(1); // ASLabsx7 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(BootAddr.AddUnsigned(new UInt8(3)), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestASLabsxZeroNoCarry()
    {
        ExecuteASLabsx(Zero, High, Low, Low);
    }

    [Fact]
    public void TestASLabsxZeroCarry()
    {
        ExecuteASLabsx(new UInt8(0x80), High, Low, High);
    }

    [Fact]
    public void TestASLabsxNegNoCarry()
    {
        ExecuteASLabsx(new UInt8(0x40), Low, High, Low);
    }

    [Fact]
    public void TestASLabsxNegCarry()
    {
        ExecuteASLabsx(new UInt8(0xC0), Low, High, High);
    }

    [Fact]
    public void TestASLabsxNotZeroNotNegNoCarry()
    {
        ExecuteASLabsx(new UInt8(0x01), Low, Low, Low);
    }

    [Fact]
    public void TestASLabsxNotZeroNotNegCarry()
    {
        ExecuteASLabsx(new UInt8(0x81), Low, Low, High);
    }
}
