// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class StackTests : UnitTestBase
{
    [Fact]
    public void TestPHAimp()
    {
        var opCode = OpCodes.PHAimp.ToUInt8();
        var aValue = new UInt8(0x12);
        var spValue = new UInt8(0xFF);
        var stackTop = new UInt16(0x100).AddUnsigned(spValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.S.UpdateValue(spValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstPHAimp2 - read the next instruction and discard

        ExecuteClockCycles(1); // InstPHAimp3 - push A reg and decrement S reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(stackTop, Pins.AddrBus);
        Assert.Equal(aValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(spValue.Dec(), Regs.S);
    }

    [Fact]
    public void TestPHPimp()
    {
        var opCode = OpCodes.PHPimp.ToUInt8();
        var pValue = "NvbdiZc";
        var spValue = new UInt8(0xFF);
        var stackTop = new UInt16(0x100).AddUnsigned(spValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.LoadFlags(pValue);
        Regs.S.UpdateValue(spValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstPHPimp2 - read the next instruction and discard
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);

        ExecuteClockCycles(1); // InstPHPimp3 - push P reg and decrement S reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(stackTop, Pins.AddrBus);
        Assert.Equal(Regs.P.ToUInt8(), Pins.DataBus);
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(spValue.Dec(), Regs.S);
    }

    [Fact]
    public void TestPHXimp()
    {
        var opCode = OpCodes.PHXimp.ToUInt8();
        var xValue = new UInt8(0x12);
        var spValue = new UInt8(0xFF);
        var stackTop = new UInt16(0x100).AddUnsigned(spValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);
        Regs.S.UpdateValue(spValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstPHAimp2 - read the next instruction and discard
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);

        ExecuteClockCycles(1); // InstPHAimp3 - push X reg and decrement S reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(stackTop, Pins.AddrBus);
        Assert.Equal(xValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(spValue.Dec(), Regs.S);
    }

    [Fact]
    public void TestPHYimp()
    {
        var opCode = OpCodes.PHXimp.ToUInt8();
        var yValue = new UInt8(0x12);
        var spValue = new UInt8(0xFF);
        var stackTop = new UInt16(0x100).AddUnsigned(spValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.X.UpdateValue(yValue);
        Regs.S.UpdateValue(spValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstPHAimp2 - read the next instruction and discard
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);

        ExecuteClockCycles(1); // InstPHAimp3 - push Y reg and decrement S reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(stackTop, Pins.AddrBus);
        Assert.Equal(yValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(spValue.Dec(), Regs.S);
    }

    [Fact]
    public void TestPLA()
    {
        var opCode = OpCodes.PLAimp.ToUInt8();
        var expectedValue = new UInt8(0x12);
        var spValue = new UInt8(0xFE);
        var stackTop = new UInt16(0x100).AddUnsigned(spValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.S.UpdateValue(spValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstPLAimp2 - read the next instruction and discard
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);

        ExecuteClockCycles(1); // InstPLAimp3 - increment S reg

        Pins.DataBus = (expectedValue);
        ExecuteClockCycles(1); // InstPLAimp4 - pop value into A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(stackTop.Inc(), Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(spValue.Inc(), Regs.S);
        Assert.Equal(expectedValue, Regs.A);
    }

    [Fact]
    public void TestPLP()
    {
        var opCode = OpCodes.PLPimp.ToUInt8();
        var expectedValue = UInt8FromFlags("NvbDiZc");
        var spValue = new UInt8(0xFE);
        var stackTop = new UInt16(0x100).AddUnsigned(spValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.S.UpdateValue(spValue);
        Regs.P.LoadFlags("nvbdizc");

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstPLPimp2 - read the next instruction and discard
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);

        ExecuteClockCycles(1); // InstPLPimp3 - increment S reg

        Pins.DataBus = expectedValue;
        ExecuteClockCycles(1); // InstPLPimp4 - pop value into P reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(stackTop.Inc(), Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(spValue.Inc(), Regs.S);
        Assert.Equal(expectedValue, Regs.P.ToUInt8());
    }

    [Fact]
    public void TestPLX()
    {
        var opCode = OpCodes.PLXimp.ToUInt8();
        var expectedValue = new UInt8(0x12);
        var spValue = new UInt8(0xFE);
        var stackTop = new UInt16(0x100).AddUnsigned(spValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.S.UpdateValue(spValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstPLXimp2 - read the next instruction and discard
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);

        ExecuteClockCycles(1); // InstPLXimp3 - increment S reg

        Pins.DataBus = (expectedValue);
        ExecuteClockCycles(1); // InstPLXimp4 - pop value into X reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(stackTop.Inc(), Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(spValue.Inc(), Regs.S);
        Assert.Equal(expectedValue, Regs.X);
    }

    [Fact]
    public void TestPLY()
    {
        var opCode = OpCodes.PLYimp.ToUInt8();
        var expectedValue = new UInt8(0x12);
        var spValue = new UInt8(0xFE);
        var stackTop = new UInt16(0x100).AddUnsigned(spValue);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.S.UpdateValue(spValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstPLYimp2 - read the next instruction and discard
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);

        ExecuteClockCycles(1); // InstPLYimp3 - increment S reg

        Pins.DataBus = (expectedValue);
        ExecuteClockCycles(1); // InstPLYimp4 - pop value into X reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(stackTop.Inc(), Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1001), Regs.PC);
        Assert.Equal(spValue.Inc(), Regs.S);
        Assert.Equal(expectedValue, Regs.Y);
    }
}
