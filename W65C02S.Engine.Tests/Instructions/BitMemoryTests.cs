// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class BitMemoryTests : UnitTestBase
{
    private static readonly OpCodes[] RmbOpCodes =
    [
        OpCodes.RMB0zpg, OpCodes.RMB1zpg, OpCodes.RMB2zpg, OpCodes.RMB3zpg,
        OpCodes.RMB4zpg, OpCodes.RMB5zpg, OpCodes.RMB6zpg, OpCodes.RMB7zpg
    ];

    private static readonly OpCodes[] SmbOpCodes =
    [
        OpCodes.SMB0zpg, OpCodes.SMB1zpg, OpCodes.SMB2zpg, OpCodes.SMB3zpg,
        OpCodes.SMB4zpg, OpCodes.SMB5zpg, OpCodes.SMB6zpg, OpCodes.SMB7zpg
    ];

    private record CycleTrace(List<UInt16> Addrs, List<byte> Rwbs, UInt8 Written);

    private byte Flags() => (byte)Regs.P.ToUInt8().ToInt();

    // runs one RMB/SMB on zero page $12 holding memValue, recording the bus on cycles 2-5
    private CycleTrace ExecuteBitMemory(OpCodes opCode, UInt8 memValue)
    {
        var addrs = new List<UInt16>();
        var rwbs = new List<byte>();

        Pins.DataBus = opCode.ToUInt8();
        ExecuteClockCycles(1); // fetch the opcode

        // cycles 2..5: zp operand, read zp, internal (dummy read zp), write zp
        UInt8[] dataPerCycle = [new UInt8(0x12), memValue, memValue, memValue];
        foreach (var data in dataPerCycle)
        {
            Pins.DataBus = data;
            ExecuteClockCycles(1);
            addrs.Add(Pins.AddrBus);
            rwbs.Add(Pins.RWB);
        }

        return new CycleTrace(addrs, rwbs, Pins.DataBus);
    }

    private void AssertBusAndRegisters(CycleTrace trace, OpCodes opCode, byte flagsBefore)
    {
        var read = (byte)High.ToInt();
        var write = (byte)Low.ToInt();

        Assert.Equal(opCode.ToUInt8(), Pins.DBGINST);
        Assert.Equal([new UInt16(0x1001), new UInt16(0x0012), new UInt16(0x0012), new UInt16(0x0012)], trace.Addrs);
        Assert.Equal([read, read, read, write], trace.Rwbs);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(flagsBefore, Flags());
    }

    /*
      TITLE: RMBn clears only bit n of the zero page byte with the datasheet bus activity
      GIVEN: a CPU booted to $1000 and zero page $12 holding $FF
      WHEN: RMBn $12 runs for its 5 cycles, for each n in 0..7
      THEN: cycles 2-5 address $1001, $0012, $0012, $0012 with RWB read, read, read, write;
            the written byte is $FF with bit n clear; PC is $1002 and the flags are unchanged
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
    public void TestRMBzpg(int bit)
    {
        // ARRANGE:
        var opCode = RmbOpCodes[bit];
        var expectedValue = new UInt8(0xFF & ~(1 << bit));
        BootToAddress(BootAddr);
        var flagsBefore = Flags();

        // ACT:
        var trace = ExecuteBitMemory(opCode, new UInt8(0xFF));

        // ASSERT:
        AssertBusAndRegisters(trace, opCode, flagsBefore);
        Assert.Equal(expectedValue, trace.Written);
    }

    /*
      TITLE: SMBn sets only bit n of the zero page byte with the datasheet bus activity
      GIVEN: a CPU booted to $1000 and zero page $12 holding $00
      WHEN: SMBn $12 runs for its 5 cycles, for each n in 0..7
      THEN: cycles 2-5 address $1001, $0012, $0012, $0012 with RWB read, read, read, write;
            the written byte has only bit n set; PC is $1002 and the flags are unchanged
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
    public void TestSMBzpg(int bit)
    {
        // ARRANGE:
        var opCode = SmbOpCodes[bit];
        var expectedValue = new UInt8(1 << bit);
        BootToAddress(BootAddr);
        var flagsBefore = Flags();

        // ACT:
        var trace = ExecuteBitMemory(opCode, new UInt8(0x00));

        // ASSERT:
        AssertBusAndRegisters(trace, opCode, flagsBefore);
        Assert.Equal(expectedValue, trace.Written);
    }

    /*
      TITLE: RMBn and SMBn leave a bit that already has the target value unchanged
      GIVEN: zero page $12 holding $5A
      WHEN: RMB0 (bit 0 already clear) and SMB1 (bit 1 already set) run
      THEN: the byte written back is $5A
    */
    [Theory]
    [InlineData(OpCodes.RMB0zpg)]
    [InlineData(OpCodes.SMB1zpg)]
    public void TestBitAlreadyAtTarget(OpCodes opCode)
    {
        // ARRANGE:
        var memValue = new UInt8(0x5A);
        BootToAddress(BootAddr);
        var flagsBefore = Flags();

        // ACT:
        var trace = ExecuteBitMemory(opCode, memValue);

        // ASSERT:
        AssertBusAndRegisters(trace, opCode, flagsBefore);
        Assert.Equal(memValue, trace.Written);
    }
}
