using System.Diagnostics.CodeAnalysis;

namespace W65C22.Engine.Tests;

[ExcludeFromCodeCoverage]
public class Timer1Tests : ViaTestBase
{
    private const byte OneShot = 0x00;
    private const byte FreeRun = 0x40;
    private const byte Pb7Output = 0x80;

    /*
       TITLE: Latch writes fill the T1 latches without touching the counter
       GIVEN: a reset VIA
       WHEN: 0x34 is written to T1C-L and 0x12 to T1L-H, and T1L-L and T1L-H are read
       THEN: the latch is 0x1234, the reads return 0x34 and 0x12, and the counter is still 0
     */
    [Fact]
    public void LatchWritesAndReads()
    {
        // ARRANGE:

        // ACT:
        Write(T1CL, 0x34);
        Write(T1LH, 0x12);
        var low = Read(T1LL);
        var high = Read(T1LH);

        // ASSERT:
        Assert.Equal(0x1234, Engine.T1Latch);
        Assert.Equal(0x34, low);
        Assert.Equal(0x12, high);
        Assert.Equal(0x0000, Engine.T1Counter);
    }

    /*
       TITLE: A T1L-L write is the same as a T1C-L write
       GIVEN: a reset VIA
       WHEN: 0x56 is written to T1L-L
       THEN: the latch low byte is 0x56
     */
    [Fact]
    public void LatchLowRegisterWritesLatch()
    {
        // ARRANGE:

        // ACT:
        Write(T1LL, 0x56);

        // ASSERT:
        Assert.Equal(0x0056, Engine.T1Latch);
    }

    /*
       TITLE: Writing T1C-H loads the counter from the latches at the PHI2 fall
       GIVEN: a VIA with 0x34 in the T1 low latch
       WHEN: 0x12 is written to T1C-H
       THEN: the latch and the counter are both 0x1234
     */
    [Fact]
    public void CounterHighWriteLoadsCounter()
    {
        // ARRANGE:
        Write(T1CL, 0x34);

        // ACT:
        Write(T1CH, 0x12);

        // ASSERT:
        Assert.Equal(0x1234, Engine.T1Latch);
        Assert.Equal(0x1234, Engine.T1Counter);
    }

    /*
       TITLE: The counter decrements at each PHI2 fall and reads return its value at the rise
       GIVEN: a VIA with T1 just loaded with 0x1234
       WHEN: T1C-L is read, then T1C-H is read, then two idle cycles run
       THEN: T1C-L reads 0x34 (no fall yet), T1C-H reads 0x12, and the counter ends at 0x1230
     */
    [Fact]
    public void CounterDecrementsEachFall()
    {
        // ARRANGE:
        Write(T1CL, 0x34);
        Write(T1CH, 0x12);

        // ACT:
        var low = Read(T1CL);
        var high = Read(T1CH);
        IdleCycles(2);

        // ASSERT:
        Assert.Equal(0x34, low);
        Assert.Equal(0x12, high);
        Assert.Equal(0x1230, Engine.T1Counter);
    }

    /*
       TITLE: A one-shot time-out sets IFR6 and pulls IRQB low N+1.5 cycles after the load (Figure 2-3)
       GIVEN: a VIA with the T1 interrupt enabled and T1 loaded with 3 at a PHI2 fall
       WHEN: four idle cycles run (counter 3 to 0 to $FFFF), then PHI2 rises
       THEN: the flag is clear and IRQB high before the rise; after it the flag is set and IRQB low
     */
    [Fact]
    public void OneShotTimeout()
    {
        // ARRANGE:
        Write(Ier, IerSet | T1Flag);
        LoadT1(3);

        // ACT:
        IdleCycles(4);
        var counterBefore = Engine.T1Counter;
        var flagBefore = Engine.IFR;
        var irqBefore = Pins.IRQB;
        Rise();

        // ASSERT:
        Assert.Equal(0xFFFF, counterBefore);
        Assert.Equal(0x00, flagBefore);
        Assert.True(irqBefore);
        Assert.Equal(T1Flag, Engine.IFR);
        Assert.False(Pins.IRQB);
    }

    /*
       TITLE: After a one-shot time-out the counter reloads from the latch but sets no new flag
       GIVEN: a VIA whose one-shot T1 (N=3) has timed out and whose flag was cleared through IFR
       WHEN: twenty idle cycles run
       THEN: IFR6 stays clear and the counter keeps counting from the reloaded latch
     */
    [Fact]
    public void OneShotFiresOnce()
    {
        // ARRANGE:
        LoadT1(3);
        IdleCycles(5);
        Write(Ifr, T1Flag);

        // ACT:
        IdleCycles(20);

        // ASSERT:
        Assert.Equal(0x00, Engine.IFR);
        Assert.True(Engine.T1Counter <= 3);
    }

    /*
       TITLE: The fall after the 0 to $FFFF transition reloads the counter from the latch
       GIVEN: a VIA with one-shot T1 loaded with 3
       WHEN: five idle cycles run
       THEN: the counter is 3 again
     */
    [Fact]
    public void CounterReloadsAfterTimeout()
    {
        // ARRANGE:
        LoadT1(3);

        // ACT:
        IdleCycles(5);

        // ASSERT:
        Assert.Equal(3, Engine.T1Counter);
    }

    /*
       TITLE: Free-run mode times out every N+2 cycles after the first N+1.5 (Figure 2-4)
       GIVEN: a VIA with ACR in free-run with PB7 output and T1 loaded with 3
       WHEN: sixteen cycles run, recording PB7 after each PHI2 rise
       THEN: PB7 toggles at rises 5, 10 and 15 counted from the load
     */
    [Fact]
    public void FreeRunPeriod()
    {
        // ARRANGE:
        Write(Acr, FreeRun | Pb7Output);
        LoadT1(3);
        var toggles = new List<int>();
        var level = Pb7Level();

        // ACT:
        for (var rise = 1; rise <= 16; rise++)
        {
            Rise();
            if (Pb7Level() != level)
                toggles.Add(rise);
            level = Pb7Level();
            Fall();
        }

        // ASSERT:
        Assert.Equal([5, 10, 15], toggles);
    }

    /*
       TITLE: Free-run mode sets IFR6 at every time-out
       GIVEN: a VIA in free-run with T1 loaded with 3 and its first time-out flag cleared through IFR
       WHEN: cycles run until the next time-out
       THEN: IFR6 is set again
     */
    [Fact]
    public void FreeRunRepeatsInterrupt()
    {
        // ARRANGE:
        Write(Acr, FreeRun);
        LoadT1(3);
        IdleCycles(5);
        Write(Ifr, T1Flag);

        // ACT:
        IdleCycles(5);

        // ASSERT:
        Assert.Equal(T1Flag, Engine.IFR);
    }

    /*
       TITLE: In one-shot PB7 mode PB7 goes low at the load and high at the time-out, then stays high
       GIVEN: a VIA with ACR=0x80 and DDRB=0
       WHEN: T1 is loaded with 3, four cycles run, PHI2 rises, then a further twenty cycles run
       THEN: PB7 is driven low after the load, high after the time-out rise, and still high at the end
     */
    [Fact]
    public void OneShotPb7Pulse()
    {
        // ARRANGE:
        Write(Acr, OneShot | Pb7Output);

        // ACT:
        LoadT1(3);
        var afterLoad = Pb7Level();
        var driven = Pins.PBDrive;
        IdleCycles(4);
        Rise();
        var afterTimeout = Pb7Level();
        Fall();
        IdleCycles(20);

        // ASSERT:
        Assert.Equal(0x80, driven);
        Assert.False(afterLoad);
        Assert.True(afterTimeout);
        Assert.True(Pb7Level());
    }

    /*
       TITLE: With ACR bit 7 clear PB7 follows DDRB and ORB
       GIVEN: a VIA with ACR=0, DDRB=0x80 and ORB=0x80
       WHEN: T1 is loaded with 3
       THEN: PB7 is driven high from ORB
     */
    [Fact]
    public void Pb7FollowsOrbWithoutTimerOutput()
    {
        // ARRANGE:
        Write(Ddrb, 0x80);
        Write(Orb, 0x80);

        // ACT:
        LoadT1(3);

        // ASSERT:
        Assert.Equal(0x80, Pins.PBDrive);
        Assert.True(Pb7Level());
    }

    /*
       TITLE: With ACR bit 7 set an IRB read returns the T1 PB7 level in bit 7
       GIVEN: a VIA with ACR=0x80, ORB=0x80 and T1 just loaded (PB7 low), PB pins all high
       WHEN: IRB is read
       THEN: bit 7 reads 0
     */
    [Fact]
    public void IrbReadsTimerPb7()
    {
        // ARRANGE:
        Write(Acr, Pb7Output);
        Write(Orb, 0x80);
        LoadT1(100);

        // ACT:
        var value = Read(Orb);

        // ASSERT:
        Assert.Equal(0x7F, value);
    }

    /*
       TITLE: Reading T1C-L clears IFR6, reading T1L-L does not
       GIVEN: a VIA whose one-shot T1 has timed out
       WHEN: T1L-L is read, then T1C-L is read
       THEN: the flag is still set after the first read and clear after the second
     */
    [Fact]
    public void ReadCounterLowClearsFlag()
    {
        // ARRANGE:
        LoadT1(3);
        IdleCycles(5);

        // ACT:
        Read(T1LL);
        var afterLatchRead = Engine.IFR;
        Read(T1CL);

        // ASSERT:
        Assert.Equal(T1Flag, afterLatchRead);
        Assert.Equal(0x00, Engine.IFR);
    }

    /*
       TITLE: Writing T1L-H clears IFR6 without reloading the counter
       GIVEN: a VIA whose one-shot T1 has timed out
       WHEN: 0x01 is written to T1L-H
       THEN: the flag is clear, the latch is 0x0103 and the counter was not loaded from the latch
     */
    [Fact]
    public void WriteLatchHighClearsFlag()
    {
        // ARRANGE:
        LoadT1(3);
        IdleCycles(5);

        // ACT:
        Write(T1LH, 0x01);

        // ASSERT:
        Assert.Equal(0x00, Engine.IFR);
        Assert.Equal(0x0103, Engine.T1Latch);
        Assert.True(Engine.T1Counter < 0x0100);
    }

    /*
       TITLE: Writing T1C-H clears IFR6 and re-arms the one-shot
       GIVEN: a VIA whose one-shot T1 (N=3) has timed out
       WHEN: T1C-H is written again and five cycles run
       THEN: the flag is clear just after the write and set again after the new time-out
     */
    [Fact]
    public void ReloadRearmsOneShot()
    {
        // ARRANGE:
        LoadT1(3);
        IdleCycles(5);

        // ACT:
        Write(T1CH, 0x00);
        var afterWrite = Engine.IFR;
        IdleCycles(5);

        // ASSERT:
        Assert.Equal(0x00, afterWrite);
        Assert.Equal(T1Flag, Engine.IFR);
    }

    /*
       TITLE: Reloading T1 before it reaches zero prevents the time-out
       GIVEN: a VIA with one-shot T1 loaded with 3
       WHEN: T1C-H is rewritten every three cycles for thirty cycles
       THEN: IFR6 is never set
     */
    [Fact]
    public void RetriggerPreventsTimeout()
    {
        // ARRANGE:
        LoadT1(3);

        // ACT:
        for (var i = 0; i < 10; i++)
        {
            IdleCycles(2);
            Write(T1CH, 0x00);
        }

        // ASSERT:
        Assert.Equal(0x00, Engine.IFR);
    }

    /*
       TITLE: A new VIA does not count T1 before its first load
       GIVEN: a new VIA
       WHEN: ten idle cycles run
       THEN: the counter stays 0 and IFR6 stays clear
     */
    [Fact]
    public void IdleBeforeFirstLoad()
    {
        // ARRANGE:

        // ACT:
        IdleCycles(10);

        // ASSERT:
        Assert.Equal(0x0000, Engine.T1Counter);
        Assert.Equal(0x00, Engine.IFR);
    }

    /*
       TITLE: RESB stops T1 but keeps its counter and latch
       GIVEN: a VIA with T1 loaded with 0x1234 and two cycles run
       WHEN: RESB pulses low, then ten idle cycles run
       THEN: the counter is still 0x1232 and the latch 0x1234
     */
    [Fact]
    public void ResetStopsTimer()
    {
        // ARRANGE:
        Write(T1CL, 0x34);
        Write(T1CH, 0x12);
        IdleCycles(2);

        // ACT:
        Pins.RESB = false;
        Engine.Evaluate();
        Pins.RESB = true;
        Engine.Evaluate();
        IdleCycles(10);

        // ASSERT:
        Assert.Equal(0x1232, Engine.T1Counter);
        Assert.Equal(0x1234, Engine.T1Latch);
    }

    private void LoadT1(ushort value)
    {
        Write(T1CL, (byte)value);
        Write(T1CH, (byte)(value >> 8));
    }

    private bool Pb7Level()
    {
        return (Pins.PBOut & 0x80) != 0;
    }
}
