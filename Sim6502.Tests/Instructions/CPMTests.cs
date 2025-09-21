// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class CMPTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // CMP immediate
    // -------------------------------------------------------------------------
    private void ExecuteCMPimm(UInt8 aValue, UInt8 operand, BitFlag expectedC, BitFlag expectedZ,
                               BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CMPimm.ToUInt8();

        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(expectedC.Copy().Not());
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstCMPimm2 - load the operand into the temp reg.

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCMPimmWhenEQ()
    {
        var aValue = new UInt8(0x22);
        var operand = new UInt8(0x22);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCMPimm(aValue, operand, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPimmWhenLT()
    {
        var aValue = new UInt8(0x11);
        var operand = new UInt8(0x22);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCMPimm(aValue, operand, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPimmWhenGT()
    {
        var aValue = new UInt8(0x22);
        var operand = new UInt8(0x11);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCMPimm(aValue, operand, expectedC, expectedZ, expectedN);
    }

    // -------------------------------------------------------------------------
    // CMP zeropage
    // -------------------------------------------------------------------------
    private void ExecuteCMPzpg(UInt8 aValue, UInt8 memValue, BitFlag expectedC, BitFlag expectedZ,
                               BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CMPzpg.ToUInt8();
        var operand = new UInt8(0x12);

        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(expectedC.Copy().Not());
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstCMPzpg2 - fetch the second byte of the op-code into EA

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstLDAzpg3 - fetch the value at EA into temp and do the compare

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCMPzpgWhenEQ()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x22);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCMPzpg(aValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPzpgWhenLT()
    {
        var aValue = new UInt8(0x11);
        var memValue = new UInt8(0x22);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCMPzpg(aValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPzpgWhenGT()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x11);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCMPzpg(aValue, memValue, expectedC, expectedZ, expectedN);
    }

    // -------------------------------------------------------------------------
    // CMP zeropage,X
    // -------------------------------------------------------------------------
    private void ExecuteCMPzpgx(UInt8 aValue, UInt8 memValue, BitFlag expectedC, BitFlag expectedZ,
                                BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CMPzpgx.ToUInt8();
        var operand = new UInt8(0x12);
        var xValue = new UInt8(0x10);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstCMPzpgx2 - fetch the operand into EA reg

        ExecuteClockCycles(1); // InstCMPzpgx3 - EA = EA + X

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstCMPzpgx4 - fetch the memory value at EA into temp then CMP Temp reg with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCMPzpgxWhenEQ()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x22);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCMPzpgx(aValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPzpgxWhenLT()
    {
        var aValue = new UInt8(0x11);
        var memValue = new UInt8(0x22);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCMPzpgx(aValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPzpgxWhenGT()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x11);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCMPzpgx(aValue, memValue, expectedC, expectedZ, expectedN);
    }

    // -------------------------------------------------------------------------
    // CMP absolute
    // -------------------------------------------------------------------------
    private void ExecuteCMPabs(UInt8 aValue, UInt8 memValue, BitFlag expectedC, BitFlag expectedZ,
                               BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CMPabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);

        BootToAddress(BootAddr);
        Regs.P.Carry.UpdateValue(expectedC.Copy().Not());
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstCMPabs2 - fetch the second byte of the op-code (EA low)

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstCMPabs3 - fetch the thrid byte of the op-code (EA high)

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstCMPabs4 - fetch the value at EA into Temp and do cmpare

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCMPabsWhenEQ()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x22);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCMPabs(aValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPabsWhenLT()
    {
        var aValue = new UInt8(0x11);
        var memValue = new UInt8(0x22);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCMPabs(aValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPabsWhenGT()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x11);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCMPabs(aValue, memValue, expectedC, expectedZ, expectedN);
    }

    // -------------------------------------------------------------------------
    // CMP absolute,X
    // -------------------------------------------------------------------------
    private void ExecuteCMPabsx(UInt8 aValue, UInt8 memValue, UInt8 xValue, BitFlag expectedC,
                                BitFlag expectedZ, BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CMPabsx.ToUInt8();
        var operand1 = new UInt8(0xE0);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstCMPabsx2 - fetch the operand into EAL reg

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstCMPabsx3 - fetch the operand into EAH reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstCMPabsx4 - fetch the memory value at EA into temp then CMP Temp reg with A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.SetDataBusPins(memValue);
            ExecuteClockCycles(1); // InstADDabsx5 - fetch the value at address EA + Y       
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCMPabsxWhenEQ()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x22);
        var xValue = new UInt8(0x10);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCMPabsx(aValue, memValue, xValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPabsxWhenEQPageChanged()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x22);
        var xValue = new UInt8(0x20);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCMPabsx(aValue, memValue, xValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPabsxWhenLT()
    {
        var aValue = new UInt8(0x11);
        var memValue = new UInt8(0x22);
        var xValue = new UInt8(0x10);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCMPabsx(aValue, memValue, xValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPabsxWhenLTPageChanged()
    {
        var aValue = new UInt8(0x11);
        var memValue = new UInt8(0x22);
        var xValue = new UInt8(0x20);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCMPabsx(aValue, memValue, xValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPabsxWhenGT()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x11);
        var xValue = new UInt8(0x10);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCMPabsx(aValue, memValue, xValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPabsxWhenGTPageChanged()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x11);
        var xValue = new UInt8(0x20);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCMPabsx(aValue, memValue, xValue, expectedC, expectedZ, expectedN);
    }

    // -------------------------------------------------------------------------
    // CMP absolute,Y
    // -------------------------------------------------------------------------
    private void ExecuteCMPabsy(UInt8 aValue, UInt8 memValue, UInt8 yValue, BitFlag expectedC,
                                BitFlag expectedZ, BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CMPabsy.ToUInt8();
        var operand1 = new UInt8(0xE0);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstCMPabsy2 - fetch the operand into EAL reg

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstCMPabsy3 - fetch the operand into EAH reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstCMPabsy4 - fetch the memory value at EA into temp then CMP Temp reg with A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.SetDataBusPins(memValue);
            ExecuteClockCycles(1); // InstADDabsy5 - fetch the value at address EA + Y       
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCMPabsyWhenEQ()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x22);
        var yValue = new UInt8(0x10);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCMPabsy(aValue, memValue, yValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPabsyWhenEQPageChanged()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x22);
        var yValue = new UInt8(0x20);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCMPabsy(aValue, memValue, yValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPabsyWhenLT()
    {
        var aValue = new UInt8(0x11);
        var memValue = new UInt8(0x22);
        var yValue = new UInt8(0x10);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCMPabsy(aValue, memValue, yValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPabsyWhenLTPageChanged()
    {
        var aValue = new UInt8(0x11);
        var memValue = new UInt8(0x22);
        var yValue = new UInt8(0x20);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCMPabsy(aValue, memValue, yValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPabsyWhenGT()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x11);
        var yValue = new UInt8(0x10);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCMPabsy(aValue, memValue, yValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPabsyWhenGTPageChanged()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x11);
        var yValue = new UInt8(0x20);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCMPabsy(aValue, memValue, yValue, expectedC, expectedZ, expectedN);
    }

    // -------------------------------------------------------------------------
    // CMP (indirect,X)
    // -------------------------------------------------------------------------
    private void ExecuteCMPindx(UInt8 aValue, UInt8 memValue, UInt8 xValue, BitFlag expectedC,
                                BitFlag expectedZ, BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CMPindx.ToUInt8();
        var operand = new UInt8(0x80);
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xEF);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstCMPidx2 - fetch the second byte of the op-code (EA low)

        ExecuteClockCycles(1); // InstCMPidx3 - add X reg to the EA

        Pins.SetDataBusPins(indAddrLsb);
        ExecuteClockCycles(1); // InstCMPidx4 - fetch the value at EA, put in EA2 Low

        Pins.SetDataBusPins(indAddrMsb);
        ExecuteClockCycles(1); // InstCMPidx5 - fetch the value at EA + 1, put in EA2 High

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstCMPidx6 - load the A reg. with the value at 

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCMPindxWhenEQ()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x22);
        var xValue = new UInt8(0x10);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCMPindx(aValue, memValue, xValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPindxWhenLT()
    {
        var aValue = new UInt8(0x11);
        var memValue = new UInt8(0x22);
        var xValue = new UInt8(0x10);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCMPindx(aValue, memValue, xValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPindxWhenGT()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x11);
        var xValue = new UInt8(0x10);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCMPindx(aValue, memValue, xValue, expectedC, expectedZ, expectedN);
    }

    // -------------------------------------------------------------------------
    // CMP (indirect),Y
    // -------------------------------------------------------------------------
    private void ExecuteCMPindy(UInt8 aValue, UInt8 memValue, UInt8 yValue, BitFlag expectedC,
                                BitFlag expectedZ, BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CMPindy.ToUInt8();
        var operand = new UInt8(0x4C);
        var indAddrLsb = new UInt8(0xE0);
        var indAddrMsb = new UInt8(0x43);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstCMPindy2 - fetch the second byte of the op-code (EA low)
        Assert.Equal(new UInt16(0x1001), Pins.GetAddrBusPins());

        Pins.SetDataBusPins(indAddrLsb);
        ExecuteClockCycles(1); // InstCMPindy3 - fetch the value at EA, put in EA2 Low
        Assert.Equal(new UInt16(operand), Pins.GetAddrBusPins());

        Pins.SetDataBusPins(indAddrMsb);
        ExecuteClockCycles(1); // InstCMPindy4 - fetch the value at EA + 1, put in EA2 High
        Assert.Equal(new UInt16(operand).Inc(), Pins.GetAddrBusPins());

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstCMPindy5 - load the A reg. with the value at EA + Y

        // if adding the Y reg cause the page to change
        if (!indAddrMsb.Equals(expectedAddr.Msb()))
        {
            Pins.SetDataBusPins(memValue);
            ExecuteClockCycles(1); // InstCMPindy6 - load the A reg. with the value at
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCMPindyWhenEQ()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x22);
        var yValue = new UInt8(0x10);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCMPindy(aValue, memValue, yValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPindyWhenEQPageChanged()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x22);
        var yValue = new UInt8(0x20);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCMPindy(aValue, memValue, yValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPindyWhenLT()
    {
        var aValue = new UInt8(0x11);
        var memValue = new UInt8(0x22);
        var yValue = new UInt8(0x10);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCMPindy(aValue, memValue, yValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPindyWhenLTPageChanged()
    {
        var aValue = new UInt8(0x11);
        var memValue = new UInt8(0x22);
        var yValue = new UInt8(0x20);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCMPindy(aValue, memValue, yValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPindyWhenGT()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x11);
        var yValue = new UInt8(0x10);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCMPindy(aValue, memValue, yValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPindyWhenGTPageChanged()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x11);
        var yValue = new UInt8(0x20);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCMPindy(aValue, memValue, yValue, expectedC, expectedZ, expectedN);
    }

    // -------------------------------------------------------------------------
    // CMP (indirect)
    // -------------------------------------------------------------------------
    private void ExecuteCMPind(UInt8 aValue, UInt8 memValue, BitFlag expectedC, BitFlag expectedZ,
                               BitFlag expectedN)
    {
        // ARRANGE:
        var opCode = OpCodes.CMPind.ToUInt8();
        var operand = new UInt8(0x80);
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xEF);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstCMPid2 - fetch the second byte of the op-code into EA low

        Pins.SetDataBusPins(indAddrLsb);
        ExecuteClockCycles(1); // InstCMPid3 - fetch the value at EA into EA2 Low

        Pins.SetDataBusPins(indAddrMsb);
        ExecuteClockCycles(1); // InstCMPid4 - fetch the value at EA + 1 into EA2 High

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstCMPidx5 - load the A reg. with the value at EA2

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedC, Regs.P.Carry);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
    }

    [Fact]
    public void TestCMPindWhenEQ()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x22);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.High();
        var expectedN = BitFlag.Low();

        ExecuteCMPind(aValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPindWhenLT()
    {
        var aValue = new UInt8(0x11);
        var memValue = new UInt8(0x22);
        var expectedC = BitFlag.Low();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.High();

        ExecuteCMPind(aValue, memValue, expectedC, expectedZ, expectedN);
    }

    [Fact]
    public void TestCMPindWhenGT()
    {
        var aValue = new UInt8(0x22);
        var memValue = new UInt8(0x11);
        var expectedC = BitFlag.High();
        var expectedZ = BitFlag.Low();
        var expectedN = BitFlag.Low();

        ExecuteCMPind(aValue, memValue, expectedC, expectedZ, expectedN);
    }
}
