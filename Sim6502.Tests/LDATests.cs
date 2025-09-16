// ReSharper disable InconsistentNaming
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests;

public class LDATests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // LDA immediate
    // -------------------------------------------------------------------------
    private void ExecuteLDAimm(UInt8 operand1, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.LDAimm.ToUInt8();

        BootToAddress(BootAddr);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstLDAimm - load operand1 into A reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(operand1, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDAimmWith0()
    {
        ExecuteLDAimm(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDAimmWithNeg()
    {
        ExecuteLDAimm(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDAimmWithNotZeroNotNeg()
    {
        ExecuteLDAimm(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDA zeropage
    // -------------------------------------------------------------------------
    private void ExecuteLDAzpg(UInt8 data, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.LDAzpg.ToUInt8();
        var operand = new UInt8(0x12);

        BootToAddress(BootAddr);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstLDAzpg2 - fetch the second byte of the op-code (zpg-value)

        Pins.SetDataBusPins(data);
        ExecuteClockCycles(1); // InstLDAzpg3 - fetch the value at address zpg-value

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x12), Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(data, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDAzpgWith0()
    {
        ExecuteLDAzpg(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDAzpgWithNeg()
    {
        ExecuteLDAzpg(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDAzpgWithNotZeroNotNeg()
    {
        ExecuteLDAzpg(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDA zeropage,X
    // -------------------------------------------------------------------------
    private void ExecuteLDAzpgX(UInt8 expectedA, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.LDAzpgx.ToUInt8();
        var operand1 = new UInt8(0xE0);
        var xValue = new UInt8(0x10);
        var yValue = new UInt8(0x10);
        var finalAddr = new UInt16(operand1).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);

        // ACT:
        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstLDAzpgX2 - fetch the second byte of the op-code (zpg-value)

        ExecuteClockCycles(1); // InstLDAzpgX3 - zpg-value + X

        Pins.SetDataBusPins(expectedA);
        ExecuteClockCycles(1); // InstLDAzpgX4 - fetch the value at address zpg-value + X

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedA, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDAzpgXWith0()
    {
        ExecuteLDAzpgX(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDAzpgXWithNeg()
    {
        ExecuteLDAzpgX(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDAzpgXWithNotZeroNotNeg()
    {
        ExecuteLDAzpgX(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDA absolute
    // -------------------------------------------------------------------------
    private void ExecuteLDAabs(UInt8 data, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.LDAabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var finalAddr = new UInt16(operand1, operand2);

        BootToAddress(BootAddr);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstLDAabs2 - fetch the second byte of the op-code (EA low)

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstLDAabs3 - fetch the third byte of the op-code (EA high)

        Pins.SetDataBusPins(data);
        ExecuteClockCycles(1); // InstLDAabs4 - fetch the value at EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(data, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDAabsWith0()
    {
        ExecuteLDAabs(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDAabsWithNeg()
    {
        ExecuteLDAabs(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDAabsWithNotZeroNotNeg()
    {
        ExecuteLDAabs(new UInt8(1), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDA absolute,X
    // -------------------------------------------------------------------------
    private void ExecuteLDAabsX(UInt8 data, UInt8 xValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.LDAabsx.ToUInt8();
        var operand1 = new UInt8(0x80);
        var operand2 = new UInt8(0x04);
        var opAddr = new UInt16(operand1, operand2);
        var finalAddr = opAddr.Copy().AddUnsigned(xValue);

        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        // fetch the opcode
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        // ACT:
        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstLDAabsx2 - fetch the second byte of the op-code (EA low)

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstLDAabsx3 - fetch the third byte of the op-code (EA high)

        Pins.SetDataBusPins(data);
        ExecuteClockCycles(1); // InstLDAabsx4 - fetch the value at address EA + X

        // if adding the X reg cause the page to change
        if (!finalAddr.Msb().Equals(operand2))
        {
            Pins.SetDataBusPins(data);
            ExecuteClockCycles(1); // InstLDAabsx5 - fetch the value at address EA + X
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(data, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDAabsXWith0()
    {
        ExecuteLDAabsX(new UInt8(0), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestLDAabsXWithNeg()
    {
        ExecuteLDAabsX(new UInt8(0xFF), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestLDAabsXWithNotZeroNotNeg()
    {
        ExecuteLDAabsX(new UInt8(1), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestLDAabsXWith0PageChange()
    {
        ExecuteLDAabsX(new UInt8(0), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestLDAabsXWithNegPageChange()
    {
        ExecuteLDAabsX(new UInt8(0xFF), new UInt8(0x80), Low, High);
    }

    [Fact]
    public void TestLDAabsXWithNotZeroNotNegPageChange()
    {
        ExecuteLDAabsX(new UInt8(1), new UInt8(0x80), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDA absolute,Y
    // -------------------------------------------------------------------------
    private void ExecuteLDAabsY(UInt8 data, UInt8 yValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.LDAabsy.ToUInt8();
        var operand1 = new UInt8(0x80);
        var operand2 = new UInt8(0x04);
        var opAddr = new UInt16(operand1, operand2);
        var finalAddr = opAddr.Copy().AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.Y.UpdateValue(yValue);

        // fetch the opcode
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);

        // ACT:
        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstLDAabsy2 - fetch the second byte of the op-code (EA low)

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstLDAabsy3 - fetch the third byte of the op-code (EA high)

        Pins.SetDataBusPins(data);
        ExecuteClockCycles(1); // InstLDAabsy4 - fetch the value at address zpg-value + Y

        // if adding the Y reg cause the page to change
        if (!opAddr.Msb().Equals(finalAddr.Msb()))
        {
            Pins.SetDataBusPins(data);
            ExecuteClockCycles(1); // InstLDAabsy5 - fetch the value at address zpg-value + Y
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(data, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDAabsYWith0()
    {
        ExecuteLDAabsY(new UInt8(0), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestLDAabsYWithNeg()
    {
        ExecuteLDAabsY(new UInt8(0xFF), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestLDAabsYWithNotZeroNotNeg()
    {
        ExecuteLDAabsY(new UInt8(1), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestLDAabsYWith0PageChange()
    {
        ExecuteLDAabsY(new UInt8(0), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestLDAabsYWithNegPageChange()
    {
        ExecuteLDAabsY(new UInt8(0xFF), new UInt8(0x80), Low, High);
    }

    [Fact]
    public void TestLDAabsYWithNotZeroNotNegPageChange()
    {
        ExecuteLDAabsY(new UInt8(1), new UInt8(0x80), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDA (indirect,X)
    // -------------------------------------------------------------------------
    private void ExecuteLDAindx(UInt8 data, UInt8 xValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.LDAindx.ToUInt8();
        var operand1 = new UInt8(0x80);
        var addrLsb = new UInt8(0x22);
        var addrMsb = new UInt8(0xEF);
        var finalAddr = new UInt16(addrLsb, addrMsb);

        BootToAddress(BootAddr);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstLDAidx2 - fetch the second byte of the op-code (EA low)

        ExecuteClockCycles(1); // InstLDAidx3 - add X reg to the EA

        Pins.SetDataBusPins(addrLsb);
        ExecuteClockCycles(1); // InstLDAidx4 - fetch the value at EA, put in EA2 Low

        Pins.SetDataBusPins(addrMsb);
        ExecuteClockCycles(1); // InstLDAidx5 - fetch the value at EA + 1, put in EA2 High

        Pins.SetDataBusPins(data);
        ExecuteClockCycles(1); // InstLDAidx6 - load the A reg. with the value at

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(data, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDAindxWith0()
    {
        ExecuteLDAindx(new UInt8(0), new UInt8(0x80), High, Low);
    }

    [Fact]
    public void TestLDAindxWithNeg()
    {
        ExecuteLDAindx(new UInt8(0xFF), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestLDAindxWithNotZeroNotNeg()
    {
        ExecuteLDAindx(new UInt8(1), new UInt8(0x10), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDA (indirect),Y
    // -------------------------------------------------------------------------
    private void ExecuteLDAindy(UInt8 data, UInt8 yValue, BitFlag expectedZFlag,
                                BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.LDAindy.ToUInt8();
        var operand1 = new UInt8(0x4C);
        var addrLsb = new UInt8(0x41);
        var addrMsb = new UInt8(0x0C);
        var finalAddr = new UInt16(addrLsb, addrMsb).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstLDAindy2 - fetch the second byte of the op-code (EA low)
        Assert.Equal(new UInt16(0x1001), Pins.GetAddrBusPins());

        Pins.SetDataBusPins(addrLsb);
        ExecuteClockCycles(1); // InstLDAindy3 - fetch the value at EA, put in EA2 Low
        Assert.Equal(new UInt16(operand1), Pins.GetAddrBusPins());

        Pins.SetDataBusPins(addrMsb);
        ExecuteClockCycles(1); // InstLDAindy4 - fetch the value at EA + 1, put in EA2 High
        Assert.Equal(new UInt16(operand1).Inc(), Pins.GetAddrBusPins());

        Pins.SetDataBusPins(data);
        ExecuteClockCycles(1); // InstLDAindy5 - load the A reg. with the value at EA + Y

        // if adding the Y reg cause the page to change
        if (!addrMsb.Equals(finalAddr.Msb()))
        {
            Pins.SetDataBusPins(data);
            ExecuteClockCycles(1); // InstLDAindy6 - load the A reg. with the value at
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(data, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDAindyWith0()
    {
        ExecuteLDAindy(new UInt8(0), new UInt8(0x10), High, Low);
    }

    [Fact]
    public void TestLDAindyWithNeg()
    {
        ExecuteLDAindy(new UInt8(0xFF), new UInt8(0x10), Low, High);
    }

    [Fact]
    public void TestLDAindyWithNotZeroNotNeg()
    {
        ExecuteLDAindy(new UInt8(1), new UInt8(0x10), Low, Low);
    }

    [Fact]
    public void TestLDAindyWith0PageChange()
    {
        ExecuteLDAindy(new UInt8(0), new UInt8(0xBF), High, Low);
    }

    [Fact]
    public void TestLDAindyWithNegPageChange()
    {
        ExecuteLDAindy(new UInt8(0xFF), new UInt8(0xBF), Low, High);
    }

    [Fact]
    public void TestLDAindyWithNotZeroNotNegPageChange()
    {
        ExecuteLDAindy(new UInt8(1), new UInt8(0xBF), Low, Low);
    }

    // -------------------------------------------------------------------------
    // LDA (indirect)
    // -------------------------------------------------------------------------
    private void ExecuteLDAind(UInt8 data, BitFlag expectedZFlag, BitFlag expectedNFlag)
    {
        // ARRANGE:
        var opCode = OpCodes.LDAind.ToUInt8();
        var operand1 = new UInt8(0x80);
        var addrLsb = new UInt8(0x22);
        var addrMsb = new UInt8(0xEF);
        var finalAddr = new UInt16(addrLsb, addrMsb);

        BootToAddress(BootAddr);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstLDAid2 - fetch the second byte of the op-code into EA low

        Pins.SetDataBusPins(addrLsb);
        ExecuteClockCycles(1); // InstLDAid3 - fetch the value at EA into EA2 Low

        Pins.SetDataBusPins(addrMsb);
        ExecuteClockCycles(1); // InstLDAid4 - fetch the value at EA + 1 into EA2 High

        Pins.SetDataBusPins(data);
        ExecuteClockCycles(1); // InstLDAidx5 - load the A reg. with the value at EA2

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(finalAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(data, Regs.A);
        Assert.Equal(expectedZFlag, Regs.P.Zero);
        Assert.Equal(expectedNFlag, Regs.P.Negative);
    }

    [Fact]
    public void TestLDAindWith0()
    {
        ExecuteLDAind(new UInt8(0), High, Low);
    }

    [Fact]
    public void TestLDAindWithNeg()
    {
        ExecuteLDAind(new UInt8(0xFF), Low, High);
    }

    [Fact]
    public void TestLDAindWithNotZeroNotNeg()
    {
        ExecuteLDAind(new UInt8(1), Low, Low);
    }
}
