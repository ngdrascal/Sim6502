// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class FlagsTests : UnitTestBase
{
    [Fact]
    public void TestCLC()
    {
        // ARRANGE:
        var opCode = OpCodes.CLCimp.ToUInt8();
        var expected = Low;

        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(High);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstCLCimp2 - clear the carry flag

        var actual = Regs.P.Carry;

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestCLD()
    {
        // ARRANGE:
        var opCode = OpCodes.CLDimp.ToUInt8();
        var expected = Low;

        BootToAddress(BootAddr);
        Regs.P.Decimal.UpdateValue(High);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstCLDimp2 - clear the decimal flag

        var actual = Regs.P.Decimal;

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestCLI()
    {
        // ARRANGE:
        var opCode = OpCodes.CLIimp.ToUInt8();
        var expected = Low;

        BootToAddress(BootAddr);
        Regs.P.IRQDisabled.UpdateValue(High);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstCLIimp2 - clear the IRQ flag

        var actual = Regs.P.IRQDisabled;

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestCLV()
    {
        // ARRANGE:
        var opCode = OpCodes.CLVimp.ToUInt8();
        var expected = Low;

        BootToAddress(BootAddr);
        Regs.P.Overflow.UpdateValue(High);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstCLVimp2 - clear the overflow flag

        var actual = Regs.P.Overflow;

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestSEC()
    {
        // ARRANGE:
        var opCode = OpCodes.SECimp.ToUInt8();
        var expected = High;

        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(Low);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstSECimp2 - set the carry flag

        var actual = Regs.P.Carry;

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestSED()
    {
        // ARRANGE:
        var opCode = OpCodes.SEDimp.ToUInt8();
        var expected = High;

        BootToAddress(BootAddr);
        Regs.P.Decimal.UpdateValue(Low);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstSECimp2 - set the decimal flag

        var actual = Regs.P.Decimal;

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestSEI()
    {
        // ARRANGE:
        var opCode = OpCodes.SEIimp.ToUInt8();
        var expected = High;

        BootToAddress(BootAddr);
        Regs.P.IRQDisabled.UpdateValue(Low);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstSECimp2 - set the IRQ flag

        var actual = Regs.P.IRQDisabled;

        // ASSERT:
        Assert.Equal(expected, actual);
    }
}
