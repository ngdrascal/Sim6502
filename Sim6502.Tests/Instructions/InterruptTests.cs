// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class InterruptTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // BRK implied
    // -------------------------------------------------------------------------
    [Fact]
    public void TestBRKimp()
    {
        // ARRANGE:
        var opCode = OpCodes.BRKimp.ToUInt8();
        var intVecLsb = new UInt8(0x21);
        var intVecMsb = new UInt8(0x43);
        var expectedPC = new UInt16(intVecLsb, intVecMsb);
        var stackTop = new UInt8(0xFF);
        var expectedStackTop = stackTop.Copy().Dec().Dec().Dec();
        var expectedReturnAddr = BootAddr.Copy().AddUnsigned(new UInt8(2));

        BootToAddress(BootAddr);
        Regs.S.UpdateValue(stackTop);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (new UInt8(0x55));
        ExecuteClockCycles(1); // InstBRKimp2 - fetch the operand (which is ignored)

        ExecuteClockCycles(1); // InstBRKimp3 - push PCH
        Assert.Equal(expectedReturnAddr.Msb(), Pins.DataBus);

        ExecuteClockCycles(1); // InstBRKimp4 - push PCL
        Assert.Equal(expectedReturnAddr.Lsb(), Pins.DataBus);

        var p = Regs.P.ToUInt8().Copy();
        ExecuteClockCycles(1); // InstBRKimp5 - push flags
        Assert.Equal(p, Pins.DataBus);

        Pins.DataBus = (intVecLsb);
        ExecuteClockCycles(1); // InstBRKimp6 - read 0xFFFE into the PCL

        Pins.DataBus = (intVecMsb);
        ExecuteClockCycles(1); // InstBRKimp7 - read 0xFFFF into the PCH

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(expectedPC, Regs.PC);
        Assert.Equal(expectedStackTop, Regs.S);
        Assert.Equal(High, Regs.P.Break);
        Assert.Equal(Low, Regs.P.Decimal);
        Assert.Equal(High, Regs.P.IRQDisabled);
    }

    // -------------------------------------------------------------------------
    // NMI
    // -------------------------------------------------------------------------
    [Fact]
    public void TestNmi()
    {
        // ARRANGE:
        var interruptedOpCode = OpCodes.LDAzpg.ToUInt8(); // 3 cycle opcode
        var operand = new UInt8(0x12);
        var data = new UInt8(0x55);
        var vectorLsb = new UInt8(0x80);
        var vectorMsb = new UInt8(0x04);
        var handlerAddr = new UInt16(vectorLsb, vectorMsb);

        var stackTop = new UInt8(0xFF);

        BootToAddress(BootAddr);
        Regs.P.IRQDisabled.UpdateValue(High);
        Regs.S.UpdateValue(stackTop);

        // ACT:
        Pins.DataBus = (interruptedOpCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetNMIB(0);

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstLDAzpg2 - fetch the second byte of the op-code (zpg-value)

        Pins.DataBus = (data);
        ExecuteClockCycles(1); // InstLDAzpg3 - fetch the value at address zpg-value

        ExecuteClockCycles(1); // Interrupt1
        ExecuteClockCycles(1); // InstBRKimp2
        ExecuteClockCycles(1); // InstBRKimp3
        ExecuteClockCycles(1); // InstBRKimp4
        ExecuteClockCycles(1); // InstBRKimp5
        Pins.DataBus = (vectorLsb);
        ExecuteClockCycles(1); // InstBRKimp6
        Pins.DataBus = (vectorMsb);
        ExecuteClockCycles(1); // InstBRKimp7

        // ASSERT:
        Assert.Equal(new UInt16(0xFFFB), Pins.AddrBus);
        Assert.Equal(handlerAddr, Regs.PC);
        Assert.Equal(stackTop.Sbc(new UInt8(3), High, Low).Value(), Regs.S);
        Assert.Equal(Low, Regs.P.Break);
        Assert.Equal(Low, Regs.P.Decimal);
        Assert.Equal(High, Regs.P.IRQDisabled);
    }

    // -------------------------------------------------------------------------
    // IRQ
    // -------------------------------------------------------------------------
    [Fact]
    public void TestIrq()
    {
        // ARRANGE:
        var interruptedOpCode = OpCodes.LDAzpg.ToUInt8(); // 3 cycle opcode
        var operand = new UInt8(0x12);
        var data = new UInt8(0x55);
        var vectorLsb = new UInt8(0x80);
        var vectorMsb = new UInt8(0x04);
        var handlerAddr = new UInt16(vectorLsb, vectorMsb);

        var stackTop = new UInt8(0xFF);

        BootToAddress(BootAddr);
        Regs.P.IRQDisabled.UpdateValue(Low);
        Regs.S.UpdateValue(stackTop);

        // ACT:
        Pins.DataBus = (interruptedOpCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetIRQB(0);

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstLDAzpg2 - fetch the second byte of the op-code (zpg-value)

        Pins.DataBus = (data);
        ExecuteClockCycles(1); // InstLDAzpg3 - fetch the value at address zpg-value

        ExecuteClockCycles(1); // Irq1
        ExecuteClockCycles(1); // InstBRKimp2
        ExecuteClockCycles(1); // InstBRKimp3
        ExecuteClockCycles(1); // InstBRKimp4
        ExecuteClockCycles(1); // InstBRKimp5
        Pins.DataBus = (vectorLsb);
        ExecuteClockCycles(1); // InstBRKimp6
        Pins.DataBus = (vectorMsb);
        ExecuteClockCycles(1); // InstBRKimp7

        // ASSERT:
        Assert.Equal(new UInt16(0xFFFF), Pins.AddrBus);
        Assert.Equal(handlerAddr, Regs.PC);
        Assert.Equal(stackTop.Sbc(new UInt8(3), High, Low).Value(), Regs.S);
        Assert.Equal(Low, Regs.P.Break);
        Assert.Equal(Low, Regs.P.Decimal);
        Assert.Equal(High, Regs.P.IRQDisabled);
    }
}
