using System.Diagnostics.CodeAnalysis;

namespace W65C22.Engine.Tests;

[ExcludeFromCodeCoverage]
public class InterruptTests : ViaTestBase
{
    /*
       TITLE: A falling CA1 sets the CA1 flag but IRQB stays high while disabled
       GIVEN: a reset VIA (CA1 active on falling edge, interrupts disabled)
       WHEN: CA1 falls
       THEN: IFR bit 1 is set and IRQB is high
     */
    [Fact]
    public void Ca1FallingSetsFlag()
    {
        // ARRANGE:

        // ACT:
        SetCa1(false);

        // ASSERT:
        Assert.Equal(Ca1Flag, Engine.IFR);
        Assert.True(Pins.IRQB);
    }

    /*
       TITLE: Each control line sets its own IFR bit on its active transition
       GIVEN: a VIA with PCR selecting the given edges
       WHEN: the given control line goes to the given level
       THEN: only the expected IFR bit is set
     */
    [Theory]
    [InlineData(C1Rising, "CA1", Ca1Flag)]
    [InlineData(C2InRising, "CA2", Ca2Flag)]
    [InlineData(C1Rising << 4, "CB1", Cb1Flag)]
    [InlineData(C2InRising << 4, "CB2", Cb2Flag)]
    public void RisingEdgesSelected(int pcr, string line, byte flag)
    {
        // ARRANGE:
        Write(Pcr, (byte)pcr);
        SetLine(line, false);
        var afterFall = Engine.IFR;

        // ACT:
        SetLine(line, true);

        // ASSERT:
        Assert.Equal(0x00, afterFall);
        Assert.Equal(flag, Engine.IFR);
    }

    /*
       TITLE: Falling edges on every control line set their IFR bits with a reset PCR
       GIVEN: a reset VIA
       WHEN: CA1, CA2, CB1 and CB2 fall
       THEN: IFR bits 0, 1, 3 and 4 are set
     */
    [Fact]
    public void FallingEdgesByDefault()
    {
        // ARRANGE:

        // ACT:
        SetCa1(false);
        SetCa2(false);
        SetCb1(false);
        SetCb2(false);

        // ASSERT:
        Assert.Equal(Ca1Flag | Ca2Flag | Cb1Flag | Cb2Flag, Engine.IFR);
    }

    /*
       TITLE: An enabled CA1 interrupt pulls IRQB low immediately, without a clock
       GIVEN: a VIA with the CA1 interrupt enabled and PHI2 low
       WHEN: CA1 falls
       THEN: IRQB is low
     */
    [Fact]
    public void EnabledCa1DrivesIrq()
    {
        // ARRANGE:
        Write(Ier, IerSet | Ca1Flag);

        // ACT:
        SetCa1(false);

        // ASSERT:
        Assert.False(Pins.IRQB);
    }

    /*
       TITLE: Enabling an interrupt while its flag is already set pulls IRQB low
       GIVEN: a VIA whose CA1 flag was set while disabled
       WHEN: the CA1 enable bit is set in IER
       THEN: IRQB is low
     */
    [Fact]
    public void EnablingWithFlagSetDrivesIrq()
    {
        // ARRANGE:
        SetCa1(false);

        // ACT:
        Write(Ier, IerSet | Ca1Flag);

        // ASSERT:
        Assert.False(Pins.IRQB);
    }

    /*
       TITLE: Writing IER with bit 7 set sets the 1 bits and leaves the others; clear form clears
       GIVEN: a reset VIA
       WHEN: IER is written 0x83, then 0x84, then 0x01
       THEN: IER reads 0x83, then 0x87, then 0x86 (bit 7 always reads 1)
     */
    [Fact]
    public void IerSetAndClear()
    {
        // ARRANGE:

        // ACT:
        Write(Ier, 0x83);
        var first = Read(Ier);
        Write(Ier, 0x84);
        var second = Read(Ier);
        Write(Ier, 0x01);
        var third = Read(Ier);

        // ASSERT:
        Assert.Equal(0x83, first);
        Assert.Equal(0x87, second);
        Assert.Equal(0x86, third);
    }

    /*
       TITLE: IFR bit 7 shows whether any flagged interrupt is enabled
       GIVEN: a VIA with the CA1 flag set
       WHEN: IFR is read with CA1 disabled, then with CA1 enabled
       THEN: the reads return 0x02, then 0x82
     */
    [Fact]
    public void IfrBit7FollowsEnabledFlags()
    {
        // ARRANGE:
        SetCa1(false);

        // ACT:
        var disabled = Read(Ifr);
        Write(Ier, IerSet | Ca1Flag);
        var enabled = Read(Ifr);

        // ASSERT:
        Assert.Equal(Ca1Flag, disabled);
        Assert.Equal(IrqBit | Ca1Flag, enabled);
    }

    /*
       TITLE: Writing 1s to IFR clears those flags and releases IRQB; bit 7 is ignored
       GIVEN: a VIA with CA1 and CB1 flags set and both enabled
       WHEN: 0x82 is written to IFR, then 0x10
       THEN: after the first write only CB1 remains and IRQB is low; after the second IRQB is high
     */
    [Fact]
    public void IfrWriteClearsFlags()
    {
        // ARRANGE:
        Write(Ier, IerSet | Ca1Flag | Cb1Flag);
        SetCa1(false);
        SetCb1(false);

        // ACT:
        Write(Ifr, IrqBit | Ca1Flag);
        var afterFirst = Engine.IFR;
        var irqAfterFirst = Pins.IRQB;
        Write(Ifr, Cb1Flag);

        // ASSERT:
        Assert.Equal(Cb1Flag, afterFirst);
        Assert.False(irqAfterFirst);
        Assert.Equal(0x00, Engine.IFR);
        Assert.True(Pins.IRQB);
    }

    /*
       TITLE: Reading IRA through register 1 clears CA1 and CA2 flags and releases IRQB at the rise
       GIVEN: a VIA with CA1 and CA2 flags set and enabled
       WHEN: a register 1 read reaches the PHI2 rise
       THEN: both flags are clear and IRQB is high before the fall
     */
    [Fact]
    public void ReadOraClearsFlagsAtRise()
    {
        // ARRANGE:
        Write(Ier, IerSet | Ca1Flag | Ca2Flag);
        SetCa1(false);
        SetCa2(false);
        Address(Ora, read: true);

        // ACT:
        Rise();

        // ASSERT:
        Assert.Equal(0x00, Engine.IFR);
        Assert.True(Pins.IRQB);
    }

    /*
       TITLE: Writing ORA clears the CA1 and CA2 flags
       GIVEN: a VIA with CA1 and CA2 flags set
       WHEN: ORA is written
       THEN: IFR is 0
     */
    [Fact]
    public void WriteOraClearsFlags()
    {
        // ARRANGE:
        SetCa1(false);
        SetCa2(false);

        // ACT:
        Write(Ora, 0x00);

        // ASSERT:
        Assert.Equal(0x00, Engine.IFR);
    }

    /*
       TITLE: Reading or writing ORB clears the CB1 and CB2 flags, not the Port A flags
       GIVEN: a VIA with all four control line flags set
       WHEN: IRB is read (then the flags are set again and ORB is written)
       THEN: only CA1 and CA2 flags remain each time
     */
    [Fact]
    public void OrbAccessClearsPortBFlags()
    {
        // ARRANGE:
        SetCa1(false);
        SetCa2(false);
        SetCb1(false);
        SetCb2(false);

        // ACT:
        Read(Orb);
        var afterRead = Engine.IFR;
        SetCb1(true);
        SetCb2(true);
        SetCb1(false);
        SetCb2(false);
        Write(Orb, 0x00);

        // ASSERT:
        Assert.Equal(Ca1Flag | Ca2Flag, afterRead);
        Assert.Equal(Ca1Flag | Ca2Flag, Engine.IFR);
    }

    /*
       TITLE: Register $F accesses leave the CA1 and CA2 flags set
       GIVEN: a VIA with CA1 and CA2 flags set
       WHEN: register $F is read and written
       THEN: both flags are still set
     */
    [Fact]
    public void RegisterFKeepsFlags()
    {
        // ARRANGE:
        SetCa1(false);
        SetCa2(false);

        // ACT:
        Read(OraNoHandshake);
        Write(OraNoHandshake, 0x00);

        // ASSERT:
        Assert.Equal(Ca1Flag | Ca2Flag, Engine.IFR);
    }

    /*
       TITLE: In independent interrupt mode ORx accesses do not clear the C2 flag
       GIVEN: a VIA with CA2 and CB2 as independent falling-edge inputs and all four flags set
       WHEN: IRA and IRB are read
       THEN: the CA2 and CB2 flags remain, the CA1 and CB1 flags are clear
     */
    [Fact]
    public void IndependentModeKeepsC2Flag()
    {
        // ARRANGE:
        Write(Pcr, C2IndependentFalling | (C2IndependentFalling << 4));
        SetCa1(false);
        SetCa2(false);
        SetCb1(false);
        SetCb2(false);

        // ACT:
        Read(Ora);
        Read(Orb);

        // ASSERT:
        Assert.Equal(Ca2Flag | Cb2Flag, Engine.IFR);
    }

    /*
       TITLE: Independent rising-edge mode sets the flag on a rising C2
       GIVEN: a VIA with CA2 as an independent rising-edge input
       WHEN: CA2 falls, then rises
       THEN: the CA2 flag is clear after the fall and set after the rise
     */
    [Fact]
    public void IndependentRisingEdge()
    {
        // ARRANGE:
        Write(Pcr, C2IndependentRising);

        // ACT:
        SetCa2(false);
        var afterFall = Engine.IFR;
        SetCa2(true);

        // ASSERT:
        Assert.Equal(0x00, afterFall);
        Assert.Equal(Ca2Flag, Engine.IFR);
    }

    /*
       TITLE: CA2 in an output mode never sets its flag
       GIVEN: a VIA with CA2 as manual high output
       WHEN: the CA2 net falls and rises
       THEN: IFR stays 0
     */
    [Fact]
    public void Ca2OutputModeSetsNoFlag()
    {
        // ARRANGE:
        Write(Pcr, C2ManualHigh);

        // ACT:
        SetCa2(false);
        SetCa2(true);

        // ASSERT:
        Assert.Equal(0x00, Engine.IFR);
    }

    /*
       TITLE: An active transition after ORA read in the same cycle sets the flag again
       GIVEN: a VIA with the CA1 flag set, in the high phase of a register 1 read
       WHEN: CA1 rises and falls again before the PHI2 fall
       THEN: the CA1 flag is set
     */
    [Fact]
    public void TransitionAfterClearSetsFlagAgain()
    {
        // ARRANGE:
        SetCa1(false);
        Address(Ora, read: true);
        Rise();

        // ACT:
        SetCa1(true);
        SetCa1(false);
        Fall();

        // ASSERT:
        Assert.Equal(Ca1Flag, Engine.IFR);
    }

    private void SetLine(string line, bool level)
    {
        switch (line)
        {
            case "CA1":
                SetCa1(level);
                break;
            case "CA2":
                SetCa2(level);
                break;
            case "CB1":
                SetCb1(level);
                break;
            default:
                SetCb2(level);
                break;
        }
    }
}
