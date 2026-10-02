using System.Diagnostics.CodeAnalysis;

namespace W65C22.Engine.Tests;

[ExcludeFromCodeCoverage]
public class BusTimingTests : ViaTestBase
{
    /*
       TITLE: A selected read drives D from the PHI2 rise to the PHI2 fall
       GIVEN: a VIA with PCR=0x15, addressed for a PCR read with PHI2 low
       WHEN: PHI2 rises and then falls
       THEN: D is not driven before the rise, drives 0x15 while PHI2 is high and floats after the fall
     */
    [Fact]
    public void ReadDrivesDataOnlyWhilePhi2High()
    {
        // ARRANGE:
        Write(Pcr, 0x15);
        Address(Pcr, read: true);
        var beforeRise = Pins.DataDrive;

        // ACT:
        Rise();
        var duringHigh = Pins.DataDrive;
        var value = Pins.DataOut;
        Fall();

        // ASSERT:
        Assert.Equal(0x00, beforeRise);
        Assert.Equal(0xFF, duringHigh);
        Assert.Equal(0x15, value);
        Assert.Equal(0x00, Pins.DataDrive);
    }

    /*
       TITLE: A write never drives D
       GIVEN: a VIA addressed for a PCR write with PHI2 low
       WHEN: PHI2 rises
       THEN: D is not driven
     */
    [Fact]
    public void WriteDoesNotDriveData()
    {
        // ARRANGE:
        Address(Pcr, read: false);

        // ACT:
        Rise();

        // ASSERT:
        Assert.Equal(0x00, Pins.DataDrive);
    }

    /*
       TITLE: The part is selected only when CS1 is high and CS2B is low
       GIVEN: a VIA addressed for a PCR read with the given CS1 and CS2B levels
       WHEN: PHI2 rises
       THEN: D is driven only for CS1=1, CS2B=0
     */
    [Theory]
    [InlineData(true, false, 0xFF)]
    [InlineData(false, false, 0x00)]
    [InlineData(true, true, 0x00)]
    [InlineData(false, true, 0x00)]
    public void ChipSelect(bool cs1, bool cs2b, byte expectedDrive)
    {
        // ARRANGE:
        Address(Pcr, read: true);
        Pins.CS1 = cs1;
        Pins.CS2B = cs2b;
        Engine.Evaluate();

        // ACT:
        Rise();

        // ASSERT:
        Assert.Equal(expectedDrive, Pins.DataDrive);
    }

    /*
       TITLE: A deselected write changes no register
       GIVEN: a VIA with CS1 low, addressed for a PCR write
       WHEN: a full PHI2 cycle runs with D=0x55
       THEN: PCR stays 0
     */
    [Fact]
    public void DeselectedWriteIgnored()
    {
        // ARRANGE:
        Address(Pcr, read: false);
        Pins.CS1 = false;
        Pins.DataIn = 0x55;

        // ACT:
        Rise();
        Fall();

        // ASSERT:
        Assert.Equal(0x00, Engine.PCR);
    }

    /*
       TITLE: Read data is latched at the PHI2 rise
       GIVEN: a VIA with PA pins reading 0x11, addressed for an IRA read
       WHEN: PHI2 rises and the PA pins change to 0x22 while PHI2 is high
       THEN: D keeps driving 0x11
     */
    [Fact]
    public void ReadDataLatchedAtRise()
    {
        // ARRANGE:
        Pins.PAIn = 0x11;
        Address(Ora, read: true);

        // ACT:
        Rise();
        Pins.PAIn = 0x22;
        Engine.Evaluate();

        // ASSERT:
        Assert.Equal(0x11, Pins.DataOut);
    }

    /*
       TITLE: Register select and RWB are sampled at the PHI2 rise
       GIVEN: a VIA addressed for a PCR write with PHI2 low
       WHEN: PHI2 rises, RS0 changes (selecting IFR) while PHI2 is high, and PHI2 falls with D=0x07
       THEN: PCR receives 0x07
     */
    [Fact]
    public void AddressSampledAtRise()
    {
        // ARRANGE:
        Address(Pcr, read: false);

        // ACT:
        Rise();
        Pins.RS0 = true;
        Pins.DataIn = 0x07;
        Engine.Evaluate();
        Fall();

        // ASSERT:
        Assert.Equal(0x07, Engine.PCR);
    }

    /*
       TITLE: Write data is latched and port outputs change at the PHI2 fall
       GIVEN: a VIA with DDRA=0xFF, addressed for an ORA write
       WHEN: PHI2 rises with D=0x12, D changes to 0x34 while PHI2 is high, then PHI2 falls
       THEN: PA still shows the old ORA while PHI2 is high and shows 0x34 after the fall
     */
    [Fact]
    public void WriteLatchedAtFall()
    {
        // ARRANGE:
        Write(Ddra, 0xFF);
        Address(Ora, read: false);
        Pins.DataIn = 0x12;

        // ACT:
        Rise();
        Pins.DataIn = 0x34;
        Engine.Evaluate();
        var duringHigh = Pins.PAOut;
        Fall();

        // ASSERT:
        Assert.Equal(0x00, duringHigh);
        Assert.Equal(0x34, Pins.PAOut);
    }

    /*
       TITLE: Evaluating without a PHI2 change starts no new bus cycle
       GIVEN: a VIA in the high phase of a PCR write with D=0x01
       WHEN: Evaluate is called repeatedly before the fall, and again after it
       THEN: PCR is written once with 0x01 and a later Evaluate with D=0x02 changes nothing
     */
    [Fact]
    public void RepeatedEvaluateIsIdempotent()
    {
        // ARRANGE:
        Address(Pcr, read: false);
        Rise();
        Pins.DataIn = 0x01;

        // ACT:
        Engine.Evaluate();
        Engine.Evaluate();
        Fall();
        Pins.DataIn = 0x02;
        Engine.Evaluate();

        // ASSERT:
        Assert.Equal(0x01, Engine.PCR);
    }
}
