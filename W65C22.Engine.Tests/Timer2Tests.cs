using System.Diagnostics.CodeAnalysis;

namespace W65C22.Engine.Tests;

[ExcludeFromCodeCoverage]
public class Timer2Tests : ViaTestBase
{
    private const byte PulseCounting = 0x20;

    /*
       TITLE: A T2C-L write fills the low-order latch and a T2C-L read returns the counter
       GIVEN: a reset VIA
       WHEN: 0x34 is written to T2C-L and T2C-L is read
       THEN: the latch is 0x34 and the read returns the counter low byte 0
     */
    [Fact]
    public void LowWriteFillsLatch()
    {
        // ARRANGE:

        // ACT:
        Write(T2CL, 0x34);
        var value = Read(T2CL);

        // ASSERT:
        Assert.Equal(0x34, Engine.T2LatchLow);
        Assert.Equal(0x00, value);
    }

    /*
       TITLE: Writing T2C-H loads the counter from the value and the low-order latch
       GIVEN: a VIA with 0x34 in the T2 low latch
       WHEN: 0x12 is written to T2C-H, and T2C-L and T2C-H are read
       THEN: the counter was 0x1234 at the load, T2C-L reads 0x34 and T2C-H reads 0x12
     */
    [Fact]
    public void HighWriteLoadsCounter()
    {
        // ARRANGE:
        Write(T2CL, 0x34);

        // ACT:
        Write(T2CH, 0x12);
        var loaded = Engine.T2Counter;
        var low = Read(T2CL);
        var high = Read(T2CH);

        // ASSERT:
        Assert.Equal(0x1234, loaded);
        Assert.Equal(0x34, low);
        Assert.Equal(0x12, high);
    }

    /*
       TITLE: An interval time-out sets IFR5 and pulls IRQB low N+1.5 cycles after the load
       GIVEN: a VIA with the T2 interrupt enabled and T2 loaded with 3
       WHEN: four idle cycles run, then PHI2 rises
       THEN: the flag is clear before the rise; after it the flag is set and IRQB low
     */
    [Fact]
    public void IntervalTimeout()
    {
        // ARRANGE:
        Write(Ier, IerSet | T2Flag);
        LoadT2(3);

        // ACT:
        IdleCycles(4);
        var flagBefore = Engine.IFR;
        Rise();

        // ASSERT:
        Assert.Equal(0x00, flagBefore);
        Assert.Equal(T2Flag, Engine.IFR);
        Assert.False(Pins.IRQB);
    }

    /*
       TITLE: After a time-out T2 rolls over and keeps counting without reloading (Figure 2-3)
       GIVEN: a VIA with T2 loaded with 3
       WHEN: five idle cycles run (3, 2, 1, 0, $FFFF, $FFFE)
       THEN: the counter is $FFFE
     */
    [Fact]
    public void RollsOverWithoutReload()
    {
        // ARRANGE:
        LoadT2(3);

        // ACT:
        IdleCycles(5);

        // ASSERT:
        Assert.Equal(0xFFFE, Engine.T2Counter);
    }

    /*
       TITLE: A later pass through zero sets no new flag until T2C-H is written again
       GIVEN: a VIA whose T2 (N=3) has timed out and whose flag was cleared through IFR
       WHEN: a full 65536-cycle wrap runs
       THEN: IFR5 stays clear
     */
    [Fact]
    public void IntervalFiresOnce()
    {
        // ARRANGE:
        LoadT2(3);
        IdleCycles(5);
        Write(Ifr, T2Flag);

        // ACT:
        IdleCycles(0x10000);

        // ASSERT:
        Assert.Equal(0x00, Engine.IFR);
    }

    /*
       TITLE: Reading T2C-L clears IFR5, reading T2C-H does not
       GIVEN: a VIA whose T2 has timed out
       WHEN: T2C-H is read, then T2C-L is read
       THEN: the flag is still set after the first read and clear after the second
     */
    [Fact]
    public void ReadLowClearsFlag()
    {
        // ARRANGE:
        LoadT2(3);
        IdleCycles(5);

        // ACT:
        Read(T2CH);
        var afterHighRead = Engine.IFR;
        Read(T2CL);

        // ASSERT:
        Assert.Equal(T2Flag, afterHighRead);
        Assert.Equal(0x00, Engine.IFR);
    }

    /*
       TITLE: Writing T2C-H clears IFR5 and re-arms the timer
       GIVEN: a VIA whose T2 (N=3) has timed out
       WHEN: T2C-H is written with 0 and five cycles run
       THEN: the flag is clear just after the write and set again after the new time-out
     */
    [Fact]
    public void ReloadRearms()
    {
        // ARRANGE:
        LoadT2(3);
        IdleCycles(5);

        // ACT:
        Write(T2CH, 0x00);
        var afterWrite = Engine.IFR;
        IdleCycles(5);

        // ASSERT:
        Assert.Equal(0x00, afterWrite);
        Assert.Equal(T2Flag, Engine.IFR);
    }

    /*
       TITLE: In pulse counting mode T2 ignores PHI2 and counts falling PB6 pulses
       GIVEN: a VIA with ACR=0x20 and T2 loaded with 5
       WHEN: ten idle cycles run with PB6 high, then two PB6 pulses are applied
       THEN: the counter is 5 after the idle cycles and 3 after the pulses
     */
    [Fact]
    public void PulseCountingCountsPb6()
    {
        // ARRANGE:
        Write(Acr, PulseCounting);
        LoadT2(5);

        // ACT:
        IdleCycles(10);
        var afterIdle = Engine.T2Counter;
        PulsePb6();
        PulsePb6();

        // ASSERT:
        Assert.Equal(5, afterIdle);
        Assert.Equal(3, Engine.T2Counter);
    }

    /*
       TITLE: PB6 is sampled at PHI2 rises, so a long low pulse counts once
       GIVEN: a VIA counting pulses with T2 loaded with 5
       WHEN: PB6 is held low across four PHI2 rises, then returns high
       THEN: the counter is 4
     */
    [Fact]
    public void LongPulseCountsOnce()
    {
        // ARRANGE:
        Write(Acr, PulseCounting);
        LoadT2(5);

        // ACT:
        Pins.PBIn = 0xBF;
        IdleCycles(4);
        Pins.PBIn = 0xFF;
        IdleCycles(2);

        // ASSERT:
        Assert.Equal(4, Engine.T2Counter);
    }

    /*
       TITLE: A PB6 pulse between two PHI2 rises is not seen
       GIVEN: a VIA counting pulses with T2 loaded with 5, PHI2 low
       WHEN: PB6 falls and rises again before the next PHI2 rise
       THEN: the counter is still 5
     */
    [Fact]
    public void PulseBetweenRisesMissed()
    {
        // ARRANGE:
        Write(Acr, PulseCounting);
        LoadT2(5);

        // ACT:
        Pins.PBIn = 0xBF;
        Engine.Evaluate();
        Pins.PBIn = 0xFF;
        Engine.Evaluate();
        IdleCycles(2);

        // ASSERT:
        Assert.Equal(5, Engine.T2Counter);
    }

    /*
       TITLE: In pulse counting mode IFR5 is set when the count reaches zero (Figure 2-5)
       GIVEN: a VIA counting pulses with the T2 interrupt enabled and T2 loaded with 2
       WHEN: one PB6 pulse is applied, then PB6 falls and PHI2 rises
       THEN: the flag is clear after the first pulse; after the second rise it is set and IRQB low
     */
    [Fact]
    public void PulseCountingInterruptAtZero()
    {
        // ARRANGE:
        Write(Acr, PulseCounting);
        Write(Ier, IerSet | T2Flag);
        LoadT2(2);

        // ACT:
        PulsePb6();
        var afterFirst = Engine.IFR;
        Pins.PBIn = 0xBF;
        Rise();

        // ASSERT:
        Assert.Equal(0x00, afterFirst);
        Assert.Equal(0, Engine.T2Counter);
        Assert.Equal(T2Flag, Engine.IFR);
        Assert.False(Pins.IRQB);
    }

    /*
       TITLE: After reaching zero in pulse counting mode T2 keeps counting but sets no new flag
       GIVEN: a VIA counting pulses whose T2 (N=1) reached zero and whose flag was cleared
       WHEN: two more PB6 pulses are applied
       THEN: the counter is $FFFE and IFR5 stays clear
     */
    [Fact]
    public void PulseCountingFiresOnce()
    {
        // ARRANGE:
        Write(Acr, PulseCounting);
        LoadT2(1);
        PulsePb6();
        Write(Ifr, T2Flag);

        // ACT:
        PulsePb6();
        PulsePb6();

        // ASSERT:
        Assert.Equal(0xFFFE, Engine.T2Counter);
        Assert.Equal(0x00, Engine.IFR);
    }

    /*
       TITLE: PB6 pulses before the first T2C-H write are not counted
       GIVEN: a new VIA with ACR=0x20
       WHEN: two PB6 pulses are applied
       THEN: the counter is still 0
     */
    [Fact]
    public void PulsesIgnoredBeforeLoad()
    {
        // ARRANGE:
        Write(Acr, PulseCounting);

        // ACT:
        PulsePb6();
        PulsePb6();

        // ASSERT:
        Assert.Equal(0, Engine.T2Counter);
    }

    /*
       TITLE: RESB stops T2 but keeps its counter and latch
       GIVEN: a VIA with T2 loaded with 0x1234 and two cycles run
       WHEN: RESB pulses low, then ten idle cycles run
       THEN: the counter is still 0x1232 and the low latch 0x34
     */
    [Fact]
    public void ResetStopsTimer()
    {
        // ARRANGE:
        Write(T2CL, 0x34);
        Write(T2CH, 0x12);
        IdleCycles(2);

        // ACT:
        Pins.RESB = false;
        Engine.Evaluate();
        Pins.RESB = true;
        Engine.Evaluate();
        IdleCycles(10);

        // ASSERT:
        Assert.Equal(0x1232, Engine.T2Counter);
        Assert.Equal(0x34, Engine.T2LatchLow);
    }

    private void LoadT2(ushort value)
    {
        Write(T2CL, (byte)value);
        Write(T2CH, (byte)(value >> 8));
    }

    // PB6 low across one PHI2 rise, then high across the next
    private void PulsePb6()
    {
        Pins.PBIn = 0xBF;
        IdleCycle();
        Pins.PBIn = 0xFF;
        IdleCycle();
    }
}
