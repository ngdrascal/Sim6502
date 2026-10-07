// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine.Types;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class EORTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // EOR immediate
    // -------------------------------------------------------------------------
    private void ExecuteEORimm(UInt8 aValue, UInt8 operand, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.EORimm.ToUInt8();
        var expectedValue = new UInt8(aValue.ToInt() ^ operand.ToInt());

        BootToAddress(BootAddr);
        Regs.A = aValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstEORimm2 - EOR A reg. with operand

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestEORimmPos()
    {
        ExecuteEORimm(new UInt8(0b00000001), new UInt8(0b00000010), Low, Low);
    }

    [Fact]
    public void TestEORimmZero()
    {
        ExecuteEORimm(new UInt8(0b00000001), new UInt8(0b00000001), High, Low);
    }

    [Fact]
    public void TestEORimmNeg()
    {
        ExecuteEORimm(new UInt8(0b10000000), new UInt8(0b00000000), Low, High);
    }

    // -------------------------------------------------------------------------
    // EOR zeropage
    // -------------------------------------------------------------------------
    private void ExecuteEORzpg(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.EORzpg.ToUInt8();
        var operand = new UInt8(0x12);
        var expectedValue = new UInt8(aValue.ToInt() ^ memValue.ToInt());
        var expectedAddr = new UInt16(operand);

        BootToAddress(BootAddr);
        Regs.A = aValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstEORzpg2 - fetch the operand into EA reg

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstEORzpg3 - fetch the memory value at EA then EOR it with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestEORzpgPos()
    {
        ExecuteEORzpg(new UInt8(0b00000001), new UInt8(0b00000010), Low, Low);
    }

    [Fact]
    public void TestEORzpgZero()
    {
        ExecuteEORzpg(new UInt8(0b00000001), new UInt8(0b00000001), High, Low);
    }

    [Fact]
    public void TestEORzpgNeg()
    {
        ExecuteEORzpg(new UInt8(0b10000000), new UInt8(0b00000000), Low, High);
    }

    // -------------------------------------------------------------------------
    // EOR zeropage,X
    // -------------------------------------------------------------------------
    private void ExecuteEORzpgx(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.EORzpgx.ToUInt8();
        var operand = new UInt8(0x12);
        var xValue = new UInt8(0x10);
        var expectedValue = new UInt8(aValue.ToInt() ^ memValue.ToInt());
        var expectedAddr = new UInt16(operand.Copy().AddWithWrapAround(xValue));

        BootToAddress(BootAddr);
        Regs.A = aValue;
        Regs.X = xValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstEORzpgx2 - fetch the operand into EA reg

        ExecuteClockCycles(1); // InstEORzpgx3 - EA = EA + X

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstEORzpgx4 - fetch the memory value at EA into temp then EOR Temp reg with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestEORzpgxPos()
    {
        ExecuteEORzpgx(new UInt8(0b00000001), new UInt8(0b00000010), Low, Low);
    }

    [Fact]
    public void TestEORzpgxZero()
    {
        ExecuteEORzpgx(new UInt8(0b00000001), new UInt8(0b00000001), High, Low);
    }

    [Fact]
    public void TestEORzpgxNeg()
    {
        ExecuteEORzpgx(new UInt8(0b10000000), new UInt8(0b00000000), Low, High);
    }

    // -------------------------------------------------------------------------
    // EOR absolute
    // -------------------------------------------------------------------------
    private void ExecuteEORabs(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.EORabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = new UInt8(aValue.ToInt() ^ memValue.ToInt());
        var expectedAddr = new UInt16(operand1, operand2);

        BootToAddress(BootAddr);
        Regs.A = aValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstEORabs2 - fetch the operand into EAL reg

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstEORabs3 - fetch the operand into EAH reg

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstEORabs4 - fetch the memory value at EA into temp then EOR Temp reg with A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestEORabsPos()
    {
        ExecuteEORabs(new UInt8(0b00000001), new UInt8(0b00000010), Low, Low);
    }

    [Fact]
    public void TestEORabsZero()
    {
        ExecuteEORabs(new UInt8(0b00000001), new UInt8(0b00000001), High, Low);
    }

    [Fact]
    public void TestEORabsNeg()
    {
        ExecuteEORabs(new UInt8(0b10000000), new UInt8(0b00000000), Low, High);
    }

    // -------------------------------------------------------------------------
    // EOR absolute,X
    // -------------------------------------------------------------------------
    private void ExecuteEORabsx(UInt8 aValue, UInt8 memValue, UInt8 xValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.EORabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = new UInt8(aValue.ToInt() ^ memValue.ToInt());
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);

        BootToAddress(BootAddr);
        Regs.A = aValue;
        Regs.X = xValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstEORabsx2 - fetch the operand into EAL reg

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstEORabsx3 - fetch the operand into EAH reg

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstEORabsx4 - fetch the memory value at EA into temp then EOR Temp reg with A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.DataBus = (memValue);
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
    public void TestEORabsxPos()
    {
        ExecuteEORabsx(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestEORabsxZero()
    {
        ExecuteEORabsx(new UInt8(0b00000001), new UInt8(0b00000001), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestEORabsxNeg()
    {
        ExecuteEORabsx(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestEORabsxPosPageChanged()
    {
        ExecuteEORabsx(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x80), Low, Low);
    }

    [Fact]
    public void TestEORabsxZeroPageChanged()
    {
        ExecuteEORabsx(new UInt8(0b00000001), new UInt8(0b00000001), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestEORabsxNegPageChanged()
    {
        ExecuteEORabsx(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x80), Low, High);
    }

    // -------------------------------------------------------------------------
    // EOR absolute,Y
    // -------------------------------------------------------------------------
    private void ExecuteEORabsy(UInt8 aValue, UInt8 memValue, UInt8 yValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.EORabsy.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = new UInt8(aValue.ToInt() ^ memValue.ToInt());
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.A = aValue;
        Regs.Y = yValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstEORabsy2 - fetch the operand into EAL reg

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstEORabsy3 - fetch the operand into EAH reg

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstEORabsy4 - fetch the memory value at EA into temp then EOR Temp reg with A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.DataBus = (memValue);
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
    public void TestEORabsyPos()
    {
        ExecuteEORabsy(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestEORabsyZero()
    {
        ExecuteEORabsy(new UInt8(0b00000001), new UInt8(0b00000001), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestEORabsyNeg()
    {
        ExecuteEORabsy(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestEORabsyPosPageChanged()
    {
        ExecuteEORabsy(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x80), Low, Low);
    }

    [Fact]
    public void TestEORabsyZeroPageChanged()
    {
        ExecuteEORabsy(new UInt8(0b00000001), new UInt8(0b00000001), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestEORabsyNegPageChanged()
    {
        ExecuteEORabsy(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x80), Low, High);
    }

    // -------------------------------------------------------------------------
    // EOR (indirect,X)
    // -------------------------------------------------------------------------
    private void ExecuteEORindx(UInt8 aValue, UInt8 memValue, UInt8 xValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.EORindx.ToUInt8();
        var operand = new UInt8(0x80);
        var expectedValue = new UInt8(aValue.ToInt() ^ memValue.ToInt());
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xEF);
        var finalAddr = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);
        Regs.A = aValue;
        Regs.X = xValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstEORidx2 - fetch the second byte of the op-code (EA low)

        ExecuteClockCycles(1); // InstEORidx3 - add X reg to the EA

        Pins.DataBus = (indAddrLsb);
        ExecuteClockCycles(1); // InstEORidx4 - fetch the value at EA, put in EA2 Low

        Pins.DataBus = (indAddrMsb);
        ExecuteClockCycles(1); // InstEORidx5 - fetch the value at EA + 1, put in EA2 High

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstEORidx6 - load the A reg. with the value at 

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(finalAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestEORindxPos()
    {
        ExecuteEORindx(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestEORindxZero()
    {
        ExecuteEORindx(new UInt8(0b00000001), new UInt8(0b00000001), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestEORindxNeg()
    {
        ExecuteEORindx(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x10), Low, High);
    }

    // -------------------------------------------------------------------------
    // EOR (indirect),Y
    // -------------------------------------------------------------------------
    private void ExecuteEORindy(UInt8 aValue, UInt8 memValue, UInt8 yValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.EORindy.ToUInt8();
        var operand = new UInt8(0x4C);
        var expectedValue = new UInt8(aValue.ToInt() ^ memValue.ToInt());
        var indAddrLsb = new UInt8(0x41);
        var indAddrMsb = new UInt8(0x0C);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.A = aValue;
        Regs.Y = yValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstEORindy2 - fetch the second byte of the op-code (EA low)
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);

        Pins.DataBus = (indAddrLsb);
        ExecuteClockCycles(1); // InstEORindy3 - fetch the value at EA, put in EA2 Low
        Assert.Equal(new UInt16(operand), Pins.AddrBus);

        Pins.DataBus = (indAddrMsb);
        ExecuteClockCycles(1); // InstEORindy4 - fetch the value at EA + 1, put in EA2 High
        Assert.Equal(new UInt16(operand).Inc(), Pins.AddrBus);

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstEORindy5 - load the A reg. with the value at EA + Y

        // if adding the Y reg cause the page to change
        if (!indAddrMsb.Equals(expectedAddr.Msb()))
        {
            Pins.DataBus = (aValue);
            ExecuteClockCycles(1); // InstEORindy6 - load the A reg. with the value at
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
    public void TestEORindyPos()
    {
        ExecuteEORindy(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestEORindyZero()
    {
        ExecuteEORindy(new UInt8(0b00000001), new UInt8(0b00000001), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestEORindyNeg()
    {
        ExecuteEORindy(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestEORindyPosPageChanged()
    {
        ExecuteEORindy(new UInt8(0b00000001), new UInt8(0b00000010), new UInt8(0x80), Low, Low);
    }

    [Fact]
    public void TestEORindyZeroPageChanged()
    {
        ExecuteEORindy(new UInt8(0b00000001), new UInt8(0b00000001), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestEORindyNegPageChanged()
    {
        ExecuteEORindy(new UInt8(0b10000000), new UInt8(0b00000000), new UInt8(0x80), Low, High);
    }

    // -------------------------------------------------------------------------
    // EOR (indirect)
    // -------------------------------------------------------------------------
    private void ExecuteEORind(UInt8 aValue, UInt8 memValue, BitFlag expectedZFlag,
                               BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.EORind.ToUInt8();
        var operand = new UInt8(0x80);
        var expectedValue = new UInt8(aValue.ToInt() ^ memValue.ToInt());
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xEF);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);
        Regs.A = aValue;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstEORid2 - fetch the second byte of the op-code into EA low

        Pins.DataBus = (indAddrLsb);
        ExecuteClockCycles(1); // InstEORid3 - fetch the value at EA into EA2 Low

        Pins.DataBus = (indAddrMsb);
        ExecuteClockCycles(1); // InstEORid4 - fetch the value at EA + 1 into EA2 High

        Pins.DataBus = (memValue);
        ExecuteClockCycles(1); // InstEORidx5 - load the A reg. with the value at EA2

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestEORindPos()
    {
        ExecuteEORind(new UInt8(0b00000001), new UInt8(0b00000010), Low, Low);
    }

    [Fact]
    public void TestEORindZero()
    {
        ExecuteEORind(new UInt8(0b00000001), new UInt8(0b00000001), High, Low);
    }

    [Fact]
    public void TestEORindNeg()
    {
        ExecuteEORind(new UInt8(0b10000000), new UInt8(0b00000000), Low, High);
    }
}
