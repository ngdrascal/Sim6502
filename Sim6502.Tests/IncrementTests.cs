// ReSharper disable InconsistentNaming
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests;

public class IncrementTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // INC accumulator
    // -------------------------------------------------------------------------
    private void ExecuteINCacc(UInt8 accValue, UInt8 expectedValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        var opCode = OpCodes.INCacc.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A.UpdateValue(accValue);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstINCacc2 - increment accumulator and set flags

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1000), Pins.GetAddrBusPins());
        Assert.Equal(opCode, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    // -------------------------------------------------------------------------
    // INC zeropage
    // -------------------------------------------------------------------------
    private void ExecuteINCzpg(UInt8 memValue, UInt8 expectedValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        var opCode = OpCodes.INCzpg.ToUInt8();
        var operand1 = new UInt8(0x12);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstINCzpg2 - fetch address into EA

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstINCzpg3 - fetch value from memory

        ExecuteClockCycles(1); // InstINCzpg4 - increment value and update flags

        ExecuteClockCycles(1); // InstINCzpg5 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(operand1), Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestINCzpgNeg1ToZero()
    {
        ExecuteINCzpg(new UInt8(0xFF), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestINCzpgPosToNeg()
    {
        ExecuteINCzpg(new UInt8(0x7F), new UInt8(0x80), Low, High);
    }

    [Fact]
    public void TestINCzpgPosToPos()
    {
        ExecuteINCzpg(new UInt8(0x01), new UInt8(0x02), Low, Low);
    }

    [Fact]
    public void TestINCaccNeg1ToZero()
    {
        ExecuteINCacc(new UInt8(0xFF), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestINCaccPosToNeg()
    {
        ExecuteINCacc(new UInt8(0x7F), new UInt8(0x80), Low, High);
    }

    [Fact]
    public void TestINCaccPosToPos()
    {
        ExecuteINCacc(new UInt8(0x01), new UInt8(0x02), Low, Low);
    }

    // -------------------------------------------------------------------------
    // INC zeropage,X
    // -------------------------------------------------------------------------
    private void ExecuteINCzpgx(UInt8 memValue, UInt8 expectedValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.INCzpgx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var xValue = new UInt8(0x10);
        var expectedAddr = new UInt16(operand1.Copy().AddWithWrapAround(xValue));

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstINCzpgx2 - fetch operand1 into EA low

        ExecuteClockCycles(1); // InstINCzpgx3 - add X reg to EA

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstINCzpgx4 - fetch value from memory

        ExecuteClockCycles(1); // InstINCzpgx5 - increment value and update flags

        ExecuteClockCycles(1); // InstINCzpgx6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestINCzpgxNeg1ToZero()
    {
        ExecuteINCzpgx(new UInt8(0xFF), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestINCzpgxPosToNeg()
    {
        ExecuteINCzpgx(new UInt8(0x7F), new UInt8(0x80), Low, High);
    }

    [Fact]
    public void TestINCzpgxPosToPos()
    {
        ExecuteINCzpgx(new UInt8(0x01), new UInt8(0x02), Low, Low);
    }

    // -------------------------------------------------------------------------
    // INC absolute
    // -------------------------------------------------------------------------
    private void ExecuteINCabs(UInt8 memValue, UInt8 expectedValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.INCabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2);

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstINCabs2 - fetch operand1 into EA low

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstINCabs3 - fetch operand2 into EA high

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstINCabs4 - fetch value from memory

        ExecuteClockCycles(1); // InstINCabs5 - increment value and update flags

        ExecuteClockCycles(1); // InstINCabs6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestINCabsNeg1ToZero()
    {
        ExecuteINCabs(new UInt8(0xFF), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestINCabsPosToNeg()
    {
        ExecuteINCabs(new UInt8(0x7F), new UInt8(0x80), Low, High);
    }

    [Fact]
    public void TestINCabsPosToPos()
    {
        ExecuteINCabs(new UInt8(0x01), new UInt8(0x02), Low, Low);
    }

    // -------------------------------------------------------------------------
    // INC absolute,X
    // -------------------------------------------------------------------------
    private void ExecuteINCabsx(UInt8 memValue, UInt8 expectedValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.INCabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var xValue = new UInt8(0x10);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // INCabsx2 - fetch operand1 into EA low

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // INCabsx3 - fetch operand2 into EA high

        ExecuteClockCycles(1); // INCabsx4 - add X to the EA reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // INCabsx5 - fetch value from memory

        ExecuteClockCycles(1); // INCabsx6 - increment value and update flags

        ExecuteClockCycles(1); // INCabsx7 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestINCabsxNeg1ToZero()
    {
        ExecuteINCabsx(new UInt8(0xFF), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestINCabsxPosToNeg()
    {
        ExecuteINCabsx(new UInt8(0x7F), new UInt8(0x80), Low, High);
    }

    [Fact]
    public void TestINCabsxPosToPos()
    {
        ExecuteINCabsx(new UInt8(0x01), new UInt8(0x02), Low, Low);
    }

    // -------------------------------------------------------------------------
    // INX implied
    // -------------------------------------------------------------------------
    private void ExecuteINXimp(UInt8 regValue, UInt8 expectedValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        var opCode = OpCodes.INXimp.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(regValue);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstINXimp2 - increment register and set flags

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(expectedValue, Regs.X);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestINXimpNeg1ToZero()
    {
        ExecuteINXimp(new UInt8(0xFF), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestINXimpPosToNeg()
    {
        ExecuteINXimp(new UInt8(0x7F), new UInt8(0x80), Low, High);
    }

    [Fact]
    public void TestINXimpPosToPos()
    {
        ExecuteINXimp(new UInt8(0x01), new UInt8(0x02), Low, Low);
    }

    // -------------------------------------------------------------------------
    // INY implied
    // -------------------------------------------------------------------------
    private void ExecuteINYimp(UInt8 regValue, UInt8 expectedValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        var opCode = OpCodes.INYimp.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.Y.UpdateValue(regValue);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstINYimp2 - increment register and set flags

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(expectedValue, Regs.Y);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestINYimpNeg1ToZero()
    {
        ExecuteINYimp(new UInt8(0xFF), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestINYimpPosToNeg()
    {
        ExecuteINYimp(new UInt8(0x7F), new UInt8(0x80), Low, High);
    }

    [Fact]
    public void TestINYimpPosToPos()
    {
        ExecuteINYimp(new UInt8(0x01), new UInt8(0x02), Low, Low);
    }
}
