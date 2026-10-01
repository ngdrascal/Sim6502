using System.Diagnostics.CodeAnalysis;

namespace W65C21.Engine.Tests;

[ExcludeFromCodeCoverage]
public class Cb2OutputTests : PiaTestBase
{
    /*
       TITLE: Manual output modes drive CB2 from CRB bit 3
       GIVEN: a reset PIA
       WHEN: CRB selects manual low, then manual high
       THEN: CB2 is driven low, then driven high
     */
    [Fact]
    public void ManualOutput()
    {
        // ARRANGE:

        // ACT:
        Write(Crb, C2ManualLow);
        var low = Pins.CB2Out;
        Write(Crb, C2ManualHigh);

        // ASSERT:
        Assert.False(low);
        Assert.True(Pins.CB2Out);
        Assert.True(Pins.CB2Drive);
    }

    /*
       TITLE: In handshake mode Write B Data takes CB2 low at the next PHI2 rise
       GIVEN: a PIA with CB2 handshake and DDR Access set
       WHEN: ORB is written, then PHI2 rises
       THEN: CB2 is still high after the write's fall and low after the next rise
     */
    [Fact]
    public void HandshakeLowAtRiseAfterWriteBData()
    {
        // ARRANGE:
        Write(Crb, C2Handshake | DdrAccess);

        // ACT:
        Write(PortB, 0x55);
        var afterWrite = Pins.CB2Out;
        Rise();

        // ASSERT:
        Assert.True(afterWrite);
        Assert.False(Pins.CB2Out);
    }

    /*
       TITLE: In handshake mode CB2 stays low until the CB1 active transition sets it high
       GIVEN: a PIA with CB2 handshake taken low after Write B Data, CB1 active on falling edge
       WHEN: two idle cycles run, then CB1 falls
       THEN: CB2 stays low through the idle cycles and is high after CB1 falls
     */
    [Fact]
    public void HandshakeHighOnCb1ActiveTransition()
    {
        // ARRANGE:
        Write(Crb, C2Handshake | DdrAccess);
        Write(PortB, 0x55);

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
       TITLE: In pulse mode CB2 is low for one PHI2 cycle, rise to rise, after Write B Data
       GIVEN: a PIA with CB2 pulse and DDR Access set
       WHEN: ORB is written and two PHI2 rises follow
       THEN: CB2 goes low at the first rise, stays low through its fall, and is high at the second rise
     */
    [Fact]
    public void PulseLowForOneCycle()
    {
        // ARRANGE:
        Write(Crb, C2Pulse | DdrAccess);
        Write(PortB, 0x55);

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
       TITLE: Only Write B Data strobes CB2
       GIVEN: a PIA with CB2 pulse mode
       WHEN: DDRB is written, Port B is read (DDR Access set), and idle cycles follow each
       THEN: CB2 stays high throughout
     */
    [Fact]
    public void OtherAccessesDoNotStrobe()
    {
        // ARRANGE:
        Write(Crb, C2Pulse);

        // ACT:
        Write(PortB, 0xFF);
        IdleCycle();
        var afterDdrWrite = Pins.CB2Out;
        Write(Crb, C2Pulse | DdrAccess);
        Read(PortB);
        IdleCycle();

        // ASSERT:
        Assert.True(afterDdrWrite);
        Assert.True(Pins.CB2Out);
    }

    /*
       TITLE: A Write B Data in input mode does not strobe CB2 once output mode is selected
       GIVEN: a PIA with CB2 as input and DDR Access set
       WHEN: ORB is written, CRB selects pulse mode, and idle cycles run
       THEN: CB2 is driven high throughout
     */
    [Fact]
    public void WriteInInputModeDoesNotStrobe()
    {
        // ARRANGE:
        Write(Crb, DdrAccess);

        // ACT:
        Write(PortB, 0x55);
        Write(Crb, C2Pulse | DdrAccess);
        IdleCycle();
        IdleCycle();

        // ASSERT:
        Assert.True(Pins.CB2Drive);
        Assert.True(Pins.CB2Out);
    }
}
