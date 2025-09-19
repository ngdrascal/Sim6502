// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using Sim6502.Tests.Types;
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class TRBTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // TRBzpg
    // -------------------------------------------------------------------------
    private void ExecuteTRBzpg(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag)
    {
        var opCode = OpCodes.TRBzpg.ToUInt8();
        var operand1 = new UInt8(0x12);
        var expectedAddr = new UInt16(operand1);
        var expectedValue = aValue.Copy().Not().And(memValue.Copy());

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // TRBzpg2 - fetch address into EA

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // TRBzpg3 - fetch value from memory

        ExecuteClockCycles(1); // TRBzpg4 - shift value and update flags

        ExecuteClockCycles(1); // TRBzpg5 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
    }

    [Fact]
    public void TestTRBzpgZero()
    {
        ExecuteTRBzpg(new UInt8(0xF0), new UInt8(0xA0), High);
    }

    [Fact]
    public void TestTRBzpgNotZero()
    {
        ExecuteTRBzpg(new UInt8(0xF0), new UInt8(0xA1), Low);
    }

    // -------------------------------------------------------------------------
    // TRBabs
    // -------------------------------------------------------------------------
    private void ExecuteTRBabs(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag)
    {
        var opCode = OpCodes.TRBabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2);
        var expectedValue = aValue.Copy().Not().And(memValue.Copy());

        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // TRBabs2 - fetch operand1 into EA low

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // TRBabs3 - fetch operand2 into EA high

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // TRBabs4 - fetch value from memory

        ExecuteClockCycles(1); // TRBabs5 - shift value and update flags

        ExecuteClockCycles(1); // TRBabs6 - store the value back to EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
    }

    [Fact]
    public void TestTRBabsZero()
    {
        ExecuteTRBabs(new UInt8(0xF0), new UInt8(0xA0), High);
    }

    [Fact]
    public void TestTRBabsNotZero()
    {
        ExecuteTRBabs(new UInt8(0xF0), new UInt8(0xA1), Low);
    }
}
