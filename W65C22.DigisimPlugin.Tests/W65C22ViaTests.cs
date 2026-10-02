using System.Diagnostics.CodeAnalysis;
using Digisim.Components;
using Digisim.Engine;
using Digisim.Shared;
using DigisimPlugin.TestHelpers;
using Microsoft.Extensions.Logging;

namespace W65C22.DigisimPlugin.Tests;

[ExcludeFromCodeCoverage]
public class W65C22ViaTests
{
    private const byte Ca2Flag = 0x01;
    private const byte Ca1Flag = 0x02;
    private const byte SrFlag = 0x04;
    private const byte Cb1Flag = 0x10;
    private const byte IerSet = 0x80;
    private const byte Ca1Rising = 0x01;
    private const byte Ca2ManualLow = 0x0C;
    private const byte Cb2Pulse = 0xA0;
    private const byte SrOutPhi2 = 0x18;
    private const byte SrInExternal = 0x0C;

    /*
       TITLE: The descriptor registers the W65C22 in the Peripherals category with every logical pin
       GIVEN: nothing
       WHEN: BuildDescriptor is called
       THEN: TypeId, PartName and Category are set, there are 17 pins, D, PA, PB are 8-bit
             bidirectional and CB1 is bidirectional
     */
    [Fact]
    public void DescriptorDescribesTheVia()
    {
        // ARRANGE:

        // ACT:
        var descriptor = W65C22Via.BuildDescriptor();

        // ASSERT:
        Assert.Equal("W65C22", descriptor.TypeId);
        Assert.Equal("W65C22", descriptor.PartName);
        Assert.Equal("Peripherals", descriptor.Category);
        Assert.Equal(typeof(W65C22Via).FullName, descriptor.ClassFullName);
        Assert.Equal(17, descriptor.Pins.Count);
        foreach (var number in new[] { 10, 12, 15 })
        {
            var pin = descriptor.Pins.Single(p => p.Number == number);
            Assert.Equal(8, pin.BitWidth);
            Assert.Equal(PinTypes.DataBi, pin.PinType);
        }

        Assert.Equal(PinTypes.DataBi, descriptor.Pins.Single(p => p.Number == 16).PinType);
    }

    /*
       TITLE: Create builds a VIA carrying the label property
       GIVEN: a property dictionary whose label is "U3"
       WHEN: Create is called
       THEN: the VIA has label "U3", part name "W65C22" and is sequential
     */
    [Fact]
    public void CreateUsesTheLabelProperty()
    {
        // ARRANGE:
        var properties = W65C22Via.BuildDescriptor().CreateDefaultProperties();
        properties.SetString(ComponentDescriptor.LabelProperty, "U3");

        // ACT:
        var via = W65C22Via.Create(Guid.NewGuid(), properties);

        // ASSERT:
        Assert.Equal("U3", via.Label);
        Assert.Equal("W65C22", via.PartName);
        Assert.True(via.IsSequential);
    }

    /*
       TITLE: A written register reads back over D, addressed through all four RS lines
       GIVEN: a started VIA bench
       WHEN: 0x15 is written to PCR ($C) and PCR is read
       THEN: the read drives 0x15 on D
     */
    [Fact]
    public void RegisterRoundTrip()
    {
        // ARRANGE:
        var bench = new ViaBench();
        bench.Start();

        // ACT:
        bench.Write(ViaBench.Pcr, 0x15);
        var value = bench.Read(ViaBench.Pcr);

        // ASSERT:
        Assert.Equal((byte)0x15, value);
    }

    /*
       TITLE: D is driven only from the PHI2 rise to the fall of a selected read
       GIVEN: a started VIA bench addressed for a PCR read with PHI2 low
       WHEN: PHI2 rises and then falls, then a PCR write cycle reaches its high phase
       THEN: D is driven only while PHI2 is high during the read
     */
    [Fact]
    public void DataDrivenOnlyDuringReadHighPhase()
    {
        // ARRANGE:
        var bench = new ViaBench();
        bench.Start();
        bench.Address(ViaBench.Pcr, read: true);
        var beforeRise = bench.Via.D.IsDriving;

        // ACT:
        bench.Rise();
        var duringRead = bench.Via.D.IsDriving;
        bench.Fall();
        var afterFall = bench.Via.D.IsDriving;
        bench.Address(ViaBench.Pcr, read: false);
        bench.Rise();
        var duringWrite = bench.Via.D.IsDriving;

        // ASSERT:
        Assert.False(beforeRise);
        Assert.True(duringRead);
        Assert.False(afterFall);
        Assert.False(duringWrite);
    }

    /*
       TITLE: Port A drives only its output bits and reads the net on the others
       GIVEN: a VIA bench with a source on PA driving 0xA0 on the high nibble (low nibble high-Z)
       WHEN: DDRA=0x0F and ORA=0x05 are written and IRA is read
       THEN: PA drives 0x05 with the high nibble high-Z, and the read returns 0xA5
     */
    [Fact]
    public void PortMixesOutputAndInputBits()
    {
        // ARRANGE:
        var bench = new ViaBench();
        var peripheral = new SignalSource("pa", 8, 0xA0) { HighZMask = 0x0F };
        bench.Attach(peripheral, y => y.Connect(bench.Via.PA));
        bench.Start();

        // ACT:
        bench.Write(ViaBench.Ddra, 0x0F);
        bench.Write(ViaBench.Ora, 0x05);
        var value = bench.Read(ViaBench.Ora);

        // ASSERT:
        Assert.Equal(0x05, bench.Via.PA.Value);
        Assert.Equal(0xF0, bench.Via.PA.HighZMask & 0xFF);
        Assert.Equal((byte)0xA5, value);
    }

    /*
       TITLE: Floating Port lines start at 1 and the VIA leaves them floating
       GIVEN: a VIA bench with nothing on PA and a fully high-Z source on PB
       WHEN: both Ports are read as inputs
       THEN: both reads return 0xFF and neither Port is driven
     */
    [Fact]
    public void FloatingPortLinesStartAtOne()
    {
        // ARRANGE:
        var bench = new ViaBench();
        var peripheral = new SignalSource("pb", 8, 0) { HighZ = true };
        bench.Attach(peripheral, y => y.Connect(bench.Via.PB));
        bench.Start();

        // ACT:
        var portA = bench.Read(ViaBench.Ora);
        var portB = bench.Read(ViaBench.Orb);

        // ASSERT:
        Assert.Equal((byte)0xFF, portA);
        Assert.Equal((byte)0xFF, portB);
        Assert.False(bench.Via.PA.IsDriving);
        Assert.False(bench.Via.PB.IsDriving);
    }

    /*
       TITLE: A Port line that stops being driven keeps its last level (bus hold)
       GIVEN: a VIA bench with a source driving 0x5A on PA
       WHEN: the source goes high-Z and IRA is read
       THEN: the read returns 0x5A and the VIA does not drive PA
     */
    [Fact]
    public void FloatingPortLineHoldsLastLevel()
    {
        // ARRANGE:
        var bench = new ViaBench();
        var peripheral = new SignalSource("pa", 8, 0x5A);
        bench.Attach(peripheral, y => y.Connect(bench.Via.PA));
        bench.Start();

        // ACT:
        peripheral.HighZ = true;
        bench.Settle();
        var value = bench.Read(ViaBench.Ora);

        // ASSERT:
        Assert.Equal((byte)0x5A, value);
        Assert.False(bench.Via.PA.IsDriving);
    }

    /*
       TITLE: A weak pull on a Port line is read unless a normal driver overrides it
       GIVEN: a VIA bench with a weak pull-down on PB and a normal source driving 0x0F on PB's low nibble
       WHEN: IRB is read
       THEN: the read returns 0x0F
     */
    [Fact]
    public void NormalDriverOverridesWeakPull()
    {
        // ARRANGE:
        var bench = new ViaBench();
        var pullDown = new SignalSource("pd", 8, 0, DriveStrength.Weak);
        var driver = new SignalSource("drv", 8, 0x0F) { HighZMask = 0xF0 };
        bench.Attach(pullDown, y => y.Connect(bench.Via.PB));
        bench.Attach(driver, y => y.Connect(bench.Via.PB));
        bench.Start();

        // ACT:
        var value = bench.Read(ViaBench.Orb);

        // ASSERT:
        Assert.Equal((byte)0x0F, value);
    }

    /*
       TITLE: IRQB is totem-pole: driven high when idle, low on an enabled CA1 interrupt, high after IRA read
       GIVEN: a VIA bench with the CA1 interrupt enabled
       WHEN: CA1 falls, then IRA is read
       THEN: IRQB drives 1 before, 0 after the fall and 1 after the read, never high-Z
     */
    [Fact]
    public void IrqIsTotemPole()
    {
        // ARRANGE:
        var bench = new ViaBench();
        bench.Start();
        bench.Write(ViaBench.Ier, IerSet | Ca1Flag);
        var idle = bench.Via.IRQB.Value;
        var idleHighZ = bench.Via.IRQB.IsHighZ;

        // ACT:
        bench.Ca1.Value = 0;
        bench.Settle();
        var asserted = bench.Via.IRQB.Value;
        bench.Read(ViaBench.Ora);

        // ASSERT:
        Assert.Equal(1, idle);
        Assert.False(idleHighZ);
        Assert.Equal(0, asserted);
        Assert.Equal(1, bench.Via.IRQB.Value);
        Assert.False(bench.Via.IRQB.IsHighZ);
    }

    /*
       TITLE: A CA1 line that stops being driven keeps its last level and makes no edge
       GIVEN: a VIA bench with CA1 active on rising edges, its source driving CA1 low
       WHEN: the source goes high-Z and IFR is read
       THEN: the CA1 flag is clear
     */
    [Fact]
    public void FloatingCa1HoldsLastLevel()
    {
        // ARRANGE:
        var bench = new ViaBench();
        bench.Start();
        bench.Write(ViaBench.Pcr, Ca1Rising);
        bench.Ca1.Value = 0;
        bench.Settle();

        // ACT:
        bench.Ca1.HighZ = true;
        bench.Settle();
        var ifr = bench.Read(ViaBench.Ifr);

        // ASSERT:
        Assert.Equal(0, ifr & Ca1Flag);
    }

    /*
       TITLE: CA2 in input mode listens to the net and sets its flag on the falling edge
       GIVEN: a VIA bench with a source driving CA2 high and CA2 in input mode
       WHEN: the source drives CA2 low and IFR is read
       THEN: the VIA does not drive CA2 and IFR bit 0 is set
     */
    [Fact]
    public void Ca2InputSetsFlag()
    {
        // ARRANGE:
        var bench = new ViaBench();
        var source = new SignalSource("ca2", 1, 1);
        bench.Attach(source, y => y.Connect(bench.Via.CA2));
        bench.Start();

        // ACT:
        source.Value = 0;
        bench.Settle();
        var ifr = bench.Read(ViaBench.Ifr);

        // ASSERT:
        Assert.False(bench.Via.CA2.IsDriving);
        Assert.Equal(Ca2Flag, ifr & Ca2Flag);
    }

    /*
       TITLE: CA2 in manual output mode drives the pin
       GIVEN: a started VIA bench
       WHEN: PCR selects CA2 manual low
       THEN: CA2 drives 0
     */
    [Fact]
    public void Ca2ManualLowDrives()
    {
        // ARRANGE:
        var bench = new ViaBench();
        bench.Start();

        // ACT:
        bench.Write(ViaBench.Pcr, Ca2ManualLow);

        // ASSERT:
        Assert.True(bench.Via.CA2.IsDriving);
        Assert.Equal(0, bench.Via.CA2.Value);
    }

    /*
       TITLE: A CB2 pulse appears on the pin for one cycle after an ORB write
       GIVEN: a VIA bench with CB2 pulse mode
       WHEN: ORB is written and two PHI2 rises follow
       THEN: CB2 is high after the write, low after the first rise and high after the second
     */
    [Fact]
    public void Cb2PulseOnPin()
    {
        // ARRANGE:
        var bench = new ViaBench();
        bench.Start();
        bench.Write(ViaBench.Pcr, Cb2Pulse);

        // ACT:
        bench.Write(ViaBench.Orb, 0x55);
        var afterWrite = bench.Via.CB2.Value;
        bench.Rise();
        var firstRise = bench.Via.CB2.Value;
        bench.Fall();
        bench.Rise();

        // ASSERT:
        Assert.Equal(1, afterWrite);
        Assert.Equal(0, firstRise);
        Assert.Equal(1, bench.Via.CB2.Value);
    }

    /*
       TITLE: In an internal shift mode CB1 drives the shift clock, and its own edges set the CB1 flag
       GIVEN: a VIA bench with a listener on CB1 and ACR in shift-out-under-PHI2 mode
       WHEN: SR is written and three cycles run
       THEN: CB1 is driven and has been seen low, and IFR shows the CB1 flag
     */
    [Fact]
    public void Cb1DrivesShiftClock()
    {
        // ARRANGE:
        var bench = new ViaBench();
        var listener = new SignalSource("cb1", 1, 0) { HighZ = true };
        bench.Attach(listener, y => y.Connect(bench.Via.CB1));
        bench.Start();
        bench.Write(ViaBench.Acr, SrOutPhi2);

        // ACT:
        bench.Write(ViaBench.Sr, 0x55);
        var levels = new List<long>();
        for (var i = 0; i < 3; i++)
        {
            bench.Cycle();
            levels.Add(bench.Via.CB1.Value);
        }

        var ifr = bench.Read(ViaBench.Ifr);

        // ASSERT:
        Assert.True(bench.Via.CB1.IsDriving);
        Assert.Contains(0L, levels);
        Assert.Equal(Cb1Flag, ifr & Cb1Flag);
    }

    /*
       TITLE: In an external shift mode CB1 is an input clocked by the net
       GIVEN: a VIA bench with a source on CB1 and CB2 and ACR in shift-in-under-CB1 mode
       WHEN: SR is read and eight CB1 pulses carry 1s on CB2
       THEN: CB1 is not driven, SR reads 0xFF and IFR shows the SR flag
     */
    [Fact]
    public void Cb1ExternalShiftClock()
    {
        // ARRANGE:
        var bench = new ViaBench();
        var clock = new SignalSource("cb1", 1, 1);
        var data = new SignalSource("cb2", 1, 1);
        bench.Attach(clock, y => y.Connect(bench.Via.CB1));
        bench.Attach(data, y => y.Connect(bench.Via.CB2));
        bench.Start();
        bench.Write(ViaBench.Acr, SrInExternal);
        bench.Read(ViaBench.Sr);

        // ACT:
        for (var i = 0; i < 8; i++)
        {
            clock.Value = 0;
            bench.Settle();
            bench.Cycle();
            clock.Value = 1;
            bench.Settle();
            bench.Cycle();
        }

        var ifr = bench.Read(ViaBench.Ifr);
        var sr = bench.Read(ViaBench.Sr);

        // ASSERT:
        Assert.False(bench.Via.CB1.IsDriving);
        Assert.Equal(SrFlag, ifr & SrFlag);
        Assert.Equal((byte)0xFF, sr);
    }

    /*
       TITLE: A floating CS2B reads high and deselects the VIA
       GIVEN: a VIA bench with nothing connected to CS2B
       WHEN: a PCR write and a PCR read run with Debug logging captured
       THEN: no register access is logged and D is never driven
     */
    [Fact]
    public void FloatingCs2BDeselects()
    {
        // ARRANGE:
        var logger = new CapturingLogger(LogLevel.Debug);
        var bench = new ViaBench(connectCs2B: false, logger: logger);
        bench.Start();

        // ACT:
        bench.Write(ViaBench.Pcr, 0x15);
        var value = bench.Read(ViaBench.Pcr);

        // ASSERT:
        Assert.Null(value);
        Assert.Empty(logger.Messages);
    }

    /*
       TITLE: RESB low resets the VIA through the pin
       GIVEN: a VIA bench with PCR=0x15
       WHEN: RESB is pulsed low and PCR is read
       THEN: the read returns 0
     */
    [Fact]
    public void ResbLowResets()
    {
        // ARRANGE:
        var bench = new ViaBench();
        bench.Start();
        bench.Write(ViaBench.Pcr, 0x15);

        // ACT:
        bench.Resb.Value = 0;
        bench.Settle();
        bench.Resb.Value = 1;
        bench.Settle();
        var value = bench.Read(ViaBench.Pcr);

        // ASSERT:
        Assert.Equal((byte)0x00, value);
    }

    /*
       TITLE: Initialize starts a fresh engine and resets the bus-hold levels
       GIVEN: a VIA bench with PCR=0x15 and PA held at 0x5A by a source that then went high-Z
       WHEN: the VIA is initialized again and PCR and IRA are read
       THEN: PCR reads 0 and IRA reads 0xFF
     */
    [Fact]
    public void InitializeResetsEngine()
    {
        // ARRANGE:
        var bench = new ViaBench();
        var peripheral = new SignalSource("pa", 8, 0x5A);
        bench.Attach(peripheral, y => y.Connect(bench.Via.PA));
        bench.Start();
        bench.Write(ViaBench.Pcr, 0x15);
        peripheral.HighZ = true;
        bench.Settle();

        // ACT:
        bench.Via.Initialize();
        var pcr = bench.Read(ViaBench.Pcr);
        var ira = bench.Read(ViaBench.Ora);

        // ASSERT:
        Assert.Equal((byte)0x00, pcr);
        Assert.Equal((byte)0xFF, ira);
    }

    /*
       TITLE: Register accesses are logged at Debug level
       GIVEN: a VIA bench with a logger capturing Debug and above
       WHEN: 0x15 is written to PCR and PCR is read
       THEN: a write and a read of PCR with $15 are logged
     */
    [Fact]
    public void AccessesLoggedAtDebug()
    {
        // ARRANGE:
        var logger = new CapturingLogger(LogLevel.Debug);
        var bench = new ViaBench(logger: logger);
        bench.Start();

        // ACT:
        bench.Write(ViaBench.Pcr, 0x15);
        bench.Read(ViaBench.Pcr);

        // ASSERT:
        Assert.Equal(["via write PCR $15", "via read PCR $15"], logger.Messages);
    }

    /*
       TITLE: Register accesses are not logged above Debug level
       GIVEN: a VIA bench with a logger capturing Information and above
       WHEN: 0x15 is written to PCR
       THEN: nothing is logged
     */
    [Fact]
    public void AccessesNotLoggedAboveDebug()
    {
        // ARRANGE:
        var logger = new CapturingLogger(LogLevel.Information);
        var bench = new ViaBench(logger: logger);
        bench.Start();

        // ACT:
        bench.Write(ViaBench.Pcr, 0x15);

        // ASSERT:
        Assert.Empty(logger.Messages);
    }
}
