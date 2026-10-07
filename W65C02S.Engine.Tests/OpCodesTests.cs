using System.Diagnostics.CodeAnalysis;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class OpCodesTests
{
    /*
       TITLE: FromValue maps a defined byte value to its opcode
       GIVEN: the byte value of LDA immediate (0xA9)
       WHEN: FromValue is called with it as an int and as a UInt8
       THEN: both return OpCodes.LDAimm
     */
    [Fact]
    public void FromValueMapsADefinedValue()
    {
        // ARRANGE:
        const int value = 0xA9;

        // ACT:
        var fromInt = OpCodesExtensions.FromValue(value);
        var fromUInt8 = OpCodesExtensions.FromValue(new UInt8(value));

        // ASSERT:
        Assert.Equal(OpCodes.LDAimm, fromInt);
        Assert.Equal(OpCodes.LDAimm, fromUInt8);
    }

    /*
       TITLE: FromValue rejects a value outside the byte range
       GIVEN: the values -1 and 256
       WHEN: FromValue is called
       THEN: ArgumentOutOfRangeException is thrown
     */
    [Theory]
    [InlineData(-1)]
    [InlineData(256)]
    public void FromValueRejectsOutOfRange(int value)
    {
        // ARRANGE:

        // ACT:
        Action act = () => OpCodesExtensions.FromValue(value);

        // ASSERT:
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    /*
       TITLE: FromValue rejects a byte value with no opcode
       GIVEN: a byte value that no OpCodes member has
       WHEN: FromValue is called
       THEN: ArgumentException is thrown
     */
    [Fact]
    public void FromValueRejectsAnUndefinedValue()
    {
        // ARRANGE:
        var undefined = Enumerable.Range(0, 256).First(v => !Enum.IsDefined((OpCodes)v));

        // ACT:
        Action act = () => OpCodesExtensions.FromValue(undefined);

        // ASSERT:
        Assert.Throws<ArgumentException>(act);
    }
}
