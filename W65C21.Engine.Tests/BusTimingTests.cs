using System.Diagnostics.CodeAnalysis;

namespace W65C21.Engine.Tests;

[ExcludeFromCodeCoverage]
public class BusTimingTests : PiaTestBase
{
    /*
       TITLE: A selected read drives D from the PHI2 rise to the PHI2 fall
       GIVEN: a PIA with CRA=0x15, addressed for a CRA read with PHI2 low
       WHEN: PHI2 rises and then falls
       THEN: D is not driven before the rise, drives 0x15 while PHI2 is high and floats after the fall
     */
    [Fact]
    public void ReadDrivesDataOnlyWhilePhi2High()
    {
        // ARRANGE:
        Write(Cra, 0x15);
        Address(Cra, read: true);
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
       GIVEN: a PIA addressed for a CRA write with PHI2 low
       WHEN: PHI2 rises
       THEN: D is not driven
     */
    [Fact]
    public void WriteDoesNotDriveData()
    {
        // ARRANGE:
        Address(Cra, read: false);

        // ACT:
        Rise();

        // ASSERT:
        Assert.Equal(0x00, Pins.DataDrive);
    }

    /*
       TITLE: Read data is latched at the PHI2 rise
       GIVEN: a PIA with DDR Access set on Side A and PA pins reading 0x11, addressed for a Port A read
       WHEN: PHI2 rises and the PA pins change to 0x22 while PHI2 is high
       THEN: D keeps driving 0x11
     */
    [Fact]
    public void ReadDataLatchedAtRise()
    {
        // ARRANGE:
        Write(Cra, DdrAccess);
        Pins.PAIn = 0x11;
        Address(PortA, read: true);

        // ACT:
        Rise();
        Pins.PAIn = 0x22;
        Engine.Evaluate();

        // ASSERT:
        Assert.Equal(0x11, Pins.DataOut);
    }

    /*
       TITLE: Register select and RWB are sampled at the PHI2 rise
       GIVEN: a PIA addressed for a CRA write with PHI2 low
       WHEN: PHI2 rises, RS changes to CRB while PHI2 is high, and PHI2 falls with D=0x07
       THEN: CRA receives 0x07 and CRB is unchanged
     */
    [Fact]
    public void AddressSampledAtRise()
    {
        // ARRANGE:
        Address(Cra, read: false);

        // ACT:
        Rise();
        Pins.RS1 = true;
        Pins.DataIn = 0x07;
        Engine.Evaluate();
        Fall();

        // ASSERT:
        Assert.Equal(0x07, Engine.CRA);
        Assert.Equal(0x00, Engine.CRB);
    }

    /*
       TITLE: Write data is latched and port outputs change at the PHI2 fall
       GIVEN: a PIA with DDRA=0xFF and DDR Access set, addressed for a Port A write
       WHEN: PHI2 rises with D=0x12, D changes to 0x34 while PHI2 is high, then PHI2 falls
       THEN: PA still shows the old ORA while PHI2 is high and shows 0x34 after the fall
     */
    [Fact]
    public void WriteLatchedAtFall()
    {
        // ARRANGE:
        Write(PortA, 0xFF);
        Write(Cra, DdrAccess);
        Address(PortA, read: false);
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
       GIVEN: a PIA in the high phase of a CRA write with D=0x01
       WHEN: Evaluate is called repeatedly before the fall, and again after it
       THEN: CRA is written once with 0x01 and a later Evaluate with D=0x02 changes nothing
     */
    [Fact]
    public void RepeatedEvaluateIsIdempotent()
    {
        // ARRANGE:
        Address(Cra, read: false);
        Rise();
        Pins.DataIn = 0x01;

        // ACT:
        Engine.Evaluate();
        Engine.Evaluate();
        Fall();
        Pins.DataIn = 0x02;
        Engine.Evaluate();

        // ASSERT:
        Assert.Equal(0x01, Engine.CRA);
    }
}
