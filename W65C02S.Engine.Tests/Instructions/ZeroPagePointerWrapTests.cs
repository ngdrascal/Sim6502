// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using UInt8 = W65C02S.Engine.Types.UInt8;
using UInt16 = W65C02S.Engine.Types.UInt16;

namespace W65C02S.Engine.Tests;

// A zero page pointer at $FF takes its high byte from $00, not $0100.
[ExcludeFromCodeCoverage]
public class ZeroPagePointerWrapTests : UnitTestBase
{
    // pointer $FF/$00 -> $1234; $0100 holds a decoy high byte that would point at $9934
    private void LoadPointerAtFF()
    {
        Memory[0x00FF] = 0x34;
        Memory[0x0000] = 0x12;
        Memory[0x0100] = 0x99;
    }

    // boots to $1000 holding opCode and its zero page operand
    private void BootWithInstruction(OpCodes opCode, int operand)
    {
        Memory[0x1000] = (byte)opCode.ToUInt8().ToInt();
        Memory[0x1001] = (byte)operand;
        BootToAddress(new UInt16(0x1000));
    }

    private void Execute(int cycles)
    {
        for (var i = 0; i < cycles; i++)
            ExecuteCycleWithMemory();
    }

    /*
      TITLE: Indirect loads through a zero page pointer at $FF read the high byte from $00
      GIVEN: $FF/$00 point at $1234, $0100 holds a decoy, and the target holds $5A
      WHEN: LDA ($FF), LDA ($FF),Y with Y = 1, or LDA ($FE,X) with X = 1 runs
      THEN: A is $5A, loaded from $1234 (or $1235 for the Y-indexed form)
    */
    [Theory]
    [InlineData(OpCodes.LDAind, 0xFF, 0, 0, 0x1234, 5)]
    [InlineData(OpCodes.LDAindy, 0xFF, 0, 1, 0x1235, 5)]
    [InlineData(OpCodes.LDAindx, 0xFE, 1, 0, 0x1234, 6)]
    public void TestLoadThroughPointerAtFF(OpCodes opCode, int operand, int x, int y, int target, int cycles)
    {
        // ARRANGE:
        LoadPointerAtFF();
        Memory[target] = 0x5A;
        BootWithInstruction(opCode, operand);
        Regs.X = new UInt8(x);
        Regs.Y = new UInt8(y);

        // ACT:
        Execute(cycles);

        // ASSERT:
        Assert.Equal(new UInt8(0x5A), Regs.A);
    }

    /*
      TITLE: An indirect store through a zero page pointer at $FF writes to the wrapped address
      GIVEN: $FF/$00 point at $1234, $0100 holds a decoy, and A = $A5
      WHEN: STA ($FF) runs
      THEN: $1234 holds $A5 and the decoy target $9934 is untouched
    */
    [Fact]
    public void TestStoreThroughPointerAtFF()
    {
        // ARRANGE:
        LoadPointerAtFF();
        BootWithInstruction(OpCodes.STAind, 0xFF);
        Regs.A = new UInt8(0xA5);

        // ACT:
        Execute(5);

        // ASSERT:
        Assert.Equal(0xA5, Memory[0x1234]);
        Assert.Equal(0x00, Memory[0x9934]);
    }
}
