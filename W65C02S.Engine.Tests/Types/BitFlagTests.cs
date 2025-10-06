using Sim6502.Tests;
using W65C02S.Engine.Types;

namespace W65C02S.Engine.Tests;

public class BitFlagTests : UnitTestBase
{
    ////////////////////////////////////////////////////////////////////////////
    // Constructor
    ////////////////////////////////////////////////////////////////////////////

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    private void TestCtor(bool expected)
    {
        // ARRANGE:

        // ACT:
        var actual = new BitFlag(expected);

        // ASSERT:
        Assert.Equal(expected, actual.GetValue());
    }

    ////////////////////////////////////////////////////////////////////////////
    // Equals
    ////////////////////////////////////////////////////////////////////////////

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void TestEqualsValue(bool leftValue, bool rightValue, bool expected)
    {
        // ARRANGE:
        var left = new BitFlag(leftValue);
        var right = new BitFlag(rightValue);

        // ACT:
        var actual = left.Equals(right);

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestEqualsReference()
    {
        // ARRANGE:
        var left = new UInt8(3);
        var right = left;

        // ACT:
        var actual = left.Equals(right);

        // ASSERT:
        Assert.True(actual);
    }

    [Fact]
    public void TestEqualsNotBitFlag()
    {
        // ARRANGE:
        var left = new BitFlag(false);
        var right = new UInt8(3);

        // ACT:
        // ReSharper disable once SuspiciousTypeConversion.Global
        var actual = left.Equals(right);

        // ASSERT:
        Assert.False(actual);
    }

    ////////////////////////////////////////////////////////////////////////////
    // Equals
    ////////////////////////////////////////////////////////////////////////////
    [Fact]
    public void TestToString()
    {
        // ARRANGE:
        var flag = new BitFlag(true);

        // ACT:
        var actual = flag.ToString();

        // ASSERT:
        Assert.Equal("True", actual);
    }
}