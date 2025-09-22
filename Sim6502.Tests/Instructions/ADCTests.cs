using System.Diagnostics.CodeAnalysis;
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class ADCTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // ADC immediate
    // -------------------------------------------------------------------------
    private void ExecuteADCimm(UInt8 aValue, UInt8 operand, BitFlag cIn, BitFlag dIn,
                               BitFlag expectedZ, BitFlag expectedN, BitFlag expectedC, BitFlag expectedV)
    {
        // ARRANGE:
        var opCode = OpCodes.ADCimm.ToUInt8();
        var expectedValue = aValue.Copy().Adc(operand, cIn, dIn).Value();

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Carry.UpdateValue(cIn);
        Regs.P.Decimal.UpdateValue(dIn);
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.P.Overflow.UpdateValue(expectedV.Copy().Not());
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand;
        ExecuteClockCycles(1); // InstADCimm2 - ADC A reg. with operand

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstADCimm3 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
        Assert.Equal(expectedC, Regs.P.Carry);
    }

    private void ExecuteADCImmWhenResultIsZero(BitFlag dIn)
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var expectedZ = High;
        var expectedN = Low;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCimm(aValue, operand, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    [Fact]
    public void TestADCimmWhenResultIsZero()
    {
        ExecuteADCImmWhenResultIsZero(Low);
    }

    [Fact]
    public void TestADCimmBcdWhenResultIsZero()
    {
        ExecuteADCImmWhenResultIsZero(High);
    }

    [Fact]
    public void TestADCimmWhenResultIsNegative()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0xFF); // -1
        var cIn = Low;
        var dIn = Low;
        var expectedZ = Low;
        var expectedN = High;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCimm(aValue, operand, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    // -------------------------------------------------------------------------
    // ADC zeropage
    // -------------------------------------------------------------------------
    private void ExecuteADCzpg(UInt8 aValue, UInt8 memValue, BitFlag cIn, BitFlag dIn,
                               BitFlag expectedZ, BitFlag expectedN, BitFlag expectedC, BitFlag expectedV)
    {
        // ARRANGE:
        var opCode = OpCodes.ADCzpg.ToUInt8();
        var operand = new UInt8(0x12);
        var expectedValue = aValue.Copy().Adc(memValue, cIn, dIn).Value();
        var expectedAddr = new UInt16(operand);

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Carry.UpdateValue(cIn);
        Regs.P.Decimal.UpdateValue(dIn);
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.P.Overflow.UpdateValue(expectedV.Copy().Not());
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand;
        ExecuteClockCycles(1); // InstADCzpg2 - ADC A reg. with operand

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstADCzpg3 - fetch the memory value at EA then ADD it with A reg

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstADCzpg4 - fetch the operand into EA reg

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
        Assert.Equal(expectedC, Regs.P.Carry);
    }

    private void ExecuteADCzpgWhenResultIsZero(BitFlag dIn)
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var expectedZ = High;
        var expectedN = Low;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCzpg(aValue, operand, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    [Fact]
    public void TestADCzpgWhenResultIsZero()
    {
        ExecuteADCzpgWhenResultIsZero(Low);
    }

    [Fact]
    public void TestADCzpgBcdWhenResultIsZero()
    {
        ExecuteADCzpgWhenResultIsZero(High);
    }

    [Fact]
    public void TestADCzpgWhenResultIsNegative()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0xFF); // -1
        var cIn = Low;
        var dIn = Low;
        var expectedZ = Low;
        var expectedN = High;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCzpg(aValue, operand, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    // -------------------------------------------------------------------------
    // ADC zeropage,X
    // -------------------------------------------------------------------------
    private void ExecuteADCzpgx(UInt8 aValue, UInt8 memValue, BitFlag cIn, BitFlag dIn,
                                BitFlag expectedZ, BitFlag expectedN, BitFlag expectedC, BitFlag expectedV)
    {
        // ARRANGE:
        var opCode = OpCodes.ADCzpgx.ToUInt8();
        var operand = new UInt8(0x12);
        var xValue = new UInt8(0x10);
        var expectedValue = aValue.Copy().Adc(memValue, cIn, dIn).Value();
        var expectedAddr = new UInt16(operand.Copy().AddWithWrapAround(xValue));

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Carry.UpdateValue(cIn);
        Regs.P.Decimal.UpdateValue(dIn);
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.P.Overflow.UpdateValue(expectedV.Copy().Not());
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand;
        ExecuteClockCycles(1); // InstANDzpgx2 - fetch the operand into EA reg

        ExecuteClockCycles(1); // InstANDzpgx3 - EA = EA + X

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstANDzpgx4 - fetch the memory value at EA into temp then AND Temp reg with
                               // A reg

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstADCzpgx5 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
        Assert.Equal(expectedC, Regs.P.Carry);
    }

    private void ExecuteADCzpgxWhenResultIsZero(BitFlag dIn)
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var expectedZ = High;
        var expectedN = Low;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCzpgx(aValue, operand, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    [Fact]
    public void TestADCzpgxWhenResultIsZero()
    {
        ExecuteADCzpgxWhenResultIsZero(Low);
    }

    [Fact]
    public void TestADCzpgxBcdWhenResultIsZero()
    {
        ExecuteADCzpgxWhenResultIsZero(High);
    }

    [Fact]
    public void TestADCzpgxWhenResultIsNegative()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0xFF); // -1
        var cIn = Low;
        var dIn = Low;
        var expectedZ = Low;
        var expectedN = High;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCzpgx(aValue, operand, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    // -------------------------------------------------------------------------
    // ADC absolute
    // -------------------------------------------------------------------------
    private void ExecuteADCabs(UInt8 aValue, UInt8 memValue, BitFlag cIn, BitFlag dIn,
                               BitFlag expectedZ, BitFlag expectedN, BitFlag expectedC, BitFlag expectedV)
    {
        // ARRANGE:
        var opCode = OpCodes.ADCabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = aValue.Copy().Adc(memValue, cIn, dIn).Value();
        var expectedAddr = new UInt16(operand1, operand2);

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Carry.UpdateValue(cIn);
        Regs.P.Decimal.UpdateValue(dIn);
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.P.Overflow.UpdateValue(expectedV.Copy().Not());
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand1;
        ExecuteClockCycles(1); // InstADCabs2 - fetch the operand into EAL reg

        Pins.DataBus = operand2;
        ExecuteClockCycles(1); // InstADCabs3 - fetch the operand into EAH reg

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstADCabs4 - fetch the memory value at EA into temp then AND Temp reg with A
                               // reg

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstADCabs5 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
        Assert.Equal(expectedC, Regs.P.Carry);
    }

    private void ExecuteADCabsWhenResultIsZero(BitFlag dIn)
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var expectedZ = High;
        var expectedN = Low;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCabs(aValue, operand, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    [Fact]
    public void TestADCabsWhenResultIsZero()
    {
        ExecuteADCabsWhenResultIsZero(Low);
    }

    [Fact]
    public void TestADCabsBcdWhenResultIsZero()
    {
        ExecuteADCabsWhenResultIsZero(High);
    }

    [Fact]
    public void TestADCabsWhenResultIsNegative()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0xFF); // -1
        var cIn = Low;
        var dIn = Low;
        var expectedZ = Low;
        var expectedN = High;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCabs(aValue, operand, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    // -------------------------------------------------------------------------
    // ADC absolute,X
    // -------------------------------------------------------------------------
    private void ExecuteADCabsx(UInt8 aValue, UInt8 memValue, UInt8 xValue, BitFlag cIn, BitFlag dIn,
                                BitFlag expectedZ, BitFlag expectedN, BitFlag expectedC, BitFlag expectedV)
    {
        // ARRANGE:
        var opCode = OpCodes.ADCabsx.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = aValue.Copy().Adc(memValue, cIn, dIn).Value();
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(xValue);

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Carry.UpdateValue(cIn);
        Regs.P.Decimal.UpdateValue(dIn);
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.P.Overflow.UpdateValue(expectedV.Copy().Not());
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand1;
        ExecuteClockCycles(1); // InstADCabsx2 - fetch the operand into EAL reg

        Pins.DataBus = operand2;
        ExecuteClockCycles(1); // InstADCabsx3 - fetch the operand into EAH reg

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstADCabsx4 - fetch the memory value at EA into temp then ADC Temp reg with
                               // A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.DataBus = memValue;
            ExecuteClockCycles(1); // InstADDabsx5 - fetch the memory value at EA into temp then ADC Temp reg with
                                   // A reg
        }

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstADCabsx6 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
        Assert.Equal(expectedC, Regs.P.Carry);
    }

    private void ExecuteADCabsxWhenResultIsZero(BitFlag dIn)
    {
        var aValue = new UInt8(0x00);
        var xValue = new UInt8(0x0A);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var expectedZ = High;
        var expectedN = Low;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCabsx(aValue, operand, xValue, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    [Fact]
    public void TestADCabsxWhenResultIsZero()
    {
        ExecuteADCabsxWhenResultIsZero(Low);
    }

    [Fact]
    public void TestADCabsxBcdWhenResultIsZero()
    {
        ExecuteADCabsxWhenResultIsZero(High);
    }

    [Fact]
    public void TestADCabsxWhenResultIsNegative()
    {
        var aValue = new UInt8(0x00);
        var xValue = new UInt8(0x0A);
        var operand = new UInt8(0xFF); // -1
        var cIn = Low;
        var dIn = Low;
        var expectedZ = Low;
        var expectedN = High;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCabsx(aValue, operand, xValue, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    // -------------------------------------------------------------------------
    // ADC absolute,Y
    // -------------------------------------------------------------------------
    private void ExecuteADCabsy(UInt8 aValue, UInt8 memValue, UInt8 yValue, BitFlag cIn, BitFlag dIn,
                                BitFlag expectedZ, BitFlag expectedN, BitFlag expectedC, BitFlag expectedV)
    {
        // ARRANGE:
        var opCode = OpCodes.ADCabsy.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedValue = aValue.Copy().Adc(memValue, cIn, dIn).Value();
        var expectedAddr = new UInt16(operand1, operand2).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Carry.UpdateValue(cIn);
        Regs.P.Decimal.UpdateValue(dIn);
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.P.Overflow.UpdateValue(expectedV.Copy().Not());
        Regs.A.UpdateValue(aValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand1;
        ExecuteClockCycles(1); // InstADCabsy2 - fetch the operand into EAL reg

        Pins.DataBus = operand2;
        ExecuteClockCycles(1); // InstADCabsy3 - fetch the operand into EAH reg

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstADCabsy4 - fetch the memory value at EA into temp then ADC Temp reg with
                               // A reg

        // if adding the X reg cause the page to change
        if (!expectedAddr.Msb().Equals(operand2))
        {
            Pins.DataBus = memValue;
            ExecuteClockCycles(1); // InstADDabsy5 - fetch the memory value at EA into temp then ADC Temp reg with
                                   // A reg
        }

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstADCabsy6 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1003), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
        Assert.Equal(expectedC, Regs.P.Carry);
    }

    private void ExecuteADCabsyWhenResultIsZero(BitFlag dIn)
    {
        var aValue = new UInt8(0x00);
        var xValue = new UInt8(0x0A);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var expectedZ = High;
        var expectedN = Low;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCabsy(aValue, operand, xValue, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    [Fact]
    public void TestADCabsyWhenResultIsZero()
    {
        ExecuteADCabsyWhenResultIsZero(Low);
    }

    [Fact]
    public void TestADCabsyBcdWhenResultIsZero()
    {
        ExecuteADCabsyWhenResultIsZero(High);
    }

    [Fact]
    public void TestADCabsyWhenResultIsNegative()
    {
        var aValue = new UInt8(0x00);
        var xValue = new UInt8(0x0A);
        var operand = new UInt8(0xFF); // -1
        var cIn = Low;
        var dIn = Low;
        var expectedZ = Low;
        var expectedN = High;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCabsy(aValue, operand, xValue, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    // -------------------------------------------------------------------------
    // ADC (indirect,X)
    // -------------------------------------------------------------------------
    private void ExecuteADCindx(UInt8 aValue, UInt8 memValue, UInt8 xValue, BitFlag cIn, BitFlag dIn,
                                BitFlag expectedZ, BitFlag expectedN, BitFlag expectedC, BitFlag expectedV)
    {
        // ARRANGE:
        var opCode = OpCodes.ADCindx.ToUInt8();
        var operand = new UInt8(0x80);
        var expectedValue = aValue.Copy().Adc(memValue, cIn, dIn).Value();
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xEF);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Carry.UpdateValue(cIn);
        Regs.P.Decimal.UpdateValue(dIn);
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.P.Overflow.UpdateValue(expectedV.Copy().Not());
        Regs.A.UpdateValue(aValue);
        Regs.X.UpdateValue(xValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = operand;
        ExecuteClockCycles(1); // InstADCidx2 - fetch the second byte of the op-code (EA low)

        ExecuteClockCycles(1); // InstADCidx3 - add X reg to the EA

        Pins.DataBus = indAddrLsb;
        ExecuteClockCycles(1); // InstADCidx4 - fetch the value at EA, put in EA2 Low

        Pins.DataBus = indAddrMsb;
        ExecuteClockCycles(1); // InstADCidx5 - fetch the value at EA + 1, put in EA2 High

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstADCidx6 - load the A reg. with the value at

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstADCindx7 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
        Assert.Equal(expectedC, Regs.P.Carry);
    }

    private void ExecuteADCindxWhenResultIsZero(BitFlag dIn)
    {
        var aValue = new UInt8(0x00);
        var xValue = new UInt8(0x0A);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var expectedZ = High;
        var expectedN = Low;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCindx(aValue, operand, xValue, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    [Fact]
    public void TestADCindxWhenResultIsZero()
    {
        ExecuteADCindxWhenResultIsZero(Low);
    }

    [Fact]
    public void TestADCindxBcdWhenResultIsZero()
    {
        ExecuteADCindxWhenResultIsZero(High);
    }

    [Fact]
    public void TestADCindxWhenResultIsNegative()
    {
        var aValue = new UInt8(0x00);
        var xValue = new UInt8(0x0A);
        var operand = new UInt8(0xFF); // -1
        var cIn = Low;
        var dIn = Low;
        var expectedZ = Low;
        var expectedN = High;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCindx(aValue, operand, xValue, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    // -------------------------------------------------------------------------
    // ADC (indirect),Y
    // -------------------------------------------------------------------------
    private void ExecuteADCindy(UInt8 aValue, UInt8 memValue, UInt8 yValue, BitFlag cIn, BitFlag dIn,
                                BitFlag expectedZ, BitFlag expectedN, BitFlag expectedC, BitFlag expectedV)
    {
        // ARRANGE:
        var opCode = OpCodes.ADCindy.ToUInt8();
        var operand = new UInt8(0x4C);
        var expectedValue = aValue.Copy().Adc(memValue, cIn, dIn).Value();
        var indAddrLsb = new UInt8(0x41);
        var indAddrMsb = new UInt8(0x0C);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb).AddUnsigned(yValue);

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Carry.UpdateValue(cIn);
        Regs.P.Decimal.UpdateValue(dIn);
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.P.Overflow.UpdateValue(expectedV.Copy().Not());
        Regs.A.UpdateValue(aValue);
        Regs.Y.UpdateValue(yValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = operand;
        ExecuteClockCycles(1); // InstADCindy2 - fetch the second byte of the op-code (EA low)
        Assert.Equal(new UInt16(0x1001), Pins.AddrBus);

        Pins.DataBus = indAddrLsb;
        ExecuteClockCycles(1); // InstADCindy3 - fetch the value at EA, put in EA2 Low
        Assert.Equal(new UInt16(operand), Pins.AddrBus);

        Pins.DataBus = indAddrMsb;
        ExecuteClockCycles(1); // InstADCindy4 - fetch the value at EA + 1, put in EA2 High
        Assert.Equal(new UInt16(operand).Inc(), Pins.AddrBus);

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstADCindy5 - load the A reg. with the value at EA + Y

        // if adding the Y reg cause the page to change
        if (!indAddrMsb.Equals(expectedAddr.Msb()))
        {
            Pins.DataBus = aValue;
            ExecuteClockCycles(1); // InstADCindy6 - load the A reg. with the value at
        }

        if (dIn.IsSet())
            ExecuteClockCycles(1); // InstADCindy7 - internal operation

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
        Assert.Equal(expectedC, Regs.P.Carry);
    }

    private void ExecuteADCindyWhenResultIsZero(BitFlag dIn)
    {
        var aValue = new UInt8(0x00);
        var yValue = new UInt8(0x0A);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var expectedZ = High;
        var expectedN = Low;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCindy(aValue, operand, yValue, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    [Fact]
    public void TestADCindyWhenResultIsZero()
    {
        ExecuteADCindyWhenResultIsZero(Low);
    }

    [Fact]
    public void TestADCindyBcdWhenResultIsZero()
    {
        ExecuteADCindyWhenResultIsZero(High);
    }

    [Fact]
    public void TestADCindyWhenResultIsNegative()
    {
        var aValue = new UInt8(0x00);
        var yValue = new UInt8(0x0A);
        var operand = new UInt8(0xFF); // -1
        var cIn = Low;
        var dIn = Low;
        var expectedZ = Low;
        var expectedN = High;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCindy(aValue, operand, yValue, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    // -------------------------------------------------------------------------
    // ADC (indirect)
    // -------------------------------------------------------------------------

    private void ExecuteADCind(UInt8 aValue, UInt8 memValue, BitFlag cIn, BitFlag dIn,
                               BitFlag expectedZ, BitFlag expectedN, BitFlag expectedC, BitFlag expectedV)
    {
        // ARRANGE:
        var opCode = OpCodes.ADCind.ToUInt8();
        var operand = new UInt8(0x80);
        var expectedValue = aValue.Copy().Adc(memValue, cIn, dIn).Value();
        var indAddrLsb = new UInt8(0x22);
        var indAddrMsb = new UInt8(0xEF);
        var expectedAddr = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);
        Regs.P.Zero.UpdateValue(expectedZ.Copy().Not());
        Regs.P.Carry.UpdateValue(cIn);
        Regs.P.Decimal.UpdateValue(dIn);
        Regs.P.Negative.UpdateValue(expectedN.Copy().Not());
        Regs.P.Overflow.UpdateValue(expectedV.Copy().Not());
        Regs.A.UpdateValue(aValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1);// fetch the opcode

        Pins.DataBus = operand;
        ExecuteClockCycles(1); // InstADCid2 - fetch the second byte of the op-code into EA low

        Pins.DataBus = indAddrLsb;
        ExecuteClockCycles(1); // InstADCid3 - fetch the value at EA into EA2 Low

        Pins.DataBus = indAddrMsb;
        ExecuteClockCycles(1); // InstADCid4 - fetch the value at EA + 1 into EA2 High

        Pins.DataBus = memValue;
        ExecuteClockCycles(1); // InstADCidx5 - load the A reg. with the value at EA2

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedAddr, Pins.AddrBus);
        Assert.Equal(new UInt16(0x1002), Regs.PC);
        Assert.Equal(expectedValue, Regs.A);
        Assert.Equal(expectedZ, Regs.P.Zero);
        Assert.Equal(expectedN, Regs.P.Negative);
        Assert.Equal(expectedC, Regs.P.Carry);
    }

    private void ExecuteADCindWhenResultIsZero(BitFlag dIn)
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0x00);
        var cIn = Low;
        var expectedZ = High;
        var expectedN = Low;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCind(aValue, operand, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    [Fact]
    public void TestADCindWhenResultIsZero()
    {
        ExecuteADCindWhenResultIsZero(Low);
    }

    [Fact]
    public void TestADCindBcdWhenResultIsZero()
    {
        ExecuteADCindWhenResultIsZero(High);
    }

    [Fact]
    public void TestADCindWhenResultIsNegative()
    {
        var aValue = new UInt8(0x00);
        var operand = new UInt8(0xFF); // -1
        var cIn = Low;
        var dIn = Low;
        var expectedZ = Low;
        var expectedN = High;
        var expectedC = Low;
        var expectedV = Low;

        ExecuteADCind(aValue, operand, cIn, dIn, expectedZ, expectedN, expectedC, expectedV);
    }

    // -------------------------------------------------------------------------
    // Integration
    // -------------------------------------------------------------------------
    private void ExecuteADC(byte acc, byte operand, BitFlag cFlag, BitFlag dFlag, MathResult expected)
    {
        // ARRANGE:
        var cFlagOpCode = cFlag.IsSet() ? OpCodes.SECimp.ToByte() : OpCodes.CLCimp.ToByte();
        var dFlagOpCode = dFlag.IsSet() ? OpCodes.SEDimp.ToByte() : OpCodes.CLDimp.ToByte();

        byte[] program =
        [
                                // 0000:        .ORG $0000
            0xA2, 0xFF,         // 0000:        LDX #$FF            ; set the stack pointer
            0x9A,               // 0002:        TXS
            0xA9, 0x00,         // 0003:        LDA #$00            ; clear the flags
            0x48,               // 0005:        PHA
            0x28,               // 0006:        PLP
            dFlagOpCode,        // 0007:        SED|CLD             ; set|clear decimal flag
            cFlagOpCode,        // 0008:        SEC|CLC             ; set|clear the carry flag
            0xA9, acc,          // 0009:        LDA #$'ACC'         ; A = acc
            0x69, operand,      // 000B:        ADC #$'operand'     ; A = A - operand
            0x00 // 000D:        BRK
        ];

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
    public void TestADCIntegration()
    {
        const byte accumulator = 0x1F;
        const byte operand = 0x0A;
        var cFlag = High;
        var dFlag = High;

        var expected = new MathResult(new UInt8(0x20), "nvzc");

        ExecuteADC(accumulator, operand, cFlag, dFlag, expected);
    }
}
