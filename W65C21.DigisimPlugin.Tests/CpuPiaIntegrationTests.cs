using System.Diagnostics.CodeAnalysis;
using DigisimPlugin.TestHelpers;

namespace W65C21.DigisimPlugin.Tests;

[ExcludeFromCodeCoverage]
public class CpuPiaIntegrationTests
{
    private const int ResultAddress = 0x10;
    private const int IsrStart = 0x0300;

    // LDA #$FF / STA $8002 (DDRB) / LDA #$2C / STA $8003 (CRB: CB2 pulse, DDR Access)
    // LDA #$A5 / STA $8002 (ORB) / LDA #$01 / STA $10 / STP
    private static readonly byte[] PortBOutputProgram =
    [
        0xA9, 0xFF, 0x8D, 0x02, 0x80, 0xA9, 0x2C, 0x8D, 0x03, 0x80,
        0xA9, 0xA5, 0x8D, 0x02, 0x80, 0xA9, 0x01, 0x85, 0x10, 0xDB
    ];

    // LDA #$04 / STA $8001 (CRA: DDR Access) / LDA $8000 (Port A) / STA $10 / STP
    private static readonly byte[] PortAInputProgram =
        [0xA9, 0x04, 0x8D, 0x01, 0x80, 0xAD, 0x00, 0x80, 0x85, 0x10, 0xDB];

    // LDA #$05 / STA $8001 (CRA: DDR Access, CA1 IRQ enabled, falling edge) / CLI / wait: JMP wait
    private static readonly byte[] WaitForIrqProgram =
        [0xA9, 0x05, 0x8D, 0x01, 0x80, 0x58, 0x4C, 0x06, 0x02];

    // LDA $8000 (Read A Data clears the flag) / STA $11 / INC $12 / RTI
    private static readonly byte[] IsrProgram = [0xAD, 0x00, 0x80, 0x85, 0x11, 0xE6, 0x12, 0x40];

    /*
       TITLE: The CPU programs Port B as output and strobes CB2
       GIVEN: a CPU, memory and PIA system running a program that sets DDRB, CB2 pulse mode and ORB
       WHEN: the program runs to its end
       THEN: PB drives 0xA5 on every line and CB2 pulsed low and returned high
     */
    [Fact]
    public void CpuWritesPortBAndPulsesCb2()
    {
        // ARRANGE:
        var bench = new SystemBench();
        bench.Memory.Load(SystemBench.ProgramStart, PortBOutputProgram);
        var cb2Levels = new List<long>();
        bench.OnHalfCycle = () => cb2Levels.Add(bench.Pia.CB2.Value);
        bench.Start();

        // ACT:
        bench.RunUntil(() => bench.Memory[ResultAddress] == 0x01);

        // ASSERT:
        Assert.Equal(0xA5, bench.Pia.PB.Value);
        Assert.Equal(0, bench.Pia.PB.HighZMask & 0xFF);
        Assert.Contains(0L, cb2Levels);
        Assert.Equal(1, bench.Pia.CB2.Value);
    }

    /*
       TITLE: The CPU reads the Port A pins
       GIVEN: a CPU, memory and PIA system with a peripheral driving 0x3C on PA
       WHEN: a program sets DDR Access on Side A, reads Port A and stores the result
       THEN: memory holds 0x3C
     */
    [Fact]
    public void CpuReadsPortA()
    {
        // ARRANGE:
        var bench = new SystemBench();
        var peripheral = new SignalSource("pa", 8, 0x3C);
        bench.Attach(peripheral, y => y.Connect(bench.Pia.PA));
        bench.Memory.Load(SystemBench.ProgramStart, PortAInputProgram);
        bench.Memory[ResultAddress] = 0xFF;
        bench.Start();

        // ACT:
        bench.RunUntil(() => bench.Memory[ResultAddress] != 0xFF);

        // ASSERT:
        Assert.Equal(0x3C, bench.Memory[ResultAddress]);
    }

    /*
       TITLE: A CA1 interrupt reaches the CPU, and the ISR's Port A read acknowledges it
       GIVEN: a CPU, memory and PIA system with the CA1 IRQ enabled, the CPU waiting with interrupts on,
              and a peripheral driving 0x77 on PA
       WHEN: CA1 falls and the system runs until the ISR has run, then runs on
       THEN: the ISR stored 0x77, ran exactly once, and IRQAB is released
     */
    [Fact]
    public void Ca1InterruptRunsIsr()
    {
        // ARRANGE:
        var bench = new SystemBench();
        var peripheral = new SignalSource("pa", 8, 0x77);
        bench.Attach(peripheral, y => y.Connect(bench.Pia.PA));
        bench.Memory.Load(SystemBench.ProgramStart, WaitForIrqProgram);
        bench.Memory.Load(IsrStart, IsrProgram);
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
        Assert.True(bench.Pia.IRQAB.IsHighZ);
    }
}
