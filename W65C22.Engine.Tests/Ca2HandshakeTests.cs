using System.Diagnostics.CodeAnalysis;

namespace W65C22.Engine.Tests;

[ExcludeFromCodeCoverage]
public class Ca2HandshakeTests : ViaTestBase
{
    /*
       TITLE: CA2 floats while in an input mode
       GIVEN: a reset VIA
       WHEN: PCR selects CA2 independent rising-edge input
       THEN: CA2 is not driven
     */
    [Fact]
    public void InputModeFloats()
    {
        // ARRANGE:

        // ACT:
        Write(Pcr, C2IndependentRising);

        // ASSERT:
        Assert.False(Pins.CA2Drive);
    }

    /*
       TITLE: Manual output modes drive CA2 low or high
       GIVEN: a reset VIA
       WHEN: PCR selects manual low, then manual high
       THEN: CA2 is driven low, then driven high
     */
    [Fact]
    public void ManualOutput()
    {
        // ARRANGE:

        // ACT:
        Write(Pcr, C2ManualLow);
        var low = Pins.CA2Out;
        var lowDriven = Pins.CA2Drive;
        Write(Pcr, C2ManualHigh);

        // ASSERT:
        Assert.False(low);
        Assert.True(lowDriven);
        Assert.True(Pins.CA2Out);
        Assert.True(Pins.CA2Drive);
    }

    /*
       TITLE: Entering handshake mode drives CA2 high
       GIVEN: a VIA with CA2 manual low
       WHEN: PCR selects handshake mode
       THEN: CA2 is driven high
     */
    [Fact]
    public void HandshakeStartsHigh()
    {
        // ARRANGE:
        Write(Pcr, C2ManualLow);

        // ACT:
        Write(Pcr, C2Handshake);

        // ASSERT:
        Assert.True(Pins.CA2Drive);
        Assert.True(Pins.CA2Out);
    }

    /*
       TITLE: Read handshake takes CA2 low at the PHI2 fall ending the IRA read (Figure 4-4)
       GIVEN: a VIA with CA2 handshake, addressed for a register 1 read
       WHEN: PHI2 rises, then falls
       THEN: CA2 is still high while PHI2 is high and low after the fall
     */
    [Fact]
    public void ReadHandshakeLowAtFall()
    {
        // ARRANGE:
        Write(Pcr, C2Handshake);
        Address(Ora, read: true);

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
       GIVEN: a VIA with CA2 handshake taken low by an IRA read, CA1 active on rising edge
       WHEN: two idle cycles run, CA1 falls, then CA1 rises
       THEN: CA2 stays low through the idle cycles and the falling CA1, and is high after the rise
     */
    [Fact]
    public void HandshakeHighOnCa1ActiveTransition()
    {
        // ARRANGE:
        Write(Pcr, C2Handshake | C1Rising);
        Read(Ora);

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
       TITLE: Read pulse mode holds CA2 low for one cycle, fall to fall (Figure 4-3)
       GIVEN: a VIA with CA2 pulse mode
       WHEN: IRA is read and one idle cycle follows
       THEN: CA2 is low after the read's fall, low during the next high phase, high at the next fall
     */
    [Fact]
    public void ReadPulseLowForOneCycle()
    {
        // ARRANGE:
        Write(Pcr, C2Pulse);

        // ACT:
        Read(Ora);
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
       TITLE: Write handshake takes CA2 low at the PHI2 rise after the ORA write (Figure 4-6)
       GIVEN: a VIA with CA2 handshake
       WHEN: ORA is written, then PHI2 rises
       THEN: CA2 is still high after the write's fall and low after the next rise
     */
    [Fact]
    public void WriteHandshakeLowAtNextRise()
    {
        // ARRANGE:
        Write(Pcr, C2Handshake);

        // ACT:
        Write(Ora, 0x55);
        var afterWrite = Pins.CA2Out;
        Rise();

        // ASSERT:
        Assert.True(afterWrite);
        Assert.False(Pins.CA2Out);
    }

    /*
       TITLE: Write pulse mode holds CA2 low for one cycle, rise to rise (Figure 4-5)
       GIVEN: a VIA with CA2 pulse mode
       WHEN: ORA is written and two PHI2 rises follow
       THEN: CA2 goes low at the first rise, stays low through its fall, and is high at the second rise
     */
    [Fact]
    public void WritePulseLowForOneCycle()
    {
        // ARRANGE:
        Write(Pcr, C2Pulse);
        Write(Ora, 0x55);

        // ACT:
        Rise();
        var firstRise = Pins.CA2Out;
        Fall();
        var firstFall = Pins.CA2Out;
        Rise();

        // ASSERT:
        Assert.False(firstRise);
        Assert.False(firstFall);
        Assert.True(Pins.CA2Out);
    }

    /*
       TITLE: Register $F and other registers do not strobe CA2
       GIVEN: a VIA with CA2 pulse mode
       WHEN: register $F is read and written, DDRA is written, and idle cycles follow
       THEN: CA2 stays high throughout
     */
    [Fact]
    public void OtherAccessesDoNotStrobe()
    {
        // ARRANGE:
        Write(Pcr, C2Pulse);

        // ACT:
        Read(OraNoHandshake);
        var afterRead = Pins.CA2Out;
        Write(OraNoHandshake, 0x12);
        IdleCycle();
        var afterWrite = Pins.CA2Out;
        Write(Ddra, 0xFF);
        IdleCycle();

        // ASSERT:
        Assert.True(afterRead);
        Assert.True(afterWrite);
        Assert.True(Pins.CA2Out);
    }

    /*
       TITLE: Switching from handshake to pulse keeps the current CA2 level
       GIVEN: a VIA with CA2 handshake taken low by an IRA read
       WHEN: PCR selects pulse mode
       THEN: CA2 stays low
     */
    [Fact]
    public void HandshakeToPulseKeepsLevel()
    {
        // ARRANGE:
        Write(Pcr, C2Handshake);
        Read(Ora);

        // ACT:
        Write(Pcr, C2Pulse);

        // ASSERT:
        Assert.False(Pins.CA2Out);
    }

    /*
       TITLE: Leaving pulse mode mid-pulse cancels the pulse
       GIVEN: a VIA with CA2 pulse low after an IRA read
       WHEN: PCR selects manual low and an idle cycle runs
       THEN: CA2 stays low
     */
    [Fact]
    public void LeavingPulseModeCancelsPulse()
    {
        // ARRANGE:
        Write(Pcr, C2Pulse);
        Read(Ora);

        // ACT:
        Write(Pcr, C2ManualLow);
        IdleCycle();

        // ASSERT:
        Assert.False(Pins.CA2Out);
    }
}
