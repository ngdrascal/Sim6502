// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using UInt8 = W65C02S.Engine.Types.UInt8;
using UInt16 = W65C02S.Engine.Types.UInt16;

namespace W65C02S.Engine.Tests;

// Bus activity of the read-modify-write modify cycle. It re-reads EA, per the SingleStepTests 65x02
// wdc65c02 vectors and the W65C02S datasheet ("two read and one write cycle" at the effective
// address). MAME's ow65c02s.lst reads EA + 1 for zp, zp,X and abs,X instead.
[ExcludeFromCodeCoverage]
public class RmwBusActivityTests : UnitTestBase
{
    private record BusCycle(int Addr, byte Rwb, byte Mlb);

    // runs one instruction at $1000 with X = x, sampling the bus after each cycle up to and including
    // the first write cycle
    private List<BusCycle> ExecuteUntilWrite(OpCodes opCode, int x, int[] operands)
    {
        BootToAddress(new UInt16(0x1000));
        Regs.X = new UInt8(x);

        var cycles = new List<BusCycle>();
        var data = new Queue<int>([opCode.ToUInt8().ToInt(), .. operands]);
        var write = (byte)Low.ToInt();
        while (cycles.Count < 8)
        {
            // opcode and operands, then the memory byte for every later read
            Pins.DataBus = new UInt8(data.Count > 0 ? data.Dequeue() : 0x55);
            ExecuteClockCycles(1);
            cycles.Add(new BusCycle(Pins.AddrBus.ToInt(), Pins.RWB, Pins.MLB));
            if (Pins.RWB == write)
                break;
        }

        return cycles;
    }

    /*
      TITLE: The read-modify-write modify cycle re-reads the effective address
      GIVEN: a CPU at $1000 with X set
      WHEN: the read-modify-write instruction runs until its write cycle
      THEN: the cycle before the write reads EA (RWB high) with MLB low, and the write goes to EA
            with MLB low; zp does not wrap to EA + 1 and abs,X does not read EA + 1
    */
    [Theory]
    // zp
    [InlineData(OpCodes.ASLzpg, 0, new[] { 0x40 }, 0x0040)]
    [InlineData(OpCodes.LSRzpg, 0, new[] { 0x40 }, 0x0040)]
    [InlineData(OpCodes.ROLzpg, 0, new[] { 0x40 }, 0x0040)]
    [InlineData(OpCodes.RORzpg, 0, new[] { 0x40 }, 0x0040)]
    [InlineData(OpCodes.INCzpg, 0, new[] { 0x40 }, 0x0040)]
    [InlineData(OpCodes.DECzpg, 0, new[] { 0x40 }, 0x0040)]
    [InlineData(OpCodes.TRBzpg, 0, new[] { 0x40 }, 0x0040)]
    [InlineData(OpCodes.TSBzpg, 0, new[] { 0x40 }, 0x0040)]
    [InlineData(OpCodes.RMB0zpg, 0, new[] { 0x40 }, 0x0040)]
    [InlineData(OpCodes.SMB0zpg, 0, new[] { 0x40 }, 0x0040)]
    // zp,X
    [InlineData(OpCodes.ASLzpgx, 5, new[] { 0x40 }, 0x0045)]
    [InlineData(OpCodes.LSRzpgx, 5, new[] { 0x40 }, 0x0045)]
    [InlineData(OpCodes.ROLzpgx, 5, new[] { 0x40 }, 0x0045)]
    [InlineData(OpCodes.RORzpgx, 5, new[] { 0x40 }, 0x0045)]
    [InlineData(OpCodes.INCzpgx, 5, new[] { 0x40 }, 0x0045)]
    [InlineData(OpCodes.DECzpgx, 5, new[] { 0x40 }, 0x0045)]
    // abs
    [InlineData(OpCodes.ASLabs, 0, new[] { 0x34, 0x12 }, 0x1234)]
    [InlineData(OpCodes.LSRabs, 0, new[] { 0x34, 0x12 }, 0x1234)]
    [InlineData(OpCodes.ROLabs, 0, new[] { 0x34, 0x12 }, 0x1234)]
    [InlineData(OpCodes.RORabs, 0, new[] { 0x34, 0x12 }, 0x1234)]
    [InlineData(OpCodes.INCabs, 0, new[] { 0x34, 0x12 }, 0x1234)]
    [InlineData(OpCodes.DECabs, 0, new[] { 0x34, 0x12 }, 0x1234)]
    [InlineData(OpCodes.TRBabs, 0, new[] { 0x34, 0x12 }, 0x1234)]
    [InlineData(OpCodes.TSBabs, 0, new[] { 0x34, 0x12 }, 0x1234)]
    // abs,X
    [InlineData(OpCodes.ASLabsx, 5, new[] { 0x34, 0x12 }, 0x1239)]
    [InlineData(OpCodes.LSRabsx, 5, new[] { 0x34, 0x12 }, 0x1239)]
    [InlineData(OpCodes.ROLabsx, 5, new[] { 0x34, 0x12 }, 0x1239)]
    [InlineData(OpCodes.RORabsx, 5, new[] { 0x34, 0x12 }, 0x1239)]
    [InlineData(OpCodes.INCabsx, 5, new[] { 0x34, 0x12 }, 0x1239)]
    [InlineData(OpCodes.DECabsx, 5, new[] { 0x34, 0x12 }, 0x1239)]
    // edges: zp at $FF, zp,X wrapping past $FF, abs,X with EA + 1 on the next page
    [InlineData(OpCodes.ASLzpg, 0, new[] { 0xFF }, 0x00FF)]
    [InlineData(OpCodes.ASLzpgx, 5, new[] { 0xFE }, 0x0003)]
    [InlineData(OpCodes.ASLabsx, 5, new[] { 0xFA, 0x12 }, 0x12FF)]
    [InlineData(OpCodes.INCabsx, 5, new[] { 0xFA, 0x12 }, 0x12FF)]
    public void TestModifyCycleReadsEffectiveAddress(OpCodes opCode, int x, int[] operands, int ea)
    {
        // ARRANGE:
        var read = (byte)High.ToInt();
        var write = (byte)Low.ToInt();
        var mlbLow = (byte)Low.ToInt();

        // ACT:
        var cycles = ExecuteUntilWrite(opCode, x, operands);

        // ASSERT:
        Assert.Equal(new BusCycle(ea, write, mlbLow), cycles[^1]);
        Assert.Equal(new BusCycle(ea, read, mlbLow), cycles[^2]);
    }

    /*
      TITLE: abs,X shifts take the extra PC+2 dummy cycle only on a page cross; INC/DEC always do
      GIVEN: a CPU at $1000 with X = 5
      WHEN: the abs,X read-modify-write instruction runs until its write cycle
      THEN: the bus reads the expected addresses, then writes EA; MLB is low for exactly the read,
            modify and write cycles
    */
    [Theory]
    [InlineData(OpCodes.ASLabsx, 0x1234, new[] { 0x1000, 0x1001, 0x1002, 0x1239, 0x1239, 0x1239 })]
    [InlineData(OpCodes.ASLabsx, 0x12FD, new[] { 0x1000, 0x1001, 0x1002, 0x1002, 0x1302, 0x1302, 0x1302 })]
    [InlineData(OpCodes.LSRabsx, 0x1234, new[] { 0x1000, 0x1001, 0x1002, 0x1239, 0x1239, 0x1239 })]
    [InlineData(OpCodes.LSRabsx, 0x12FD, new[] { 0x1000, 0x1001, 0x1002, 0x1002, 0x1302, 0x1302, 0x1302 })]
    [InlineData(OpCodes.ROLabsx, 0x1234, new[] { 0x1000, 0x1001, 0x1002, 0x1239, 0x1239, 0x1239 })]
    [InlineData(OpCodes.ROLabsx, 0x12FD, new[] { 0x1000, 0x1001, 0x1002, 0x1002, 0x1302, 0x1302, 0x1302 })]
    [InlineData(OpCodes.RORabsx, 0x1234, new[] { 0x1000, 0x1001, 0x1002, 0x1239, 0x1239, 0x1239 })]
    [InlineData(OpCodes.RORabsx, 0x12FD, new[] { 0x1000, 0x1001, 0x1002, 0x1002, 0x1302, 0x1302, 0x1302 })]
    [InlineData(OpCodes.INCabsx, 0x1234, new[] { 0x1000, 0x1001, 0x1002, 0x1002, 0x1239, 0x1239, 0x1239 })]
    [InlineData(OpCodes.INCabsx, 0x12FD, new[] { 0x1000, 0x1001, 0x1002, 0x1002, 0x1302, 0x1302, 0x1302 })]
    [InlineData(OpCodes.DECabsx, 0x1234, new[] { 0x1000, 0x1001, 0x1002, 0x1002, 0x1239, 0x1239, 0x1239 })]
    [InlineData(OpCodes.DECabsx, 0x12FD, new[] { 0x1000, 0x1001, 0x1002, 0x1002, 0x1302, 0x1302, 0x1302 })]
    public void TestAbsXCycles(OpCodes opCode, int baseAddr, int[] expectedAddrs)
    {
        // ARRANGE:
        var read = (byte)High.ToInt();
        var write = (byte)Low.ToInt();
        var count = expectedAddrs.Length;
        var expected = expectedAddrs
            .Select((addr, i) => new BusCycle(addr, i == count - 1 ? write : read,
                                              (byte)(i >= count - 3 ? Low : High).ToInt()))
            .ToList();

        // ACT:
        var cycles = ExecuteUntilWrite(opCode, 5, [baseAddr & 0xFF, baseAddr >> 8]);

        // ASSERT:
        Assert.Equal(expected, cycles);
    }
}
