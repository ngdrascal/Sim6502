using System.Diagnostics.CodeAnalysis;
using Digisim.Components;
using Digisim.Shared;
using DigisimPlugin.TestHelpers;
using Microsoft.Extensions.Logging;

namespace W65C02S.DigisimPlugin.Tests;

[ExcludeFromCodeCoverage]
public class W65C02SCpuTests
{
    private const int Zp10 = 0x10;

    // LDA #$42 / STA $10 / JMP $0204
    private static readonly byte[] StoreProgram = [0xA9, 0x42, 0x85, 0x10, 0x4C, 0x04, 0x02];

    // LDX #5 / loop: INC $10 / DEX / BNE loop / STP
    private static readonly byte[] CountProgram = [0xA2, 0x05, 0xE6, 0x10, 0xCA, 0xD0, 0xFB, 0xDB];

    // CLV / wait: BVC wait / LDA #$42 / STA $10 / STP
    private static readonly byte[] WaitForOverflowProgram = [0xB8, 0x50, 0xFE, 0xA9, 0x42, 0x85, 0x10, 0xDB];

    /*
       TITLE: The descriptor registers the W65C02S in the Processors category with every logical pin
       GIVEN: nothing
       WHEN: BuildDescriptor is called
       THEN: TypeId, PartName and Category are set, there are 15 pins, A is 16 bits wide and D is 8 bits wide
     */
    [Fact]
    public void DescriptorDescribesTheCpu()
    {
        // ARRANGE:

        // ACT:
        var descriptor = W65C02SCpu.BuildDescriptor();

        // ASSERT:
        Assert.Equal("W65C02S", descriptor.TypeId);
        Assert.Equal("W65C02S", descriptor.PartName);
        Assert.Equal("Processors", descriptor.Category);
        Assert.Equal(typeof(W65C02SCpu).FullName, descriptor.ClassFullName);
        Assert.Equal(15, descriptor.Pins.Count);
        Assert.Equal(16, descriptor.Pins.Single(p => p.Number == 8).BitWidth);
        Assert.Equal(8, descriptor.Pins.Single(p => p.Number == 9).BitWidth);
        Assert.Equal(PinTypes.DataBi, descriptor.Pins.Single(p => p.Number == 9).PinType);
    }

    /*
       TITLE: Create builds a CPU carrying the label property
       GIVEN: a property dictionary whose label is "U1"
       WHEN: Create is called
       THEN: the CPU has label "U1", part name "W65C02S" and is sequential
     */
    [Fact]
    public void CreateUsesTheLabelProperty()
    {
        // ARRANGE:
        var properties = W65C02SCpu.BuildDescriptor().CreateDefaultProperties();
        properties.SetString(ComponentDescriptor.LabelProperty, "U1");

        // ACT:
        var cpu = W65C02SCpu.Create(Guid.NewGuid(), properties);

        // ASSERT:
        Assert.Equal("U1", cpu.Label);
        Assert.Equal("W65C02S", cpu.PartName);
        Assert.True(cpu.IsSequential);
    }

    /*
       TITLE: Booting reads the reset vector with VPB low, then fetches from it with SYNC high
       GIVEN: a CPU on a clock and memory whose reset vector points at $0200
       WHEN: the clock runs until the CPU puts $0200 on A
       THEN: $FFFC and $FFFD are read with VPB low and SYNC low, and $0200 is read with SYNC high and VPB high
     */
    [Fact]
    public void BootReadsResetVectorThenFetches()
    {
        // ARRANGE:
        var bench = new CpuBench();
        bench.Memory.Load(CpuBench.ProgramStart, StoreProgram);
        bench.Start();

        // ACT:
        bench.RunUntil(() => bench.Cpu.A.Value == CpuBench.ProgramStart);

        // ASSERT:
        var cycles = bench.Samples.Where(s => !s.Phi2).ToList();
        var lsb = cycles.Single(s => s.Address == 0xFFFC);
        var msb = cycles.Single(s => s.Address == 0xFFFD);
        var fetch = cycles.Last();
        Assert.Equal((0L, 0L, 1L), (lsb.Vpb, lsb.Sync, lsb.Rwb));
        Assert.Equal((0L, 0L, 1L), (msb.Vpb, msb.Sync, msb.Rwb));
        Assert.Equal((1L, 1L, (long)CpuBench.ProgramStart), (fetch.Vpb, fetch.Sync, fetch.Address));
    }

    /*
       TITLE: A program reads operands from memory and writes a result back
       GIVEN: LDA #$42 / STA $10 at $0200
       WHEN: the clock runs
       THEN: memory $10 becomes $42
     */
    [Fact]
    public void ProgramStoresToMemory()
    {
        // ARRANGE:
        var bench = new CpuBench();
        bench.Memory.Load(CpuBench.ProgramStart, StoreProgram);
        bench.Start();

        // ACT:
        bench.RunUntil(() => bench.Memory[Zp10] == 0x42);

        // ASSERT:
        Assert.Equal(0x42, bench.Memory[Zp10]);
    }

    /*
       TITLE: A loop of read-modify-write instructions runs to completion and pulls MLB low
       GIVEN: LDX #5 / INC $10 / DEX / BNE back / STP at $0200
       WHEN: 100 cycles run
       THEN: memory $10 is 5 and MLB was low on some cycles, only while INC was writing or reading $10
     */
    [Fact]
    public void ReadModifyWriteLoopCountsAndLocksTheBus()
    {
        // ARRANGE:
        var bench = new CpuBench();
        bench.Memory.Load(CpuBench.ProgramStart, CountProgram);
        bench.Start();

        // ACT:
        bench.RunCycles(100);

        // ASSERT:
        Assert.Equal(5, bench.Memory[Zp10]);
        var locked = bench.Samples.Where(s => s.Mlb == 0).ToList();
        Assert.NotEmpty(locked);
        Assert.All(locked, s => Assert.Equal(Zp10, s.Address));
    }

    /*
       TITLE: D is driven only during the PHI2-high half of a write cycle
       GIVEN: the INC loop program, which writes memory repeatedly
       WHEN: 100 cycles run
       THEN: D was driven at least once, and every time it was driven PHI2 was high and RWB was low
     */
    [Fact]
    public void DataBusDrivenOnlyWhileWritingWithPhi2High()
    {
        // ARRANGE:
        var bench = new CpuBench();
        bench.Memory.Load(CpuBench.ProgramStart, CountProgram);
        bench.Start();

        // ACT:
        bench.RunCycles(100);

        // ASSERT:
        var driven = bench.Samples.Where(s => s.DataDriven).ToList();
        Assert.NotEmpty(driven);
        Assert.All(driven, s => Assert.True(s.Phi2 && s.Rwb == 0));
    }

    /*
       TITLE: Control inputs left unconnected read as inactive
       GIVEN: a CPU whose RESB, IRQB, NMIB, RDY and BE are not connected
       WHEN: the store program runs
       THEN: it boots and memory $10 becomes $42
     */
    [Fact]
    public void FloatingControlInputsAreInactive()
    {
        // ARRANGE:
        var bench = new CpuBench(connectControls: false);
        bench.Memory.Load(CpuBench.ProgramStart, StoreProgram);
        bench.Start();

        // ACT:
        bench.RunUntil(() => bench.Memory[Zp10] == 0x42);

        // ASSERT:
        Assert.Equal(0x42, bench.Memory[Zp10]);
    }

    /*
       TITLE: Holding RESB low restarts the CPU through the reset vector
       GIVEN: the store program has already stored $42 to $10, and $10 is then cleared
       WHEN: RESB is held low for 3 cycles and released
       THEN: the CPU reads $FFFC with VPB low again and stores $42 to $10 again
     */
    [Fact]
    public void ResetRestartsTheProgram()
    {
        // ARRANGE:
        var bench = new CpuBench();
        bench.Memory.Load(CpuBench.ProgramStart, StoreProgram);
        bench.Start();
        bench.RunUntil(() => bench.Memory[Zp10] == 0x42);
        bench.Memory[Zp10] = 0;
        var before = bench.Samples.Count;

        // ACT:
        bench.Resb.Value = 0;
        bench.RunCycles(3);
        bench.Resb.Value = 1;
        bench.RunUntil(() => bench.Memory[Zp10] == 0x42);

        // ASSERT:
        Assert.Contains(bench.Samples.Skip(before), s => s.Address == 0xFFFC && s.Vpb == 0);
        Assert.Equal(0x42, bench.Memory[Zp10]);
    }

    /*
       TITLE: A low IRQB with interrupts enabled runs the IRQ handler
       GIVEN: CLI / JMP self at $0200 and an IRQ handler at $0300 that stores $99 to $20
       WHEN: the CPU is running the loop and IRQB is pulled low
       THEN: memory $20 becomes $99
     */
    [Fact]
    public void IrqRunsHandler()
    {
        // ARRANGE:
        var bench = new CpuBench();
        bench.Memory.Load(CpuBench.ProgramStart, 0x58, 0x4C, 0x01, 0x02);
        bench.Memory.Load(0x0300, 0xA9, 0x99, 0x85, 0x20, 0xDB);
        bench.SetVector(0xFFFE, 0x0300);
        bench.Start();
        bench.RunCycles(20);

        // ACT:
        bench.Irqb.Value = 0;
        bench.RunUntil(() => bench.Memory[0x20] == 0x99);

        // ASSERT:
        Assert.Equal(0x99, bench.Memory[0x20]);
    }

    /*
       TITLE: A falling NMIB runs the NMI handler
       GIVEN: JMP self at $0200 and an NMI handler at $0310 that stores $77 to $21
       WHEN: the CPU is running the loop and NMIB is pulled low
       THEN: memory $21 becomes $77 and $FFFA was read with VPB low
     */
    [Fact]
    public void NmiRunsHandler()
    {
        // ARRANGE:
        var bench = new CpuBench();
        bench.Memory.Load(CpuBench.ProgramStart, 0x4C, 0x00, 0x02);
        bench.Memory.Load(0x0310, 0xA9, 0x77, 0x85, 0x21, 0xDB);
        bench.SetVector(0xFFFA, 0x0310);
        bench.Start();
        bench.RunCycles(20);

        // ACT:
        bench.Nmib.Value = 0;
        bench.RunUntil(() => bench.Memory[0x21] == 0x77);

        // ASSERT:
        Assert.Equal(0x77, bench.Memory[0x21]);
        Assert.Contains(bench.Samples, s => s.Address == 0xFFFA && s.Vpb == 0);
    }

    /*
       TITLE: RDY low stalls the CPU and RDY high lets it continue
       GIVEN: the store program, stopped on its first fetch
       WHEN: RDY is held low for 5 cycles, then released
       THEN: A does not change while RDY is low, and the program then stores $42 to $10
     */
    [Fact]
    public void RdyLowStallsTheCpu()
    {
        // ARRANGE:
        var bench = new CpuBench();
        bench.Memory.Load(CpuBench.ProgramStart, StoreProgram);
        bench.Start();
        bench.RunUntil(() => bench.Cpu.A.Value == CpuBench.ProgramStart);

        // ACT:
        bench.Rdy.Value = 0;
        bench.RunCycles(5);
        var stalled = bench.Samples.TakeLast(10).Select(s => s.Address).Distinct().ToList();
        bench.Rdy.Value = 1;
        bench.RunUntil(() => bench.Memory[Zp10] == 0x42);

        // ASSERT:
        Assert.Single(stalled);
        Assert.Equal(0x42, bench.Memory[Zp10]);
    }

    /*
       TITLE: A falling SOB sets V, releasing a BVC wait loop
       GIVEN: a CPU spinning in CLV / BVC * with SOB high
       WHEN: SOB is pulled low
       THEN: the loop exits and the following STA writes $42 to $10
     */
    [Fact]
    public void FallingSobReleasesBvcWaitLoop()
    {
        // ARRANGE:
        var bench = new CpuBench();
        bench.Memory.Load(CpuBench.ProgramStart, WaitForOverflowProgram);
        bench.Start();
        bench.RunCycles(30);
        var storedWhileWaiting = bench.Memory[Zp10];

        // ACT:
        bench.Sob.Value = 0;
        bench.RunUntil(() => bench.Memory[Zp10] == 0x42);

        // ASSERT:
        Assert.Equal(0, storedWhileWaiting);
        Assert.Equal(0x42, bench.Memory[Zp10]);
    }

    /*
       TITLE: BE low floats A, RWB and D; BE high drives them again
       GIVEN: a running CPU with a valid address on A
       WHEN: BE is pulled low and the model settles, then BE is returned high
       THEN: while BE is low A and RWB are high-Z and D is not driven; afterwards A and RWB are driven
     */
    [Fact]
    public void BusEnableLowFloatsTheBus()
    {
        // ARRANGE:
        var bench = new CpuBench();
        bench.Memory.Load(CpuBench.ProgramStart, StoreProgram);
        bench.Start();
        bench.RunCycles(10);

        // ACT:
        bench.Be.Value = 0;
        bench.Settle();
        var floated = (bench.Cpu.A.IsHighZ, bench.Cpu.RWB.IsHighZ, bench.Cpu.D.IsDriving);
        bench.Be.Value = 1;
        bench.Settle();
        var driven = (bench.Cpu.A.IsHighZ, bench.Cpu.RWB.IsHighZ);

        // ASSERT:
        Assert.Equal((true, true, false), floated);
        Assert.Equal((false, false), driven);
    }

    /*
       TITLE: PHI1O is the inverse of PHI2 and PHI2O follows PHI2
       GIVEN: a started CPU
       WHEN: PHI2 goes high, then low
       THEN: PHI1O/PHI2O are 0/1 while PHI2 is high and 1/0 while it is low
     */
    [Fact]
    public void ClockOutputsFollowPhi2()
    {
        // ARRANGE:
        var bench = new CpuBench();
        bench.Start();

        // ACT:
        bench.HalfCycle(true);
        var high = (bench.Cpu.PHI1O.Value, bench.Cpu.PHI2O.Value);
        bench.HalfCycle(false);
        var low = (bench.Cpu.PHI1O.Value, bench.Cpu.PHI2O.Value);

        // ASSERT:
        Assert.Equal((0L, 1L), high);
        Assert.Equal((1L, 0L), low);
    }

    /*
       TITLE: Initialize replaces the engine, so the CPU boots again
       GIVEN: a CPU that has already run the store program
       WHEN: Initialize is called and the clock runs on
       THEN: the CPU reads the reset vector at $FFFC again
     */
    [Fact]
    public void InitializeRestartsTheEngine()
    {
        // ARRANGE:
        var bench = new CpuBench();
        bench.Memory.Load(CpuBench.ProgramStart, StoreProgram);
        bench.Start();
        bench.RunUntil(() => bench.Memory[Zp10] == 0x42);
        var before = bench.Samples.Count;

        // ACT:
        bench.Cpu.Initialize();
        bench.RunCycles(10);

        // ASSERT:
        Assert.Contains(bench.Samples.Skip(before), s => s.Address == 0xFFFC && s.Vpb == 0);
    }

    /*
       TITLE: With Debug logging enabled each completed instruction is traced as a disassembly line
       GIVEN: a CPU whose logger is enabled at Debug
       WHEN: the store program runs until $10 is written
       THEN: the log holds an LDA and an STA line naming the CPU label
     */
    [Fact]
    public void TracesInstructionsAtDebug()
    {
        // ARRANGE:
        var logger = new CapturingLogger(LogLevel.Debug);
        var bench = new CpuBench(logger: logger);
        bench.Memory.Load(CpuBench.ProgramStart, StoreProgram);
        bench.Start();

        // ACT:
        bench.RunUntil(() => bench.Memory[Zp10] == 0x42);
        bench.RunCycles(2);

        // ASSERT:
        Assert.Contains(logger.Messages, m => m.StartsWith("cpu") && m.Contains("LDA"));
        Assert.Contains(logger.Messages, m => m.Contains("STA"));
    }

    /*
       TITLE: Without Debug logging no instruction is traced
       GIVEN: a CPU whose logger is enabled at Information only
       WHEN: the store program runs until $10 is written
       THEN: nothing is logged
     */
    [Fact]
    public void DoesNotTraceBelowDebug()
    {
        // ARRANGE:
        var logger = new CapturingLogger(LogLevel.Information);
        var bench = new CpuBench(logger: logger);
        bench.Memory.Load(CpuBench.ProgramStart, StoreProgram);
        bench.Start();

        // ACT:
        bench.RunUntil(() => bench.Memory[Zp10] == 0x42);

        // ASSERT:
        Assert.Empty(logger.Messages);
    }
}
