// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class TSBTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // TSBzpg
    // -------------------------------------------------------------------------
    private void ExecuteTSBzpg(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag)
    {
        var opCode = OpCodes.TSBzpg.ToUInt8();
        var operand1 = new UInt8(0x12);
        var expectedAddr = new UInt16(operand1);
        var expectedValue = aValue.Copy().Or(memValue.Copy());

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // TSBzpg2 - fetch address into EA

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // TSBzpg3 - fetch value from memory

        ExecuteClockCycles(1); // TSBzpg4 - shift value and update flags

        ExecuteClockCycles(1); // TSBzpg5 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
    }

    [Fact]
    public void TestTSBzpgZero()
    {
        ExecuteTSBzpg(new UInt8(0x00), new UInt8(0x00), High);
    }

    [Fact]
    public void TestTSBzpgNotZero()
    {
        ExecuteTSBzpg(new UInt8(0xF0), new UInt8(0xA1), Low);
    }

    // -------------------------------------------------------------------------
    // TSBabs
    // -------------------------------------------------------------------------
    private void ExecuteTSBabs(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag)
    {
        var opCode = OpCodes.TSBabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2);
        var expectedValue = new UInt8((aValue.ToInt() | memValue.ToInt()) & 0x000000FF);

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // TSBabs2 - fetch operand1 into EA low

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // TSBabs3 - fetch operand2 into EA high

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // TSBabs4 - fetch value from memory

        ExecuteClockCycles(1); // TSBabs5 - shift value and update flags

        ExecuteClockCycles(1); // TSBabs6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(expectedValue, Pins.DataBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
    }

    [Fact]
    public void TestTSBabsZero()
    {
        ExecuteTSBabs(new UInt8(0x00), new UInt8(0x00), High);
    }

    [Fact]
    public void TestTSBabsNotZero()
    {
        ExecuteTSBabs(new UInt8(0xF0), new UInt8(0xA1), Low);
    }
}
