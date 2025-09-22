// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class STYTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // STY zeropage
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTYzpg()
    {
        var opCode = OpCodes.STYzpg.ToUInt8();
        var operand1 = new UInt8(0x34);
        var yValue = new UInt8(0x12);
        var expectedAddr = new UInt16(operand1);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstSTYzpg2 - fetch the second byte of the op-code (zpg-value)

        ExecuteClockCycles(1); // InstSTYzpg3 - store the value in Y at address zpg-value

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(yValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.RWB);
    }

    // -------------------------------------------------------------------------
    // STY zeropage,X
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTYzpgx()
    {
        var opCode = OpCodes.STYzpgx.ToUInt8();
        var operand1 = new UInt8(0x34);
        var yValue = new UInt8(0x12);
        var xValue = new UInt8(0x56);
        var expectedAddr = new UInt16(operand1).AddUnsigned(xValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstSTYzpgx2 - fetch the second byte of the op-code (zpg-value)

        ExecuteClockCycles(1); // InstSTYzpgx3 - EA = zpg-value + X reg

        ExecuteClockCycles(1); // InstSTYzpgx4 - store the value in Y at EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(yValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.RWB);
    }

    // -------------------------------------------------------------------------
    // STY absolute
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTYabs()
    {
        var opCode = OpCodes.STYabs.ToUInt8();
        var operand1 = new UInt8(0x34);
        var operand2 = new UInt8(0x12);
        var yValue = new UInt8(0x56);
        var expectedAddr = new UInt16(operand1, operand2);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstSTYabs2 - fetch the first byte of the addr

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstSTYabs3 - fetch the second byte of the addr, EA = op2,op1

        ExecuteClockCycles(1); // InstSTYabs4 - store the value in Y at EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(yValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.RWB);
    }
}
