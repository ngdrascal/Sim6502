using System.Diagnostics.CodeAnalysis;
using Sim6502.types;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Types;

[ExcludeFromCodeCoverage]
public class UInt8Tests : UnitTestBase
{
    private void ExecuteCtorReturnExpectedValue(int expectedValue)
    {
        // ARRANGE:

        // ACT:
        var test = new UInt8(expectedValue);

        // ASSERT:
        Assert.Equal(expectedValue, test.ToInt());
    }

    [Fact]
    public void TestCtorReturns()
    {
        ExecuteCtorReturnExpectedValue(0);
        ExecuteCtorReturnExpectedValue(255);
    }

    [Fact]
    public void TestEqOverrideWhenEq()
    {
        // ARRANGE:
        const int testValue = 3;
        var left = new UInt8(testValue);
        var right = new UInt8(testValue);

        // ACT:
        var actual = left.Equals(right);

        // ASSERT:
        Assert.True(actual);
    }

    [Fact]
    public void TestEqOverrideWhenNEq()
    {
        // ARRANGE:
        const int leftValue = 3;
        const int rightValue = leftValue + 1;
        var left = new UInt8(leftValue);
        var right = new UInt8(rightValue);

        // ACT:
        var areEqual = left.Equals(right);

        // ASSERT:
        Assert.False(areEqual);
    }

    [Fact]
    public void TestEqualZeroWhenZero()
    {
        // ARRANGE:
        var test = new UInt8(0);

        // ACT:
        var actual = test.EqualsZero();

        // ASSERT:
        Assert.True(actual);
    }

    [Fact]
    public void TestEqualZeroWhenNotZero()
    {
        // ARRANGE:
        var test = new UInt8(1);

        // ACT:
        var actual = test.EqualsZero();

        // ASSERT:
        Assert.False(actual);
    }

    private void ExecuteAnd(UInt8 left, UInt8 right)
    {
        // ARRANGE:
        var expected = new UInt8(left.ToInt() & right.ToInt());

        // ACT:
        var actual = left.And(right);

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestAnd1()
    {
        ExecuteAnd(new UInt8(0b00000111), new UInt8(0b00000011));
    }

    [Fact]
    public void TestAnd2()
    {
        ExecuteAnd(new UInt8(0b11110000), new UInt8(0b00001111));
    }

    private void ExecuteOr(UInt8 left, UInt8 right)
    {
        // ARRANGE:
        var expected = new UInt8(left.ToInt() | right.ToInt());

        // ACT:
        var actual = left.Or(right);

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestOr1()
    {
        ExecuteOr(new UInt8(0b00000001), new UInt8(0b00000010));
    }

    private void ExecuteXor(UInt8 left, UInt8 right)
    {
        // ARRANGE:
        var expected = new UInt8(left.ToInt() ^ right.ToInt());

        // ACT:
        var actual = left.Xor(right);

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestXor1()
    {
        ExecuteXor(new UInt8(0b00000001), new UInt8(0b00000010));
    }

    [Fact]
    public void TestXor2()
    {
        ExecuteXor(new UInt8(0b00000001), new UInt8(0b00000001));
    }

    private void ExecuteNot(UInt8 left)
    {
        // ARRANGE:
        var expected = new UInt8(~left.ToInt() & 0x000000FF);

        // ACT:
        var actual = left.Not();

        // ASSERT:
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestNotNeg1()
    {
        ExecuteNot(new UInt8(0xFF));
    }

    [Fact]
    public void TestNot0()
    {
        ExecuteNot(new UInt8(0));
    }

    //      c6                   cIn
    //      m7 m6 m5 m4 m3 m2 m1 m0
    //      n7 n6 n5 n4 n3 n2 n1 n0
    // ----------------------------
    //   c7 s7 s6 s5 s4 s3 s2 s1 s0
    //
    // +---+------------+----------+----------------------------------------------------------+
    // |   |   Inputs   | Outputs  |                           Examples                       |
    // |---|------------+----------+---------------------+-----------------+------------------|
    // | # | C6  M7  N7 | S7  C  V |         Hex         |     Unsigned    |      Signed      |
    // |---|------------+----------+---------------------+-----------------+------------------|
    // | 1 | 0   0   0  | 0   0  0 | 0x50 + 0x10 = 0x060    80 +  16 =  96    80 +   16 =  96 |
    // | 2 | 1   0   0  | 1   0  1 | 0x50 + 0x50 = 0x0A0    80 +  80 = 160    80 +   80 = -96 |
    // | 3 | 0   0   1  | 1   0  0 | 0x50 + 0x90 = 0x0E0    80 + 144 = 224    80 + -112 = -32 |
    // | 4 | 1   0   1  | 0   1  0 | 0x50 + 0xD0 = 0x120    80 + 208 = 288    80 +  -48 =  32 |
    // | 5 | 0   1   0  | 1   0  0 | 0xD0 + 0x10 = 0x0E0   208 +  16 = 224   -48 +   16 = -32 |
    // | 6 | 1   1   0  | 0   1  0 | 0xD0 + 0x50 = 0x120   208 +  80 = 288   -48 +   80 =  32 |
    // | 7 | 0   1   1  | 0   1  1 | 0xD0 + 0x90 = 0x160   208 + 144 = 352   -48 + -112 =  96 |
    // | 8 | 1   1   1  | 1   1  0 | 0xD0 + 0xD0 = 0x1A0   208 + 208 = 416   -48 +  -48 = -96 |
    // +---+------------+---------------------------------------------------------------------+

    private void ExecuteAdcBinary(UInt8 leftValue, UInt8 rightValue, UInt8 expectedValue,
                                  BitFlag expectedC, BitFlag expectedV)
    {
        // ARRANGE:
        var carryIn = BitFlag.Low();

        // ACT:
        var actual = leftValue.Adc(rightValue, carryIn, BitFlag.Low());

        // ASSERT:
        Assert.Equal(expectedValue, actual.Value());
        Assert.Equal(expectedC, actual.Carry());
        Assert.Equal(expectedV, actual.Overflow());
    }

    [Fact]
    public void TestAdcBin1()
    {
        //               left             right            sum              cOut vOut
        ExecuteAdcBinary(new UInt8(0x50), new UInt8(0x10), new UInt8(0x60), Low, Low);
    }

    [Fact]
    public void TestAdcBin2()
    {
        //               left             right            sum              cOut vOut
        ExecuteAdcBinary(new UInt8(0x50), new UInt8(0x50), new UInt8(0xA0), Low, High);
    }

    [Fact]
    public void TestAdcBin3()
    {
        //               left             right            sum              cOut vOut
        ExecuteAdcBinary(new UInt8(0x50), new UInt8(0x90), new UInt8(0xE0), Low, Low);
    }

    [Fact]
    public void TestAdcBin4()
    {
        //               left             right            sum              cOut vOut
        ExecuteAdcBinary(new UInt8(0x50), new UInt8(0xD0), new UInt8(0x20), High, Low);
    }

    [Fact]
    public void TestAdcBin5()
    {
        //               left             right            sum              cOut vOut
        ExecuteAdcBinary(new UInt8(0xD0), new UInt8(0x10), new UInt8(0xE0), Low, Low);
    }

    [Fact]
    public void TestAdcBin6()
    {
        //               left             right            sum              cOut vOut
        ExecuteAdcBinary(new UInt8(0xD0), new UInt8(0x50), new UInt8(0x20), High, Low);
    }

    [Fact]
    public void TestAdcBin7()
    {
        //               left             right            sum              cOut vOut
        ExecuteAdcBinary(new UInt8(0xD0), new UInt8(0x90), new UInt8(0x60), High, High);
    }

    [Fact]
    public void TestAdcBin8()
    {
        //               left             right            sum              cOut vOut
        ExecuteAdcBinary(new UInt8(0xD0), new UInt8(0xD0), new UInt8(0xA0), High, Low);
    }

    private void ExecuteAdcDecimal(UInt8 leftValue, UInt8 rightValue, UInt8 expectedValue,
                                   BitFlag expectedC, BitFlag expectedV)
    {
        // ARRANGE:
        var carryIn = BitFlag.Low();

        // ACT:
        var actual = leftValue.Adc(rightValue, carryIn, BitFlag.High());

        // ASSERT:
        Assert.Equal(expectedValue, actual.Value());
        Assert.Equal(expectedC, actual.Carry());
        Assert.Equal(expectedV, actual.Overflow());
    }

    [Fact]
    public void TestAdcDecimalWhenZero()
    {
        //                left             right            sum              cOut           vOut
        ExecuteAdcDecimal(new UInt8(0x00), new UInt8(0x00), new UInt8(0x00), BitFlag.Low(), BitFlag.Low());
    }

    [Fact]
    public void TestAdcDecimalWhenNoCarryOut()
    {
        //                left             right            sum              cOut           vOut
        ExecuteAdcDecimal(new UInt8(0x09), new UInt8(0x01), new UInt8(0x10), BitFlag.Low(), BitFlag.Low());
    }

    [Fact]
    public void TestAdcDecimalWhenCarryOut()
    {
        //                left             right            sum              cOut           vOut
        ExecuteAdcDecimal(new UInt8(0x99), new UInt8(0x01), new UInt8(0x00), BitFlag.High(), BitFlag.Low());
    }

    [Fact]
    public void TestADC()
    {
        // ARRANGE:
        const int cIn = 1;
        const int left = 0x70;
        const int right = 0x10;
        const int expectedValue = 0x81;
        const string expectedFlags = "NVzc";

        var carryIn = new BitFlag(cIn == 1);
        var accumulator = new UInt8(left);
        var operand = new UInt8(right);
        var expected = new MathResult(new UInt8(expectedValue), expectedFlags);

        // ACT:
        var actual = accumulator.Adc(operand, carryIn, High);

        // ASSERT:
        Assert.Equal(expected.Value(), actual.Value());
        Assert.Equal(expected.Negative(), actual.Negative());
        Assert.Equal(expected.Overflow(), actual.Overflow());
        Assert.Equal(expected.Zero(), actual.Zero());
        Assert.Equal(expected.Carry(), actual.Carry());
    }

    [Fact]
    public void TestAdcDecimalAllCombinations()
    {
        // Read a csv file.
        // There are 4 fields in the file which are:
        //    carry-flag-in, input-1, input-2, expected-result, expected-flags.
        // The carry-flag-in is either 0 or 1.
        // The expected flags are in the order of: negative, overflow, zero, carry.
        // The flags are represented by the letters n, v, z, c.
        // If the flag is set then the letter is upper case, otherwise it is lower case.
        // The input-1, input-2, and expected-result is in hex format.

        const string csvFile = ".\\types\\adc.csv";
        try
        {
            var lines = File.ReadAllLines(csvFile);
            foreach (var line in lines)
            {
                // use comma as separator
                var fields = line.Split(',');
                var cFlag = Convert.ToInt32(fields[0], 16);
                var accValue = Convert.ToInt32(fields[1], 16);
                var operandValue = Convert.ToInt32(fields[2], 16);
                var expectedValue = Convert.ToInt32(fields[3], 16);
                var expectedFlags = fields[4];

                var cFlagBit = cFlag == 0 ? Low : High;
                var dFlagBit = High;
                var expected = new MathResult(new UInt8(expectedValue), expectedFlags);

                // ARRANGE:
                var accumulator = new UInt8(accValue);
                var operand = new UInt8(operandValue);

                // ACT:
                var actual = accumulator.Adc(operand, cFlagBit, dFlagBit);

                // var msg = $" cf: {cFlag:X} acc: {accValue:X2}, operand: {operandValue:X2}";

                // ASSERT:
                Assert.Equal(expected.Value(), actual.Value());
                Assert.Equal(expected.Negative(), actual.Negative());
                Assert.Equal(expected.Overflow(), actual.Overflow());
                Assert.Equal(expected.Zero(), actual.Zero());
                Assert.Equal(expected.Carry(), actual.Carry());
            }
        }
        catch (FileNotFoundException)
        {
            Assert.Fail("File not found: " + csvFile);
        }
        catch (IOException)
        {
            Assert.Fail("IO Exception: " + csvFile);
        }
    }

    // +---+------------+--------------+----------------------------------------------------------+
    // |   |   Inputs   |   Outputs    |                           Examples                       |
    // |---|------------+--------------+---------------------+-----------------+------------------|
    // | # | C6  M7  N7 | C7  S7  B  V |         Hex         |     Unsigned    |      Signed      |
    // |---|------------+--------------+---------------------+-----------------+------------------|
    // | 1 | 0   0   1  | 0   0   1  0 | 0x50 - 0xf0 = 0x060    80 - 240 =  96    80 - -16 =  96  | 
    // | 2 | 1   0   1  | 0   1   1  1 | 0x50 - 0xb0 = 0x0A0    80 - 176 = 160    80 - -80 = -96  |
    // | 3 | 0   0   0  | 0   1   1  0 | 0x50 - 0x70 = 0x0E0    80 - 112 = 224    80 - 112 = -32  |
    // | 4 | 1   0   0  | 1   0   0  0 | 0x50 - 0x30 = 0x120    80 -  48 =  32    80 -  48 =  32  |
    // | 5 | 0   1   1  | 0   1   1  0 | 0xd0 - 0xf0 = 0x0E0   208 - 240 = 224   -48 - -16 = -32  |
    // | 6 | 1   1   1  | 1   0   0  0 | 0xd0 - 0xb0 = 0x120   208 - 176 =  32   -48 - -80 =  32  |
    // | 7 | 0   1   0  | 1   0   0  1 | 0xd0 - 0x70 = 0x160   208 - 112 =  96   -48 - 112 =  96  |
    // | 8 | 1   1   0  | 1   1   0  0 | 0xd0 - 0x30 = 0x1A0   208 -  48 = 160   -48 -  48 = -96  |
    // +---+------------+--------------+----------------------------------------------------------+   

    private void ExecuteSBCBinary(UInt8 leftValue, UInt8 rightValue, BitFlag carryIn,
                                  UInt8 expectedValue, string expectedFlags)
    {
        // ARRANGE:

        // ACT:
        var actual = leftValue.Sbc(rightValue, carryIn, BitFlag.Low());

        // ASSERT:
        Assert.Equal(expectedValue, actual.Value());
        Assert.Equal(FlagValue(expectedFlags, 'N'), actual.Negative());
        Assert.Equal(FlagValue(expectedFlags, 'V'), actual.Overflow());
        Assert.Equal(FlagValue(expectedFlags, 'Z'), actual.Zero());
        Assert.Equal(FlagValue(expectedFlags, 'C'), actual.Carry());
    }

    [Fact]
    public void TestSbcBin1()
    {
        ExecuteSBCBinary(new UInt8(0x50), new UInt8(0xF0), High, new UInt8(0x60), "nvzc");
    }

    [Fact]
    public void TestSbcBin2()
    {
        ExecuteSBCBinary(new UInt8(0x50), new UInt8(0xB0), High, new UInt8(0xA0), "NVzc");
    }

    [Fact]
    public void TestSbcBin3()
    {
        ExecuteSBCBinary(new UInt8(0x50), new UInt8(0x70), High, new UInt8(0xE0), "Nvzc");
    }

    [Fact]
    public void TestSbcBin4()
    {
        ExecuteSBCBinary(new UInt8(0x50), new UInt8(0x30), High, new UInt8(0x20), "nvzC");
    }

    [Fact]
    public void TestSbcBin5()
    {
        ExecuteSBCBinary(new UInt8(0xD0), new UInt8(0xF0), High, new UInt8(0xE0), "Nvzc");
    }

    [Fact]
    public void TestSbcBin6()
    {
        ExecuteSBCBinary(new UInt8(0xD0), new UInt8(0xB0), High, new UInt8(0x20), "nvzC");
    }

    [Fact]
    public void TestSbcBin7()
    {
        ExecuteSBCBinary(new UInt8(0xD0), new UInt8(0x70), High, new UInt8(0x60), "nVzC");
    }

    [Fact]
    public void TestSbcBin8()
    {
        ExecuteSBCBinary(new UInt8(0xD0), new UInt8(0x30), High, new UInt8(0xA0), "NvzC");
    }

    private void ExecuteSBCDecimal(UInt8 leftValue, UInt8 rightValue, BitFlag carryIn,
                                   UInt8 expectedValue, string expectedFlags)
    {
        // ARRANGE:

        // ACT:
        var actual = leftValue.Sbc(rightValue, carryIn, BitFlag.High());

        // ASSERT:
        Assert.Equal(expectedValue, actual.Value());
        Assert.Equal(FlagValue(expectedFlags, 'N'), actual.Negative());
        Assert.Equal(FlagValue(expectedFlags, 'V'), actual.Overflow());
        Assert.Equal(FlagValue(expectedFlags, 'Z'), actual.Zero());
        Assert.Equal(FlagValue(expectedFlags, 'C'), actual.Carry());
    }

    [Fact]
    public void TestSbcDecimalWhenZero()
    {
        var left = new UInt8(0x00);
        var right = new UInt8(0x00);
        var carryIn = BitFlag.High();
        var expected = new UInt8(0);
        const string expectedFlags = "nvZC";

        ExecuteSBCDecimal(left, right, carryIn, expected, expectedFlags);
    }

    [Fact]
    public void TestSBC()
    {
        // ARRANGE:
        var accumulator = new UInt8(0x00);
        var operand = new UInt8(0xA1);

        // ACT:
        var result = accumulator.Sbc(operand, High, High);

        // ASSERT:
        Assert.Equal(new UInt8(0xF9), result.Value());
        Assert.Equal(High, result.Negative());
        Assert.Equal(Low, result.Overflow());
        Assert.Equal(Low, result.Zero());
        Assert.Equal(Low, result.Carry());
    }

    [Fact]
    public void TestSbcDecimalAllCombinations()
    {
        const string csvFile = @".\types\sbc.csv";
        try
        {
            var lines = File.ReadAllLines(csvFile);
            foreach (var line in lines)
            {
                // use comma as separator
                var fields = line.Split(',');
                var cFlag = Convert.ToInt32(fields[0], 16);
                var accValue = Convert.ToInt32(fields[1], 16);
                var operandValue = Convert.ToInt32(fields[2], 16);
                var expectedValue = Convert.ToInt32(fields[3], 16);
                var expectedFlags = fields[4];

                var cFlagBit = cFlag == 0 ? Low : High;
                var dFlagBit = High;
                var expected = new MathResult(new UInt8(expectedValue), expectedFlags);

                // ARRANGE:
                var accumulator = new UInt8(accValue);
                var operand = new UInt8(operandValue);

                // ACT:
                var actual = accumulator.Sbc(operand, cFlagBit, dFlagBit);

                //  var msg = $" cf: {cFlag:X} acc: {accValue:X2}, operand: {operandValue:X2}";
                // Console.Write(msg);

                // ASSERT:
                Assert.Equal(expected.Value(), actual.Value());
                Assert.Equal(expected.Negative(), actual.Negative());
                Assert.Equal(expected.Overflow(), actual.Overflow());
                Assert.Equal(expected.Zero(), actual.Zero());
                Assert.Equal(expected.Carry(), actual.Carry());
            }
        }
        catch (FileNotFoundException)
        {
            Assert.Fail("File not found: " + csvFile);
        }
        catch (IOException)
        {
            Assert.Fail("IO Exception: " + csvFile);
        }
    }
}
