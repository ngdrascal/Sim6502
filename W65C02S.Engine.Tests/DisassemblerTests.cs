using System.Diagnostics.CodeAnalysis;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine.Tests;

[ExcludeFromCodeCoverage]
public class DisassemblerTests
{
    /*
       TITLE: Disassemble formats each addressing mode's operand in standard syntax
       GIVEN: an opcode at $1000 with operand bytes $34, $12; cases cover JMP (abs), JMP (abs,X),
              zp,X, zp,Y, (zp,X), (zp),Y,
              RMBn/SMBn zero page, and BBRn/BBSn zero page,relative
       WHEN: Disassemble is called
       THEN: it returns the address, mnemonic and operand in that mode's syntax
     */
    [Theory]
    [InlineData(OpCodes.JMPind, "1000: JMP ($1234)")]
    [InlineData(OpCodes.JMPabsxind, "1000: JMP ($1234,X)")]
    [InlineData(OpCodes.LDAzpgx, "1000: LDA $34,X")]
    [InlineData(OpCodes.LDXzpgy, "1000: LDX $34,Y")]
    [InlineData(OpCodes.LDAindx, "1000: LDA ($34,X)")]
    [InlineData(OpCodes.LDAindy, "1000: LDA ($34),Y")]
    [InlineData(OpCodes.RMB3zpg, "1000: RMB3 $34")]
    [InlineData(OpCodes.SMB7zpg, "1000: SMB7 $34")]
    [InlineData(OpCodes.BBR3zpgrel, "1000: BBR3 $34,$12")]
    [InlineData(OpCodes.BBS7zpgrel, "1000: BBS7 $34,$12")]
    public void DisassemblesOperandSyntax(OpCodes opCode, string expected)
    {
        // ARRANGE:
        var address = new UInt16(0x1000);

        // ACT:
        var text = Disassembler.Disassemble(address, opCode, new UInt8(0x34), new UInt8(0x12));

        // ASSERT:
        Assert.Equal(expected, text);
    }
}
