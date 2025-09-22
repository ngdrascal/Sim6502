// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class ROLTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // ROLacc
    // -------------------------------------------------------------------------
    private void ExecuteROLacc(UInt8 input, BitFlag carryIn, BitFlag expectedZFlag,
                               BitFlag expectedNFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.ROLacc.ToUInt8();
        var expectedValue = input.Copy().Shl(carryIn);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(carryIn);
        Regs.A.UpdateValue(input);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1);// fetch the opcode

        ExecuteClockCycles(1); // ROLacc - shift value and update flags

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
    public void TestROLaccZeroNoCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteROLacc(new UInt8(0), Low, High, Low, Low);
    }

    [Fact]
    public void TestROLaccZeroCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteROLacc(new UInt8(0), High, Low, Low, Low);
    }

    [Fact]
    public void TestROLaccPosNoCarryIn()
    {
        //                   value  Cin   Z    N   Cout
        ExecuteROLacc(new UInt8(1), Low, Low, Low, Low);
    }

    [Fact]
    public void TestROLaccPosCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteROLacc(new UInt8(1), High, Low, Low, Low);
    }

    [Fact]
    public void TestROLaccNegNoCarryIn()
    {
        //                      value  Cin   Z     N   Cout
        ExecuteROLacc(new UInt8(0x80), Low, High, Low, High);
    }

    [Fact]
    public void TestROLaccNegCarryIn()
    {
        //                      value  Cin   Z     N   Cout
        ExecuteROLacc(new UInt8(0x80), High, Low, Low, High);
    }

    // -------------------------------------------------------------------------
    // ROLzpg
    // -------------------------------------------------------------------------
    private void ExecuteROLzpg(UInt8 memValue, BitFlag carryIn, BitFlag expectedZFlag,
                               BitFlag expectedNFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.ROLzpg.ToUInt8();
        var operand1 = new UInt8(0x12);
        var expectedValue = memValue.Copy().Shl(carryIn);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(carryIn);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // ROLzpg2 - fetch address into EA

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // ROLzpg3 - fetch value from memory

        ExecuteClockCycles(1); // ROLzpg4 - shift value and update flags

        ExecuteClockCycles(1); // ROLzpg5 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(operand1), Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestROLzpgZeroNoCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteROLzpg(new UInt8(0), Low, High, Low, Low);
    }

    [Fact]
    public void TestROLzpgZeroCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteROLzpg(new UInt8(0), High, Low, Low, Low);
    }

    [Fact]
    public void TestROLzpgPosNoCarryIn()
    {
        //                   value  Cin   Z    N   Cout
        ExecuteROLzpg(new UInt8(1), Low, Low, Low, Low);
    }

    [Fact]
    public void TestROLzpgPosCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteROLzpg(new UInt8(1), High, Low, Low, Low);
    }

    [Fact]
    public void TestROLzpgNegNoCarryIn()
    {
        //                      value  Cin   Z     N   Cout
        ExecuteROLzpg(new UInt8(0x80), Low, High, Low, High);
    }

    [Fact]
    public void TestROLzpgNegCarryIn()
    {
        //                      value  Cin   Z     N   Cout
        ExecuteROLzpg(new UInt8(0x80), High, Low, Low, High);
    }

    // -------------------------------------------------------------------------
    // ROLzpgx
    // -------------------------------------------------------------------------
    private void ExecuteROLzpgx(UInt8 memValue, BitFlag carryIn, BitFlag expectedZFlag,
                                BitFlag expectedNFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.ROLzpgx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var xValue = new UInt8(0x10);
        var expectedAddr = new UInt16(operand1.Copy().AddWithWrapAround(xValue));
        var expectedValue = memValue.Copy().Shl(carryIn);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(carryIn);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // ROLzpgx2 - fetch operand1 into EA low

        ExecuteClockCycles(1); // ROLzpgx3 - add X reg to EA

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // ROLzpgx4 - fetch value from memory

        ExecuteClockCycles(1); // ROLzpgx5 - shift value and update flags

        ExecuteClockCycles(1); // ROLzpgx6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestROLzpgxZeroNoCarryIn()
    {
        //                    value  Cin   Z     N   Cout
        ExecuteROLzpgx(new UInt8(0), Low, High, Low, Low);
    }

    [Fact]
    public void TestROLzpgxZeroCarryIn()
    {
        //                    value  Cin   Z     N   Cout
        ExecuteROLzpgx(new UInt8(0), High, Low, Low, Low);
    }

    [Fact]
    public void TestROLzpgxPosNoCarryIn()
    {
        //                    value  Cin   Z    N   Cout
        ExecuteROLzpgx(new UInt8(1), Low, Low, Low, Low);
    }

    [Fact]
    public void TestROLzpgxPosCarryIn()
    {
        //                    value  Cin   Z     N   Cout
        ExecuteROLzpgx(new UInt8(1), High, Low, Low, Low);
    }

    [Fact]
    public void TestROLzpgxNegNoCarryIn()
    {
        //                       value  Cin   Z     N   Cout
        ExecuteROLzpgx(new UInt8(0x80), Low, High, Low, High);
    }

    [Fact]
    public void TestROLzpgxNegCarryIn()
    {
        //                       value  Cin   Z     N   Cout
        ExecuteROLzpgx(new UInt8(0x80), High, Low, Low, High);
    }

    // -------------------------------------------------------------------------
    // ROLabs
    // -------------------------------------------------------------------------
    private void ExecuteROLabs(UInt8 memValue, BitFlag carryIn, BitFlag expectedZFlag,
                               BitFlag expectedNFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.ROLabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2);
        var expectedValue = memValue.Copy().Shl(carryIn);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(carryIn);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // ROLabs2 - fetch operand1 into EA low

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // ROLabs3 - fetch operand2 into EA high

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // ROLabs4 - fetch value from memory

        ExecuteClockCycles(1); // ROLabs5 - shift value and update flags

        ExecuteClockCycles(1); // ROLabs6 - store the value back to EA

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
    public void TestROLabsZeroNoCarryIn()
    {
        //                  value  Cin   Z     N   Cout
        ExecuteROLabs(new UInt8(0), Low, High, Low, Low);
    }

    [Fact]
    public void TestROLabsZeroCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteROLabs(new UInt8(0), High, Low, Low, Low);
    }

    [Fact]
    public void TestROLabsPosNoCarryIn()
    {
        //                   value  Cin   Z    N   Cout
        ExecuteROLabs(new UInt8(1), Low, Low, Low, Low);
    }

    [Fact]
    public void TestROLabsPosCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteROLabs(new UInt8(1), High, Low, Low, Low);
    }

    [Fact]
    public void TestROLabsNegNoCarryIn()
    {
        //                      value  Cin   Z     N   Cout
        ExecuteROLabs(new UInt8(0x80), Low, High, Low, High);
    }

    [Fact]
    public void TestROLabsNegCarryIn()
    {
        //                      value  Cin   Z     N   Cout
        ExecuteROLabs(new UInt8(0x80), High, Low, Low, High);
    }

    // -------------------------------------------------------------------------
    // ROLabsx
    // -------------------------------------------------------------------------
    private void ExecuteROLabsx(UInt8 memValue, BitFlag carryIn, BitFlag expectedZFlag,
                                BitFlag expectedNFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.ROLabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var xValue = new UInt8(0x10);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);
        var expectedValue = memValue.Copy().Shl(carryIn);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(carryIn);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // ROLabsx2 - fetch operand1 into EA low

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // ROLabsx3 - fetch operand2 into EA high

        ExecuteClockCycles(1); // ROLabsx4 - add X to the EA reg

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // ROLabsx5 - fetch value from memory

        ExecuteClockCycles(1); // ROLabsx6 - shift value and update flags

        ExecuteClockCycles(1); // ROLabsx7 - store the value back to EA

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
    public void TestROLabsxZeroNoCarryIn()
    {
        //                    value  Cin   Z     N   Cout
        ExecuteROLabsx(new UInt8(0), Low, High, Low, Low);
    }

    [Fact]
    public void TestROLabsxZeroCarryIn()
    {
        //                    value  Cin   Z     N   Cout
        ExecuteROLabsx(new UInt8(0), High, Low, Low, Low);
    }

    [Fact]
    public void TestROLabsxPosNoCarryIn()
    {
        //                    value  Cin   Z    N   Cout
        ExecuteROLabsx(new UInt8(1), Low, Low, Low, Low);
    }

    [Fact]
    public void TestROLabsxPosCarryIn()
    {
        //                    value  Cin   Z     N   Cout
        ExecuteROLabsx(new UInt8(1), High, Low, Low, Low);
    }

    [Fact]
    public void TestROLabsxNegNoCarryIn()
    {
        //                       value  Cin   Z     N   Cout
        ExecuteROLabsx(new UInt8(0x80), Low, High, Low, High);
    }

    [Fact]
    public void TestROLabsxNegCarryIn()
    {
        //                      value  Cin   Z     N   Cout
        ExecuteROLabsx(new UInt8(0x80), High, Low, Low, High);
    }
}
