// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class RORTests : UnitTestBase
{
    ////////////////////////////////////////////
    //
    // -----------|----------------|--------------
    //      IN    |      OUT       | 
    // -----------|----------------| Test Name
    //   VALUE  C |   VALUE  Z N C | 
    // -----------|----------------|--------------
    // 00000010 0 | 00000001 0 0 0 | EvenNoCarryIn
    // 00000011 0 | 00000001 0 0 1 | OddNoCarryIn
    // 00000000 0 | 00000000 1 0 0 | ZeroNoCarryIn
    // 00000000 1 | 10000000 0 1 0 | ZeroCarryIn
    // 00000001 0 | 00000000 1 0 1 | OneNoCarryIn
    // 00000001 1 | 10000000 0 1 1 | OneCarryIn
    // xxxxxxxx x | xxxxxxxx 1 1 x | <no-such-state>
    ////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // RORacc
    // -------------------------------------------------------------------------
    private void ExecuteRORacc(UInt8 input, BitFlag carryIn, BitFlag expectedZFlag,
                               BitFlag expectedNFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.RORacc.ToUInt8();
        var expectedValue = input.Copy().Shr(carryIn);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(carryIn);
        Regs.A.UpdateValue(input);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        ExecuteClockCycles(1); // RORacc - shift value and update flags

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1000), Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestRORaccEvenNoCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteRORacc(new UInt8(2), Low, Low, Low, Low);
    }

    [Fact]
    public void TestRORaccOddNoCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteRORacc(new UInt8(3), Low, Low, Low, High);
    }

    [Fact]
    public void TestRORaccZeroNoCarryIn()
    {
        //                  value  Cin   Z    N   Cout
        ExecuteRORacc(new UInt8(0), Low, High, Low, Low);
    }

    [Fact]
    public void TestRORaccZeroCarryIn()
    {
        //                   value  Cin    Z     N   Cout
        ExecuteRORacc(new UInt8(0), High, Low, High, Low);
    }

    [Fact]
    public void TestRORaccOneNoCarryIn()
    {
        //                  value  Cin   Z     N   Cout
        ExecuteRORacc(new UInt8(1), Low, High, Low, High);
    }

    [Fact]
    public void TestRORaccOneCarryIn()
    {
        //                  value  Cin   Z     N   Cout
        ExecuteRORacc(new UInt8(1), High, Low, High, High);
    }

    // -------------------------------------------------------------------------
    // RORzpg
    // -------------------------------------------------------------------------
    private void ExecuteRORzpg(UInt8 memValue, BitFlag carryIn, BitFlag expectedZFlag,
                               BitFlag expectedNFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.RORzpg.ToUInt8();
        var operand1 = new UInt8(0x12);
        var expectedAddr = new UInt16(operand1);
        var expectedValue = memValue.Copy().Shr(carryIn);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(carryIn);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // RORzpg2 - fetch address into EA

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // RORzpg3 - fetch value from memory

        ExecuteClockCycles(1); // RORzpg4 - shift value and update flags

        ExecuteClockCycles(1); // RORzpg5 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestRORzpgEvenNoCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteRORzpg(new UInt8(2), Low, Low, Low, Low);
    }

    [Fact]
    public void TestRORzpgOddNoCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteRORzpg(new UInt8(3), Low, Low, Low, High);
    }

    [Fact]
    public void TestRORzpgZeroNoCarryIn()
    {
        //                   value  Cin   Z    N   Cout
        ExecuteRORzpg(new UInt8(0), Low, High, Low, Low);
    }

    [Fact]
    public void TestRORzpgZeroCarryIn()
    {
        //                   value  Cin    Z     N   Cout
        ExecuteRORzpg(new UInt8(0), High, Low, High, Low);
    }

    [Fact]
    public void TestRORzpgOneNoCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteRORzpg(new UInt8(1), Low, High, Low, High);
    }

    [Fact]
    public void TestRORzpgOneCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteRORzpg(new UInt8(1), High, Low, High, High);
    }

    // -------------------------------------------------------------------------
    // RORzpgx
    // -------------------------------------------------------------------------
    private void ExecuteRORzpgx(UInt8 memValue, BitFlag carryIn, BitFlag expectedZFlag,
                                BitFlag expectedNFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.RORzpgx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var xValue = new UInt8(0x10);
        var expectedAddr = new UInt16(operand1.Copy().AddWithWrapAround(xValue));
        var expectedValue = memValue.Copy().Shr(carryIn);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(carryIn);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // RORzpgx2 - fetch operand1 into EA low

        ExecuteClockCycles(1); // RORzpgx3 - add X reg to EA

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // RORzpgx4 - fetch value from memory

        ExecuteClockCycles(1); // RORzpgx5 - shift value and update flags

        ExecuteClockCycles(1); // RORzpgx6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestRORzpgxEvenNoCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteRORzpgx(new UInt8(2), Low, Low, Low, Low);
    }

    [Fact]
    public void TestRORzpgxOddNoCarryIn()
    {
        //                    value  Cin   Z     N   Cout
        ExecuteRORzpgx(new UInt8(3), Low, Low, Low, High);
    }

    [Fact]
    public void TestRORzpgxZeroNoCarryIn()
    {
        //                    value  Cin   Z    N   Cout
        ExecuteRORzpgx(new UInt8(0), Low, High, Low, Low);
    }

    [Fact]
    public void TestRORzpgxZeroCarryIn()
    {
        //                    value  Cin    Z     N   Cout
        ExecuteRORzpgx(new UInt8(0), High, Low, High, Low);
    }

    [Fact]
    public void TestRORzpgxOneNoCarryIn()
    {
        //                    value  Cin   Z     N   Cout
        ExecuteRORzpgx(new UInt8(1), Low, High, Low, High);
    }

    [Fact]
    public void TestRORzpgxOneCarryIn()
    {
        //                    value  Cin   Z     N   Cout
        ExecuteRORzpgx(new UInt8(1), High, Low, High, High);
    }

    // -------------------------------------------------------------------------
    // RORabs
    // -------------------------------------------------------------------------
    private void ExecuteRORabs(UInt8 memValue, BitFlag carryIn, BitFlag expectedZFlag,
                               BitFlag expectedNFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.RORabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2);
        var expectedValue = memValue.Copy().Shr(carryIn);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(carryIn);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // RORabs2 - fetch operand1 into EA low

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // RORabs3 - fetch operand2 into EA high

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // RORabs4 - fetch value from memory

        ExecuteClockCycles(1); // RORabs5 - shift value and update flags

        ExecuteClockCycles(1); // RORabs6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestRORabsEvenNoCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteRORabs(new UInt8(2), Low, Low, Low, Low);
    }

    [Fact]
    public void TestRORabsOddNoCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteRORabs(new UInt8(3), Low, Low, Low, High);
    }

    [Fact]
    public void TestRORabsZeroNoCarryIn()
    {
        //                  value  Cin   Z    N   Cout
        ExecuteRORabs(new UInt8(0), Low, High, Low, Low);
    }

    [Fact]
    public void TestRORabsZeroCarryIn()
    {
        //                   value  Cin    Z     N   Cout
        ExecuteRORabs(new UInt8(0), High, Low, High, Low);
    }

    [Fact]
    public void TestRORabsOneNoCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteRORabs(new UInt8(1), Low, High, Low, High);
    }

    [Fact]
    public void TestRORabsOneCarryIn()
    {
        //                   value  Cin   Z     N   Cout
        ExecuteRORabs(new UInt8(1), High, Low, High, High);
    }

    // -------------------------------------------------------------------------
    // RORabsx
    // -------------------------------------------------------------------------
    private void ExecuteRORabsx(UInt8 memValue, BitFlag carryIn, BitFlag expectedZFlag,
                                BitFlag expectedNFlag, BitFlag expectedCFlag)
    {
        var opCode = OpCodes.RORabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var xValue = new UInt8(0x10);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);
        var expectedValue = memValue.Copy().Shr(carryIn);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(carryIn);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // RORabsx2 - fetch operand1 into EA low

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // RORabsx3 - fetch operand2 into EA high

        ExecuteClockCycles(1); // RORabsx4 - add X to the EA reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // RORabsx5 - fetch value from memory

        ExecuteClockCycles(1); // RORabsx6 - shift value and update flags

        ExecuteClockCycles(1); // RORabsx7 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
        Assert.Equal(expectedCFlag, Regs.P.Carry);
    }

    [Fact]
    public void TestRORabsxEvenNoCarryIn()
    {
        //                    value  Cin   Z     N   Cout
        ExecuteRORabsx(new UInt8(2), Low, Low, Low, Low);
    }

    [Fact]
    public void TestRORabsxOddNoCarryIn()
    {
        //                    value  Cin   Z     N   Cout
        ExecuteRORabsx(new UInt8(3), Low, Low, Low, High);
    }

    [Fact]
    public void TestRORabsxZeroNoCarryIn()
    {
        //                    value  Cin   Z    N   Cout
        ExecuteRORabsx(new UInt8(0), Low, High, Low, Low);
    }

    [Fact]
    public void TestRORabsxZeroCarryIn()
    {
        //                    value  Cin    Z     N   Cout
        ExecuteRORabsx(new UInt8(0), High, Low, High, Low);
    }

    [Fact]
    public void TestRORabsxOneNoCarryIn()
    {
        //                    value  Cin   Z     N   Cout
        ExecuteRORabsx(new UInt8(1), Low, High, Low, High);
    }

    [Fact]
    public void TestRORabsxOneCarryIn()
    {
        //                    value  Cin    Z     N   Cout
        ExecuteRORabsx(new UInt8(1), High, Low, High, High);
    }
}
