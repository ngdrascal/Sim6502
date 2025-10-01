// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class STPTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // STP implied
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTPimp()
    {
        // ARRANGE:
        var opCode = OpCodes.STPimp.ToUInt8();

        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode
        ExecuteClockCycles(1); // STPimp2
        ExecuteClockCycles(1); // STPimp3

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(0x1001), Regs.PC);
    }

    [Fact]
    public void TestSTPimpWaitsForResetWhenHeldGE2Cycles()
    {
        // ARRANGE:
        var opCode = OpCodes.STPimp.ToUInt8();

        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode
        ExecuteClockCycles(1); // STPimp2
        ExecuteClockCycles(1); // STPimp3

        Pins.RESB = 0;       // Assert reset
        ExecuteClockCycles(2); // hold low for 2 or more clock cycles

        Pins.DataBus = (new UInt8(0x21));
        Pins.RESB = 1;       // De-assert reset
        ExecuteClockCycles(2); // Boot1

        Pins.DataBus = (new UInt8(0x43));
        ExecuteClockCycles(1); // Boot2

        // ASSERT:
        //Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x4321), Regs.PC);
    }

    [Fact]
    public void TestSTPimpWIgnoresResetWhenHeldOnly1Cycle()
    {
        // ARRANGE:
        var opCode = OpCodes.STPimp.ToUInt8();

        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode
        ExecuteClockCycles(1); // STPimp2
        ExecuteClockCycles(1); // STPimp3

        Pins.RESB = 0; // Assert reset
        ExecuteClockCycles(1); // hold low for only 1 cycle

        Pins.DataBus = (new UInt8(0x21));
        Pins.RESB = 1; // De-assert reset
        ExecuteClockCycles(2); // Boot1

        Pins.DataBus = (new UInt8(0x43));
        ExecuteClockCycles(1); // Boot2

        // ASSERT:
        //Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
    }
}
