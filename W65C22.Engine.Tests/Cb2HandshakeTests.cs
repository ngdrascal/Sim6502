using System.Diagnostics.CodeAnalysis;

namespace W65C22.Engine.Tests;

[ExcludeFromCodeCoverage]
public class Cb2HandshakeTests : ViaTestBase
{
    /*
       TITLE: Manual output modes drive CB2 from PCR bits 7-5
       GIVEN: a reset VIA
       WHEN: PCR selects CB2 manual low, then manual high
       THEN: CB2 is driven low, then driven high
     */
    [Fact]
    public void ManualOutput()
    {
        // ARRANGE:

        // ACT:
        Write(Pcr, C2ManualLow << 4);
        var low = Pins.CB2Out;
        Write(Pcr, C2ManualHigh << 4);

        // ASSERT:
        Assert.False(low);
        Assert.True(Pins.CB2Out);
        Assert.True(Pins.CB2Drive);
    }

    /*
       TITLE: Write handshake takes CB2 low at the PHI2 rise after the ORB write
       GIVEN: a VIA with CB2 handshake
       WHEN: ORB is written, then PHI2 rises
       THEN: CB2 is still high after the write's fall and low after the next rise
     */
    [Fact]
    public void HandshakeLowAtRiseAfterOrbWrite()
    {
        // ARRANGE:
        Write(Pcr, C2Handshake << 4);

        // ACT:
        Write(Orb, 0x55);
        var afterWrite = Pins.CB2Out;
        Rise();

        // ASSERT:
        Assert.True(afterWrite);
        Assert.False(Pins.CB2Out);
    }

    /*
       TITLE: In handshake mode CB2 stays low until the CB1 active transition sets it high
       GIVEN: a VIA with CB2 handshake taken low after an ORB write, CB1 active on falling edge
       WHEN: two idle cycles run, then CB1 falls
       THEN: CB2 stays low through the idle cycles and is high after CB1 falls
     */
    [Fact]
    public void HandshakeHighOnCb1ActiveTransition()
    {
        // ARRANGE:
        Write(Pcr, C2Handshake << 4);
        Write(Orb, 0x55);

        // ACT:
        IdleCycle();
        IdleCycle();
        var beforeCb1 = Pins.CB2Out;
        SetCb1(false);

        // ASSERT:
        Assert.False(beforeCb1);
        Assert.True(Pins.CB2Out);
    }

    /*
       TITLE: In pulse mode CB2 is low for one PHI2 cycle, rise to rise, after an ORB write
       GIVEN: a VIA with CB2 pulse mode
       WHEN: ORB is written and two PHI2 rises follow
       THEN: CB2 goes low at the first rise, stays low through its fall, and is high at the second rise
     */
    [Fact]
    public void PulseLowForOneCycle()
    {
        // ARRANGE:
        Write(Pcr, C2Pulse << 4);
        Write(Orb, 0x55);

        // ACT:
        Rise();
        var firstRise = Pins.CB2Out;
        Fall();
        var firstFall = Pins.CB2Out;
        Rise();

        // ASSERT:
        Assert.False(firstRise);
        Assert.False(firstFall);
        Assert.True(Pins.CB2Out);
    }

    /*
       TITLE: Reading IRB does not strobe CB2 (no read handshake on Port B)
       GIVEN: a VIA with CB2 pulse mode
       WHEN: IRB is read and idle cycles follow
       THEN: CB2 stays high
     */
    [Fact]
    public void ReadDoesNotStrobe()
    {
        // ARRANGE:
        Write(Pcr, C2Pulse << 4);

        // ACT:
        Read(Orb);
        IdleCycle();
        IdleCycle();

        // ASSERT:
        Assert.True(Pins.CB2Out);
    }

    /*
       TITLE: An ORB write in input mode does not strobe CB2 once output mode is selected
       GIVEN: a VIA with CB2 as input
       WHEN: ORB is written, PCR selects CB2 pulse mode, and idle cycles run
       THEN: CB2 is driven high throughout
     */
    [Fact]
    public void WriteInInputModeDoesNotStrobe()
    {
        // ARRANGE:

        // ACT:
        Write(Orb, 0x55);
        Write(Pcr, C2Pulse << 4);
        IdleCycle();
        IdleCycle();

        // ASSERT:
        Assert.True(Pins.CB2Drive);
        Assert.True(Pins.CB2Out);
    }
}
