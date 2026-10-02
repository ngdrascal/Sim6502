// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine.Types;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class CPXTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // CPX immediate
    // -------------------------------------------------------------------------
    private void ExecuteCPXimm(UInt8 xValue, UInt8 operand, BitFlag expectedC, BitFlag expectedZ,
                               BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CPXimm.ToUInt8();

        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(expectedC.Copy().Not());
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstCPXimm2 - load the operand into the temp reg.

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCPXimmWhenEQ()
    {
        var xValue = new UInt8(22);
        var operand = new UInt8(22);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCPXimm(xValue, operand, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCPXimmWhenLT()
    {
        var xValue = new UInt8(11);
        var operand = new UInt8(22);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCPXimm(xValue, operand, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCPXimmWhenGT()
    {
        var xValue = new UInt8(22);
        var operand = new UInt8(11);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCPXimm(xValue, operand, expectedC, expectedZ, expectedN);
    }

    // -------------------------------------------------------------------------
    // CPX zeropage
    // -------------------------------------------------------------------------
    private void ExecuteCPXzpg(UInt8 xValue, UInt8 memValue, BitFlag expectedC, BitFlag expectedZ,
                               BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CPXzpg.ToUInt8();
        var operand = new UInt8(0x12);

        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(expectedC.Copy().Not());
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstCPXzpg2 - fetch the second byte of the op-code (zpg-value)

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
    public void TestCPXzpgWhenEQ()
    {
        var xValue = new UInt8(22);
        var memValue = new UInt8(22);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCPXzpg(xValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCPXzpgWhenLT()
    {
        var xValue = new UInt8(11);
        var memValue = new UInt8(22);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCPXzpg(xValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCPXzpgWhenGT()
    {
        var xValue = new UInt8(22);
        var memValue = new UInt8(11);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCPXzpg(xValue, memValue, expectedC, expectedZ, expectedN);
    }

    // -------------------------------------------------------------------------
    // CPX absolute
    // -------------------------------------------------------------------------
    private void ExecuteCPXabs(UInt8 xValue, UInt8 memValue, BitFlag expectedC, BitFlag expectedZ,
                               BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CPXabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);

        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(expectedC.Copy().Not());
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstCPXabs2 - fetch the second byte of the op-code (EA low)

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstCPXabs3 - fetch the third byte of the op-code (EA high)

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstCPXabs4 - fetch the value at EA

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCPXabsWhenEQ()
    {
        var xValue = new UInt8(22);
        var memValue = new UInt8(22);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCPXabs(xValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCPXabsWhenLT()
    {
        var xValue = new UInt8(11);
        var memValue = new UInt8(22);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCPXabs(xValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCPXabsWhenGT()
    {
        var xValue = new UInt8(22);
        var memValue = new UInt8(11);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCPXabs(xValue, memValue, expectedC, expectedZ, expectedN);
    }
}
