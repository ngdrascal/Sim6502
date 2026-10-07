// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class ControlTests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // JMP absolute
    // -------------------------------------------------------------------------
    [Fact]
    public void TestJMPabs()
    {
        // ARRANGE:
        var opCode = OpCodes.JMPabs.ToUInt8();
        var operand1 = new UInt8(0x33);
        var operand2 = new UInt8(0x04);
        var expectedPC = new UInt16(operand1, operand2);

        BootToAddress(new UInt16(0x040B));

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstJMPabs2 - fetch the low byte of the target address

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstJMPabs3 - fetch the high byte of the target address

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedPC, Regs.PC);
    }

    // -------------------------------------------------------------------------
    // JMP indirect
    // -------------------------------------------------------------------------
    [Fact]
    public void TestJMPind()
    {
        // ARRANGE:
        var opCode = OpCodes.JMPind.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var indAddrLsb = new UInt8(0xBA);
        var indAddrMsb = new UInt8(0xDC);
        var expectedPC = new UInt16(indAddrLsb, indAddrMsb);

        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstJMPind2 - fetch the low byte of the indirect address

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstJMPind3 - fetch the high byte of the indirect address

        Pins.DataBus = (indAddrLsb);
        ExecuteClockCycles(1); // InstJMPind4 - fetch the low byte of the target address

        Pins.DataBus = (indAddrMsb);
        ExecuteClockCycles(1); // InstJMPind5 - fetch the high byte of the target address

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedPC, Regs.PC);
    }

    // -------------------------------------------------------------------------
    // JMP absolute indexed indirect
    // -------------------------------------------------------------------------
    /*
      TITLE: JMP (abs,X) loads PC from the pointer at base + X with the datasheet bus activity
      GIVEN: a CPU booted to $1000 and X set; cases cover X = 0, an ordinary X, base + X carrying
             into the high byte, a pointer at $xxFF, base + X wrapping past $FFFF, and a pointer
             at $FFFF
      WHEN: JMP (abs,X) runs for its 6 cycles with the operand and target bytes on the data bus
      THEN: cycles 2-4 address PC+1, PC+2, PC+2; cycles 5-6 address base + X and base + X + 1
            (full 16-bit adds, no page wrap); every cycle is a read; PC is the target
    */
    [Theory]
    [InlineData(0x1234, 0x00, 0x1234)]
    [InlineData(0x1234, 0x05, 0x1239)]
    [InlineData(0x12F0, 0x20, 0x1310)]
    [InlineData(0x20F0, 0x0F, 0x20FF)]
    [InlineData(0xFFF0, 0x20, 0x0010)]
    [InlineData(0xFFFF, 0x00, 0xFFFF)]
    public void TestJMPabsxind(int baseAddr, int xValue, int pointer)
    {
        // ARRANGE:
        var opCode = OpCodes.JMPabsxind.ToUInt8();
        var operand = new UInt16(baseAddr);
        var targetLsb = new UInt8(0xBA);
        var targetMsb = new UInt8(0xDC);
        var expectedAddrs = new[]
        {
            new UInt16(0x1001), new UInt16(0x1002), new UInt16(0x1002),
            new UInt16(pointer), new UInt16(pointer + 1)
        };
        var addrs = new List<UInt16>();
        var rwbs = new List<byte>();

        BootToAddress(BootAddr);
        Regs.X = new UInt8(xValue);

        // ACT:
        Pins.DataBus = opCode;
        ExecuteClockCycles(1); // fetch the opcode

        var dataPerCycle = new[] { operand.Lsb(), operand.Msb(), Pins.DataBus, targetLsb, targetMsb };
        foreach (var data in dataPerCycle)
        {
            // JMPabsxind2..6 - base low, base high, internal op (add X), target low, target high
            Pins.DataBus = data;
            ExecuteClockCycles(1);
            addrs.Add(Pins.AddrBus);
            rwbs.Add(Pins.RWB);
        }

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedAddrs, addrs);
        Assert.All(rwbs, rwb => Assert.Equal((byte)High.ToInt(), rwb));
        Assert.Equal(new UInt16(targetLsb, targetMsb), Regs.PC);
    }

    // -------------------------------------------------------------------------
    // JSR absolute
    // -------------------------------------------------------------------------
    [Fact]
    public void TestJSRabs()
    {
        // ARRANGE:
        var opCode = OpCodes.JSRabs.ToUInt8();
        var operand1 = new UInt8(0x21);
        var operand2 = new UInt8(0x43);
        var expectedPC = new UInt16(operand1, operand2);
        var stackTop = new UInt8(0xFF);
        var expectedStackTop = stackTop.Copy().Dec().Dec();
        var expectedReturnAddr = BootAddr.Copy().AddUnsigned(new UInt8(2));

        BootToAddress(BootAddr);
        Regs.S = stackTop;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand1);
        ExecuteClockCycles(1); // InstJSRabs2 - fetch the low byte of the subroutine address

        Pins.DataBus = (operand2);
        ExecuteClockCycles(1); // InstJSRabs3 - fetch the high byte of the subroutine address

        ExecuteClockCycles(1); // InstJSRabs4 - push the high byte of the PC
        Assert.Equal(expectedReturnAddr.Msb(), Pins.DataBus);

        ExecuteClockCycles(1); // InstJSRabs5 - push the low byte of the PC
        Assert.Equal(expectedReturnAddr.Lsb(), Pins.DataBus);

        ExecuteClockCycles(1); // InstJSRabs6 - set PC reg to the subroutine address

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedPC, Regs.PC);
        Assert.Equal(expectedStackTop, Regs.S);
    }

    // -------------------------------------------------------------------------
    // RTI implied
    // -------------------------------------------------------------------------
    [Fact]
    public void TestRTIimp()
    {
        // ARRANGE:
        var opCode = OpCodes.RTIimp.ToUInt8();
        var returnAddrLsb = new UInt8(0x21);
        var returnAddrMsb = new UInt8(0x43);
        var expectedReturnAddr = new UInt16(returnAddrLsb, returnAddrMsb);
        var stackTop = new UInt8(0xFC);
        var expectedStackTop = stackTop.Copy().Inc().Inc().Inc();
        var expectedFlags = UInt8FromFlags("NvbdizC");

        BootToAddress(BootAddr);
        Regs.S = stackTop;
        Regs.P.LoadFlags("nvbdizc");

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstRTIimp2 - internal operation

        ExecuteClockCycles(1); // InstRTIimp3 - internal operation

        Pins.DataBus = expectedFlags;
        ExecuteClockCycles(1); // InstRTIimp4 - pull the status register

        Pins.DataBus = (returnAddrLsb);
        ExecuteClockCycles(1); // InstRTIimp5 - pull the low byte of the PC reg

        Pins.DataBus = (returnAddrMsb);
        ExecuteClockCycles(1); // InstRTIimp6 - pull the high byte of the PC reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedReturnAddr, Regs.PC);
        Assert.Equal(expectedStackTop, Regs.S);
        Assert.Equal(expectedFlags, Regs.P.ToUInt8());
    }

    // -------------------------------------------------------------------------
    // RTS implied
    // -------------------------------------------------------------------------
    [Fact]
    public void TestRTSimp()
    {
        // ARRANGE:
        var opCode = OpCodes.RTSimp.ToUInt8();
        var returnAddrLsb = new UInt8(0x21);
        var returnAddrMsb = new UInt8(0x43);
        var expectedReturnAddr = new UInt16(returnAddrLsb, returnAddrMsb).Inc();
        var stackTop = new UInt8(0xFD);
        var expectedStackTop = stackTop.Copy().Inc().Inc();

        BootToAddress(BootAddr);
        Regs.S = stackTop;

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(1); // InstRTSimp2 - internal operation

        ExecuteClockCycles(1); // InstRTSimp3 - internal operation

        Pins.DataBus = (returnAddrLsb);
        ExecuteClockCycles(1); // InstRTSimp4 - pull the low byte of the PC reg

        Pins.DataBus = (returnAddrMsb);
        ExecuteClockCycles(1); // InstRTSimp5 - pull the high byte of the PC reg

        ExecuteClockCycles(1); // InstRTSimp6 - update the PC reg

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(expectedReturnAddr, Regs.PC);
        Assert.Equal(expectedStackTop, Regs.S);
    }

    // -------------------------------------------------------------------------
    // Integration
    // -------------------------------------------------------------------------
    [Fact]
    public void TestJsrRtsIntegration()
    {
        // ARRANGE:
        byte[] program =
        [ 
                                    // 0000:       .ORG $0000
             0xA2,  0xFF,           // 0000:       LDX #$FF        ; load SP with $FF
             0x9A,                  // 0002:       TXS
             0x18,                  // 0003:       CLC             ; clear the carry flag
             0xA9,  0x01,           // 0004:       LDA #01         ; pass $01 to the subroutine
             0x20,  0x0A,  0x00,    // 0006:       JSR SUB1        ; JMP to subroutine
             0x00,                  // 0009:       BRK             ; stop execution
             0x38,                  // 000A: SUB1: SEC             ; set the carry flag
             0x60                   // 000B:       RTS
        ];

        // ACT:
        ExecuteProgram(program, 0, 0);

        // ASSERT:
        Assert.True(Regs.P.Carry.IsSet());
    }

    [Fact]
    public void TestJsrRtsIntegration2()
    {
        // ARRANGE
        byte[] program =
        [
            0xA9, 0x1F,             // 0000:            LDA #$1F        ; 8-N-1, 19200 baud
            0x8D, 0x03, 0x80,       // 0002:            STA ACIA_CTRL
            0xA9, 0x0B,             // 0005:            LDA #$0B        ; no parity, no echo, no interrupts
            0x8D, 0x02, 0x80,       // 0007:            STA ACIA_CMD
            0xA9, 0x1B,             // 000A:            LDA #$1B        ; begin with escape
            0x20, 0x10, 0x00,       // 000C:            JSR ECHO
            0x00,                   // 000F:            WAI
            0x48,                   // 0010: ECHO:      PHA             ; save A
            0x8D, 0x00, 0x80,       // 0011:            STA ACIA_DATA   ; output character
            0xA9, 0x02,             // 0014:            LDA #$02        ; initialize delay loop
            0x3A,                   // 0016: TXDELAY:   DEC             ; decrement A
            0xD0, 0xFD,             // 0017:            BNE TXDELAY     ; until A gets to 0
            0x68,                   // 0019:            PLA             ; restore A
            0x60                    // 001A:            RTS             ; return         
        ];

        // ACT:
        ExecuteProgram(program, 0, 0);

        // ASSERT:
        Assert.False(Regs.P.Carry.IsSet());
    }
}
