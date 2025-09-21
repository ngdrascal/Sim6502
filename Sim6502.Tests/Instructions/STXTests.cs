// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class STXTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // STX zeropage
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTXzpg()
    {
        var opCode = OpCodes.STXzpg.ToUInt8();
        var operand = new UInt8(0x34);
        var xValue = new UInt8(0x12);
        var finalAddr = new UInt16(operand);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstSTXzpg2 - fetch the second byte of the op-code (zpg-value)

        ExecuteClockCycles(1); // InstSTXzpg3 - store the value in X at address zpg-value

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(xValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.GetRWB());
    }

    // -------------------------------------------------------------------------
    // STX zeropage,Y
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTXzpgy()
    {
        var opCode = OpCodes.STXzpgy.ToUInt8();
        var operand = new UInt8(0x34);
        var yValue = new UInt8(0x12);
        var xValue = new UInt8(0x12);
        var finalAddr = new UInt16(operand.Copy().AddWithWrapAround(yValue));

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstSTXzpgy2 - fetch the second byte of the op-code (zpg-value)

        ExecuteClockCycles(1); // InstSTXzpgy3 - EA = zpg-value + Y reg

        ExecuteClockCycles(1); // InstSTXzpgy4 - store the value in X at EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(xValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.GetRWB());
    }

    // -------------------------------------------------------------------------
    // STX absolute
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTXabs()
    {
        var opCode = OpCodes.STXabs.ToUInt8();
        var operand1 = new UInt8(0x34);
        var operand2 = new UInt8(0x12);
        var xValue = new UInt8(0x56);
        var finalAddr = new UInt16(operand1, operand2);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstSTXabs2 - fetch the first byte of the addr

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstSTXabs3 - fetch the second byte of the addr, EA = op2,op1

        ExecuteClockCycles(1); // InstSTXabs4 - store the value in X at EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(xValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.GetRWB());
    }
}
