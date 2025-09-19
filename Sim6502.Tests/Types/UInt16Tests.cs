using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502.Tests.Types;

public class UInt16Tests : UnitTestBase
{
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

    [Fact]
    public void TestEqualReturnsTrueWhenEqual()
    {
        // ARRANGE:
        const int value = 1234;

        // ACT:
        var instance1 = new UInt16(value);
        var instance2 = new UInt16(value);
        var actual = instance2.Equals(instance1);

        // ASSERT:
        Assert.True(actual);
    }

    [Fact]
    public void TestEqualReturnsFalseWhenNotEqual()
    {
        // ARRANGE:
        const int value = 1234;

        // ACT:
        var instance1 = new UInt16(value);
        var instance2 = new UInt16(value + 1);
        var actual = instance2.Equals(instance1);

        // ASSERT:
        Assert.False(actual);
    }

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
    public void TestEqualsReturnsTrueWhen0()
    {
        // ARRANGE:
        const int value = 0;
        const bool expected = true;

        var instance = new UInt16(value);

        // ACT:
        var actual = instance.EqualsZero();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestEqualsReturnsFalseWhenNot0()
    {
        // ARRANGE:
        const int value = 1;
        const bool expected = false;

        var instance = new UInt16(value);

        // ACT:
        var actual = instance.EqualsZero();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    private void ExecuteIsBitSet(int bitIndex)
    {
        // ARRANGE:
        var expected = bitIndex % 2 == 0;
        var instance = new UInt16(0x55);

        // ACT:
        var actual = instance.IsBitSet(0);

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestIsBitSet()
    {
        for (var bitIndex = 0; bitIndex < UInt16.MaxBit; bitIndex++)
            ExecuteIsBitSet(0);
    }

    [Fact]
    public void TestIsBitSetReturnsFalseWhenBitIsNotSet()
    {
        // ARRANGE:
        const bool expected = false;
        var instance = new UInt16(0x55);

        // ACT:
        var actual = instance.IsBitSet(1);

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestSetBitSetsTheBitTo1()
    {
        // ARRANGE:
        const int bitIndex = 4;
        var expected = 1 << bitIndex;
        var instance = new UInt16(0);

        // ACT:
        instance.SetBit(bitIndex);
        var actual = instance.ToInt();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestClearBitSetsTheBitTo0()
    {
        // ARRANGE:
        const int bitIndex = 4;
        var expected = ~(1 << bitIndex) & 0x0000FFFF;
        var instance = new UInt16(0xFFFF);

        // ACT:
        instance.ClearBit(bitIndex);
        var actual = instance.ToInt();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    private void ExecuteGetBitValue(int value, int bitIndex)
    {
        // ARRANGE:
        var mask = 1 << bitIndex;
        var expected = (value & mask) >> bitIndex;

        var instance = new UInt16(0x55);

        // ACT:
        var actual = instance.GetBitValue(0);

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestGetBitValue()
    {
        const int value = 0x5555;
        for (var bitIndex = 0; bitIndex < UInt16.MaxBit; bitIndex++)
            ExecuteGetBitValue(value, 0);
    }

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
}
