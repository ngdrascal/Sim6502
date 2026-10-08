// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class NOPTests : UnitTestBase
{
    // Inst: DBGINST after the opcode fetch; Addrs/Rwbs: cycles 2..n; Cycles: instruction length;
    // FetchAddr: address of the next opcode fetch
    private record NopTrace(UInt8 Inst, List<UInt16> Addrs, List<byte> Rwbs, int Cycles, UInt16 FetchAddr);

    private record RegisterSnapshot(UInt8 A, UInt8 X, UInt8 Y, UInt8 P);

    private RegisterSnapshot Snapshot() => new(Regs.A, Regs.X, Regs.Y, Regs.P.ToUInt8());

    // runs one NOP at BootAddr with operand bytes $34, $12, recording the bus until the next opcode fetch
    private NopTrace ExecuteNOP(OpCodes opCode)
    {
        var addrs = new List<UInt16>();
        var rwbs = new List<byte>();

        Pins.DataBus = opCode.ToUInt8();
        ExecuteClockCycles(1); // fetch the opcode
        var inst = Pins.DBGINST;

        UInt8[] operands = [new UInt8(0x34), new UInt8(0x12)];
        var cycles = 1;
        while (cycles < 10)
        {
            Pins.DataBus = cycles <= operands.Length ? operands[cycles - 1] : new UInt8(0x00);
            ExecuteClockCycles(1);
            if (Pins.SYNC == (byte)High.ToInt())
                break;

            addrs.Add(Pins.AddrBus);
            rwbs.Add(Pins.RWB);
            cycles++;
        }

        return new NopTrace(inst, addrs, rwbs, cycles, Pins.AddrBus);
    }

    private void AssertNop(NopTrace trace, OpCodes opCode, int cycles, int fetchAddr, RegisterSnapshot before)
    {
        Assert.Equal(opCode.ToUInt8(), trace.Inst);
        Assert.Equal(cycles, trace.Cycles);
        Assert.Equal(new UInt16(fetchAddr), trace.FetchAddr);
        Assert.All(trace.Rwbs, rwb => Assert.Equal((byte)High.ToInt(), rwb));
        Assert.Equal(before, Snapshot());
    }

    private void ArrangeRegisters(int x)
    {
        BootToAddress(BootAddr);
        Regs.A = new UInt8(0xAA);
        Regs.X = new UInt8(x);
        Regs.Y = new UInt8(0x55);
    }

    /*
      TITLE: The reserved 1-byte NOPs take 1 cycle and change nothing
      GIVEN: a CPU at $1000 with A = $AA, X = $00, Y = $55
      WHEN: a reserved $x3 or $xB opcode runs
      THEN: the next opcode is fetched from $1001 in the following cycle; A, X, Y and P are unchanged
    */
    [Theory]
    [InlineData(OpCodes.NOP03)]
    [InlineData(OpCodes.NOP0B)]
    [InlineData(OpCodes.NOP13)]
    [InlineData(OpCodes.NOP1B)]
    [InlineData(OpCodes.NOP23)]
    [InlineData(OpCodes.NOP2B)]
    [InlineData(OpCodes.NOP33)]
    [InlineData(OpCodes.NOP3B)]
    [InlineData(OpCodes.NOP43)]
    [InlineData(OpCodes.NOP4B)]
    [InlineData(OpCodes.NOP53)]
    [InlineData(OpCodes.NOP5B)]
    [InlineData(OpCodes.NOP63)]
    [InlineData(OpCodes.NOP6B)]
    [InlineData(OpCodes.NOP73)]
    [InlineData(OpCodes.NOP7B)]
    [InlineData(OpCodes.NOP83)]
    [InlineData(OpCodes.NOP8B)]
    [InlineData(OpCodes.NOP93)]
    [InlineData(OpCodes.NOP9B)]
    [InlineData(OpCodes.NOPA3)]
    [InlineData(OpCodes.NOPAB)]
    [InlineData(OpCodes.NOPB3)]
    [InlineData(OpCodes.NOPBB)]
    [InlineData(OpCodes.NOPC3)]
    [InlineData(OpCodes.NOPD3)]
    [InlineData(OpCodes.NOPE3)]
    [InlineData(OpCodes.NOPEB)]
    [InlineData(OpCodes.NOPF3)]
    [InlineData(OpCodes.NOPFB)]
    public void TestOneByteNOP(OpCodes opCode)
    {
        // ARRANGE:
        ArrangeRegisters(0x00);
        var before = Snapshot();

        // ACT:
        var trace = ExecuteNOP(opCode);

        // ASSERT:
        AssertNop(trace, opCode, 1, 0x1001, before);
        Assert.Empty(trace.Addrs);
    }

    /*
      TITLE: The implied NOP ($EA) takes 2 cycles and changes nothing
      GIVEN: a CPU at $1000 with A = $AA, X = $00, Y = $55
      WHEN: NOP runs
      THEN: the next opcode is fetched from $1001 after 2 cycles; A, X, Y and P are unchanged
    */
    [Fact]
    public void TestNOPImplied()
    {
        // ARRANGE:
        ArrangeRegisters(0x00);
        var before = Snapshot();

        // ACT:
        var trace = ExecuteNOP(OpCodes.NOP);

        // ASSERT:
        AssertNop(trace, OpCodes.NOP, 2, 0x1001, before);
    }

    /*
      TITLE: The reserved multi-byte NOPs skip their operands with the W65C02S bus activity (zp,X: dummy read of the zp base)
      GIVEN: a CPU at $1000 with A = $AA, the given X, Y = $55, and operand bytes $34, $12
      WHEN: the reserved NOP runs; cases cover every 2- and 3-byte reserved NOP, and a zp,X case
            where $34 + X wraps past $FF
      THEN: cycles 2..n read the given addresses; the instruction takes the given number of cycles
            and the next opcode is fetched after its operands; A, X, Y and P are unchanged
    */
    [Theory]
    [InlineData(OpCodes.NOP02, 0x00, 2, 0x1002, new[] { 0x1001 })]
    [InlineData(OpCodes.NOP22, 0x00, 2, 0x1002, new[] { 0x1001 })]
    [InlineData(OpCodes.NOP42, 0x00, 2, 0x1002, new[] { 0x1001 })]
    [InlineData(OpCodes.NOP62, 0x00, 2, 0x1002, new[] { 0x1001 })]
    [InlineData(OpCodes.NOP82, 0x00, 2, 0x1002, new[] { 0x1001 })]
    [InlineData(OpCodes.NOPC2, 0x00, 2, 0x1002, new[] { 0x1001 })]
    [InlineData(OpCodes.NOPE2, 0x00, 2, 0x1002, new[] { 0x1001 })]
    [InlineData(OpCodes.NOP44, 0x00, 3, 0x1002, new[] { 0x1001, 0x0034 })]
    [InlineData(OpCodes.NOP54, 0x05, 4, 0x1002, new[] { 0x1001, 0x0034, 0x0039 })]
    [InlineData(OpCodes.NOPD4, 0x05, 4, 0x1002, new[] { 0x1001, 0x0034, 0x0039 })]
    [InlineData(OpCodes.NOPF4, 0x05, 4, 0x1002, new[] { 0x1001, 0x0034, 0x0039 })]
    [InlineData(OpCodes.NOP54, 0xF0, 4, 0x1002, new[] { 0x1001, 0x0034, 0x0024 })]
    [InlineData(OpCodes.NOPDC, 0x05, 4, 0x1003, new[] { 0x1001, 0x1002, 0x1002 })]
    [InlineData(OpCodes.NOPFC, 0x05, 4, 0x1003, new[] { 0x1001, 0x1002, 0x1002 })]
    [InlineData(OpCodes.NOP5C, 0x00, 8, 0x1003, new[] { 0x1001, 0x1002, 0x1003, 0x1003, 0x1003, 0x1003, 0x1003 })]
    public void TestMultiByteNOP(OpCodes opCode, int x, int cycles, int fetchAddr, int[] addrs)
    {
        // ARRANGE:
        ArrangeRegisters(x);
        var before = Snapshot();

        // ACT:
        var trace = ExecuteNOP(opCode);

        // ASSERT:
        AssertNop(trace, opCode, cycles, fetchAddr, before);
        Assert.Equal(addrs.Select(addr => new UInt16(addr)), trace.Addrs);
    }
}
