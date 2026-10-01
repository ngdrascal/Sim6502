using System.Diagnostics.CodeAnalysis;

namespace W65C21.Engine.Tests;

[ExcludeFromCodeCoverage]
public class InterruptTests : PiaTestBase
{
    /*
       TITLE: A falling CA1 sets the IRQA1 flag but IRQAB stays released while disabled
       GIVEN: a reset PIA (CA1 active on falling edge, IRQ disabled)
       WHEN: CA1 falls
       THEN: CRA bit 7 is set and IRQAB is not driven
     */
    [Fact]
    public void Ca1FallingSetsFlag()
    {
        // ARRANGE:

        // ACT:
        SetCa1(false);

        // ASSERT:
        Assert.Equal(Irq1Flag, Engine.CRA);
        Assert.False(Pins.IRQABDrive);
    }

    /*
       TITLE: With rising edge selected only a rising CA1 sets the IRQA1 flag
       GIVEN: a PIA with CRA bit 1 set
       WHEN: CA1 falls, and then rises
       THEN: the flag is clear after the fall and set after the rise
     */
    [Fact]
    public void Ca1RisingEdgeSelected()
    {
        // ARRANGE:
        Write(Cra, C1Rising);

        // ACT:
        SetCa1(false);
        var afterFall = Engine.CRA & Irq1Flag;
        SetCa1(true);

        // ASSERT:
        Assert.Equal(0, afterFall);
        Assert.Equal(Irq1Flag, Engine.CRA & Irq1Flag);
    }

    /*
       TITLE: An enabled CA1 interrupt pulls IRQAB low immediately, without a clock
       GIVEN: a PIA with the CA1 IRQ enabled and PHI2 low
       WHEN: CA1 falls
       THEN: IRQAB is driven low
     */
    [Fact]
    public void EnabledCa1DrivesIrq()
    {
        // ARRANGE:
        Write(Cra, C1IrqEnable);

        // ACT:
        SetCa1(false);

        // ASSERT:
        Assert.True(Pins.IRQABDrive);
    }

    /*
       TITLE: Enabling the IRQ while its flag is already set pulls IRQ low
       GIVEN: a PIA whose IRQA1 flag was set while disabled
       WHEN: the CA1 IRQ enable bit is written
       THEN: IRQAB is driven low
     */
    [Fact]
    public void EnablingWithFlagSetDrivesIrq()
    {
        // ARRANGE:
        SetCa1(false);

        // ACT:
        Write(Cra, C1IrqEnable);

        // ASSERT:
        Assert.True(Pins.IRQABDrive);
    }

    /*
       TITLE: Read A Data clears Side A's Interrupt Flags and releases IRQAB at the PHI2 rise
       GIVEN: a PIA with DDR Access set, CA1 and CA2 interrupts enabled and both flags set
       WHEN: a Port A read reaches the PHI2 rise
       THEN: CRA bits 6-7 are clear and IRQAB is released before the fall
     */
    [Fact]
    public void ReadADataClearsFlags()
    {
        // ARRANGE:
        Write(Cra, DdrAccess | C1IrqEnable | C2InIrqEnable);
        SetCa1(false);
        SetCa2(false);
        Address(PortA, read: true);

        // ACT:
        Rise();

        // ASSERT:
        Assert.Equal(0, Engine.CRA & (Irq1Flag | Irq2Flag));
        Assert.False(Pins.IRQABDrive);
    }

    /*
       TITLE: Reading CRA or DDRA leaves the Interrupt Flags set
       GIVEN: a PIA with the IRQA1 flag set and DDR Access clear
       WHEN: CRA and DDRA are read
       THEN: the CRA read shows bit 7 set and the flag is still set afterwards
     */
    [Fact]
    public void OtherReadsKeepFlags()
    {
        // ARRANGE:
        SetCa1(false);

        // ACT:
        var cra = Read(Cra);
        Read(PortA);

        // ASSERT:
        Assert.Equal(Irq1Flag, cra);
        Assert.Equal(Irq1Flag, Engine.CRA & Irq1Flag);
    }

    /*
       TITLE: An active transition after Read A Data in the same cycle sets the flag again
       GIVEN: a PIA with DDR Access set and the IRQA1 flag set, in the high phase of a Port A read
       WHEN: CA1 rises and falls again before the PHI2 fall
       THEN: the IRQA1 flag is set
     */
    [Fact]
    public void TransitionAfterClearSetsFlagAgain()
    {
        // ARRANGE:
        Write(Cra, DdrAccess);
        SetCa1(false);
        Address(PortA, read: true);
        Rise();

        // ACT:
        SetCa1(true);
        SetCa1(false);
        Fall();

        // ASSERT:
        Assert.Equal(Irq1Flag, Engine.CRA & Irq1Flag);
    }

    /*
       TITLE: A falling CA2 in input mode sets IRQA2 and drives IRQAB when enabled
       GIVEN: a PIA with CA2 as input, falling edge, IRQ enabled
       WHEN: CA2 falls
       THEN: CRA bit 6 is set and IRQAB is driven low
     */
    [Fact]
    public void Ca2InputFallingSetsFlag()
    {
        // ARRANGE:
        Write(Cra, C2InIrqEnable);

        // ACT:
        SetCa2(false);

        // ASSERT:
        Assert.Equal(Irq2Flag, Engine.CRA & Irq2Flag);
        Assert.True(Pins.IRQABDrive);
    }

    /*
       TITLE: With rising edge selected only a rising CA2 sets IRQA2
       GIVEN: a PIA with CA2 as input, rising edge, IRQ disabled
       WHEN: CA2 falls, and then rises
       THEN: the flag is clear after the fall, set after the rise, and IRQAB is never driven
     */
    [Fact]
    public void Ca2InputRisingEdgeSelected()
    {
        // ARRANGE:
        Write(Cra, C2InRising);

        // ACT:
        SetCa2(false);
        var afterFall = Engine.CRA & Irq2Flag;
        SetCa2(true);

        // ASSERT:
        Assert.Equal(0, afterFall);
        Assert.Equal(Irq2Flag, Engine.CRA & Irq2Flag);
        Assert.False(Pins.IRQABDrive);
    }

    /*
       TITLE: Switching CA2 to output clears IRQA2 and stops CA2 setting it
       GIVEN: a PIA with the IRQA2 flag set by a falling CA2 input
       WHEN: CA2 is made a manual high output and the CA2 pin rises and falls
       THEN: the IRQA2 flag is clear
     */
    [Fact]
    public void Ca2OutputModeKeepsIrq2Clear()
    {
        // ARRANGE:
        SetCa2(false);

        // ACT:
        Write(Cra, C2ManualHigh);
        SetCa2(true);
        SetCa2(false);

        // ASSERT:
        Assert.Equal(0, Engine.CRA & Irq2Flag);
    }

    /*
       TITLE: Side B's control lines set CRB flags and drive IRQBB independently of Side A
       GIVEN: a PIA with the CB1 and CB2 interrupts enabled on falling edges
       WHEN: CB1 and CB2 fall
       THEN: CRB bits 6-7 are set, IRQBB is driven and Side A is untouched
     */
    [Fact]
    public void SideBFlagsAndIrq()
    {
        // ARRANGE:
        Write(Crb, C1IrqEnable | C2InIrqEnable);

        // ACT:
        SetCb1(false);
        SetCb2(false);

        // ASSERT:
        Assert.Equal(Irq1Flag | Irq2Flag, Engine.CRB & (Irq1Flag | Irq2Flag));
        Assert.True(Pins.IRQBBDrive);
        Assert.Equal(0x00, Engine.CRA);
        Assert.False(Pins.IRQABDrive);
    }

    /*
       TITLE: Read B Data clears Side B's Interrupt Flags and releases IRQBB at the PHI2 rise
       GIVEN: a PIA with DDR Access set on Side B, the CB1 IRQ enabled and its flag set
       WHEN: a Port B read reaches the PHI2 rise
       THEN: CRB bits 6-7 are clear and IRQBB is released
     */
    [Fact]
    public void ReadBDataClearsFlags()
    {
        // ARRANGE:
        Write(Crb, DdrAccess | C1IrqEnable);
        SetCb1(false);
        Address(PortB, read: true);

        // ACT:
        Rise();

        // ASSERT:
        Assert.Equal(0, Engine.CRB & (Irq1Flag | Irq2Flag));
        Assert.False(Pins.IRQBBDrive);
    }
}
