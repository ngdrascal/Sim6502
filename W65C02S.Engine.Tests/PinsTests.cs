using System.Diagnostics.CodeAnalysis;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class PinsTests
{
    /*
       TITLE: Each data line getter reports its own bit of the data bus
       GIVEN: a Pins instance whose data bus has only the given bit set
       WHEN: D0 through D7 are read
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
    public void DataLineGettersReportTheirBit(int bit)
    {
        // ARRANGE:
        var pins = new Pins { DataBus = new UInt8(1 << bit) };

        // ACT:
        byte[] lines = [pins.D0, pins.D1, pins.D2, pins.D3, pins.D4, pins.D5, pins.D6, pins.D7];

        // ASSERT:
        for (var i = 0; i < lines.Length; i++)
            Assert.Equal(i == bit ? 1 : 0, lines[i]);
    }

    /*
       TITLE: Setting the data line pins builds the data bus value
       GIVEN: a Pins instance with the data bus at 0
       WHEN: D0 through D7 are set to the bits of 0xA5
       THEN: the data bus reads 0xA5
     */
    [Fact]
    public void DataLineSettersBuildTheDataBus()
    {
        // ARRANGE:
        var pins = new Pins();

        // ACT:
        pins.D0 = 1;
        pins.D1 = 0;
        pins.D2 = 1;
        pins.D3 = 0;
        pins.D4 = 0;
        pins.D5 = 1;
        pins.D6 = 0;
        pins.D7 = 1;

        // ASSERT:
        Assert.Equal(new UInt8(0xA5), pins.DataBus);
    }

    /*
       TITLE: Clearing a data line pin clears only that bit of the data bus
       GIVEN: a Pins instance with the data bus at 0xFF
       WHEN: D3 is set low
       THEN: the data bus reads 0xF7
     */
    [Fact]
    public void ClearingADataLineClearsOnlyItsBit()
    {
        // ARRANGE:
        var pins = new Pins { DataBus = new UInt8(0xFF) };

        // ACT:
        pins.D3 = 0;

        // ASSERT:
        Assert.Equal(new UInt8(0xF7), pins.DataBus);
    }
}
