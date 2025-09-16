// ReSharper disable InconsistentNaming
using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502.Tests;

public class STATests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // STA zeropage
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTAzpg()
    {
        // ARRANGE:
        var opCode = OpCodes.STAzpg.ToUInt8();
        var operand1 = new UInt8(0x34);
        var expectedValue = new UInt8(0x12);
        var expectedAddr = new UInt16(operand1);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(expectedValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstSTAzpg2 - fetch the second byte of the op-code (zpg-value)

        ExecuteClockCycles(1); // InstSTAzpg3 - store the value in A at address zpg-value

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.GetRWB());
    }

    // -------------------------------------------------------------------------
    // STA zeropage,X
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTAzpgx()
    {
        // ARRANGE:
        var opCode = OpCodes.STAzpgx.ToUInt8();
        var operand1 = new UInt8(0x12);
        var xValue = new UInt8(0x10);
        var expectedValue = new UInt8(0xAA);
        var expectedAddr = new UInt16(operand1).AddUnsigned(xValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(expectedValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstSTAzpgx2 - fetch the second byte of the op-code (zpg-value)

        ExecuteClockCycles(1); // InstSTAzpgx3 - EA = zpg-value + X reg

        ExecuteClockCycles(1); // InstSTAzpgx4 - store the value in A at address zpg-value,X

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.GetRWB());
    }

    // -------------------------------------------------------------------------
    // STA absolute
    // -------------------------------------------------------------------------
    [Fact]
    public void TestSTAabs()
    {
        // ARRANGE:
        var opCode = OpCodes.STAabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = new UInt8(0xAA);
        var expectedAddr = new UInt16(operand1, operand2);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(expectedValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstSTAabs2 - fetch the first byte of the addr

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstSTAabs3 - fetch the second byte of the addr

        ExecuteClockCycles(1); // InstSTAabs4 - store the value in A at absHi,absLo

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.GetRWB());
    }

    // -------------------------------------------------------------------------
    // STA absolute,X
    // -------------------------------------------------------------------------
    private void ExecuteSTAabsx(UInt8 xValue)
    {
        // ARRANGE:
        var opCode = OpCodes.STAabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = new UInt8(0xAA);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(expectedValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstSTAabsx2 - fetch the first byte of the addr

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstSTAabsx3 - fetch the second byte of the addr

        ExecuteClockCycles(1); // InstSTAabsx4 - add X reg to the effective address

        ExecuteClockCycles(1); // InstLDAabsx5 - store A reg at the effective address       

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.GetRWB());
    }

    [Fact]
    public void TestSTAabsx()
    {
        ExecuteSTAabsx(new UInt8(0x01));
    }

    [Fact]
    public void TestSTAabsxPageChange()
    {
        ExecuteSTAabsx(new UInt8(0x80));
    }

    // -------------------------------------------------------------------------
    // STA absolute,Y
    // -------------------------------------------------------------------------
    private void ExecuteSTAabsy(UInt8 yValue)
    {
        // ARRANGE:
        var opCode = OpCodes.STAabsy.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = new UInt8(0xAA);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(expectedValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstSTAabsy2 - fetch the first byte of the addr

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstSTAabsy3 - fetch the second byte of the addr

        ExecuteClockCycles(1); // InstSTAabsy4 - add Y reg to the effective address

        ExecuteClockCycles(1); // InstLDAabsy5 - store A reg at the effective address

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.GetRWB());
    }

    [Fact]
    public void TestSTAabsy()
    {
        ExecuteSTAabsy(new UInt8(0x01));
    }

    [Fact]
    public void TestSTAabsyPageChange()
    {
        ExecuteSTAabsy(new UInt8(0x80));
    }

    // -------------------------------------------------------------------------
    // STA (indirect,X)
    // -------------------------------------------------------------------------
    private void ExecuteSTAindx(UInt8 xValue)
    {
        // ARRANGE:
        var opCode = OpCodes.STAindx.ToUInt8();
        var operand = new UInt8(0x80);
        var expectedValue = new UInt8(0xAA);
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xef);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);

        Regs.A.UpdateValue(expectedValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstSTAidx2 - fetch the second byte of the op-code into EA low

        ExecuteClockCycles(1); // InstSTAidx3 - add X reg to the EA, ignore the carry

        Pins.SetDataBusPins(indAddrLsb);
        ExecuteClockCycles(1); // InstSTAidx4 - fetch the value at EA, put in EA2 Low
        Assert.Equal(new UInt16(operand.Copy().AddWithWrapAround(xValue)), Pins.GetAddrBusPins());

        Pins.SetDataBusPins(indAddrMsb);
        ExecuteClockCycles(1); // InstSTAidx5 - fetch the value at EA + 1, put in EA2 High

        ExecuteClockCycles(1); // InstSTAidx6 - store the A reg. at EA2

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.GetRWB());
    }

    [Fact]
    public void TestSTAindx()
    {
        ExecuteSTAindx(new UInt8(0x10));
    }

    [Fact]
    public void TestSTAindxPageChange()
    {
        ExecuteSTAindx(new UInt8(0x80));
    }

    // -------------------------------------------------------------------------
    // STA (indirect),Y
    // -------------------------------------------------------------------------
    private void ExecuteSTAindy(UInt8 yValue)
    {
        // ARRANGE:
        var opCode = OpCodes.STAindy.ToUInt8();
        var operand = new UInt8(0x4C);
        var expectedValue = new UInt8(0xAA);
        var indAddrLsb = new UInt8(0x41);
        var indAddrMsb = new UInt8(0x0C);
        var indAddr = new UInt16(indAddrLsb, indAddrMsb);
        var expectedAddr = indAddr.Copy().AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(expectedValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstSTAindy2 - fetch the second byte of the op-code into EA low
        Assert.Equal(new UInt16(0x1001), Pins.GetAddrBusPins());

        Pins.SetDataBusPins(indAddrLsb);
        ExecuteClockCycles(1); // InstSTAindy3 - fetch the value at EA, put in EA2 Low
        Assert.Equal(new UInt16(operand), Pins.GetAddrBusPins());

        Pins.SetDataBusPins(indAddrMsb);
        ExecuteClockCycles(1); // InstSTAindy4 - fetch the value at EA + 1, put in EA2 High
        Assert.Equal(new UInt16(operand.Copy().Inc()), Pins.GetAddrBusPins());

        ExecuteClockCycles(1); // InstSTAindy5 - add the Y reg to EA

        ExecuteClockCycles(1); // InstLDAindy6 - store the A reg. at EA

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.GetRWB());
    }

    [Fact]
    public void TestSTAindy()
    {
        ExecuteSTAindy(new UInt8(0x10));
    }

    [Fact]
    public void TestSTAindyPageChange()
    {
        ExecuteSTAindy(new UInt8(0x80));
    }

    // -------------------------------------------------------------------------
    // LDA (indirect)
    // -------------------------------------------------------------------------
    private void ExecuteSTAind()
    {
        // ARRANGE:
        var opCode = OpCodes.STAind.ToUInt8();
        var operand = new UInt8(0x4C);
        var expectedValue = new UInt8(0xAA);
        var indAddrLsb = new UInt8(0x41);
        var indAddrMsb = new UInt8(0x0C);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);
        Regs.A.UpdateValue(expectedValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstSTAid2 - fetch the second byte of the op-code into EA low

        Pins.SetDataBusPins(indAddrLsb);
        ExecuteClockCycles(1); // InstSTAid3 - fetch the value at EA into EA2 Low

        Pins.SetDataBusPins(indAddrMsb);
        ExecuteClockCycles(1); // InstSTAid4 - fetch the value at EA + 1 into EA2 High

        ExecuteClockCycles(1); // InstSTAidx5 - store the A reg. with the value at EA2

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(expectedValue, Pins.GetDataBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal((byte)Low.ToInt(), Pins.GetRWB());
    }

    [Fact]
    public void TestLDAind()
    {
        ExecuteSTAind();
    }
}
