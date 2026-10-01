using System.Diagnostics.CodeAnalysis;

namespace W65C21.Engine.Tests;

[ExcludeFromCodeCoverage]
public class Ca2OutputTests : PiaTestBase
{
    /*
       TITLE: CA2 floats while in input mode
       GIVEN: a reset PIA
       WHEN: CRA selects CA2 input with every option bit set
       THEN: CA2 is not driven
     */
    [Fact]
    public void InputModeFloats()
    {
        // ARRANGE:

        // ACT:
        Write(Cra, C2InRising | C2InIrqEnable);

        // ASSERT:
        Assert.False(Pins.CA2Drive);
    }

    /*
       TITLE: Manual output modes drive CA2 from CRA bit 3
       GIVEN: a reset PIA
       WHEN: CRA selects manual low, then manual high
       THEN: CA2 is driven low, then driven high
     */
    [Fact]
    public void ManualOutput()
    {
        // ARRANGE:

        // ACT:
        Write(Cra, C2ManualLow);
        var low = Pins.CA2Out;
        var lowDriven = Pins.CA2Drive;
        Write(Cra, C2ManualHigh);

        // ASSERT:
        Assert.False(low);
        Assert.True(lowDriven);
        Assert.True(Pins.CA2Out);
        Assert.True(Pins.CA2Drive);
    }

    /*
       TITLE: Entering handshake mode drives CA2 high
       GIVEN: a PIA with CA2 manual low
       WHEN: CRA selects handshake mode
       THEN: CA2 is driven high
     */
    [Fact]
    public void HandshakeStartsHigh()
    {
        // ARRANGE:
        Write(Cra, C2ManualLow);

        // ACT:
        Write(Cra, C2Handshake);

        // ASSERT:
        Assert.True(Pins.CA2Drive);
        Assert.True(Pins.CA2Out);
    }

    /*
       TITLE: In handshake mode Read A Data takes CA2 low at the PHI2 fall ending the read
       GIVEN: a PIA with CA2 handshake and DDR Access set, addressed for a Port A read
       WHEN: PHI2 rises, then falls
       THEN: CA2 is still high while PHI2 is high and low after the fall
     */
    [Fact]
    public void HandshakeLowAtFallOfReadAData()
    {
        // ARRANGE:
        Write(Cra, C2Handshake | DdrAccess);
        Address(PortA, read: true);

        // ACT:
        Rise();
        var duringHigh = Pins.CA2Out;
        Fall();

        // ASSERT:
        Assert.True(duringHigh);
        Assert.False(Pins.CA2Out);
    }

    /*
       TITLE: In handshake mode CA2 stays low until the CA1 active transition sets it high
       GIVEN: a PIA with CA2 handshake taken low by Read A Data, CA1 active on rising edge
       WHEN: two idle cycles run, CA1 falls, then CA1 rises
       THEN: CA2 stays low through the idle cycles and the falling CA1, and is high after the rise
     */
    [Fact]
    public void HandshakeHighOnCa1ActiveTransition()
    {
        // ARRANGE:
        Write(Cra, C2Handshake | DdrAccess | C1Rising);
        Read(PortA);

        // ACT:
        IdleCycle();
        IdleCycle();
        SetCa1(false);
        var afterFall = Pins.CA2Out;
        SetCa1(true);

        // ASSERT:
        Assert.False(afterFall);
        Assert.True(Pins.CA2Out);
    }

    /*
       TITLE: In pulse mode CA2 is low for one PHI2 cycle after Read A Data
       GIVEN: a PIA with CA2 pulse and DDR Access set
       WHEN: Port A is read and one idle cycle follows
       THEN: CA2 is low after the read's fall, still low during the next high phase, high at the next fall
     */
    [Fact]
    public void PulseLowForOneCycle()
    {
        // ARRANGE:
        Write(Cra, C2Pulse | DdrAccess);

        // ACT:
        Read(PortA);
        var afterRead = Pins.CA2Out;
        Rise();
        var nextHigh = Pins.CA2Out;
        Fall();

        // ASSERT:
        Assert.False(afterRead);
        Assert.False(nextHigh);
        Assert.True(Pins.CA2Out);
    }

    /*
       TITLE: Only Read A Data strobes CA2
       GIVEN: a PIA with CA2 pulse mode
       WHEN: DDRA is read, CRA is read, and ORA is written (DDR Access set)
       THEN: CA2 stays high throughout
     */
    [Fact]
    public void OtherAccessesDoNotStrobe()
    {
        // ARRANGE:
        Write(Cra, C2Pulse);

        // ACT:
        Read(PortA);
        var afterDdrRead = Pins.CA2Out;
        Read(Cra);
        var afterCrRead = Pins.CA2Out;
        Write(Cra, C2Pulse | DdrAccess);
        Write(PortA, 0x12);

        // ASSERT:
        Assert.True(afterDdrRead);
        Assert.True(afterCrRead);
        Assert.True(Pins.CA2Out);
    }

    /*
       TITLE: Switching from handshake to pulse keeps the current CA2 level
       GIVEN: a PIA with CA2 handshake taken low by Read A Data
       WHEN: CRA selects pulse mode
       THEN: CA2 stays low
     */
    [Fact]
    public void HandshakeToPulseKeepsLevel()
    {
        // ARRANGE:
        Write(Cra, C2Handshake | DdrAccess);
        Read(PortA);

        // ACT:
        Write(Cra, C2Pulse | DdrAccess);

        // ASSERT:
        Assert.False(Pins.CA2Out);
    }

    /*
       TITLE: Leaving pulse mode mid-pulse cancels the pulse
       GIVEN: a PIA with CA2 pulse low after Read A Data
       WHEN: CRA selects manual low and an idle cycle runs
       THEN: CA2 stays low
     */
    [Fact]
    public void LeavingPulseModeCancelsPulse()
    {
        // ARRANGE:
        Write(Cra, C2Pulse | DdrAccess);
        Read(PortA);

        // ACT:
        Write(Cra, C2ManualLow);
        IdleCycle();

        // ASSERT:
        Assert.False(Pins.CA2Out);
    }
}
