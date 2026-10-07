// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine.Types;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class CPYTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // CPY immediate
    // -------------------------------------------------------------------------
    private void ExecuteCPYimm(UInt8 yValue, UInt8 operand, BitFlag expectedC, BitFlag expectedZ,
                               BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CPYimm.ToUInt8();

        BootToAddress(BootAddr);
        Regs.P.Carry = expectedC.Copy().Not();
        Regs.P.Zero = expectedZ.Copy().Not();
        Regs.P.Negative = expectedN.Copy().Not();
        Regs.Y = yValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstCPYimm2 - load the operand into the temp reg.

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCPYimmWhenEQ()
    {
        var yValue = new UInt8(22);
        var operand = new UInt8(22);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCPYimm(yValue, operand, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCPYimmWhenLT()
    {
        var yValue = new UInt8(11);
        var operand = new UInt8(22);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCPYimm(yValue, operand, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCPYimmWhenGT()
    {
        var yValue = new UInt8(22);
        var operand = new UInt8(11);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCPYimm(yValue, operand, expectedC, expectedZ, expectedN);
    }

    // -------------------------------------------------------------------------
    // CPY zeropage
    // -------------------------------------------------------------------------
    private void ExecuteCPYzpg(UInt8 yValue, UInt8 memValue, BitFlag expectedC, BitFlag expectedZ,
                               BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CPYzpg.ToUInt8();
        var operand = new UInt8(0x12);

        BootToAddress(BootAddr);
        Regs.P.Carry = expectedC.Copy().Not();
        Regs.P.Zero = expectedZ.Copy().Not();
        Regs.P.Negative = expectedN.Copy().Not();
        Regs.Y = yValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstCPYzpg2 - fetch the second byte of the op-code (zpg-value)

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstLDAzpg3 - fetch the value at address zpg-value

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCPYzpgWhenEQ()
    {
        var yValue = new UInt8(22);
        var memValue = new UInt8(22);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCPYzpg(yValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCPYzpgWhenLT()
    {
        var yValue = new UInt8(11);
        var memValue = new UInt8(22);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCPYzpg(yValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCPYzpgWhenGT()
    {
        var yValue = new UInt8(22);
        var memValue = new UInt8(11);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCPYzpg(yValue, memValue, expectedC, expectedZ, expectedN);
    }

    // -------------------------------------------------------------------------
    // CPY absolute
    // -------------------------------------------------------------------------
    private void ExecuteCPYabs(UInt8 yValue, UInt8 memValue, BitFlag expectedC, BitFlag expectedZ,
                               BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CPYabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);

        BootToAddress(BootAddr);
        Regs.P.Carry = expectedC.Copy().Not();
        Regs.P.Zero = expectedZ.Copy().Not();
        Regs.P.Negative = expectedN.Copy().Not();
        Regs.Y = yValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstCPYabs2 - fetch the second byte of the op-code (EA low)

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstCPYabs3 - fetch the third byte of the op-code (EA high)

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstCPYabs4 - fetch the value at EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCPYabsWhenEQ()
    {
        var yValue = new UInt8(22);
        var memValue = new UInt8(22);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCPYabs(yValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCPYabsWhenLT()
    {
        var yValue = new UInt8(11);
        var memValue = new UInt8(22);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCPYabs(yValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCPYabsWhenGT()
    {
        var yValue = new UInt8(22);
        var memValue = new UInt8(11);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCPYabs(yValue, memValue, expectedC, expectedZ, expectedN);
    }
}
