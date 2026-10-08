// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class BitBranchTests : UnitTestBase
{
    private static readonly OpCodes[] BbrOpCodes =
    [
        OpCodes.BBR0zpgrel, OpCodes.BBR1zpgrel, OpCodes.BBR2zpgrel, OpCodes.BBR3zpgrel,
        OpCodes.BBR4zpgrel, OpCodes.BBR5zpgrel, OpCodes.BBR6zpgrel, OpCodes.BBR7zpgrel
    ];

    private static readonly OpCodes[] BbsOpCodes =
    [
        OpCodes.BBS0zpgrel, OpCodes.BBS1zpgrel, OpCodes.BBS2zpgrel, OpCodes.BBS3zpgrel,
        OpCodes.BBS4zpgrel, OpCodes.BBS5zpgrel, OpCodes.BBS6zpgrel, OpCodes.BBS7zpgrel
    ];

    // Inst: DBGINST at cycle 5; Addrs/Rwbs: cycles 2-5; Cycles: instruction length;
    // FetchAddr: address of the next opcode fetch
    private record BranchTrace(UInt8 Inst, List<UInt16> Addrs, List<byte> Rwbs, int Cycles, UInt16 FetchAddr);

    private byte Flags() => (byte)Regs.P.ToUInt8().ToInt();

    // runs one BBR/BBS at the current PC on zero page $12 holding memValue, until the next opcode fetch
    private BranchTrace ExecuteBitBranch(OpCodes opCode, UInt8 memValue, UInt8 offset)
    {
        var addrs = new List<UInt16>();
        var rwbs = new List<byte>();

        Pins.DataBus = opCode.ToUInt8();
        ExecuteClockCycles(1); // fetch the opcode

        // cycles 2..5: zp operand, read zp, internal (dummy read zp), offset
        UInt8[] dataPerCycle = [new UInt8(0x12), memValue, memValue, offset];
        foreach (var data in dataPerCycle)
        {
            Pins.DataBus = data;
            ExecuteClockCycles(1);
            addrs.Add(Pins.AddrBus);
            rwbs.Add(Pins.RWB);
        }

        var inst = Pins.DBGINST;

        // optional taken and page-cross cycles, then the next opcode fetch (SYNC high)
        var cycles = 5;
        Pins.DataBus = OpCodes.NOP.ToUInt8();
        ExecuteClockCycles(1);
        while (Pins.SYNC != (byte)High.ToInt() && cycles < 8)
        {
            cycles++;
            ExecuteClockCycles(1);
        }

        return new BranchTrace(inst, addrs, rwbs, cycles, Pins.AddrBus);
    }

    private void AssertFirstFiveCycles(BranchTrace trace, OpCodes opCode, UInt16 pcInit)
    {
        var read = (byte)High.ToInt();

        Assert.Equal(opCode.ToUInt8(), trace.Inst);
        Assert.Equal(
            [pcInit.AddUnsigned(new UInt8(1)), new UInt16(0x0012), new UInt16(0x0012), pcInit.AddUnsigned(new UInt8(2))],
            trace.Addrs);
        Assert.Equal([read, read, read, read], trace.Rwbs);
    }

    /*
      TITLE: BBRn branches when only bit n of the zero page byte is clear
      GIVEN: a CPU at $1000 and zero page $12 holding all ones except bit n
      WHEN: BBRn $12,+$08 runs, for each n in 0..7
      THEN: cycles 2-5 read $1001, $0012, $0012, $1002; the branch takes 6 cycles and the next
            opcode is fetched from $100B; the flags are unchanged
    */
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void TestBBRTaken(int bit)
    {
        // ARRANGE:
        var opCode = BbrOpCodes[bit];
        var pcInit = new UInt16(0x1000);
        BootToAddress(pcInit);
        var flagsBefore = Flags();

        // ACT:
        var trace = ExecuteBitBranch(opCode, new UInt8(0xFF & ~(1 << bit)), new UInt8(0x08));

        // ASSERT:
        AssertFirstFiveCycles(trace, opCode, pcInit);
        Assert.Equal(6, trace.Cycles);
        Assert.Equal(new UInt16(0x100B), trace.FetchAddr);
        Assert.Equal(flagsBefore, Flags());
    }

    /*
      TITLE: BBRn falls through when bit n of the zero page byte is set
      GIVEN: a CPU at $1000 and zero page $12 holding only bit n set
      WHEN: BBRn $12,+$08 runs, for each n in 0..7
      THEN: cycles 2-5 read $1001, $0012, $0012, $1002; the instruction takes 5 cycles and the
            next opcode is fetched from $1003; the flags are unchanged
    */
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void TestBBRNotTaken(int bit)
    {
        // ARRANGE:
        var opCode = BbrOpCodes[bit];
        var pcInit = new UInt16(0x1000);
        BootToAddress(pcInit);
        var flagsBefore = Flags();

        // ACT:
        var trace = ExecuteBitBranch(opCode, new UInt8(1 << bit), new UInt8(0x08));

        // ASSERT:
        AssertFirstFiveCycles(trace, opCode, pcInit);
        Assert.Equal(5, trace.Cycles);
        Assert.Equal(new UInt16(0x1003), trace.FetchAddr);
        Assert.Equal(flagsBefore, Flags());
    }

    /*
      TITLE: BBSn branches when only bit n of the zero page byte is set
      GIVEN: a CPU at $1000 and zero page $12 holding only bit n set
      WHEN: BBSn $12,+$08 runs, for each n in 0..7
      THEN: cycles 2-5 read $1001, $0012, $0012, $1002; the branch takes 6 cycles and the next
            opcode is fetched from $100B; the flags are unchanged
    */
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void TestBBSTaken(int bit)
    {
        // ARRANGE:
        var opCode = BbsOpCodes[bit];
        var pcInit = new UInt16(0x1000);
        BootToAddress(pcInit);
        var flagsBefore = Flags();

        // ACT:
        var trace = ExecuteBitBranch(opCode, new UInt8(1 << bit), new UInt8(0x08));

        // ASSERT:
        AssertFirstFiveCycles(trace, opCode, pcInit);
        Assert.Equal(6, trace.Cycles);
        Assert.Equal(new UInt16(0x100B), trace.FetchAddr);
        Assert.Equal(flagsBefore, Flags());
    }

    /*
      TITLE: BBSn falls through when bit n of the zero page byte is clear
      GIVEN: a CPU at $1000 and zero page $12 holding all ones except bit n
      WHEN: BBSn $12,+$08 runs, for each n in 0..7
      THEN: cycles 2-5 read $1001, $0012, $0012, $1002; the instruction takes 5 cycles and the
            next opcode is fetched from $1003; the flags are unchanged
    */
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void TestBBSNotTaken(int bit)
    {
        // ARRANGE:
        var opCode = BbsOpCodes[bit];
        var pcInit = new UInt16(0x1000);
        BootToAddress(pcInit);
        var flagsBefore = Flags();

        // ACT:
        var trace = ExecuteBitBranch(opCode, new UInt8(0xFF & ~(1 << bit)), new UInt8(0x08));

        // ASSERT:
        AssertFirstFiveCycles(trace, opCode, pcInit);
        Assert.Equal(5, trace.Cycles);
        Assert.Equal(new UInt16(0x1003), trace.FetchAddr);
        Assert.Equal(flagsBefore, Flags());
    }

    /*
      TITLE: A taken BBR/BBS branches relative to the next instruction, adding a cycle on a page cross
      GIVEN: BBR0 (bit 0 clear) or BBS0 (bit 0 set) at the given address; cases cover forward and
             backward offsets with and without a page cross
      WHEN: the instruction runs with the given offset
      THEN: the next opcode is fetched from address + 3 + offset, after 6 cycles (same page)
            or 7 cycles (page cross)
    */
    [Theory]
    [InlineData(OpCodes.BBR0zpgrel, 0x00, 0x1000, 0x08, 0x100B, 6)]
    [InlineData(OpCodes.BBR0zpgrel, 0x00, 0x10F0, 0x10, 0x1103, 7)]
    [InlineData(OpCodes.BBR0zpgrel, 0x00, 0x1080, 0xF0, 0x1073, 6)]
    [InlineData(OpCodes.BBR0zpgrel, 0x00, 0x1000, 0xF0, 0x0FF3, 7)]
    [InlineData(OpCodes.BBS0zpgrel, 0x01, 0x1000, 0x08, 0x100B, 6)]
    [InlineData(OpCodes.BBS0zpgrel, 0x01, 0x10F0, 0x10, 0x1103, 7)]
    [InlineData(OpCodes.BBS0zpgrel, 0x01, 0x1080, 0xF0, 0x1073, 6)]
    [InlineData(OpCodes.BBS0zpgrel, 0x01, 0x1000, 0xF0, 0x0FF3, 7)]
    public void TestBranchTarget(OpCodes opCode, int memValue, int pcInit, int offset, int target, int cycles)
    {
        // ARRANGE:
        var pc = new UInt16(pcInit);
        BootToAddress(pc);

        // ACT:
        var trace = ExecuteBitBranch(opCode, new UInt8(memValue), new UInt8(offset));

        // ASSERT:
        AssertFirstFiveCycles(trace, opCode, pc);
        Assert.Equal(cycles, trace.Cycles);
        Assert.Equal(new UInt16(target), trace.FetchAddr);
    }

    /*
      TITLE: An untaken BBR/BBS takes 5 cycles even when the offset would cross a page
      GIVEN: BBR0 with bit 0 set (not taken) at $10F0 and offset $10
      WHEN: the instruction runs
      THEN: the next opcode is fetched from $10F3 after 5 cycles
    */
    [Fact]
    public void TestNotTakenIgnoresPageCross()
    {
        // ARRANGE:
        var pc = new UInt16(0x10F0);
        BootToAddress(pc);

        // ACT:
        var trace = ExecuteBitBranch(OpCodes.BBR0zpgrel, new UInt8(0x01), new UInt8(0x10));

        // ASSERT:
        AssertFirstFiveCycles(trace, OpCodes.BBR0zpgrel, pc);
        Assert.Equal(5, trace.Cycles);
        Assert.Equal(new UInt16(0x10F3), trace.FetchAddr);
    }

    /*
      TITLE: The instruction-complete event reports both BBR/BBS operands for disassembly
      GIVEN: a CPU at $1000 subscribed to OnInstructionComplete
      WHEN: BBS7 $12,+$08 runs (taken) and the next instruction is fetched
      THEN: the event disassembles it as "1000: BBS7 $12,$08"
    */
    [Fact]
    public void TestInstructionCompleteReportsBothOperands()
    {
        // ARRANGE:
        BootToAddress(BootAddr);
        var lines = new List<string>();
        Engine.OnInstructionComplete += (_, e) =>
            lines.Add(Disassembler.Disassemble(e.Address, e.OpCode, e.Operand1, e.Operand2));

        // ACT:
        ExecuteBitBranch(OpCodes.BBS7zpgrel, new UInt8(0x80), new UInt8(0x08));

        // ASSERT:
        Assert.Contains("1000: BBS7 $12,$08", lines);
    }

    // Addrs/Rwbs/Syncs: one entry per extra (taken, page cross) cycle; Fetch*: the next opcode fetch
    private record BusTrace(List<int> Addrs, List<byte> Rwbs, List<byte> Syncs, int FetchAddr, byte FetchSync);

    // runs a taken BBR/BBS at pc on zero page $12 holding memValue, sampling the bus after each of
    // extraCycles cycles and the fetch that follows
    private BusTrace ExecuteTakenBitBranch(OpCodes opCode, int memValue, int pc, int offset, int extraCycles)
    {
        BootToAddress(new UInt16(pc));

        Pins.DataBus = opCode.ToUInt8();
        ExecuteClockCycles(1); // fetch the opcode

        // cycles 2..5: zp operand, read zp, internal (dummy read zp), offset
        foreach (var data in new[] { 0x12, memValue, memValue, offset })
        {
            Pins.DataBus = new UInt8(data);
            ExecuteClockCycles(1);
        }

        var trace = new BusTrace([], [], [], 0, 0);
        Pins.DataBus = OpCodes.NOP.ToUInt8();
        for (var i = 0; i < extraCycles; i++)
        {
            ExecuteClockCycles(1);
            trace.Addrs.Add(Pins.AddrBus.ToInt());
            trace.Rwbs.Add(Pins.RWB);
            trace.Syncs.Add(Pins.SYNC);
        }

        ExecuteClockCycles(1); // fetch the next opcode

        return trace with { FetchAddr = Pins.AddrBus.ToInt(), FetchSync = Pins.SYNC };
    }

    /*
      TITLE: A taken BBR or BBS dummy-reads the next instruction, again on a page cross
      GIVEN: a CPU at pc and zero page $12 holding a value that makes the branch taken
      WHEN: BBR0 or BBS0 $12 with the given offset runs
      THEN: each extra cycle reads (RWB high, SYNC low) the expected address, and the next opcode
            is fetched (SYNC high) from the branch target
    */
    [Theory]
    [InlineData(OpCodes.BBR0zpgrel, 0xFE, 0x1000, 0x08, new[] { 0x1003 }, 0x100B)]
    [InlineData(OpCodes.BBR0zpgrel, 0xFE, 0x10F0, 0x20, new[] { 0x10F3, 0x10F3 }, 0x1113)]
    [InlineData(OpCodes.BBR0zpgrel, 0xFE, 0x1080, 0xF0, new[] { 0x1083 }, 0x1073)]
    [InlineData(OpCodes.BBR0zpgrel, 0xFE, 0x1000, 0xF0, new[] { 0x1003, 0x1003 }, 0x0FF3)]
    [InlineData(OpCodes.BBS0zpgrel, 0x01, 0x1000, 0x08, new[] { 0x1003 }, 0x100B)]
    [InlineData(OpCodes.BBS0zpgrel, 0x01, 0x10F0, 0x20, new[] { 0x10F3, 0x10F3 }, 0x1113)]
    [InlineData(OpCodes.BBS0zpgrel, 0x01, 0x1080, 0xF0, new[] { 0x1083 }, 0x1073)]
    [InlineData(OpCodes.BBS0zpgrel, 0x01, 0x1000, 0xF0, new[] { 0x1003, 0x1003 }, 0x0FF3)]
    public void TestTakenBitBranchBusActivity(OpCodes opCode, int memValue, int pc, int offset,
                                              int[] expectedAddrs, int target)
    {
        // ARRANGE:
        var read = (byte)High.ToInt();
        var syncLow = (byte)Low.ToInt();

        // ACT:
        var trace = ExecuteTakenBitBranch(opCode, memValue, pc, offset, expectedAddrs.Length);

        // ASSERT:
        Assert.Equal(expectedAddrs, trace.Addrs);
        Assert.All(trace.Rwbs, rwb => Assert.Equal(read, rwb));
        Assert.All(trace.Syncs, sync => Assert.Equal(syncLow, sync));
        Assert.Equal(target, trace.FetchAddr);
        Assert.Equal((byte)High.ToInt(), trace.FetchSync);
    }
}
