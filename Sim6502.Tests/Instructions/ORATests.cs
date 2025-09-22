// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class ORATests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // ORA immediate
    // -------------------------------------------------------------------------
    private void ExecuteORAimm(UInt8 aValue, UInt8 operand, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ORAimm.ToUInt8();
        var expectedValue = new UInt8(aValue.ToInt() | operand.ToInt());

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstORAimm2 - ORA A reg. with operand

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestORAimmPos()
    {
        ExecuteORAimm(new UInt8(0b00000001), new UInt8(0b00000010), Low, Low);
    }

    [Fact]
    public void TestORAimmZero()
    {
        ExecuteORAimm(new UInt8(0b00000000), new UInt8(0b00000000), High, Low);
    }

    [Fact]
    public void TestORAimmNeg()
    {
        ExecuteORAimm(new UInt8(0b11110000), new UInt8(0b11111111), Low, High);
    }

    // -------------------------------------------------------------------------
    // ORA zeropage
    // -------------------------------------------------------------------------
    private void ExecuteORAzpg(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ORAzpg.ToUInt8();
        var operand = new UInt8(0x12);
        var expectedValue = aValue.Copy().Or(memValue);
        var expectedAddr = new UInt16(operand);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstORAzpg2 - fetch the operand into EA reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstORAzpg3 - fetch the memory value at EA then ORA it with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestORAzpgPos()
    {
        ExecuteORAzpg(new UInt8(0b00000001), new UInt8(0b00000010), Low, Low);
    }

    [Fact]
    public void TestORAzpgZero()
    {
        ExecuteORAzpg(new UInt8(0b00000000), new UInt8(0b00000000), High, Low);
    }

    [Fact]
    public void TestORAzpgNeg()
    {
        ExecuteORAzpg(new UInt8(0b11110000), new UInt8(0b11111111), Low, High);
    }

    // -------------------------------------------------------------------------
    // ORA zeropage,X
    // -------------------------------------------------------------------------
    private void ExecuteORAzpgx(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ORAzpgx.ToUInt8();
        var operand = new UInt8(0x12);
        var xValue = new UInt8(0x10);
        var expectedValue = aValue.Copy().Or(memValue);
        var expectedAddr = new UInt16(operand.Copy().AddWithWrapAround(xValue));

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstORAzpgx2 - fetch the operand into EA reg

        ExecuteClockCycles(1); // InstORAzpgx3 - EA = EA + X

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstORAzpgx4 - fetch the memory value at EA into temp then ORA Temp reg with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestORAzpgxPos()
    {
        ExecuteORAzpgx(new UInt8(0b00000001), new UInt8(0b00000010), Low, Low);
    }

    [Fact]
    public void TestORAzpgxZero()
    {
        ExecuteORAzpgx(new UInt8(0b00000000), new UInt8(0b00000000), High, Low);
    }

    [Fact]
    public void TestORAzpgxNeg()
    {
        ExecuteORAzpgx(new UInt8(0b11110000), new UInt8(0b11111111), Low, High);
    }

    // -------------------------------------------------------------------------
    // ORA absolute
    // -------------------------------------------------------------------------
    private void ExecuteORAabs(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ORAabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = aValue.Copy().Or(memValue);
        var expectedAddr = new UInt16(operand1, operand2);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstORAabs2 - fetch the operand into EAL reg

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstORAabs3 - fetch the operand into EAH reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstORAabs4 - fetch the memory value at EA into temp then ORA Temp reg with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestORAabsPos()
    {
        ExecuteORAabs(new UInt8(0b00000001), new UInt8(0b00000010), Low, Low);
    }

    [Fact]
    public void TestORAabsZero()
    {
        ExecuteORAabs(new UInt8(0b00000000), new UInt8(0b00000000), High, Low);
    }

    [Fact]
    public void TestORAabsNeg()
    {
        ExecuteORAabs(new UInt8(0b11110000), new UInt8(0b11111111), Low, High);
    }

    // -------------------------------------------------------------------------
    // ORA absolute,X
    // -------------------------------------------------------------------------
    private void ExecuteORAabsx(UInt8 aValue, UInt8 memValue, UInt8 xValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ORAabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = aValue.Copy().Or(memValue);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstORAabsx2 - fetch the operand into EAL reg

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstORAabsx3 - fetch the operand into EAH reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstORAabsx4 - fetch the memory value at EA into temp then ORA Temp reg with A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.SetDataBusPins(memValue);
            ExecuteClockCycles(1); // InstADDabsx5 - fetch the value at address EA + Y       
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestORAabsxPos()
    {
        ExecuteORAabsx(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestORAabsxZero()
    {
        ExecuteORAabsx(new UInt8(0b00000000), new UInt8(0b00000000), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestORAabsxNeg()
    {
        ExecuteORAabsx(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestORAabsxPosPageChanged()
    {
        ExecuteORAabsx(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x80), Low, Low);
    }

    [Fact]
    public void TestORAabsxZeroPageChanged()
    {
        ExecuteORAabsx(new UInt8(0b00000000), new UInt8(0b00000000), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestORAabsxNegPageChanged()
    {
        ExecuteORAabsx(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x80), Low, High);
    }

    // -------------------------------------------------------------------------
    // ORA absolute,Y
    // -------------------------------------------------------------------------
    private void ExecuteORAabsy(UInt8 aValue, UInt8 memValue, UInt8 yValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ORAabsy.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = aValue.Copy().Or(memValue);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstORAabsy2 - fetch the operand into EAL reg

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstORAabsy3 - fetch the operand into EAH reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstORAabsy4 - fetch the memory value at EA into temp then ORA Temp reg with A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.SetDataBusPins(memValue);
            ExecuteClockCycles(1); // InstADDabsy5 - fetch the value at address EA + Y       
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestORAabsyPos()
    {
        ExecuteORAabsy(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestORAabsyZero()
    {
        ExecuteORAabsy(new UInt8(0b00000000), new UInt8(0b00000000), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestORAabsyNeg()
    {
        ExecuteORAabsy(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestORAabsyPosPageChanged()
    {
        ExecuteORAabsy(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x80), Low, Low);
    }

    [Fact]
    public void TestORAabsyZeroPageChanged()
    {
        ExecuteORAabsy(new UInt8(0b00000000), new UInt8(0b00000000), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestORAabsyNegPageChanged()
    {
        ExecuteORAabsy(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x80), Low, High);
    }

    // -------------------------------------------------------------------------
    // ORA (indirect,X)
    // -------------------------------------------------------------------------
    private void ExecuteORAindx(UInt8 aValue, UInt8 memValue, UInt8 xValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ORAindx.ToUInt8();
        var operand = new UInt8(0x80);
        var expectedValue = aValue.Copy().Or(memValue);
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xEF);
        var finalAddr = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstORAidx2 - fetch the second byte of the op-code (EA low)

        ExecuteClockCycles(1); // InstORAidx3 - add X reg to the EA

        Pins.SetDataBusPins(indAddrLsb);
        ExecuteClockCycles(1); // InstORAidx4 - fetch the value at EA, put in EA2 Low

        Pins.SetDataBusPins(indAddrMsb);
        ExecuteClockCycles(1); // InstORAidx5 - fetch the value at EA + 1, put in EA2 High

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstORAidx6 - load the A reg. with the value at 

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestORAindxPos()
    {
        ExecuteORAindx(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestORAindxZero()
    {
        ExecuteORAindx(new UInt8(0b00000000), new UInt8(0b00000000), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestORAindxNeg()
    {
        ExecuteORAindx(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x10), Low, High);
    }

    // -------------------------------------------------------------------------
    // ORA (indirect),Y
    // -------------------------------------------------------------------------
    private void ExecuteORAindy(UInt8 aValue, UInt8 memValue, UInt8 yValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ORAindy.ToUInt8();
        var operand = new UInt8(0x4C);
        var expectedValue = aValue.Copy().Or(memValue);
        var indAddrLsb = new UInt8(0x41);
        var indAddrMsb = new UInt8(0x0C);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstORAindy2 - fetch the second byte of the op-code (EA low)
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);

        Pins.SetDataBusPins(indAddrLsb);
        ExecuteClockCycles(1); // InstORAindy3 - fetch the value at EA, put in EA2 Low
        Assert.Equal(new UInt16(operand), Pins.AddrBus);

        Pins.SetDataBusPins(indAddrMsb);
        ExecuteClockCycles(1); // InstORAindy4 - fetch the value at EA + 1, put in EA2 High
        Assert.Equal(new UInt16(operand).Inc(), Pins.AddrBus);

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstORAindy5 - load the A reg. with the value at EA + Y

        // if adding the Y reg cause the page to change
        if (!indAddrMsb.Equals(expectedAddr.Msb()))
        {
            Pins.SetDataBusPins(aValue);
            ExecuteClockCycles(1); // InstORAindy6 - load the A reg. with the value at
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestORAindyPos()
    {
        ExecuteORAindy(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestORAindyZero()
    {
        ExecuteORAindy(new UInt8(0b00000000), new UInt8(0b00000000), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestORAindyNeg()
    {
        ExecuteORAindy(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestORAindyPosPageChanged()
    {
        ExecuteORAindy(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x80), Low, Low);
    }

    [Fact]
    public void TestORAindyZeroPageChanged()
    {
        ExecuteORAindy(new UInt8(0b00000000), new UInt8(0b00000000), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestORAindyNegPageChanged()
    {
        ExecuteORAindy(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x80), Low, High);
    }

    // -------------------------------------------------------------------------
    // ORA (indirect)
    // -------------------------------------------------------------------------
    private void ExecuteORAind(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ORAind.ToUInt8();
        var operand = new UInt8(0x80);
        var expectedValue = aValue.Copy().Or(memValue);
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xEF);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstORAid2 - fetch the second byte of the op-code into EA low

        Pins.SetDataBusPins(indAddrLsb);
        ExecuteClockCycles(1); // InstORAid3 - fetch the value at EA into EA2 Low

        Pins.SetDataBusPins(indAddrMsb);
        ExecuteClockCycles(1); // InstORAid4 - fetch the value at EA + 1 into EA2 High

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstORAidx5 - load the A reg. with the value at EA2

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestORAindPos()
    {
        ExecuteORAind(new UInt8(0b00000001), new UInt8(0b00000010), Low, Low);
    }

    [Fact]
    public void TestORAindZero()
    {
        ExecuteORAind(new UInt8(0b00000000), new UInt8(0b00000000), High, Low);
    }

    [Fact]
    public void TestORAindNeg()
    {
        ExecuteORAind(new UInt8(0b10000000), new UInt8(0b00000000), Low, High);
    }
}
