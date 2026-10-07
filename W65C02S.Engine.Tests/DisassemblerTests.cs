using System.Diagnostics.CodeAnalysis;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class DisassemblerTests
{
    /*
       TITLE: Disassemble formats JMP absolute indexed indirect as JMP ($hhll,X)
       GIVEN: opcode JMPabsxind at $1000 with operand bytes $34, $12
       WHEN: Disassemble is called
       THEN: it returns "1000: JMP ($1234,X)"
     */
    [Fact]
    public void DisassemblesJmpAbsoluteIndexedIndirect()
    {
        // ARRANGE:
        var address = new UInt16(0x1000);

        // ACT:
        var text = Disassembler.Disassemble(address, OpCodes.JMPabsxind, new UInt8(0x34), new UInt8(0x12));

        // ASSERT:
        Assert.Equal("1000: JMP ($1234,X)", text);
    }
}
