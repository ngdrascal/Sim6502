using System.Diagnostics.CodeAnalysis;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class BusStatusPinTests : UnitTestBase
{
    private const byte PinLow = 0;
    private const byte PinHigh = 1;

    /*
       TITLE: A0 reports the A0 pin, not the VPB pin
       GIVEN: a Pins instance with VPB and the address bus set to opposite values in bit 0
       WHEN: A0 is read
       THEN: A0 equals bit 0 of the address bus
     */
    [Theory]
    [InlineData(0x0000, PinHigh, PinLow)]
    [InlineData(0x0001, PinLow, PinHigh)]
    public void A0ReportsTheA0Pin(int address, byte vpb, byte expectedA0)
    {
        // ARRANGE:
        var pins = new Pins { VPB = vpb, AddrBus = new UInt16(address) };

        // ACT:
        var a0 = pins.A0;

        // ASSERT:
        Assert.Equal(expectedA0, a0);
    }

    /*
       TITLE: Each address line getter reports its own bit of the address bus
       GIVEN: a Pins instance whose address bus has only the given bit set
       WHEN: A0 through A15 are read
       THEN: only the getter for that bit is high
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
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    public void AddressLineGettersReportTheirOwnBit(int bit)
    {
        // ARRANGE:
        IPinsExternal pins = new Pins { AddrBus = new UInt16(1 << bit) };
        var expected = Enumerable.Range(0, 16).Select(i => i == bit ? PinHigh : PinLow).ToArray();

        // ACT:
        byte[] actual =
        [
            pins.A0, pins.A1, pins.A2, pins.A3, pins.A4, pins.A5, pins.A6, pins.A7,
            pins.A8, pins.A9, pins.A10, pins.A11, pins.A12, pins.A13, pins.A14, pins.A15
        ];

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    /*
       TITLE: Status outputs start inactive at power-up
       GIVEN: a newly constructed engine
       WHEN: no clock has been applied
       THEN: SYNC is low, VPB is high and MLB is high
     */
    [Fact]
    public void StatusOutputsStartInactive()
    {
        // ARRANGE:

        // ACT:

        // ASSERT:
        Assert.Equal(PinLow, Pins.SYNC);
        Assert.Equal(PinHigh, Pins.VPB);
        Assert.Equal(PinHigh, Pins.MLB);
    }

    /*
       TITLE: VPB is low while the reset vector is read, SYNC goes high on the first fetch
       GIVEN: an engine that has finished its warm-up cycles
       WHEN: the two boot cycles and the first fetch cycle run
       THEN: VPB is low for both boot cycles and high for the fetch, SYNC is high only for the fetch
     */
    [Fact]
    public void VpbIsLowDuringResetVectorPull()
    {
        // ARRANGE:
        WarmUp();
        var vpbAfterWarmUp = Pins.VPB;

        // ACT:
        Pins.DataBus = BootAddr.Lsb();
        ExecuteClockCycles(1); // Boot1
        var boot1 = (Pins.VPB, Pins.SYNC, Pins.AddrBus.ToInt());

        Pins.DataBus = BootAddr.Msb();
        ExecuteClockCycles(1); // Boot2
        var boot2 = (Pins.VPB, Pins.SYNC, Pins.AddrBus.ToInt());

        Pins.DataBus = OpCodes.NOP.ToUInt8();
        ExecuteClockCycles(1); // Fetch
        var fetch = (Pins.VPB, Pins.SYNC, Pins.AddrBus.ToInt());

        // ASSERT:
        Assert.Equal(PinHigh, vpbAfterWarmUp);
        Assert.Equal((PinLow, PinLow, 0xFFFC), boot1);
        Assert.Equal((PinLow, PinLow, 0xFFFD), boot2);
        Assert.Equal((PinHigh, PinHigh, BootAddr.ToInt()), fetch);
    }

    /*
       TITLE: SYNC is high only during the opcode fetch cycle
       GIVEN: an engine booted to BootAddr with LDA immediate at that address
       WHEN: the fetch, operand and next fetch cycles run
       THEN: SYNC is high, low, high
     */
    [Fact]
    public void SyncIsHighOnlyDuringOpcodeFetch()
    {
        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = OpCodes.LDAimm.ToUInt8();
        ExecuteClockCycles(1); // Fetch
        var fetch = Pins.SYNC;

        Pins.DataBus = new UInt8(0x42);
        ExecuteClockCycles(1); // InstLDAimm2
        var operand = Pins.SYNC;

        Pins.DataBus = OpCodes.NOP.ToUInt8();
        ExecuteClockCycles(1); // Fetch
        var nextFetch = Pins.SYNC;

        // ASSERT:
        Assert.Equal(PinHigh, fetch);
        Assert.Equal(PinLow, operand);
        Assert.Equal(PinHigh, nextFetch);
    }

    /*
       TITLE: BRK pulls VPB low only while reading the IRQ/BRK vector
       GIVEN: an engine booted to BootAddr with BRK at that address
       WHEN: the 7 BRK cycles and the next fetch run
       THEN: VPB is low only in cycles 6 and 7, SYNC is high only in cycle 1 and the next fetch
     */
    [Fact]
    public void BrkPullsVpbLowDuringVectorRead()
    {
        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.S = new UInt8(0xFF);
        var vpb = new List<byte>();
        var sync = new List<byte>();

        // ACT:
        Pins.DataBus = OpCodes.BRKimp.ToUInt8();
        for (var cycle = 1; cycle <= 8; cycle++)
        {
            if (cycle > 1)
                Pins.DataBus = new UInt8(0x00);
            ExecuteClockCycles(1);
            vpb.Add(Pins.VPB);
            sync.Add(Pins.SYNC);
        }

        // ASSERT:
        Assert.Equal([PinHigh, PinHigh, PinHigh, PinHigh, PinHigh, PinLow, PinLow, PinHigh], vpb);
        Assert.Equal([PinHigh, PinLow, PinLow, PinLow, PinLow, PinLow, PinLow, PinHigh], sync);
    }

    /*
       TITLE: An NMI raises SYNC on the discarded fetch and pulls VPB low on the vector read
       GIVEN: an engine booted to BootAddr running LDA zeropage with NMIB pulled low after the fetch
       WHEN: the LDA cycles and the 7 interrupt cycles run
       THEN: SYNC is high on the LDA fetch and on Interrupt1; VPB is low only on the last two interrupt cycles
     */
    [Fact]
    public void NmiSequenceDrivesSyncAndVpb()
    {
        // ARRANGE:
        BootToAddress(BootAddr);
        Regs.P.IRQDisabled = High;
        Regs.S = new UInt8(0xFF);
        var vpb = new List<byte>();
        var sync = new List<byte>();

        // ACT:
        Pins.DataBus = OpCodes.LDAzpg.ToUInt8();
        ExecuteClockCycles(1); // Fetch
        vpb.Add(Pins.VPB);
        sync.Add(Pins.SYNC);

        Pins.NMIB = PinLow;
        Pins.DataBus = new UInt8(0x12);
        for (var cycle = 2; cycle <= 10; cycle++) // LDAzpg2, LDAzpg3, Interrupt1, BRKimp2..7
        {
            ExecuteClockCycles(1);
            vpb.Add(Pins.VPB);
            sync.Add(Pins.SYNC);
        }

        // ASSERT:
        Assert.Equal(new UInt16(0xFFFB), Pins.AddrBus);
        Assert.Equal([PinHigh, PinHigh, PinHigh, PinHigh, PinHigh, PinHigh, PinHigh, PinHigh, PinLow, PinLow], vpb);
        Assert.Equal([PinHigh, PinLow, PinLow, PinHigh, PinLow, PinLow, PinLow, PinLow, PinLow, PinLow], sync);
    }

    /*
       TITLE: MLB is low for the read, modify and write cycles of every read-modify-write instruction
       GIVEN: an engine booted to BootAddr with the given RMW opcode at that address and X = 0
       WHEN: every cycle of the instruction and the next fetch run
       THEN: MLB is low for exactly the last three cycles of the instruction and high otherwise
     */
    [Theory]
    [InlineData(OpCodes.ASLzpg, 5)]
    [InlineData(OpCodes.ASLzpgx, 6)]
    [InlineData(OpCodes.ASLabs, 6)]
    [InlineData(OpCodes.ASLabsx, 6)]
    [InlineData(OpCodes.LSRzpg, 5)]
    [InlineData(OpCodes.LSRzpgx, 6)]
    [InlineData(OpCodes.LSRabs, 6)]
    [InlineData(OpCodes.LSRabsx, 6)]
    [InlineData(OpCodes.ROLzpg, 5)]
    [InlineData(OpCodes.ROLzpgx, 6)]
    [InlineData(OpCodes.ROLabs, 6)]
    [InlineData(OpCodes.ROLabsx, 6)]
    [InlineData(OpCodes.RORzpg, 5)]
    [InlineData(OpCodes.RORzpgx, 6)]
    [InlineData(OpCodes.RORabs, 6)]
    [InlineData(OpCodes.RORabsx, 6)]
    [InlineData(OpCodes.INCzpg, 5)]
    [InlineData(OpCodes.INCzpgx, 6)]
    [InlineData(OpCodes.INCabs, 6)]
    [InlineData(OpCodes.INCabsx, 7)]
    [InlineData(OpCodes.DECzpg, 5)]
    [InlineData(OpCodes.DECzpgx, 6)]
    [InlineData(OpCodes.DECabs, 6)]
    [InlineData(OpCodes.DECabsx, 7)]
    [InlineData(OpCodes.TRBzpg, 5)]
    [InlineData(OpCodes.TRBabs, 6)]
    [InlineData(OpCodes.TSBzpg, 5)]
    [InlineData(OpCodes.TSBabs, 6)]
    [InlineData(OpCodes.RMB0zpg, 5)]
    [InlineData(OpCodes.RMB1zpg, 5)]
    [InlineData(OpCodes.RMB2zpg, 5)]
    [InlineData(OpCodes.RMB3zpg, 5)]
    [InlineData(OpCodes.RMB4zpg, 5)]
    [InlineData(OpCodes.RMB5zpg, 5)]
    [InlineData(OpCodes.RMB6zpg, 5)]
    [InlineData(OpCodes.RMB7zpg, 5)]
    [InlineData(OpCodes.SMB0zpg, 5)]
    [InlineData(OpCodes.SMB1zpg, 5)]
    [InlineData(OpCodes.SMB2zpg, 5)]
    [InlineData(OpCodes.SMB3zpg, 5)]
    [InlineData(OpCodes.SMB4zpg, 5)]
    [InlineData(OpCodes.SMB5zpg, 5)]
    [InlineData(OpCodes.SMB6zpg, 5)]
    [InlineData(OpCodes.SMB7zpg, 5)]
    public void MlbIsLowDuringReadModifyWriteCycles(OpCodes opCode, int cycles)
    {
        // ARRANGE:
        BootToAddress(BootAddr);
        var expected = Enumerable.Range(1, cycles + 1)
            .Select(cycle => cycle > cycles - 3 && cycle <= cycles ? PinLow : PinHigh)
            .ToList();
        var mlb = new List<byte>();

        // ACT:
        Pins.DataBus = opCode.ToUInt8();
        for (var cycle = 1; cycle <= cycles + 1; cycle++)
        {
            if (cycle > 1)
                Pins.DataBus = new UInt8(0x00);
            ExecuteClockCycles(1);
            mlb.Add(Pins.MLB);
        }

        // ASSERT:
        Assert.Equal(expected, mlb);
    }

    /*
       TITLE: Status outputs hold their value while RDY is low
       GIVEN: an engine that has just fetched LDA immediate (SYNC high)
       WHEN: RDY is pulled low and a cycle runs
       THEN: SYNC stays high instead of going low for the operand cycle
     */
    [Fact]
    public void StatusOutputsHoldWhileNotReady()
    {
        // ARRANGE:
        BootToAddress(BootAddr);
        Pins.DataBus = OpCodes.LDAimm.ToUInt8();
        ExecuteClockCycles(1); // Fetch

        // ACT:
        Pins.RDY = PinLow;
        ExecuteClockCycles(1); // NotReady

        // ASSERT:
        Assert.Equal(PinHigh, Pins.SYNC);
    }
}
