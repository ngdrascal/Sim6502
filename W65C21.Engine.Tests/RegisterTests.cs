using System.Diagnostics.CodeAnalysis;

namespace W65C21.Engine.Tests;

[ExcludeFromCodeCoverage]
public class RegisterTests : PiaTestBase
{
    /*
       TITLE: A Control Register write stores bits 0-5 and reads back
       GIVEN: a reset PIA
       WHEN: 0x3F is written to CRA and 0x2B to CRB and both are read
       THEN: the reads return 0x3F and 0x2B
     */
    [Fact]
    public void ControlRegistersReadBack()
    {
        // ARRANGE:

        // ACT:
        Write(Cra, 0x3F);
        Write(Crb, 0x2B);
        var cra = Read(Cra);
        var crb = Read(Crb);

        // ASSERT:
        Assert.Equal(0x3F, cra);
        Assert.Equal(0x2B, crb);
    }

    /*
       TITLE: The Interrupt Flags in a Control Register are read only
       GIVEN: a reset PIA
       WHEN: 0xC0 is written to CRA and CRB
       THEN: both Control Registers read 0x00
     */
    [Fact]
    public void InterruptFlagsAreReadOnly()
    {
        // ARRANGE:

        // ACT:
        Write(Cra, 0xC0);
        Write(Crb, 0xC0);

        // ASSERT:
        Assert.Equal(0x00, Read(Cra));
        Assert.Equal(0x00, Read(Crb));
    }

    /*
       TITLE: With the DDR Access bit clear the data locations reach the Data Direction Registers
       GIVEN: a reset PIA (DDR Access bits clear)
       WHEN: 0x0F is written to Side A's data location and 0xF0 to Side B's
       THEN: DDRA is 0x0F, DDRB is 0xF0, both read back, and the Output Registers are untouched
     */
    [Fact]
    public void DataLocationReachesDdrWhenDdrAccessClear()
    {
        // ARRANGE:

        // ACT:
        Write(PortA, 0x0F);
        Write(PortB, 0xF0);

        // ASSERT:
        Assert.Equal(0x0F, Engine.DDRA);
        Assert.Equal(0xF0, Engine.DDRB);
        Assert.Equal(0x0F, Read(PortA));
        Assert.Equal(0xF0, Read(PortB));
        Assert.Equal(0x00, Engine.ORA);
        Assert.Equal(0x00, Engine.ORB);
    }

    /*
       TITLE: With the DDR Access bit set the data locations reach the Output Registers
       GIVEN: a PIA with both DDR Access bits set
       WHEN: 0x5A is written to Side A's data location and 0xA5 to Side B's
       THEN: ORA is 0x5A, ORB is 0xA5 and the Data Direction Registers are untouched
     */
    [Fact]
    public void DataLocationReachesOutputRegisterWhenDdrAccessSet()
    {
        // ARRANGE:
        Write(Cra, DdrAccess);
        Write(Crb, DdrAccess);

        // ACT:
        Write(PortA, 0x5A);
        Write(PortB, 0xA5);

        // ASSERT:
        Assert.Equal(0x5A, Engine.ORA);
        Assert.Equal(0xA5, Engine.ORB);
        Assert.Equal(0x00, Engine.DDRA);
        Assert.Equal(0x00, Engine.DDRB);
    }

    /*
       TITLE: Port lines drive the Output Register only where the Data Direction Register is 1
       GIVEN: a reset PIA
       WHEN: DDRA=0x0F, ORA=0x55, DDRB=0xF0, ORB=0xAA are written
       THEN: PA drives 0x55 under mask 0x0F and PB drives 0xAA under mask 0xF0
     */
    [Fact]
    public void PortDriveFollowsDataDirection()
    {
        // ARRANGE:

        // ACT:
        Write(PortA, 0x0F);
        Write(PortB, 0xF0);
        Write(Cra, DdrAccess);
        Write(Crb, DdrAccess);
        Write(PortA, 0x55);
        Write(PortB, 0xAA);

        // ASSERT:
        Assert.Equal(0x55, Pins.PAOut);
        Assert.Equal(0x0F, Pins.PADrive);
        Assert.Equal(0xAA, Pins.PBOut);
        Assert.Equal(0xF0, Pins.PBDrive);
    }

    /*
       TITLE: Reading Port A returns the pin levels, even on output lines
       GIVEN: a PIA with DDRA=0xFF, ORA=0x00 and DDR Access set
       WHEN: the PA pins read 0x3C and Port A is read
       THEN: the read returns 0x3C
     */
    [Fact]
    public void PortAReadReturnsPinLevels()
    {
        // ARRANGE:
        Write(PortA, 0xFF);
        Write(Cra, DdrAccess);
        Write(PortA, 0x00);
        Pins.PAIn = 0x3C;

        // ACT:
        var value = Read(PortA);

        // ASSERT:
        Assert.Equal(0x3C, value);
    }

    /*
       TITLE: Reading Port B returns the Output Register on output lines and pin levels on inputs
       GIVEN: a PIA with DDRB=0xF0, ORB=0xA0 and DDR Access set
       WHEN: the PB pins read 0x55 and Port B is read
       THEN: the read returns 0xA0 from ORB merged with 0x05 from the input pins
     */
    [Fact]
    public void PortBReadMergesOutputRegisterAndPins()
    {
        // ARRANGE:
        Write(PortB, 0xF0);
        Write(Crb, DdrAccess);
        Write(PortB, 0xA0);
        Pins.PBIn = 0x55;

        // ACT:
        var value = Read(PortB);

        // ASSERT:
        Assert.Equal(0xA5, value);
    }

    /*
       TITLE: The part ignores bus cycles unless CS0 and CS1 are high and CS2B is low
       GIVEN: a reset PIA
       WHEN: a write of 0x3F to CRA and a read of CRA run with one chip select inactive
       THEN: CRA is unchanged and D is never driven
     */
    [Theory]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void DeselectedCyclesAreIgnored(bool cs0, bool cs1, bool cs2B)
    {
        // ARRANGE:
        Pins.CS0 = cs0;
        Pins.CS1 = cs1;
        Pins.RS0 = true;
        Pins.RS1 = false;

        // ACT:
        Pins.CS2B = cs2B;
        Pins.RWB = false;
        Rise();
        Pins.DataIn = 0x3F;
        Fall();
        Pins.RWB = true;
        Rise();
        var drive = Pins.DataDrive;
        Fall();

        // ASSERT:
        Assert.Equal(0x00, Engine.CRA);
        Assert.Equal(0x00, drive);
    }
}
