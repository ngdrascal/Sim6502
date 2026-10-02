// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

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
        Assert.Equal(opCode, Pins.DBGINST);
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

        Pins.NMIB = 0;

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

        Pins.IRQB = 0;

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

    /*
       TITLE: After an NMI or IRQ has been serviced the handler runs instead of re-entering the interrupt
       GIVEN: a 7-cycle interrupt sequence into a handler that starts with NOPs, with NMIB or IRQB left low
       WHEN: 3 more cycles run
       THEN: the handler's NOPs execute (PC advances by 2) and nothing more is pushed on the stack
     */
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ServicedInterruptIsNotReentered(bool nmi)
    {
        // ARRANGE:
        var nop = OpCodes.NOP.ToUInt8();
        var handlerAddr = new UInt16(0x0480);
        var stackTop = new UInt8(0xFF);
        var expectedStackTop = stackTop.Copy().Dec().Dec().Dec();
        var expectedPC = handlerAddr.Copy().AddUnsigned(new UInt8(2));

        BootToAddress(BootAddr);
        Regs.P.IRQDisabled.UpdateValue(Low);
        Regs.S.UpdateValue(stackTop);

        Pins.DataBus = nop;
        ExecuteClockCycles(1); // fetch NOP
        if (nmi)
            Pins.NMIB = 0;
        else
            Pins.IRQB = 0;
        ExecuteClockCycles(1); // NOP 2

        ExecuteClockCycles(5); // Interrupt1, InstBRKimp2..5
        Pins.DataBus = handlerAddr.Lsb();
        ExecuteClockCycles(1); // InstBRKimp6
        Pins.DataBus = handlerAddr.Msb();
        ExecuteClockCycles(1); // InstBRKimp7

        // ACT:
        Pins.DataBus = nop;
        ExecuteClockCycles(3); // fetch NOP, NOP 2, fetch NOP

        // ASSERT:
        Assert.Equal(expectedPC, Regs.PC);
        Assert.Equal(expectedStackTop, Regs.S);
    }
}
