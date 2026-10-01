using System.Diagnostics.CodeAnalysis;

namespace W65C21.Engine.Tests;

[ExcludeFromCodeCoverage]
public class ResetTests : PiaTestBase
{
    /*
       TITLE: A new PIA starts in the reset state
       GIVEN: nothing
       WHEN: a W65C21Engine is constructed
       THEN: all registers are 0, D, ports and CA2/CB2 float and both IRQs are released
     */
    [Fact]
    public void NewEngineIsReset()
    {
        // ARRANGE:

        // ACT:
        var engine = new W65C21Engine();

        // ASSERT:
        AssertResetState(engine);
    }

    /*
       TITLE: RESB low clears every register and releases every output
       GIVEN: a PIA with all registers written, CA2/CB2 manual outputs and a CA1 interrupt pending
       WHEN: RESB goes low
       THEN: all registers are 0, ports and CA2/CB2 float and both IRQs are released
     */
    [Fact]
    public void ResbLowResets()
    {
        // ARRANGE:
        Write(PortA, 0xFF);
        Write(PortB, 0xFF);
        Write(Cra, C2ManualLow | DdrAccess | C1IrqEnable);
        Write(Crb, C2ManualLow | DdrAccess);
        Write(PortA, 0x12);
        Write(PortB, 0x34);
        SetCa1(false);

        // ACT:
        Pins.RESB = false;
        Engine.Evaluate();

        // ASSERT:
        AssertResetState(Engine);
    }

    /*
       TITLE: RESB low mid-read releases D immediately
       GIVEN: a PIA in the high phase of a CRA read
       WHEN: RESB goes low
       THEN: D floats
     */
    [Fact]
    public void ResbLowReleasesData()
    {
        // ARRANGE:
        Address(Cra, read: true);
        Rise();

        // ACT:
        Pins.RESB = false;
        Engine.Evaluate();

        // ASSERT:
        Assert.Equal(0x00, Pins.DataDrive);
    }

    /*
       TITLE: Bus cycles are ignored while RESB is low
       GIVEN: a PIA with RESB held low
       WHEN: a CRA write of 0x3F and a CRA read run
       THEN: CRA stays 0 and D is never driven
     */
    [Fact]
    public void BusCyclesIgnoredDuringReset()
    {
        // ARRANGE:
        Pins.RESB = false;
        Engine.Evaluate();

        // ACT:
        Write(Cra, 0x3F);
        Address(Cra, read: true);
        Rise();
        var drive = Pins.DataDrive;
        Fall();

        // ASSERT:
        Assert.Equal(0x00, Engine.CRA);
        Assert.Equal(0x00, drive);
    }

    /*
       TITLE: Control line transitions during reset set no Interrupt Flag, before or after release
       GIVEN: a PIA with RESB held low
       WHEN: CA1 and CB1 fall, then RESB is released
       THEN: CRA and CRB have no Interrupt Flags set
     */
    [Fact]
    public void ControlTransitionsDuringResetIgnored()
    {
        // ARRANGE:
        Pins.RESB = false;
        Engine.Evaluate();

        // ACT:
        SetCa1(false);
        SetCb1(false);
        Pins.RESB = true;
        Engine.Evaluate();

        // ASSERT:
        Assert.Equal(0x00, Engine.CRA);
        Assert.Equal(0x00, Engine.CRB);
    }

    private static void AssertResetState(W65C21Engine engine)
    {
        Assert.Equal(0x00, engine.CRA);
        Assert.Equal(0x00, engine.CRB);
        Assert.Equal(0x00, engine.DDRA);
        Assert.Equal(0x00, engine.DDRB);
        Assert.Equal(0x00, engine.ORA);
        Assert.Equal(0x00, engine.ORB);
        Assert.Equal(0x00, engine.Pins.DataDrive);
        Assert.Equal(0x00, engine.Pins.PADrive);
        Assert.Equal(0x00, engine.Pins.PBDrive);
        Assert.False(engine.Pins.CA2Drive);
        Assert.False(engine.Pins.CB2Drive);
        Assert.False(engine.Pins.IRQABDrive);
        Assert.False(engine.Pins.IRQBBDrive);
    }
}
