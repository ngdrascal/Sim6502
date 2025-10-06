using System.Diagnostics.CodeAnalysis;
using Sim6502.Tests;
using W65C02S.Engine.Types;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class UInt16Tests : UnitTestBase
{
    ////////////////////////////////////////////////////////////////////////////
    // Constructor
    ////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void TestCtorReturnsCorrectValueZero()
    {
        // ARRANGE:
        const int expected = 0;

        // ACT:
        var test = new UInt16();
        var actual = test.ToInt();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestCtorReturnsCorrectValueInt()
    {
        // ARRANGE:
        const int expected = 1234;

        // ACT:
        var test = new UInt16(expected);
        var actual = test.ToInt();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestCtorReturnsCorrectValueUInt8()
    {
        // ARRANGE:
        const int expected = 12;
        var lsb = new UInt8(expected);

        // ACT:
        var test = new UInt16(lsb);
        var actual = test.ToInt();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestCtorReturnsCorrectValueUInt8UInt8()
    {
        // ARRANGE:
        const int expected = 1234;
        var lsb = new UInt8(expected & 0x000000FF);
        var msb = new UInt8(expected >> 8 & 0x000000FF);

        // ACT:
        var test = new UInt16(lsb, msb);
        var actual = test.ToInt();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    ////////////////////////////////////////////////////////////////////////////
    // Implicit conversion
    ////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void TestImplicitConversionFromInt()
    {
        // ARRANGE:
        const int expected = 1234;

        // ACT:
        UInt16 test = expected; // Implicit conversion from int to UInt16
        var actual = test.ToInt();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestImplicitConversionToInt()
    {
        // ARRANGE:
        const int initial = 1234;
        var test = new UInt16(initial);

        // ACT:
        int actual = test; // Implicit conversion from UInt16 to int

        // ASSERT:
        Assert.Equal(initial, actual);
    }

    ////////////////////////////////////////////////////////////////////////////
    // Copy
    ////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void TestCopyHasCorrectValue()
    {
        // ARRANGE:
        const int expected = 1234;

        // ACT:
        var original = new UInt16(expected);
        var copy = original.Copy();
        var actual = copy.ToInt();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    ////////////////////////////////////////////////////////////////////////////
    // Equals
    ////////////////////////////////////////////////////////////////////////////

    [Theory]
    [InlineData(3, 3, true)]
    [InlineData(3, 4, false)]
    [InlineData(4, 3, false)]
    public void TestEqualsValue(byte leftValue, byte rightValue, bool expected)
    {
        // ARRANGE:
        var left = new UInt16(leftValue);
        var right = new UInt16(rightValue);

        // ACT:
        var actual = left.Equals(right);

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestEqualsReference()
    {
        // ARRANGE:
        var left = new UInt16(3);
        var right = left;

        // ACT:
        var actual = left.Equals(right);

        // ASSERT:
        Assert.True(actual);
    }

    [Fact]
    public void TestEqualsNotUInt16()
    {
        // ARRANGE:
        var left = new UInt16(3);
        var right = new BitFlag(false);

        // ACT:
        // ReSharper disable once SuspiciousTypeConversion.Global
        var actual = left.Equals(right);

        // ASSERT:
        Assert.False(actual);
    }

    ////////////////////////////////////////////////////////////////////////////
    // EqualsZero
    ////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void TestEqualsZeroWhenZero()
    {
        // ARRANGE:
        var test = new UInt16(0);

        // ACT:
        var actual = test.EqualsZero();

        // ASSERT:
        Assert.True(actual);
    }

    [Fact]
    public void TestEqualZeroWhenNotZero()
    {
        // ARRANGE:
        var test = new UInt16(1);

        // ACT:
        var actual = test.EqualsZero();

        // ASSERT:
        Assert.False(actual);
    }

    ////////////////////////////////////////////////////////////////////////////
    // ToInt
    ////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void TestToIntReturnCorrectValue()
    {
        // ARRANGE:
        const int expected = 1234;

        // ACT:
        var instance = new UInt16(expected);
        var actual = instance.ToInt();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    ////////////////////////////////////////////////////////////////////////////
    // UpdateValue
    ////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void TestUpdateSetsNewValueUInt16()
    {
        // ARRANGE:
        const int expected = 1234;
        var instance = new UInt16(expected + 1);

        // ACT:
        instance.UpdateValue(new UInt16(expected));
        var actual = instance.ToInt();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestUpdateValueOutOfRange()
    {
        // ARRANGE:
        var value = new UInt16(0);

        // ACT:
        void TooLow() => value.UpdateValue(-1);
        void TooHigh() => value.UpdateValue(UInt16.Max + 1);

        // ASSERT:
        Assert.Throws<ArgumentOutOfRangeException>(TooLow);
        Assert.Throws<ArgumentOutOfRangeException>(TooHigh);
    }

    ////////////////////////////////////////////////////////////////////////////
    // IsBitSet
    ////////////////////////////////////////////////////////////////////////////

    [Theory]
    [InlineData(0b1111111111111110, 0, false)]
    [InlineData(0b0000000000000001, 0, true)]
    [InlineData(0b1111111101111111, 7, false)]
    [InlineData(0b0000000010000000, 7, true)]
    [InlineData(0b1111111011111111, 8, false)]
    [InlineData(0b0000000100000000, 8, true)]
    [InlineData(0b0111111111111111, 15, false)]
    [InlineData(0b1000000000000000, 15, true)]
    public void TestIsBitSet(ushort initialValue, byte bitIndex, bool expectedValue)
    {
        // ARRANGE:
        var value = new UInt16(initialValue);

        // ACT:
        var actualValue = value.IsBitSet(bitIndex);

        // ASSERT:
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void TestIsBitSetIndexOutOfRange()
    {
        // ARRANGE:
        var value = new UInt16(0);

        // ACT:
        void TooLow() => value.IsBitSet(-1);
        void TooHigh() => value.IsBitSet(UInt16.MaxBit);

        // ASSERT:
        Assert.Throws<ArgumentOutOfRangeException>(TooLow);
        Assert.Throws<ArgumentOutOfRangeException>(TooHigh);
    }

    ////////////////////////////////////////////////////////////////////////////
    // SetBit
    ////////////////////////////////////////////////////////////////////////////

    [Theory]
    [InlineData(0b0000000000000000, 0,  0b0000000000000001)]
    [InlineData(0b0000000000000000, 1,  0b0000000000000010)]
    [InlineData(0b0000000000000000, 2,  0b0000000000000100)]
    [InlineData(0b0000000000000000, 3,  0b0000000000001000)]
    [InlineData(0b0000000000000000, 4,  0b0000000000010000)]
    [InlineData(0b0000000000000000, 5,  0b0000000000100000)]
    [InlineData(0b0000000000000000, 6,  0b0000000001000000)]
    [InlineData(0b0000000000000000, 7,  0b0000000010000000)]
    [InlineData(0b0000000000000000, 8,  0b0000000100000000)]
    [InlineData(0b0000000000000000, 9,  0b0000001000000000)]
    [InlineData(0b0000000000000000, 10, 0b0000010000000000)]
    [InlineData(0b0000000000000000, 11, 0b0000100000000000)]
    [InlineData(0b0000000000000000, 12, 0b0001000000000000)]
    [InlineData(0b0000000000000000, 13, 0b0010000000000000)]
    [InlineData(0b0000000000000000, 14, 0b0100000000000000)]
    [InlineData(0b0000000000000000, 15, 0b1000000000000000)]

    [InlineData(0b0000000000010000, 4, 0b0000000000010000)] // idempotent
    public void TestSetBit(ushort initialValue, byte bitIndex, ushort expectedValue)
    {
        // ARRANGE:
        var value = new UInt16(initialValue);

        // ACT:
        value.SetBit(bitIndex);

        // ASSERT:
        Assert.Equal(expectedValue, value.ToInt());
    }

    [Fact]
    public void TestSetBitIndexOutOfRange()
    {
        // ARRANGE:
        var value = new UInt16(0);

        // ACT:
        void TooLow() => value.SetBit(-1);
        void TooHigh() => value.SetBit(UInt16.MaxBit);

        // ASSERT:
        Assert.Throws<ArgumentOutOfRangeException>(TooLow);
        Assert.Throws<ArgumentOutOfRangeException>(TooHigh);
    }

    ////////////////////////////////////////////////////////////////////////////
    // ClearBit
    ////////////////////////////////////////////////////////////////////////////

    [Theory]
    [InlineData(0b1111111111111111,  0, 0b1111111111111110)]
    [InlineData(0b1111111111111111,  1, 0b1111111111111101)]
    [InlineData(0b1111111111111111,  2, 0b1111111111111011)]
    [InlineData(0b1111111111111111,  3, 0b1111111111110111)]
    [InlineData(0b1111111111111111,  4, 0b1111111111101111)]
    [InlineData(0b1111111111111111,  5, 0b1111111111011111)]
    [InlineData(0b1111111111111111,  6, 0b1111111110111111)]
    [InlineData(0b1111111111111111,  7, 0b1111111101111111)]
    [InlineData(0b1111111111111111,  8, 0b1111111011111111)]
    [InlineData(0b1111111111111111,  9, 0b1111110111111111)]
    [InlineData(0b1111111111111111, 10, 0b1111101111111111)]
    [InlineData(0b1111111111111111, 11, 0b1111011111111111)]
    [InlineData(0b1111111111111111, 12, 0b1110111111111111)]
    [InlineData(0b1111111111111111, 13, 0b1101111111111111)]
    [InlineData(0b1111111111111111, 14, 0b1011111111111111)]
    [InlineData(0b1111111111111111, 15, 0b0111111111111111)]

    [InlineData(0b1111111111101111, 4, 0b1111111111101111)] // idempotent
    public void TestClearBit(ushort initialValue, byte bitIndex, ushort expectedValue)
    {
        // ARRANGE:
        var value = new UInt16(initialValue);

        // ACT:
        value.ClearBit(bitIndex);

        // ASSERT:
        Assert.Equal(expectedValue, value.ToInt());
    }

    [Fact]
    public void TestClearBitIndexOutOfRange()
    {
        // ARRANGE:
        var value = new UInt16(0);

        // ACT:
        void TooLow() => value.ClearBit(-1);
        void TooHigh() => value.ClearBit(UInt16.MaxBit);

        // ASSERT:
        Assert.Throws<ArgumentOutOfRangeException>(TooLow);
        Assert.Throws<ArgumentOutOfRangeException>(TooHigh);
    }

    ////////////////////////////////////////////////////////////////////////////
    // GetBitValue
    ////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void TestGetBitFlag()
    {
        // ARRANGE:
        var value = new UInt16(0b0101_0011_1010_1100);

        // ACT:

        // ASSERT:
        Assert.Equal(0, value.GetBitValue(0));
        Assert.Equal(0, value.GetBitValue(1));
        Assert.Equal(1, value.GetBitValue(2));
        Assert.Equal(1, value.GetBitValue(3));
        Assert.Equal(0, value.GetBitValue(4));
        Assert.Equal(1, value.GetBitValue(5));
        Assert.Equal(0, value.GetBitValue(6));
        Assert.Equal(1, value.GetBitValue(7));
        Assert.Equal(1, value.GetBitValue(8));
        Assert.Equal(1, value.GetBitValue(9));
        Assert.Equal(0, value.GetBitValue(10));
        Assert.Equal(0, value.GetBitValue(11));
        Assert.Equal(1, value.GetBitValue(12));
        Assert.Equal(0, value.GetBitValue(13));
        Assert.Equal(1, value.GetBitValue(14));
        Assert.Equal(0, value.GetBitValue(15));
    }

    [Fact]
    public void TestGetBitFlagIndexOutOfRange()
    {
        // ARRANGE:
        var value = new UInt16(0xFF);

        // ACT:
        void TooLow() => value.GetBitValue(-1);
        void TooHigh() => value.GetBitValue(UInt16.MaxBit);

        // // ASSERT:
        Assert.Throws<ArgumentOutOfRangeException>(TooLow);
        Assert.Throws<ArgumentOutOfRangeException>(TooHigh);
    }

    ////////////////////////////////////////////////////////////////////////////
    // SetBitValue
    ////////////////////////////////////////////////////////////////////////////

    [Theory]
    [InlineData(0b1111111111111111,  0, 0, 0b1111111111111110)]
    [InlineData(0b1111111111111111,  1, 0, 0b1111111111111101)]
    [InlineData(0b1111111111111111,  2, 0, 0b1111111111111011)]
    [InlineData(0b1111111111111111,  3, 0, 0b1111111111110111)]
    [InlineData(0b1111111111111111,  4, 0, 0b1111111111101111)]
    [InlineData(0b1111111111111111,  5, 0, 0b1111111111011111)]
    [InlineData(0b1111111111111111,  6, 0, 0b1111111110111111)]
    [InlineData(0b1111111111111111,  7, 0, 0b1111111101111111)]
    [InlineData(0b1111111111111111,  8, 0, 0b1111111011111111)]
    [InlineData(0b1111111111111111,  9, 0, 0b1111110111111111)]
    [InlineData(0b1111111111111111, 10, 0, 0b1111101111111111)]
    [InlineData(0b1111111111111111, 11, 0, 0b1111011111111111)]
    [InlineData(0b1111111111111111, 12, 0, 0b1110111111111111)]
    [InlineData(0b1111111111111111, 13, 0, 0b1101111111111111)]
    [InlineData(0b1111111111111111, 14, 0, 0b1011111111111111)]
    [InlineData(0b1111111111111111, 15, 0, 0b0111111111111111)]

    [InlineData(0b0000000000000000,  0, 1, 0b0000000000000001)]
    [InlineData(0b0000000000000000,  1, 1, 0b0000000000000010)]
    [InlineData(0b0000000000000000,  2, 1, 0b0000000000000100)]
    [InlineData(0b0000000000000000,  3, 1, 0b0000000000001000)]
    [InlineData(0b0000000000000000,  4, 1, 0b0000000000010000)]
    [InlineData(0b0000000000000000,  5, 1, 0b0000000000100000)]
    [InlineData(0b0000000000000000,  6, 1, 0b0000000001000000)]
    [InlineData(0b0000000000000000,  7, 1, 0b0000000010000000)]
    [InlineData(0b0000000000000000,  8, 1, 0b0000000100000000)]
    [InlineData(0b0000000000000000,  9, 1, 0b0000001000000000)]
    [InlineData(0b0000000000000000, 10, 1, 0b0000010000000000)]
    [InlineData(0b0000000000000000, 11, 1, 0b0000100000000000)]
    [InlineData(0b0000000000000000, 12, 1, 0b0001000000000000)]
    [InlineData(0b0000000000000000, 13, 1, 0b0010000000000000)]
    [InlineData(0b0000000000000000, 14, 1, 0b0100000000000000)]
    [InlineData(0b0000000000000000, 15, 1, 0b1000000000000000)]
    public void TestSetBitValue(ushort initialValue, byte bitIndex, ushort bitValue, ushort expectedValue)
    {
        // ARRANGE:
        var value = new UInt16(initialValue);

        // ACT:
        value.SetBitValue(bitIndex, bitValue);

        // ASSERT:
        Assert.Equal(expectedValue, value.ToInt());
    }

    [Fact]
    public void TestSetBitValueOutOfRangeIndex()
    {
        // ARRANGE:
        var value = new UInt16(0);

        // ACT:
        void TooLow() => value.SetBitValue(-1, 1);
        void TooHigh() => value.SetBitValue(16, 1);

        // ASSERT:
        Assert.Throws<ArgumentOutOfRangeException>(TooLow);
        Assert.Throws<ArgumentOutOfRangeException>(TooHigh);
    }

    [Fact]
    public void TestSetBitValueBitValueOutOfRange()
    {
        // ARRANGE:
        var value = new UInt16(0);

        // ACT:
        void TooLow() => value.SetBitValue(0, -1);
        void TooHigh() => value.SetBitValue(0, 2);

        // ASSERT:
        Assert.Throws<ArgumentOutOfRangeException>(TooLow);
        Assert.Throws<ArgumentOutOfRangeException>(TooHigh);
    }

    ////////////////////////////////////////////////////////////////////////////
    // Inc
    ////////////////////////////////////////////////////////////////////////////

    private void ExecuteInc(int value)
    {
        // ARRANGE:
        int expected;
        if (value == UInt16.Max)
            expected = 0;
        else
            expected = value + 1;

        var instance = new UInt16(value);

        // ACT:
        var actual = instance.Inc().ToInt();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestInc()
    {
        ExecuteInc(0);
        ExecuteInc(255);
        ExecuteInc(UInt16.Max);
    }

    ////////////////////////////////////////////////////////////////////////////
    // Dec
    ////////////////////////////////////////////////////////////////////////////

    private void ExecuteDec(int value)
    {
        // ARRANGE:
        int expected;
        if (value == 0)
            expected = UInt16.Max;
        else
            expected = value - 1;

        var instance = new UInt16(value);

        // ACT:
        var actual = instance.Dec().ToInt();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestDec()
    {
        ExecuteDec(UInt16.Max);
        ExecuteDec(256);
        ExecuteDec(0);
    }

    ////////////////////////////////////////////////////////////////////////////
    // AddUnsigned
    ////////////////////////////////////////////////////////////////////////////

    private void ExecuteAddUnsigned(UInt16 operand1, UInt8 operand2, UInt16 expectedValue)
    {
        // ARRANGE:

        // ACT:
        var actualValue = operand1.AddUnsigned(operand2);

        // ASSERT:
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void TestAddUnsignedWhenPos()
    {
        ExecuteAddUnsigned(new UInt16(1), new UInt8(1), new UInt16(2));
    }

    [Fact]
    public void TestAddUnsignedWhenPosCrossPage()
    {
        ExecuteAddUnsigned(new UInt16(255), new UInt8(1), new UInt16(256));
    }

    ////////////////////////////////////////////////////////////////////////////
    // AddSigned
    ////////////////////////////////////////////////////////////////////////////

    private void ExecuteAddSigned(UInt16 operand1, UInt8 operand2, UInt16 expectedValue)
    {
        // ARRANGE:

        // ACT:
        var actualValue = operand1.AddSigned(operand2);

        // ASSERT:
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void TestAddSignedWhenPos()
    {
        ExecuteAddSigned(new UInt16(1), new UInt8(1), new UInt16(2));
    }

    [Fact]
    public void TestAddSignedWhenPosCrossPage()
    {
        ExecuteAddSigned(new UInt16(255), new UInt8(1), new UInt16(256));
    }

    [Fact]
    public void TestAddSignedWhenNeg()
    {
        ExecuteAddSigned(new UInt16(2), new UInt8(0xFF), new UInt16(1));
    }

    [Fact]
    public void TestAddSignedWhenNegCrossPage()
    {
        ExecuteAddSigned(new UInt16(256), new UInt8(0xFF), new UInt16(255));
    }

    ////////////////////////////////////////////////////////////////////////////
    // ToString
    ////////////////////////////////////////////////////////////////////////////

    [Fact]
    public void TestToString()
    {
        // ARRANGE:
        var value = new UInt16(0x12AB);
        const string expected = "0x12AB";

        // ACT:
        var actual = value.ToString();

        // ASSERT:
        Assert.Equal(expected, actual);
    }
}
