using System.Diagnostics.CodeAnalysis;
using DigisimPlugin.TestHelpers;

namespace W65C22.DigisimPlugin.Tests;

[ExcludeFromCodeCoverage]
public class CpuViaIntegrationTests
{
    private const int ResultAddress = 0x10;
    private const int IsrStart = 0x0300;

    // LDA #$FF / STA $8002 (DDRB) / LDA #$A5 / STA $8000 (ORB) / LDA $8000 (IRB) / STA $10 / STP
    private static readonly byte[] PortBProgram =
    [
        0xA9, 0xFF, 0x8D, 0x02, 0x80, 0xA9, 0xA5, 0x8D, 0x00, 0x80,
        0xAD, 0x00, 0x80, 0x85, 0x10, 0xDB
    ];

    // LDA #$40 / STA $800B (ACR: T1 free-run) / LDA #$C0 / STA $800E (IER: T1)
    // LDA #$40 / STA $8004 (T1C-L) / LDA #$00 / STA $8005 (T1C-H: start) / CLI / wait: JMP wait
    private static readonly byte[] TimerProgram =
    [
        0xA9, 0x40, 0x8D, 0x0B, 0x80, 0xA9, 0xC0, 0x8D, 0x0E, 0x80,
        0xA9, 0x40, 0x8D, 0x04, 0x80, 0xA9, 0x00, 0x8D, 0x05, 0x80,
        0x58, 0x4C, 0x15, 0x02
    ];

    // LDA $8004 (T1C-L read clears IFR6) / INC $12 / RTI
    private static readonly byte[] TimerIsr = [0xAD, 0x04, 0x80, 0xE6, 0x12, 0x40];

    // LDA #$82 / STA $800E (IER: CA1) / CLI / wait: JMP wait
    private static readonly byte[] WaitForCa1Program = [0xA9, 0x82, 0x8D, 0x0E, 0x80, 0x58, 0x4C, 0x06, 0x02];

    // LDA $8001 (IRA read clears the CA1 flag) / STA $11 / INC $12 / RTI
    private static readonly byte[] Ca1Isr = [0xAD, 0x01, 0x80, 0x85, 0x11, 0xE6, 0x12, 0x40];

    /*
       TITLE: The CPU programs Port B as output and reads it back
       GIVEN: a CPU, memory and VIA system running a program that sets DDRB=$FF, ORB=$A5 and reads IRB
       WHEN: the program runs to its end
       THEN: PB drives 0xA5 on every line and memory holds 0xA5
     */
    [Fact]
    public void CpuWritesAndReadsPortB()
    {
        // ARRANGE:
        var bench = new SystemBench();
        bench.Memory.Load(SystemBench.ProgramStart, PortBProgram);
        bench.Start();

        // ACT:
        bench.RunUntil(() => bench.Memory[ResultAddress] == 0xA5);

        // ASSERT:
        Assert.Equal(0xA5, bench.Via.PB.Value);
        Assert.Equal(0, bench.Via.PB.HighZMask & 0xFF);
        Assert.Equal(0xA5, bench.Memory[ResultAddress]);
    }

    /*
       TITLE: T1 in free-run mode interrupts the CPU repeatedly, each ISR acknowledging via T1C-L
       GIVEN: a CPU, memory and VIA system with T1 free-running at $40 and its interrupt enabled
       WHEN: the system runs until the ISR has run three times, recording the cycle of each ISR entry
       THEN: the ISR runs three times, entries are about N+2 = 66 cycles apart (give or take the CPU's
             instruction-boundary latency), and IRQB is high after the last
     */
    [Fact]
    public void T1FreeRunInterruptsCpu()
    {
        // ARRANGE:
        var bench = new SystemBench();
        bench.Memory.Load(SystemBench.ProgramStart, TimerProgram);
        bench.Memory.Load(IsrStart, TimerIsr);
        bench.SetVector(0xFFFE, IsrStart);
        var entries = new List<int>();
        var halfCycles = 0;
        var previous = 0;
        bench.OnHalfCycle = () =>
        {
            halfCycles++;
            if (bench.Memory[0x12] != previous)
            {
                previous = bench.Memory[0x12];
                entries.Add(halfCycles);
            }
        };
        bench.Start();

        // ACT:
        bench.RunUntil(() => bench.Memory[0x12] >= 3);
        bench.RunCycles(10);

        // ASSERT:
        Assert.Equal(3, entries.Count);
        Assert.InRange((entries[1] - entries[0]) / 2, 63, 69);
        Assert.InRange((entries[2] - entries[1]) / 2, 63, 69);
        Assert.Equal(1, bench.Via.IRQB.Value);
    }

    /*
       TITLE: A CA1 interrupt reaches the CPU, and the ISR's IRA read acknowledges it
       GIVEN: a CPU, memory and VIA system with the CA1 interrupt enabled, the CPU waiting with
              interrupts on, and a peripheral driving 0x77 on PA
       WHEN: CA1 falls and the system runs until the ISR has run, then runs on
       THEN: the ISR stored 0x77, ran exactly once, and IRQB is high
     */
    [Fact]
    public void Ca1InterruptRunsIsr()
    {
        // ARRANGE:
        var bench = new SystemBench();
        var peripheral = new SignalSource("pa", 8, 0x77);
        bench.Attach(peripheral, y => y.Connect(bench.Via.PA));
        bench.Memory.Load(SystemBench.ProgramStart, WaitForCa1Program);
        bench.Memory.Load(IsrStart, Ca1Isr);
        bench.SetVector(0xFFFE, IsrStart);
        bench.Start();
        bench.RunCycles(30);

        // ACT:
        bench.Ca1.Value = 0;
        bench.Settle();
        bench.RunUntil(() => bench.Memory[0x12] != 0);
        bench.RunCycles(30);

        // ASSERT:
        Assert.Equal(0x77, bench.Memory[0x11]);
        Assert.Equal(1, bench.Memory[0x12]);
        Assert.Equal(1, bench.Via.IRQB.Value);
    }
}
