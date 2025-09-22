// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class DecrementTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // DEC accumulator
    // -------------------------------------------------------------------------
    private void ExecuteDECacc(UInt8 accValue, UInt8 expectedValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        var opCode = OpCodes.DECacc.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A.UpdateValue(accValue);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        ExecuteClockCycles(1); // InstDECacc2 - decrement accumulator and set flags

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1000), Pins.AddrBus);
        Assert.Equal(opCode, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestDECaccZero()
    {
        ExecuteDECacc(new UInt8(0), new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestDECaccOne()
    {
        ExecuteDECacc(new UInt8(1), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestDECaccPos()
    {
        ExecuteDECacc(new UInt8(2), new UInt8(0x01), Low, Low);
    }

    // -------------------------------------------------------------------------
    // DEC zeropage
    // -------------------------------------------------------------------------
    private void ExecuteDECzpg(UInt8 memValue, UInt8 expectedValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        var opCode = OpCodes.DECzpg.ToUInt8();
        var operand1 = new UInt8(0x12);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstDECzpg2 - fetch address into EA

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstDECzpg3 - fetch value from memory

        ExecuteClockCycles(1); // InstDECzpg4 - decrement value and update flags

        ExecuteClockCycles(1); // InstDECzpg5 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(operand1), Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestDECzpgZero()
    {
        ExecuteDECzpg(new UInt8(0), new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestDECzpgOne()
    {
        ExecuteDECzpg(new UInt8(1), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestDECzpgPos()
    {
        ExecuteDECzpg(new UInt8(2), new UInt8(0x01), Low, Low);
    }

    // -------------------------------------------------------------------------
    // DEC zeropage,X
    // -------------------------------------------------------------------------
    private void ExecuteDECzpgx(UInt8 memValue, UInt8 expectedValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.DECzpgx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var xValue = new UInt8(0x10);
        var expectedAddr = new UInt16(operand1.Copy().AddWithWrapAround(xValue));

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstDECzpgx2 - fetch operand1 into EA low

        ExecuteClockCycles(1); // InstDECzpgx3 - add X reg to EA

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstDECzpgx4 - fetch value from memory

        ExecuteClockCycles(1); // InstDECzpgx5 - decrement value and update flags

        ExecuteClockCycles(1); // InstDECzpgx6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestDECzpgxZero()
    {
        ExecuteDECzpgx(new UInt8(0), new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestDECzpgxOne()
    {
        ExecuteDECzpgx(new UInt8(1), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestDECzpgxPos()
    {
        ExecuteDECzpgx(new UInt8(2), new UInt8(0x01), Low, Low);
    }

    // -------------------------------------------------------------------------
    // DEC absolute
    // -------------------------------------------------------------------------
    private void ExecuteDECabs(UInt8 memValue, UInt8 expectedValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.DECabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2);

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstDECabs2 - fetch operand1 into EA low

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstDECabs3 - fetch operand2 into EA high

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstDECabs4 - fetch value from memory

        ExecuteClockCycles(1); // InstDECabs5 - decrement value and update flags

        ExecuteClockCycles(1); // InstDECabs6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestDECabsZero()
    {
        ExecuteDECabs(new UInt8(0), new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestDECabsOne()
    {
        ExecuteDECabs(new UInt8(1), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestDECabsPos()
    {
        ExecuteDECabs(new UInt8(2), new UInt8(0x01), Low, Low);
    }

    // -------------------------------------------------------------------------
    // DEC absolute,X
    // -------------------------------------------------------------------------
    private void ExecuteDECabsx(UInt8 memValue, UInt8 expectedValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.DECabsx.ToUInt8();
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
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // DECabsx2 - fetch operand1 into EA low

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // DECabsx3 - fetch operand2 into EA high

        ExecuteClockCycles(1); // DECabsx4 - add X to the EA reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // DECabsx5 - fetch value from memory

        ExecuteClockCycles(1); // DECabsx6 - shift value and update flags

        ExecuteClockCycles(1); // DECabsx7 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestDECabsxZero()
    {
        ExecuteDECabsx(new UInt8(0), new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestDECabsxOne()
    {
        ExecuteDECabsx(new UInt8(1), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestDECabsxPos()
    {
        ExecuteDECabsx(new UInt8(2), new UInt8(0x01), Low, Low);
    }

    // -------------------------------------------------------------------------
    // DEX implied
    // -------------------------------------------------------------------------
    private void ExecuteDEXimp(UInt8 regValue, UInt8 expectedValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        var opCode = OpCodes.DEXimp.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(regValue);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        ExecuteClockCycles(1); // InstDEXimp2 - increment register and set flags

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(expectedValue, Regs.X);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestDEXimpZero()
    {
        ExecuteDEXimp(new UInt8(0), new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestDEXimpOne()
    {
        ExecuteDEXimp(new UInt8(1), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestDEXimpPos()
    {
        ExecuteDEXimp(new UInt8(2), new UInt8(0x01), Low, Low);
    }

    // -------------------------------------------------------------------------
    // DEY implied
    // -------------------------------------------------------------------------
    private void ExecuteDEYimp(UInt8 regValue, UInt8 expectedValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        var opCode = OpCodes.DEYimp.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.Y.UpdateValue(regValue);
        Regs.P.Zero.UpdateValue(expectedZFlag.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedNFlag.Copy().Not());

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        ExecuteClockCycles(1); // InstDEYimp2 - increment register and set flags

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(expectedValue, Regs.Y);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestDEYimpZero()
    {
        ExecuteDEYimp(new UInt8(0), new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestDEYimpOne()
    {
        ExecuteDEYimp(new UInt8(1), new UInt8(0x00), High, Low);
    }

    [Fact]
    public void TestDEYimpPos()
    {
        ExecuteDEYimp(new UInt8(2), new UInt8(0x01), Low, Low);
    }
}
