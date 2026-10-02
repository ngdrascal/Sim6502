// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine.Types;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class ANDTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // AND immediate
    // -------------------------------------------------------------------------
    private void ExecuteANDimm(UInt8 aValue, UInt8 operand, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ANDimm.ToUInt8();
        var expectedValue = new UInt8(aValue.ToInt() & operand.ToInt());

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand;
        ExecuteClockCycles(1); // InstANDimm2 - AND A reg. with operand

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestANDimmPos()
    {
        ExecuteANDimm(new UInt8(0b00000111), new UInt8(0b00000011), Low, Low);
    }

    [Fact]
    public void TestANDimmZero()
    {
        ExecuteANDimm(new UInt8(0b11110000), new UInt8(0b00001111), High, Low);
    }

    [Fact]
    public void TestANDimmNeg()
    {
        ExecuteANDimm(new UInt8(0b11110000), new UInt8(0b11111111), Low, High);
    }

    // -------------------------------------------------------------------------
    // AND zeropage
    // -------------------------------------------------------------------------
    private void ExecuteANDzpg(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ANDzpg.ToUInt8();
        var operand = new UInt8(0x12);
        var expectedValue = aValue.Copy().And(memValue);
        var expectedAddr = new UInt16(operand);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand;
        ExecuteClockCycles(1); // InstANDzpg2 - fetch the operand into EA reg

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstANDzpg3 - fetch the memory value at EA then AND it with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestANDzpgPos()
    {
        ExecuteANDzpg(new UInt8(0b00000111), new UInt8(0b00000011), Low, Low);
    }

    [Fact]
    public void TestANDzpgZero()
    {
        ExecuteANDzpg(new UInt8(0b11110000), new UInt8(0b00001111), High, Low);
    }

    [Fact]
    public void TestANDzpgNeg()
    {
        ExecuteANDzpg(new UInt8(0b11110000), new UInt8(0b11111111), Low, High);
    }

    // -------------------------------------------------------------------------
    // AND zeropage,X
    // -------------------------------------------------------------------------
    private void ExecuteANDzpgx(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ANDzpgx.ToUInt8();
        var operand = new UInt8(0x12);
        var xValue = new UInt8(0x10);
        var expectedValue = aValue.Copy().And(memValue);
        var expectedAddr = new UInt16(operand.Copy().AddWithWrapAround(xValue));

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand;
        ExecuteClockCycles(1); // InstANDzpgx2 - fetch the operand into EA reg

        ExecuteClockCycles(1); // InstANDzpgx3 - EA = EA + X

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstANDzpgx4 - fetch the memory value at EA into temp then AND Temp reg with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestANDzpgxPos()
    {
        ExecuteANDzpgx(new UInt8(0b00000111), new UInt8(0b00000011), Low, Low);
    }

    [Fact]
    public void TestANDzpgxZero()
    {
        ExecuteANDzpgx(new UInt8(0b11110000), new UInt8(0b00001111), High, Low);
    }

    [Fact]
    public void TestANDzpgxNeg()
    {
        ExecuteANDzpgx(new UInt8(0b11110000), new UInt8(0b11111111), Low, High);
    }

    // -------------------------------------------------------------------------
    // AND absolute
    // -------------------------------------------------------------------------
    private void ExecuteANDabs(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ANDabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = aValue.Copy().And(memValue);
        var expectedAddr = new UInt16(operand1, operand2);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand1;
        ExecuteClockCycles(1); // InstANDabs2 - fetch the operand into EAL reg

        Pins.DataBus = operand2;
        ExecuteClockCycles(1); // InstANDabs3 - fetch the operand into EAH reg

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstANDabs4 - fetch the memory value at EA into temp then AND Temp reg with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestANDabsPos()
    {
        ExecuteANDabs(new UInt8(0b00000111), new UInt8(0b00000011), Low, Low);
    }

    [Fact]
    public void TestANDabsZero()
    {
        ExecuteANDabs(new UInt8(0b11110000), new UInt8(0b00001111), High, Low);
    }

    [Fact]
    public void TestANDabsNeg()
    {
        ExecuteANDabs(new UInt8(0b11110000), new UInt8(0b11111111), Low, High);
    }

    // -------------------------------------------------------------------------
    // AND absolute,X
    // -------------------------------------------------------------------------
    private void ExecuteANDabsx(UInt8 aValue, UInt8 memValue, UInt8 xValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ANDabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = aValue.Copy().And(memValue);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand1;
        ExecuteClockCycles(1); // InstANDabsx2 - fetch the operand into EAL reg

        Pins.DataBus = operand2;
        ExecuteClockCycles(1); // InstANDabsx3 - fetch the operand into EAH reg

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstANDabsx4 - fetch the memory value at EA into temp then AND Temp reg with A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.DataBus = memValue;
            ExecuteClockCycles(1); // InstADDabsx5 - fetch the value at address EA + Y       
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestANDabsxPos()
    {
        ExecuteANDabsx(new UInt8(0b00000111), new UInt8(0b00000011), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestANDabsxZero()
    {
        ExecuteANDabsx(new UInt8(0b11110000), new UInt8(0b00001111), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestANDabsxNeg()
    {
        ExecuteANDabsx(new UInt8(0b11110000), new UInt8(0b11111111), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestANDabsxPosPageChanged()
    {
        ExecuteANDabsx(new UInt8(0b00000111), new UInt8(0b00000011), new UInt8(0x80), Low, Low);
    }

    [Fact]
    public void TestANDabsxZeroPageChanged()
    {
        ExecuteANDabsx(new UInt8(0b11110000), new UInt8(0b00001111), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestANDabsxNegPageChanged()
    {
        ExecuteANDabsx(new UInt8(0b11110000), new UInt8(0b11111111), new UInt8(0x80), Low, High);
    }

    // -------------------------------------------------------------------------
    // AND absolute,Y
    // -------------------------------------------------------------------------
    private void ExecuteANDabsy(UInt8 aValue, UInt8 memValue, UInt8 yValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ANDabsy.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = aValue.Copy().And(memValue);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand1;
        ExecuteClockCycles(1); // InstANDabsy2 - fetch the operand into EAL reg

        Pins.DataBus = operand2;
        ExecuteClockCycles(1); // InstANDabsy3 - fetch the operand into EAH reg

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstANDabsy4 - fetch the memory value at EA into temp then AND Temp reg with A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.DataBus = memValue;
            ExecuteClockCycles(1); // InstADDabsy5 - fetch the value at address EA + Y       
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestANDabsyPos()
    {
        ExecuteANDabsy(new UInt8(0b00000111), new UInt8(0b00000011), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestANDabsyZero()
    {
        ExecuteANDabsy(new UInt8(0b11110000), new UInt8(0b00001111), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestANDabsyNeg()
    {
        ExecuteANDabsy(new UInt8(0b11110000), new UInt8(0b11111111), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestANDabsyPosPageChanged()
    {
        ExecuteANDabsy(new UInt8(0b00000111), new UInt8(0b00000011), new UInt8(0x80), Low, Low);
    }

    [Fact]
    public void TestANDabsyZeroPageChanged()
    {
        ExecuteANDabsy(new UInt8(0b11110000), new UInt8(0b00001111), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestANDabsyNegPageChanged()
    {
        ExecuteANDabsy(new UInt8(0b11110000), new UInt8(0b11111111), new UInt8(0x80), Low, High);
    }

    // -------------------------------------------------------------------------
    // AND (indirect,X)
    // -------------------------------------------------------------------------
    private void ExecuteANDindx(UInt8 aValue, UInt8 memValue, UInt8 xValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ANDindx.ToUInt8();
        var operand = new UInt8(0x80);
        var expectedValue = aValue.Copy().And(memValue);
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xEF);
        var finalAddr = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = operand;
        ExecuteClockCycles(1); // InstANDidx2 - fetch the second byte of the op-code (EA low)

        ExecuteClockCycles(1); // InstANDidx3 - add X reg to the EA

        Pins.DataBus = indAddrLsb;
        ExecuteClockCycles(1); // InstANDidx4 - fetch the value at EA, put in EA2 Low

        Pins.DataBus = indAddrMsb;
        ExecuteClockCycles(1); // InstANDidx5 - fetch the value at EA + 1, put in EA2 High

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstANDidx6 - load the A reg. with the value at EA2

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(finalAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestANDindxPos()
    {
        ExecuteANDindx(new UInt8(0b00000111), new UInt8(0b00000011), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestANDindxZero()
    {
        ExecuteANDindx(new UInt8(0b11110000), new UInt8(0b00001111), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestANDindxNeg()
    {
        ExecuteANDindx(new UInt8(0b11110000), new UInt8(0b11111111), new UInt8(0x10), Low, High);
    }

    // -------------------------------------------------------------------------
    // AND (indirect),Y
    // -------------------------------------------------------------------------
    private void ExecuteANDindy(UInt8 aValue, UInt8 memValue, UInt8 yValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ANDindy.ToUInt8();
        var operand = new UInt8(0x4C);
        var expectedValue = aValue.Copy().And(memValue);
        var indAddrLsb = new UInt8(0x41);
        var indAddrMsb = new UInt8(0x0C);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand;
        ExecuteClockCycles(1); // InstANDindy2 - fetch the second byte of the op-code (EA low)
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);

        Pins.DataBus = indAddrLsb;
        ExecuteClockCycles(1); // InstANDindy3 - fetch the value at EA, put in EA2 Low
        Assert.Equal(new UInt16(operand), Pins.AddrBus);

        Pins.DataBus = indAddrMsb;
        ExecuteClockCycles(1); // InstANDindy4 - fetch the value at EA + 1, put in EA2 High
        Assert.Equal(new UInt16(operand).Inc(), Pins.AddrBus);

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstANDindy5 - load the A reg. with the value at EA + Y

        // if adding the Y reg cause the page to change
        if (!indAddrMsb.Equals(expectedAddr.Msb()))
        {
            Pins.DataBus = aValue;
            ExecuteClockCycles(1); // InstANDindy6 - load the A reg. with the value at
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestANDindyPos()
    {
        ExecuteANDindy(new UInt8(0b00000111), new UInt8(0b00000011), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestANDindyZero()
    {
        ExecuteANDindy(new UInt8(0b11110000), new UInt8(0b00001111), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestANDindyNeg()
    {
        ExecuteANDindy(new UInt8(0b11110000), new UInt8(0b11111111), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestANDindyPosPageChanged()
    {
        ExecuteANDindy(new UInt8(0b00000111), new UInt8(0b00000011), new UInt8(0x80), Low, Low);
    }

    [Fact]
    public void TestANDindyZeroPageChanged()
    {
        ExecuteANDindy(new UInt8(0b11110000), new UInt8(0b00001111), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestANDindyNegPageChanged()
    {
        ExecuteANDindy(new UInt8(0b11110000), new UInt8(0b11111111), new UInt8(0x80), Low, High);
    }

    // -------------------------------------------------------------------------
    // AND (indirect)
    // -------------------------------------------------------------------------
    private void ExecuteANDind(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.ANDind.ToUInt8();
        var operand = new UInt8(0x80);
        var expectedValue = aValue.Copy().And(memValue);
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xEF);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = operand;
        ExecuteClockCycles(1); // InstANDid2 - fetch the second byte of the op-code into EA low

        Pins.DataBus = indAddrLsb;
        ExecuteClockCycles(1); // InstANDid3 - fetch the value at EA into EA2 Low

        Pins.DataBus = indAddrMsb;
        ExecuteClockCycles(1); // InstANDid4 - fetch the value at EA + 1 into EA2 High

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstANDidx5 - load the A reg. with the value at EA2

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestANDindPos()
    {
        ExecuteANDind(new UInt8(0b00000111), new UInt8(0b00000011), Low, Low);
    }

    [Fact]
    public void TestANDindZero()
    {
        ExecuteANDind(new UInt8(0b11110000), new UInt8(0b00001111), High, Low);
    }

    [Fact]
    public void TestANDindNeg()
    {
        ExecuteANDind(new UInt8(0b11110000), new UInt8(0b11111111), Low, High);
    }
}
