// ReSharper disable InconsistentNaming
using Sim6502.types;
using UInt8 = Sim6502.types.UInt8;
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502.Tests;

public class SBCTests : UnitTestBase
{
    private void AssertExpected(MathResult expected)
    {
        Assert.Equal(expected.Value(), Regs.A);
        Assert.Equal(expected.Negative(), Regs.P.Negative);
        Assert.Equal(expected.Overflow(), Regs.P.Overflow);
        Assert.Equal(expected.Zero(), Regs.P.Zero);
        Assert.Equal(expected.Carry(), Regs.P.Carry);
    }

    private void PrepareFlags(MathResult expected, BitFlag cFlag, BitFlag dFlag)
    {
        Regs.P.Negative.UpdateValue(expected.Negative().Copy().Not());
        Regs.P.Overflow.UpdateValue(expected.Overflow().Copy().Not());
        Regs.P.Decimal.UpdateValue(dFlag);
        Regs.P.Zero.UpdateValue(expected.Zero().Copy().Not());
        Regs.P.Carry.UpdateValue(cFlag);
    }

    // -------------------------------------------------------------------------
    // SBC immediate
    // -------------------------------------------------------------------------
    private void ExecuteSBCimm(UInt8 aValue, UInt8 operand, BitFlag cIn, BitFlag dIn,
                               MathResult expected)
    {
        // ARRANGE:
        var opCode = OpCodes.SBCimm.ToUInt8();

        BootToAddress(BootAddr);

        PrepareFlags(expected, cIn, dIn);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstSBCimm2 - SBC A reg. with operand

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstSBCimm3 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        AssertExpected(expected);
    }

    [Fact]
    public void TestSBCimmBcdExpect_Nvzc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x99), "Nvzc");

        ExecuteSBCimm(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCimmBcdExpect_nVzc()
    {
        var aValue = new UInt8(0x01);
        var operand = new UInt8(0x80);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x20), "nVzc");

        ExecuteSBCimm(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCimmBcdExpect_nvZc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x99);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x00), "nvZc");

        ExecuteSBCimm(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCimmBcdExpect_nvzC()
    {
        var aValue = new UInt8(0x02);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x01), "nvzC");

        ExecuteSBCimm(aValue, operand, cIn, dIn, expected);
    }

    // -------------------------------------------------------------------------
    // SBC zeropage
    // -------------------------------------------------------------------------
    private void ExecuteSBCzpg(UInt8 aValue, UInt8 memValue, BitFlag cIn, BitFlag dIn,
                               MathResult expected)
    {
        // ARRANGE:
        var opCode = OpCodes.SBCzpg.ToUInt8();
        var operand = new UInt8(0x12);
        var expectedAddr = new UInt16(operand);

        BootToAddress(BootAddr);
        PrepareFlags(expected, cIn, dIn);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstSBCzpg2 - SBC A reg. with operand

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstSBCzpg3 - fetch the memory value at EA then ADD it with A reg

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstSBCzpg4 - fetch the operand into EA reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        AssertExpected(expected);
    }

    [Fact]
    public void TestSBCzpgBcdExpect_Nvzc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x99), "Nvzc");

        ExecuteSBCzpg(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCzpgBcdExpect_nVzc()
    {
        var aValue = new UInt8(0x01);
        var operand = new UInt8(0x80);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x20), "nVzc");

        ExecuteSBCzpg(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCzpgBcdExpect_nvZc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x99);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x00), "nvZc");

        ExecuteSBCzpg(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCzpgBcdExpect_nvzC()
    {
        var aValue = new UInt8(0x02);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x01), "nvzC");

        ExecuteSBCzpg(aValue, operand, cIn, dIn, expected);
    }

    // -------------------------------------------------------------------------
    // SBC zeropage,X
    // -------------------------------------------------------------------------
    private void ExecuteSBCzpgx(UInt8 aValue, UInt8 memValue, BitFlag cIn, BitFlag dIn,
                                MathResult expected)
    {
        // ARRANGE:
        var opCode = OpCodes.SBCzpgx.ToUInt8();
        var operand = new UInt8(0x12);
        var xValue = new UInt8(0x10);
        var expectedAddr = new UInt16(operand.Copy().AddWithWrapAround(xValue));

        BootToAddress(BootAddr);
        PrepareFlags(expected, cIn, dIn);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstANDzpgx2 - fetch the operand into EA reg

        ExecuteClockCycles(1); // InstANDzpgx3 - EA = EA + X

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstANDzpgx4 - fetch the memory value at EA into temp then AND Temp reg with A reg

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstSBCzpgx5 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        AssertExpected(expected);
    }

    [Fact]
    public void TestSBCzpgxBcdExpect_Nvzc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x99), "Nvzc");

        ExecuteSBCzpgx(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCzpgxBcdExpect_nVzc()
    {
        var aValue = new UInt8(0x01);
        var operand = new UInt8(0x80);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x20), "nVzc");

        ExecuteSBCzpgx(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCzpgxBcdExpect_nvZc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x99);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x00), "nvZc");

        ExecuteSBCzpgx(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCzpgxBcdExpect_nvzC()
    {
        var aValue = new UInt8(0x02);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x01), "nvzC");

        ExecuteSBCzpgx(aValue, operand, cIn, dIn, expected);
    }

    // -------------------------------------------------------------------------
    // SBC absolute
    // -------------------------------------------------------------------------
    private void ExecuteSBCabs(UInt8 aValue, UInt8 memValue, BitFlag cIn, BitFlag dIn,
                               MathResult expected)
    {
        // ARRANGE:
        var opCode = OpCodes.SBCabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedAddr = new UInt16(operand1, operand2);

        BootToAddress(BootAddr);
        PrepareFlags(expected, cIn, dIn);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstSBCabs2 - fetch the operand into EAL reg

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstSBCabs3 - fetch the operand into EAH reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstSBCabs4 - fetch the memory value at EA into temp then AND Temp reg with A reg

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstSBCabs5 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        AssertExpected(expected);
    }

    [Fact]
    public void TestSBCabsBcdExpect_Nvzc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x99), "Nvzc");

        ExecuteSBCabs(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCabsBcdExpect_nVzc()
    {
        var aValue = new UInt8(0x01);
        var operand = new UInt8(0x80);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x20), "nVzc");

        ExecuteSBCabs(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCabsBcdExpect_nvZc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x99);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x00), "nvZc");

        ExecuteSBCabs(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCabsBcdExpect_nvzC()
    {
        var aValue = new UInt8(0x02);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x01), "nvzC");

        ExecuteSBCabs(aValue, operand, cIn, dIn, expected);
    }

    // -------------------------------------------------------------------------
    // SBC absolute,X
    // -------------------------------------------------------------------------
    private void ExecuteSBCabsx(UInt8 aValue, UInt8 memValue, BitFlag cIn, BitFlag dIn,
                                MathResult expected)
    {
        // ARRANGE:
        var opCode = OpCodes.SBCabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var xValue = new UInt8(0x33);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);

        BootToAddress(BootAddr);
        PrepareFlags(expected, cIn, dIn);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstSBCabsx2 - fetch the operand into EAL reg

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstSBCabsx3 - fetch the operand into EAH reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstSBCabsx4 - fetch the memory value at EA into temp then SBC Temp reg with A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.SetDataBusPins(memValue);
            ExecuteClockCycles(1); // InstADDabsx5 - fetch the memory value at EA into temp then SBC Temp reg with A reg       
        }

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstSBCabsx6 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        AssertExpected(expected);
    }

    [Fact]
    public void TestSBCabsxBcdExpect_Nvzc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x99), "Nvzc");

        ExecuteSBCabsx(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCabsxBcdExpect_nVzc()
    {
        var aValue = new UInt8(0x01);
        var operand = new UInt8(0x80);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x20), "nVzc");

        ExecuteSBCabsx(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCabsxBcdExpect_nvZc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x99);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x00), "nvZc");

        ExecuteSBCabsx(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCabsxBcdExpect_nvzC()
    {
        var aValue = new UInt8(0x02);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x01), "nvzC");

        ExecuteSBCabsx(aValue, operand, cIn, dIn, expected);
    }

    // -------------------------------------------------------------------------
    // SBC absolute,Y
    // -------------------------------------------------------------------------
    private void ExecuteSBCabsy(UInt8 aValue, UInt8 memValue, BitFlag cIn, BitFlag dIn,
                                MathResult expected)
    {
        // ARRANGE:
        var opCode = OpCodes.SBCabsy.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var yValue = new UInt8(0x33);
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        PrepareFlags(expected, cIn, dIn);
        Regs.A.UpdateValue(aValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand1);
        ExecuteClockCycles(1); // InstSBCabsy2 - fetch the operand into EAL reg

        Pins.SetDataBusPins(operand2);
        ExecuteClockCycles(1); // InstSBCabsy3 - fetch the operand into EAH reg

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstSBCabsy4 - fetch the memory value at EA into temp then SBC Temp reg with A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.SetDataBusPins(memValue);
            ExecuteClockCycles(1); // InstADDabsy5 - fetch the memory value at EA into temp then SBC Temp reg with A reg       
        }

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstSBCabsy6 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        AssertExpected(expected);
    }

    [Fact]
    public void TestSBCabsyBcdExpect_Nvzc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x99), "Nvzc");

        ExecuteSBCabsy(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCabsyBcdExpect_nVzc()
    {
        var aValue = new UInt8(0x01);
        var operand = new UInt8(0x80);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x20), "nVzc");

        ExecuteSBCabsy(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCabsyBcdExpect_nvZc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x99);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x00), "nvZc");

        ExecuteSBCabsy(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCabsyBcdExpect_nvzC()
    {
        var aValue = new UInt8(0x02);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x01), "nvzC");

        ExecuteSBCabsy(aValue, operand, cIn, dIn, expected);
    }

    // -------------------------------------------------------------------------
    // SBC (indirect,X)
    // -------------------------------------------------------------------------
    private void ExecuteSBCindx(UInt8 aValue, UInt8 memValue, BitFlag cIn, BitFlag dIn,
                                MathResult expected)
    {
        // ARRANGE:
        var opCode = OpCodes.SBCindx.ToUInt8();
        var operand = new UInt8(0x80);
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xEF);
        var xValue = new UInt8(0x33);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);
        PrepareFlags(expected, cIn, dIn);
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstSBCidx2 - fetch the second byte of the op-code (EA low)

        ExecuteClockCycles(1); // InstSBCidx3 - add X reg to the EA

        Pins.SetDataBusPins(indAddrLsb);
        ExecuteClockCycles(1); // InstSBCidx4 - fetch the value at EA, put in EA2 Low

        Pins.SetDataBusPins(indAddrMsb);
        ExecuteClockCycles(1); // InstSBCidx5 - fetch the value at EA + 1, put in EA2 High

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstSBCidx6 - load the A reg. with the value at 

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstSBCindx7 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        AssertExpected(expected);
    }

    [Fact]
    public void TestSBCindxBcdExpect_Nvzc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x99), "Nvzc");

        ExecuteSBCindx(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCindxBcdExpect_nVzc()
    {
        var aValue = new UInt8(0x01);
        var operand = new UInt8(0x80);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x20), "nVzc");

        ExecuteSBCindx(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCindxBcdExpect_nvZc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x99);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x00), "nvZc");

        ExecuteSBCindx(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCindxBcdExpect_nvzC()
    {
        var aValue = new UInt8(0x02);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x01), "nvzC");

        ExecuteSBCindx(aValue, operand, cIn, dIn, expected);
    }

    // -------------------------------------------------------------------------
    // SBC (indirect),Y
    // -------------------------------------------------------------------------
    private void ExecuteSBCindy(UInt8 aValue, UInt8 memValue, BitFlag cIn, BitFlag dIn,
                                MathResult expected)
    {
        // ARRANGE:
        var opCode = OpCodes.SBCindy.ToUInt8();
        var operand = new UInt8(0x4C);
        var indAddrLsb = new UInt8(0x41);
        var indAddrMsb = new UInt8(0x0C);
        var yValue = new UInt8(0x33);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        PrepareFlags(expected, cIn, dIn);
        Regs.A.UpdateValue(aValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstSBCindy2 - fetch the second byte of the op-code (EA low)
        Assert.Equal(new UInt16(0x1001), Pins.GetAddrBusPins());

        Pins.SetDataBusPins(indAddrLsb);
        ExecuteClockCycles(1); // InstSBCindy3 - fetch the value at EA, put in EA2 Low
        Assert.Equal(new UInt16(operand), Pins.GetAddrBusPins());

        Pins.SetDataBusPins(indAddrMsb);
        ExecuteClockCycles(1); // InstSBCindy4 - fetch the value at EA + 1, put in EA2 High
        Assert.Equal(new UInt16(operand).Inc(), Pins.GetAddrBusPins());

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstSBCindy5 - load the A reg. with the value at EA + Y

        // if adding the Y reg cause the page to change
        if (!indAddrMsb.Equals(expectedAddr.Msb()))
        {
            Pins.SetDataBusPins(aValue);
            ExecuteClockCycles(1); // InstSBCindy6 - load the A reg. with the value at
        }

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstSBCindy7 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        AssertExpected(expected);
    }

    [Fact]
    public void TestSBCindyBcdExpect_Nvzc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x99), "Nvzc");

        ExecuteSBCindy(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCindyBcdExpect_nVzc()
    {
        var aValue = new UInt8(0x01);
        var operand = new UInt8(0x80);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x20), "nVzc");

        ExecuteSBCindy(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCindyBcdExpect_nvZc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x99);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x00), "nvZc");

        ExecuteSBCindy(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCindyBcdExpect_nvzC()
    {
        var aValue = new UInt8(0x02);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x01), "nvzC");

        ExecuteSBCindy(aValue, operand, cIn, dIn, expected);
    }

    // -------------------------------------------------------------------------
    // SBC (indirect)
    // -------------------------------------------------------------------------
    private void ExecuteSBCind(UInt8 aValue, UInt8 memValue, BitFlag cIn, BitFlag dIn,
                               MathResult expected)
    {
        // ARRANGE:
        var opCode = OpCodes.SBCind.ToUInt8();
        var operand = new UInt8(0x80);
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xEF);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);
        PrepareFlags(expected, cIn, dIn);
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1);// fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstSBCid2 - fetch the second byte of the op-code into EA low

        Pins.SetDataBusPins(indAddrLsb);
        ExecuteClockCycles(1); // InstSBCid3 - fetch the value at EA into EA2 Low

        Pins.SetDataBusPins(indAddrMsb);
        ExecuteClockCycles(1); // InstSBCid4 - fetch the value at EA + 1 into EA2 High

        Pins.SetDataBusPins(memValue);
        ExecuteClockCycles(1); // InstSBCidx5 - load the A reg. with the value at EA2

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.GetAddrBusPins());
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        AssertExpected(expected);
    }

    [Fact]
    public void TestSBCindBcdExpect_Nvzc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x99), "Nvzc");

        ExecuteSBCind(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCindBcdExpect_nVzc()
    {
        var aValue = new UInt8(0x01);
        var operand = new UInt8(0x80);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x20), "nVzc");

        ExecuteSBCind(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCindBcdExpect_nvZc()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x99);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x00), "nvZc");

        ExecuteSBCind(aValue, operand, cIn, dIn, expected);
    }

    [Fact]
    public void TestSBCindBcdExpect_nvzC()
    {
        var aValue = new UInt8(0x02);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var dIn = High;
        var expected = new MathResult(new UInt8(0x01), "nvzC");

        ExecuteSBCind(aValue, operand, cIn, dIn, expected);
    }

    // -------------------------------------------------------------------------
    // Integration test
    // -------------------------------------------------------------------------
    private void ExecuteSBC(byte acc, byte operand, BitFlag cFlag, BitFlag dFlag,
                            MathResult expected)
    {
        // ARRANGE:
        var cFlagOpCode = Equals(cFlag, High) ? OpCodes.SECimp.ToByte() : OpCodes.CLCimp.ToByte();
        var dFlagOpCode = Equals(dFlag, High) ? OpCodes.SEDimp.ToByte() : OpCodes.CLDimp.ToByte();

        byte[] program = {
                                //  0000:        .ORG $0000
             0xA2,  0xFF,       //  0000:        LDX #$FF    ; set the stack pointer
             0x9A,              //  0002:        TXS
             0xA9,  0x00,       //  0003:        LDA #$00    ; clear the flags
             0x48,              //  0005:        PHA
             0x28,              //  0006:        PLP
             dFlagOpCode,       //  0007:        SED|CLD     ; set|clear decimal flag
             cFlagOpCode,       //  0008:        SEC         ; set|clear the carry flag
             0xA9,  acc,        //  0009:        LDA #$00    ; A = acc
             0xE9,  operand,    //  000B:        SBC #$01    ; A = A - operand
             0x00,              //  000D:        BRK
        };

        // ACT:
        ExecuteProgram(program, new UInt16(0x0000));

        // ASSERT:
        Assert.Equal(expected.Value(), Regs.A);
        Assert.Equal(expected.Negative(), Regs.P.Negative);
        Assert.Equal(expected.Overflow(), Regs.P.Overflow);
        Assert.Equal(expected.Zero(), Regs.P.Zero);
        Assert.Equal(expected.Carry(), Regs.P.Carry);
    }

    [Fact]
    public void TestSBCIntegration()
    {
        const byte accumulator = 0x00;
        const byte operand = 0xA1;
        var cFlag = High;
        var dFlag = High;

        var expected = new MathResult(new UInt8(0xF9), "Nvzc");

        ExecuteSBC(accumulator, operand, cFlag, dFlag, expected);
    }
}
