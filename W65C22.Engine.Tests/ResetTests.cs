using System.Diagnostics.CodeAnalysis;

namespace W65C22.Engine.Tests;

[ExcludeFromCodeCoverage]
public class ResetTests : ViaTestBase
{
    /*
       TITLE: A new VIA starts in the reset state
       GIVEN: nothing
       WHEN: a W65C22Engine is constructed
       THEN: all registers are 0, D, ports and CA2/CB2 float and IRQB is high
     */
    [Fact]
    public void NewEngineIsReset()
    {
        // ARRANGE:

        // ACT:
        var engine = new W65C22Engine();

        // ASSERT:
        AssertResetState(engine);
    }

    /*
       TITLE: RESB low clears every register and releases every output
       GIVEN: a VIA with all registers written, CA2/CB2 manual outputs and a CA1 interrupt pending
       WHEN: RESB goes low
       THEN: all registers are 0, ports and CA2/CB2 float and IRQB is high
     */
    [Fact]
    public void ResbLowResets()
    {
        // ARRANGE:
        Write(Ddra, 0xFF);
        Write(Ddrb, 0xFF);
        Write(Ora, 0x12);
        Write(Orb, 0x34);
        Write(Acr, 0x03);
        Write(Pcr, C2ManualLow | (C2ManualLow << 4));
        Write(Ier, IerSet | Ca1Flag);
        SetCa1(false);

        // ACT:
        Pins.RESB = false;
        Engine.Evaluate();

        // ASSERT:
        AssertResetState(Engine);
    }

    /*
       TITLE: RESB low mid-read releases D immediately
       GIVEN: a VIA in the high phase of a PCR read
       WHEN: RESB goes low
       THEN: D floats
     */
    [Fact]
    public void ResbLowReleasesData()
    {
        // ARRANGE:
        Address(Pcr, read: true);
        Rise();

        // ACT:
        Pins.RESB = false;
        Engine.Evaluate();

        // ASSERT:
        Assert.Equal(0x00, Pins.DataDrive);
    }

    /*
       TITLE: Bus cycles are ignored while RESB is low
       GIVEN: a VIA with RESB held low
       WHEN: a PCR write of 0x3F and a PCR read run
       THEN: PCR stays 0 and D is never driven
     */
    [Fact]
    public void BusCyclesIgnoredDuringReset()
    {
        // ARRANGE:
        Pins.RESB = false;
        Engine.Evaluate();

        // ACT:
        Write(Pcr, 0x3F);
        Address(Pcr, read: true);
        Rise();
        var drive = Pins.DataDrive;
        Fall();

        // ASSERT:
        Assert.Equal(0x00, Engine.PCR);
        Assert.Equal(0x00, drive);
    }

    /*
       TITLE: Control line transitions during reset set no Interrupt Flag, before or after release
       GIVEN: a VIA with RESB held low
       WHEN: CA1 and CB1 fall, then RESB is released
       THEN: IFR is 0
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
        Assert.Equal(0x00, Engine.IFR);
    }

    private static void AssertResetState(W65C22Engine engine)
    {
        Assert.Equal(0x00, engine.ORA);
        Assert.Equal(0x00, engine.ORB);
        Assert.Equal(0x00, engine.DDRA);
        Assert.Equal(0x00, engine.DDRB);
        Assert.Equal(0x00, engine.ACR);
        Assert.Equal(0x00, engine.PCR);
        Assert.Equal(0x00, engine.IFR);
        Assert.Equal(0x00, engine.IER);
        Assert.Equal(0x00, engine.Pins.DataDrive);
        Assert.Equal(0x00, engine.Pins.PADrive);
        Assert.Equal(0x00, engine.Pins.PBDrive);
        Assert.False(engine.Pins.CA2Drive);
        Assert.False(engine.Pins.CB2Drive);
        Assert.True(engine.Pins.IRQB);
    }
}
