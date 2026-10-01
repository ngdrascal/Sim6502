using System.Diagnostics.CodeAnalysis;
using Digisim.Components;
using Digisim.Engine;
using Digisim.Shared;
using DigisimPlugin.TestHelpers;
using Microsoft.Extensions.Logging;

namespace W65C21.DigisimPlugin.Tests;

[ExcludeFromCodeCoverage]
public class W65C21PiaTests
{
    private const byte DdrAccess = 0x04;
    private const byte C1IrqEnable = 0x01;
    private const byte C2Pulse = 0x28;
    private const byte C2ManualLow = 0x30;
    private const byte Irq2Flag = 0x40;

    /*
       TITLE: The descriptor registers the W65C21 in the Peripherals category with every logical pin
       GIVEN: nothing
       WHEN: BuildDescriptor is called
       THEN: TypeId, PartName and Category are set, there are 17 pins and D, PA, PB are 8-bit bidirectional
     */
    [Fact]
    public void DescriptorDescribesThePia()
    {
        // ARRANGE:

        // ACT:
        var descriptor = W65C21Pia.BuildDescriptor();

        // ASSERT:
        Assert.Equal("W65C21", descriptor.TypeId);
        Assert.Equal("W65C21", descriptor.PartName);
        Assert.Equal("Peripherals", descriptor.Category);
        Assert.Equal(typeof(W65C21Pia).FullName, descriptor.ClassFullName);
        Assert.Equal(17, descriptor.Pins.Count);
        foreach (var number in new[] { 9, 12, 15 })
        {
            var pin = descriptor.Pins.Single(p => p.Number == number);
            Assert.Equal(8, pin.BitWidth);
            Assert.Equal(PinTypes.DataBi, pin.PinType);
        }
    }

    /*
       TITLE: Create builds a PIA carrying the label property
       GIVEN: a property dictionary whose label is "U2"
       WHEN: Create is called
       THEN: the PIA has label "U2", part name "W65C21" and is sequential
     */
    [Fact]
    public void CreateUsesTheLabelProperty()
    {
        // ARRANGE:
        var properties = W65C21Pia.BuildDescriptor().CreateDefaultProperties();
        properties.SetString(ComponentDescriptor.LabelProperty, "U2");

        // ACT:
        var pia = W65C21Pia.Create(Guid.NewGuid(), properties);

        // ASSERT:
        Assert.Equal("U2", pia.Label);
        Assert.Equal("W65C21", pia.PartName);
        Assert.True(pia.IsSequential);
    }

    /*
       TITLE: A written Control Register reads back over D
       GIVEN: a started PIA bench
       WHEN: 0x15 is written to CRA and CRA is read
       THEN: the read drives 0x15 on D
     */
    [Fact]
    public void ControlRegisterRoundTrip()
    {
        // ARRANGE:
        var bench = new PiaBench();
        bench.Start();

        // ACT:
        bench.Write(PiaBench.Cra, 0x15);
        var value = bench.Read(PiaBench.Cra);

        // ASSERT:
        Assert.Equal((byte)0x15, value);
    }

    /*
       TITLE: D is driven only from the PHI2 rise to the fall of a selected read
       GIVEN: a started PIA bench addressed for a CRA read with PHI2 low
       WHEN: PHI2 rises and then falls, then a CRA write cycle reaches its high phase
       THEN: D is driven only while PHI2 is high during the read
     */
    [Fact]
    public void DataDrivenOnlyDuringReadHighPhase()
    {
        // ARRANGE:
        var bench = new PiaBench();
        bench.Start();
        bench.Address(PiaBench.Cra, read: true);
        var beforeRise = bench.Pia.D.IsDriving;

        // ACT:
        bench.Rise();
        var duringRead = bench.Pia.D.IsDriving;
        bench.Fall();
        var afterFall = bench.Pia.D.IsDriving;
        bench.Address(PiaBench.Cra, read: false);
        bench.Rise();
        var duringWrite = bench.Pia.D.IsDriving;

        // ASSERT:
        Assert.False(beforeRise);
        Assert.True(duringRead);
        Assert.False(afterFall);
        Assert.False(duringWrite);
    }

    /*
       TITLE: Port A drives only its output bits and reads the net on the others
       GIVEN: a PIA bench with a source on PA driving 0xA0 on the high nibble (low nibble high-Z)
       WHEN: DDRA=0x0F and ORA=0x05 are written and Port A is read
       THEN: PA drives 0x05 with the high nibble high-Z, and the read returns 0xA5
     */
    [Fact]
    public void PortMixesOutputAndInputBits()
    {
        // ARRANGE:
        var bench = new PiaBench();
        var peripheral = new SignalSource("pa", 8, 0xA0) { HighZMask = 0x0F };
        bench.Attach(peripheral, y => y.Connect(bench.Pia.PA));
        bench.Start();

        // ACT:
        bench.Write(PiaBench.PortA, 0x0F);
        bench.Write(PiaBench.Cra, DdrAccess);
        bench.Write(PiaBench.PortA, 0x05);
        var value = bench.Read(PiaBench.PortA);

        // ASSERT:
        Assert.Equal(0x05, bench.Pia.PA.Value);
        Assert.Equal(0xF0, bench.Pia.PA.HighZMask & 0xFF);
        Assert.Equal((byte)0xA5, value);
    }

    /*
       TITLE: Floating Port lines read as 1 and the PIA leaves them floating
       GIVEN: a PIA bench with nothing on PA and a fully high-Z source on PB
       WHEN: both Ports are read as inputs
       THEN: both reads return 0xFF and neither Port is driven
     */
    [Fact]
    public void FloatingPortLinesReadOne()
    {
        // ARRANGE:
        var bench = new PiaBench();
        var peripheral = new SignalSource("pb", 8, 0) { HighZ = true };
        bench.Attach(peripheral, y => y.Connect(bench.Pia.PB));
        bench.Start();
        bench.Write(PiaBench.Cra, DdrAccess);
        bench.Write(PiaBench.Crb, DdrAccess);

        // ACT:
        var portA = bench.Read(PiaBench.PortA);
        var portB = bench.Read(PiaBench.PortB);

        // ASSERT:
        Assert.Equal((byte)0xFF, portA);
        Assert.Equal((byte)0xFF, portB);
        Assert.False(bench.Pia.PA.IsDriving);
        Assert.False(bench.Pia.PB.IsDriving);
    }

    /*
       TITLE: A weak pull on a Port line is read unless a normal driver overrides it
       GIVEN: a PIA bench with a weak pull-down on PB and a normal source driving 0x0F on PB's low nibble
       WHEN: Port B is read as input
       THEN: the read returns 0x0F
     */
    [Fact]
    public void NormalDriverOverridesWeakPull()
    {
        // ARRANGE:
        var bench = new PiaBench();
        var pullDown = new SignalSource("pd", 8, 0, DriveStrength.Weak);
        var driver = new SignalSource("drv", 8, 0x0F) { HighZMask = 0xF0 };
        bench.Attach(pullDown, y => y.Connect(bench.Pia.PB));
        bench.Attach(driver, y => y.Connect(bench.Pia.PB));
        bench.Start();
        bench.Write(PiaBench.Crb, DdrAccess);

        // ACT:
        var value = bench.Read(PiaBench.PortB);

        // ASSERT:
        Assert.Equal((byte)0x0F, value);
    }

    /*
       TITLE: IRQAB is open drain: low on an enabled CA1 interrupt, high-Z after Read A Data
       GIVEN: a PIA bench with the CA1 IRQ enabled and DDR Access set
       WHEN: CA1 falls, then Port A is read
       THEN: IRQAB drives 0 after the fall and is high-Z after the read
     */
    [Fact]
    public void IrqIsOpenDrain()
    {
        // ARRANGE:
        var bench = new PiaBench();
        bench.Start();
        bench.Write(PiaBench.Cra, DdrAccess | C1IrqEnable);
        var idle = bench.Pia.IRQAB.IsHighZ;

        // ACT:
        bench.Ca1.Value = 0;
        bench.Settle();
        var asserted = bench.Pia.IRQAB.IsHighZ;
        var level = bench.Pia.IRQAB.Value;
        bench.Read(PiaBench.PortA);

        // ASSERT:
        Assert.True(idle);
        Assert.False(asserted);
        Assert.Equal(0, level);
        Assert.True(bench.Pia.IRQAB.IsHighZ);
    }

    /*
       TITLE: IRQBB follows Side B's CB1 interrupt
       GIVEN: a PIA bench with the CB1 IRQ enabled
       WHEN: CB1 falls
       THEN: IRQBB drives 0
     */
    [Fact]
    public void IrqbFollowsCb1()
    {
        // ARRANGE:
        var bench = new PiaBench();
        bench.Start();
        bench.Write(PiaBench.Crb, C1IrqEnable);

        // ACT:
        bench.Cb1.Value = 0;
        bench.Settle();

        // ASSERT:
        Assert.False(bench.Pia.IRQBB.IsHighZ);
        Assert.Equal(0, bench.Pia.IRQBB.Value);
    }

    /*
       TITLE: CA2 in input mode listens to the net and sets IRQA2 on its falling edge
       GIVEN: a PIA bench with a source driving CA2 high and CA2 in input mode
       WHEN: the source drives CA2 low and CRA is read
       THEN: the PIA does not drive CA2 and CRA bit 6 is set
     */
    [Fact]
    public void Ca2InputSetsFlag()
    {
        // ARRANGE:
        var bench = new PiaBench();
        var source = new SignalSource("ca2", 1, 1);
        bench.Attach(source, y => y.Connect(bench.Pia.CA2));
        bench.Start();

        // ACT:
        source.Value = 0;
        bench.Settle();
        var cra = bench.Read(PiaBench.Cra);

        // ASSERT:
        Assert.False(bench.Pia.CA2.IsDriving);
        Assert.Equal(Irq2Flag, cra & Irq2Flag);
    }

    /*
       TITLE: CA2 in manual output mode drives the pin
       GIVEN: a started PIA bench
       WHEN: CRA selects CA2 manual low
       THEN: CA2 drives 0
     */
    [Fact]
    public void Ca2ManualLowDrives()
    {
        // ARRANGE:
        var bench = new PiaBench();
        bench.Start();

        // ACT:
        bench.Write(PiaBench.Cra, C2ManualLow);

        // ASSERT:
        Assert.True(bench.Pia.CA2.IsDriving);
        Assert.Equal(0, bench.Pia.CA2.Value);
    }

    /*
       TITLE: A CB2 pulse appears on the pin for one cycle after Write B Data
       GIVEN: a PIA bench with CB2 pulse mode and DDR Access set
       WHEN: ORB is written and two PHI2 rises follow
       THEN: CB2 is high after the write, low after the first rise and high after the second
     */
    [Fact]
    public void Cb2PulseOnPin()
    {
        // ARRANGE:
        var bench = new PiaBench();
        bench.Start();
        bench.Write(PiaBench.Crb, C2Pulse | DdrAccess);

        // ACT:
        bench.Write(PiaBench.PortB, 0x55);
        var afterWrite = bench.Pia.CB2.Value;
        bench.Rise();
        var firstRise = bench.Pia.CB2.Value;
        bench.Fall();
        bench.Rise();

        // ASSERT:
        Assert.Equal(1, afterWrite);
        Assert.Equal(0, firstRise);
        Assert.Equal(1, bench.Pia.CB2.Value);
    }

    /*
       TITLE: A floating CS2B reads high and deselects the PIA
       GIVEN: a PIA bench with nothing connected to CS2B
       WHEN: a CRA write and a CRA read run with Debug logging captured
       THEN: no register access is logged and D is never driven
     */
    [Fact]
    public void FloatingCs2BDeselects()
    {
        // ARRANGE:
        var logger = new CapturingLogger(LogLevel.Debug);
        var bench = new PiaBench(connectCs2B: false, logger: logger);
        bench.Start();

        // ACT:
        bench.Write(PiaBench.Cra, 0x15);
        var value = bench.Read(PiaBench.Cra);

        // ASSERT:
        Assert.Null(value);
        Assert.Empty(logger.Messages);
    }

    /*
       TITLE: RESB low resets the PIA through the pin
       GIVEN: a PIA bench with CRA=0x15
       WHEN: RESB is pulsed low and CRA is read
       THEN: the read returns 0
     */
    [Fact]
    public void ResbLowResets()
    {
        // ARRANGE:
        var bench = new PiaBench();
        bench.Start();
        bench.Write(PiaBench.Cra, 0x15);

        // ACT:
        bench.Resb.Value = 0;
        bench.Settle();
        bench.Resb.Value = 1;
        bench.Settle();
        var value = bench.Read(PiaBench.Cra);

        // ASSERT:
        Assert.Equal((byte)0x00, value);
    }

    /*
       TITLE: Initialize starts a fresh engine
       GIVEN: a PIA bench with CRA=0x15
       WHEN: the PIA is initialized again and CRA is read
       THEN: the read returns 0
     */
    [Fact]
    public void InitializeResetsEngine()
    {
        // ARRANGE:
        var bench = new PiaBench();
        bench.Start();
        bench.Write(PiaBench.Cra, 0x15);

        // ACT:
        bench.Pia.Initialize();
        var value = bench.Read(PiaBench.Cra);

        // ASSERT:
        Assert.Equal((byte)0x00, value);
    }

    /*
       TITLE: Register accesses are logged at Debug level
       GIVEN: a PIA bench with a logger capturing Debug and above
       WHEN: 0x15 is written to CRA and CRA is read
       THEN: a write and a read of CRA with $15 are logged
     */
    [Fact]
    public void AccessesLoggedAtDebug()
    {
        // ARRANGE:
        var logger = new CapturingLogger(LogLevel.Debug);
        var bench = new PiaBench(logger: logger);
        bench.Start();

        // ACT:
        bench.Write(PiaBench.Cra, 0x15);
        bench.Read(PiaBench.Cra);

        // ASSERT:
        Assert.Equal(["pia write CRA $15", "pia read CRA $15"], logger.Messages);
    }

    /*
       TITLE: Register accesses are not logged above Debug level
       GIVEN: a PIA bench with a logger capturing Information and above
       WHEN: 0x15 is written to CRA
       THEN: nothing is logged
     */
    [Fact]
    public void AccessesNotLoggedAboveDebug()
    {
        // ARRANGE:
        var logger = new CapturingLogger(LogLevel.Information);
        var bench = new PiaBench(logger: logger);
        bench.Start();

        // ACT:
        bench.Write(PiaBench.Cra, 0x15);

        // ASSERT:
        Assert.Empty(logger.Messages);
    }
}
