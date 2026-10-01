using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine;

namespace Sim6502.Tests;

[ExcludeFromCodeCoverage]
public class SetOverflowTests : UnitTestBase
{
    private const byte PinLow = 0;
    private const byte PinHigh = 1;

    /*
       TITLE: A falling edge on SOB sets the overflow flag
       GIVEN: a booted CPU running NOPs with V clear and SOB high
       WHEN: SOB is pulled low and a cycle completes
       THEN: V is set
     */
    [Fact]
    public void FallingSobSetsOverflow()
    {
        // ARRANGE:
        BootToAddress(BootAddr);
        Pins.DataBus = OpCodes.NOP.ToUInt8();
        Regs.P.ClearOverflow();
        ExecuteClockCycles(1);

        // ACT:
        Pins.SOB = PinLow;
        ExecuteClockCycles(1);

        // ASSERT:
        Assert.True(Regs.P.Overflow.IsSet());
    }

    /*
       TITLE: SOB is edge triggered, so holding it low does not set V again
       GIVEN: a booted CPU running NOPs whose V was set by a falling SOB, with SOB still low
       WHEN: V is cleared and more cycles complete with SOB held low
       THEN: V stays clear
     */
    [Fact]
    public void HoldingSobLowDoesNotSetOverflowAgain()
    {
        // ARRANGE:
        BootToAddress(BootAddr);
        Pins.DataBus = OpCodes.NOP.ToUInt8();
        ExecuteClockCycles(1);
        Pins.SOB = PinLow;
        ExecuteClockCycles(1);

        // ACT:
        Regs.P.ClearOverflow();
        ExecuteClockCycles(3);

        // ASSERT:
        Assert.True(Regs.P.Overflow.IsCleared());
    }

    /*
       TITLE: A rising edge on SOB leaves the overflow flag alone
       GIVEN: a booted CPU running NOPs with SOB low and V clear
       WHEN: SOB is released high and a cycle completes
       THEN: V is still clear
     */
    [Fact]
    public void RisingSobDoesNotSetOverflow()
    {
        // ARRANGE:
        Pins.SOB = PinLow;
        BootToAddress(BootAddr);
        Pins.DataBus = OpCodes.NOP.ToUInt8();
        ExecuteClockCycles(1);
        Regs.P.ClearOverflow();

        // ACT:
        Pins.SOB = PinHigh;
        ExecuteClockCycles(1);

        // ASSERT:
        Assert.True(Regs.P.Overflow.IsCleared());
    }
}
