// ReSharper disable InconsistentNaming
namespace W65C02S.Engine;

public enum States
{
    WarmUp0, WarmUp1, WarmUp2, // Warmup
    Boot1, Boot2, // Reboot
    Fetch, // Fetch instruction
    WaitForInterrupt, // Wait for interrupt
    Stop, // Stop processor
    NotReady, // Ready input line is low

    Interrupt1, // Start of non-maskable and maskable interrupt sequence

    InstLDAimm2, // LDA immediate
    InstLDAzpg2, InstLDAzpg3, // LDA zeropage
    InstLDAzpgx2, InstLDAzpgx3, InstLDAzpgx4, // LDA zeropage,X
    InstLDAabs2, InstLDAabs3, InstLDAabs4, // LDA absolute
    InstLDAabsx2, InstLDAabsx3, InstLDAabsx4, InstLDAabsx5, // LDA absolute,X
    InstLDAabsy2, InstLDAabsy3, InstLDAabsy4, InstLDAabsy5, // LDA absolute,Y
    InstLDAindx2, InstLDAindx3, InstLDAindx4, InstLDAindx5, InstLDAindx6, // LDA (indirect,X)
    InstLDAindy2, InstLDAindy3, InstLDAindy4, InstLDAindy5, InstLDAindy6, // LDA (indirect),Y
    InstLDAind2, InstLDAind3, InstLDAind4, InstLDAind5, // LDA (indirect)

    InstLDXimm2, // LDX immediate
    InstLDXzpg2, InstLDXzpg3, // LDX zeropage
    InstLDXzpgy2, InstLDXzpgy3, InstLDXzpgy4, // LDX zeropage,Y
    InstLDXabs2, InstLDXabs3, InstLDXabs4, // LDX absolute
    InstLDXabsy2, InstLDXabsy3, InstLDXabsy4, InstLDXabsy5, // LDX absolute,Y

    InstLDYimm2, // LDY immediate
    InstLDYzpg2, InstLDYzpg3, // LDY zeropage
    InstLDYzpgx2, InstLDYzpgx3, InstLDYzpgx4, // LDY zeropage,X
    InstLDYabs2, InstLDYabs3, InstLDYabs4, // LDY absolute
    InstLDYabsx2, InstLDYabsx3, InstLDYabsx4, InstLDYabsx5, // LDY absolute,X

    InstSTAzpg2, InstSTAzpg3, // STA zeropage
    InstSTAzpgx2, InstSTAzpgx3, InstSTAzpgx4, // STA zeropage,X
    InstSTAabs2, InstSTAabs3, InstSTAabs4, // STA absolute
    InstSTAabsx2, InstSTAabsx3, InstSTAabsx4, InstSTAabsx5, // STA absolute,X
    InstSTAabsy2, InstSTAabsy3, InstSTAabsy4, InstSTAabsy5, // STA absolute,Y
    InstSTAindx2, InstSTAindx3, InstSTAindx4, InstSTAindx5, InstSTAindx6, // STA (indirect,X)
    InstSTAindy2, InstSTAindy3, InstSTAindy4, InstSTAindy5, InstSTAindy6, // STA (indirect,Y)
    InstSTAind2, InstSTAind3, InstSTAind4, InstSTAind5, // STA (indirect)

    InstSTXzpg2, InstSTXzpg3, // STX zeropage
    InstSTXzpgy2, InstSTXzpgy3, InstSTXzpgy4, // STX zeropage,Y
    InstSTXabs2, InstSTXabs3, InstSTXabs4, // STX absolute

    InstSTYzpg2, InstSTYzpg3, // STY zeropage
    InstSTYzpgx2, InstSTYzpgx3, InstSTYzpgx4, // STY zeropage,X
    InstSTYabs2, InstSTYabs3, InstSTYabs4, // STX absolute

    InstSTZzpg2, InstSTZzpg3, // STZ zeropage
    InstSTZzpgx2, InstSTZzpgx3, InstSTZzpgx4, // STZ zeropage,X
    InstSTZabs2, InstSTZabs3, InstSTZabs4, // STZ absolute
    InstSTZabsx2, InstSTZabsx3, InstSTZabsx4, InstSTZabsx5, // STZ absolute,X

    InstTAXimp2, // TAX implied
    InstTAYimp2, // TAY implied
    InstTSXimp2, // TSX implied
    InstTXAimp2, // TXA implied
    InstTXSimp2, // TXS implied
    InstTYAimp2, // TYA implied

    InstPHAimp2, InstPHAimp3, // PHA implied
    InstPHPimp2, InstPHPimp3, // PHP implied
    InstPHXimp2, InstPHXimp3, // PHX implied
    InstPHYimp2, InstPHYimp3, // PHY implied
    InstPLAimp2, InstPLAimp3, InstPLAimp4, // PLA implied
    InstPLPimp2, InstPLPimp3, InstPLPimp4, // PLP implied
    InstPLXimp2, InstPLXimp3, InstPLXimp4, // PHX implied
    InstPLYimp2, InstPLYimp3, InstPLYimp4, // PHY implied

    InstASLacc2, // ASL accumulator
    InstASLzpg2, InstASLzpg3, InstASLzpg4, InstASLzpg5, // ASL zeropage
    InstASLzpgx2, InstASLzpgx3, InstASLzpgx4, InstASLzpgx5, InstASLzpgx6, // ASL zeropage,X
    InstASLabs2, InstASLabs3, InstASLabs4, InstASLabs5, InstASLabs6, // ASL absolute
    InstASLabsx2, InstASLabsx3, InstASLabsx4, InstASLabsx5, InstASLabsx6, InstASLabsx7, // ASL absolute,X

    InstLSRacc2, // LSR accumulator
    InstLSRzpg2, InstLSRzpg3, InstLSRzpg4, InstLSRzpg5, // LSR zeropage
    InstLSRzpgx2, InstLSRzpgx3, InstLSRzpgx4, InstLSRzpgx5, InstLSRzpgx6, // LSR zeropage,X
    InstLSRabs2, InstLSRabs3, InstLSRabs4, InstLSRabs5, InstLSRabs6, // LSR absolute
    InstLSRabsx2, InstLSRabsx3, InstLSRabsx4, InstLSRabsx5, InstLSRabsx6, InstLSRabsx7, // LSR absolute,X

    InstROLacc2, // ROL accumulator
    InstROLzpg2, InstROLzpg3, InstROLzpg4, InstROLzpg5, // ROL zeropage
    InstROLzpgx2, InstROLzpgx3, InstROLzpgx4, InstROLzpgx5, InstROLzpgx6, // ROL zeropage,X
    InstROLabs2, InstROLabs3, InstROLabs4, InstROLabs5, InstROLabs6, // ROL absolute
    InstROLabsx2, InstROLabsx3, InstROLabsx4, InstROLabsx5, InstROLabsx6, InstROLabsx7, // ROL absolute,X

    InstRORacc2, // ROR accumulator
    InstRORzpg2, InstRORzpg3, InstRORzpg4, InstRORzpg5, // ROR zeropage
    InstRORzpgx2, InstRORzpgx3, InstRORzpgx4, InstRORzpgx5, InstRORzpgx6, // ROR zeropage,X
    InstRORabs2, InstRORabs3, InstRORabs4, InstRORabs5, InstRORabs6, // ROR absolute
    InstRORabsx2, InstRORabsx3, InstRORabsx4, InstRORabsx5, InstRORabsx6, InstRORabsx7, // ROR absolute,X

    InstANDimm2, // AND immediate
    InstANDzpg2, InstANDzpg3, // AND zeropage
    InstANDzpgx2, InstANDzpgx3, InstANDzpgx4, // AND zeropage,X
    InstANDabs2, InstANDabs3, InstANDabs4, // AND absolute
    InstANDabsx2, InstANDabsx3, InstANDabsx4, InstANDabsx5, // AND absolute,X
    InstANDabsy2, InstANDabsy3, InstANDabsy4, InstANDabsy5, // AND absolute,Y
    InstANDindx2, InstANDindx3, InstANDindx4, InstANDindx5, InstANDindx6, // AND (indirect,X)
    InstANDindy2, InstANDindy3, InstANDindy4, InstANDindy5, InstANDindy6, // AND (indirect),Y
    InstANDind2, InstANDind3, InstANDind4, InstANDind5, // AND (indirect)

    InstBITimm2, // BIT immediate
    InstBITzpg2, InstBITzpg3, // BIT zeropage
    InstBITzpgx2, InstBITzpgx3, InstBITzpgx4, // BIT zeropage,X
    InstBITabs2, InstBITabs3, InstBITabs4, // BIT absolute
    InstBITabsx2, InstBITabsx3, InstBITabsx4, InstBITabsx5, // BIT absolute,X

    InstEORimm2, // EOR immediate
    InstEORzpg2, InstEORzpg3, // EOR zeropage
    InstEORzpgx2, InstEORzpgx3, InstEORzpgx4, // EOR zeropage,X
    InstEORabs2, InstEORabs3, InstEORabs4, // EOR absolute
    InstEORabsx2, InstEORabsx3, InstEORabsx4, InstEORabsx5, // EOR absolute,X
    InstEORabsy2, InstEORabsy3, InstEORabsy4, InstEORabsy5, // EOR absolute,Y
    InstEORindx2, InstEORindx3, InstEORindx4, InstEORindx5, InstEORindx6, // EOR (indirect,X)
    InstEORindy2, InstEORindy3, InstEORindy4, InstEORindy5, InstEORindy6, // EOR (indirect),Y
    InstEORind2, InstEORind3, InstEORind4, InstEORind5, // EOR (indirect)

    InstORAimm2, // ORA immediate
    InstORAzpg2, InstORAzpg3, // ORA zeropage
    InstORAzpgx2, InstORAzpgx3, InstORAzpgx4, // ORA zeropage,X
    InstORAabs2, InstORAabs3, InstORAabs4, // ORA absolute
    InstORAabsx2, InstORAabsx3, InstORAabsx4, InstORAabsx5, // ORA absolute,X
    InstORAabsy2, InstORAabsy3, InstORAabsy4, InstORAabsy5, // ORA absolute,Y
    InstORAindx2, InstORAindx3, InstORAindx4, InstORAindx5, InstORAindx6, // ORA (indirect,X)
    InstORAindy2, InstORAindy3, InstORAindy4, InstORAindy5, InstORAindy6, // ORA (indirect),Y
    InstORAind2, InstORAind3, InstORAind4, InstORAind5, // ORA (indirect)

    InstTRBzpg2, InstTRBzpg3, InstTRBzpg4, InstTRBzpg5, // TRB zeropage
    InstTRBabs2, InstTRBabs3, InstTRBabs4, InstTRBabs5, InstTRBabs6, // TRB absolute

    InstTSBzpg2, InstTSBzpg3, InstTSBzpg4, InstTSBzpg5, // TSB zeropage
    InstTSBabs2, InstTSBabs3, InstTSBabs4, InstTSBabs5, InstTSBabs6, // TSB absolute

    InstRMB0zpg2, InstRMB0zpg3, InstRMB0zpg4, InstRMB0zpg5, // RMB0 zeropage
    InstRMB1zpg2, InstRMB1zpg3, InstRMB1zpg4, InstRMB1zpg5, // RMB1 zeropage
    InstRMB2zpg2, InstRMB2zpg3, InstRMB2zpg4, InstRMB2zpg5, // RMB2 zeropage
    InstRMB3zpg2, InstRMB3zpg3, InstRMB3zpg4, InstRMB3zpg5, // RMB3 zeropage
    InstRMB4zpg2, InstRMB4zpg3, InstRMB4zpg4, InstRMB4zpg5, // RMB4 zeropage
    InstRMB5zpg2, InstRMB5zpg3, InstRMB5zpg4, InstRMB5zpg5, // RMB5 zeropage
    InstRMB6zpg2, InstRMB6zpg3, InstRMB6zpg4, InstRMB6zpg5, // RMB6 zeropage
    InstRMB7zpg2, InstRMB7zpg3, InstRMB7zpg4, InstRMB7zpg5, // RMB7 zeropage

    InstSMB0zpg2, InstSMB0zpg3, InstSMB0zpg4, InstSMB0zpg5, // SMB0 zeropage
    InstSMB1zpg2, InstSMB1zpg3, InstSMB1zpg4, InstSMB1zpg5, // SMB1 zeropage
    InstSMB2zpg2, InstSMB2zpg3, InstSMB2zpg4, InstSMB2zpg5, // SMB2 zeropage
    InstSMB3zpg2, InstSMB3zpg3, InstSMB3zpg4, InstSMB3zpg5, // SMB3 zeropage
    InstSMB4zpg2, InstSMB4zpg3, InstSMB4zpg4, InstSMB4zpg5, // SMB4 zeropage
    InstSMB5zpg2, InstSMB5zpg3, InstSMB5zpg4, InstSMB5zpg5, // SMB5 zeropage
    InstSMB6zpg2, InstSMB6zpg3, InstSMB6zpg4, InstSMB6zpg5, // SMB6 zeropage
    InstSMB7zpg2, InstSMB7zpg3, InstSMB7zpg4, InstSMB7zpg5, // SMB7 zeropage

    InstBBR0zpgrel2, InstBBR0zpgrel3, InstBBR0zpgrel4, InstBBR0zpgrel5, InstBBR0zpgrel6, InstBBR0zpgrel7, // BBR0 zeropage, relative
    InstBBR1zpgrel2, InstBBR1zpgrel3, InstBBR1zpgrel4, InstBBR1zpgrel5, InstBBR1zpgrel6, InstBBR1zpgrel7, // BBR1 zeropage, relative
    InstBBR2zpgrel2, InstBBR2zpgrel3, InstBBR2zpgrel4, InstBBR2zpgrel5, InstBBR2zpgrel6, InstBBR2zpgrel7, // BBR2 zeropage, relative
    InstBBR3zpgrel2, InstBBR3zpgrel3, InstBBR3zpgrel4, InstBBR3zpgrel5, InstBBR3zpgrel6, InstBBR3zpgrel7, // BBR3 zeropage, relative
    InstBBR4zpgrel2, InstBBR4zpgrel3, InstBBR4zpgrel4, InstBBR4zpgrel5, InstBBR4zpgrel6, InstBBR4zpgrel7, // BBR4 zeropage, relative
    InstBBR5zpgrel2, InstBBR5zpgrel3, InstBBR5zpgrel4, InstBBR5zpgrel5, InstBBR5zpgrel6, InstBBR5zpgrel7, // BBR5 zeropage, relative
    InstBBR6zpgrel2, InstBBR6zpgrel3, InstBBR6zpgrel4, InstBBR6zpgrel5, InstBBR6zpgrel6, InstBBR6zpgrel7, // BBR6 zeropage, relative
    InstBBR7zpgrel2, InstBBR7zpgrel3, InstBBR7zpgrel4, InstBBR7zpgrel5, InstBBR7zpgrel6, InstBBR7zpgrel7, // BBR7 zeropage, relative

    InstBBS0zpgrel2, InstBBS0zpgrel3, InstBBS0zpgrel4, InstBBS0zpgrel5, InstBBS0zpgrel6, InstBBS0zpgrel7, // BBS0 zeropage, relative
    InstBBS1zpgrel2, InstBBS1zpgrel3, InstBBS1zpgrel4, InstBBS1zpgrel5, InstBBS1zpgrel6, InstBBS1zpgrel7, // BBS1 zeropage, relative
    InstBBS2zpgrel2, InstBBS2zpgrel3, InstBBS2zpgrel4, InstBBS2zpgrel5, InstBBS2zpgrel6, InstBBS2zpgrel7, // BBS2 zeropage, relative
    InstBBS3zpgrel2, InstBBS3zpgrel3, InstBBS3zpgrel4, InstBBS3zpgrel5, InstBBS3zpgrel6, InstBBS3zpgrel7, // BBS3 zeropage, relative
    InstBBS4zpgrel2, InstBBS4zpgrel3, InstBBS4zpgrel4, InstBBS4zpgrel5, InstBBS4zpgrel6, InstBBS4zpgrel7, // BBS4 zeropage, relative
    InstBBS5zpgrel2, InstBBS5zpgrel3, InstBBS5zpgrel4, InstBBS5zpgrel5, InstBBS5zpgrel6, InstBBS5zpgrel7, // BBS5 zeropage, relative
    InstBBS6zpgrel2, InstBBS6zpgrel3, InstBBS6zpgrel4, InstBBS6zpgrel5, InstBBS6zpgrel6, InstBBS6zpgrel7, // BBS6 zeropage, relative
    InstBBS7zpgrel2, InstBBS7zpgrel3, InstBBS7zpgrel4, InstBBS7zpgrel5, InstBBS7zpgrel6, InstBBS7zpgrel7, // BBS7 zeropage, relative

    InstADCimm2, InstADCimm3, // ADC immediate
    InstADCzpg2, InstADCzpg3, InstADCzpg4, // ADC zeropage
    InstADCzpgx2, InstADCzpgx3, InstADCzpgx4, InstADCzpgx5, // ADC zeropage,X
    InstADCabs2, InstADCabs3, InstADCabs4, InstADCabs5, // ADC absolute
    InstADCabsx2, InstADCabsx3, InstADCabsx4, InstADCabsx5, InstADCabsx6, // ADC absolute,X
    InstADCabsy2, InstADCabsy3, InstADCabsy4, InstADCabsy5, InstADCabsy6, // ADC absolute,Y
    InstADCindx2, InstADCindx3, InstADCindx4, InstADCindx5, InstADCindx6, InstADCindx7, // ADC (indirect,X)
    InstADCindy2, InstADCindy3, InstADCindy4, InstADCindy5, InstADCindy6, InstADCindy7, // ADC (indirect),Y
    InstADCind2, InstADCind3, InstADCind4, InstADCind5, InstADCind6, // ADC (indirect)

    // CMP
    InstCMPimm2, // CMP immediate
    InstCMPzpg2, InstCMPzpg3, // CMP zeropage
    InstCMPzpgx2, InstCMPzpgx3, InstCMPzpgx4, // CMP zeropage,X
    InstCMPabs2, InstCMPabs3, InstCMPabs4, // CMP absolute
    InstCMPabsx2, InstCMPabsx3, InstCMPabsx4, InstCMPabsx5, // CMP absolute,X
    InstCMPabsy2, InstCMPabsy3, InstCMPabsy4, InstCMPabsy5, // CMP absolute,Y
    InstCMPindx2, InstCMPindx3, InstCMPindx4, InstCMPindx5, InstCMPindx6, // CMP (indirect,X)
    InstCMPindy2, InstCMPindy3, InstCMPindy4, InstCMPindy5, InstCMPindy6, // CMP (indirect),Y
    InstCMPind2, InstCMPind3, InstCMPind4, InstCMPind5, // CMP (indirect)

    // CPX
    InstCPXimm2, // CPX immediate
    InstCPXzpg2, InstCPXzpg3, // CPX zeropage
    InstCPXabs2, InstCPXabs3, InstCPXabs4, // CPX absolute

    // CPY
    InstCPYimm2, // CPY immediate
    InstCPYzpg2, InstCPYzpg3, // CPY zeropage
    InstCPYabs2, InstCPYabs3, InstCPYabs4, // CPY absolute

    // SBC
    InstSBCimm2, InstSBCimm3, // SBC immediate
    InstSBCzpg2, InstSBCzpg3, InstSBCzpg4, // SBC zeropage
    InstSBCzpgx2, InstSBCzpgx3, InstSBCzpgx4, InstSBCzpgx5, // SBC zeropage,X
    InstSBCabs2, InstSBCabs3, InstSBCabs4, InstSBCabs5, // SBC absolute
    InstSBCabsx2, InstSBCabsx3, InstSBCabsx4, InstSBCabsx5, InstSBCabsx6, // SBC absolute,X
    InstSBCabsy2, InstSBCabsy3, InstSBCabsy4, InstSBCabsy5, InstSBCabsy6, // SBC absolute,Y
    InstSBCindx2, InstSBCindx3, InstSBCindx4, InstSBCindx5, InstSBCindx6, InstSBCindx7, // SBC (indirect,X)
    InstSBCindy2, InstSBCindy3, InstSBCindy4, InstSBCindy5, InstSBCindy6, InstSBCindy7, // SBC (indirect),Y
    InstSBCind2, InstSBCind3, InstSBCind4, InstSBCind5, InstSBCind6, // SBC (indirect)

    InstDECacc2, // DEC accumulator,
    InstDECzpg2, InstDECzpg3, InstDECzpg4, InstDECzpg5, // DEC zeropage
    InstDECzpgx2, InstDECzpgx3, InstDECzpgx4, InstDECzpgx5, InstDECzpgx6, // DEC zeropage,X
    InstDECabs2, InstDECabs3, InstDECabs4, InstDECabs5, InstDECabs6, // DEC absolute
    InstDECabsx2, InstDECabsx3, InstDECabsx4, InstDECabsx5, InstDECabsx6, InstDECabsx7, // DEC absolute,X

    InstDEXimp2, // DEX implied

    InstDEYimp2, // DEY implied

    InstINCacc2, // INC accumulator
    InstINCzpg2, InstINCzpg3, InstINCzpg4, InstINCzpg5, // INC zeropage
    InstINCzpgx2, InstINCzpgx3, InstINCzpgx4, InstINCzpgx5, InstINCzpgx6, // INC zeropage,X
    InstINCabs2, InstINCabs3, InstINCabs4, InstINCabs5, InstINCabs6, // INC absolute
    InstINCabsx2, InstINCabsx3, InstINCabsx4, InstINCabsx5, InstINCabsx6, InstINCabsx7, // INC absolute,X

    InstINXimp2, // INX implied

    InstINYimp2, // INY implied

    InstBRArel2, InstBRArel3, InstBRArel4, // BRA relative

    InstBRKimp2, InstBRKimp3, InstBRKimp4, InstBRKimp5, InstBRKimp6, InstBRKimp7, // BRK implied

    InstJMPabs2, InstJMPabs3, // JMP absolute
    InstJMPind2, InstJMPind3, InstJMPind4, InstJMPind5, // JMP indirect
    InstJMPabsxind2, InstJMPabsxind3, InstJMPabsxind4, InstJMPabsxind5, InstJMPabsxind6, // JMP absolute indexed indirect

    InstJSRabs2, InstJSRabs3, InstJSRabs4, InstJSRabs5, InstJSRabs6, // JSR absolute

    InstRTIimp2, InstRTIimp3, InstRTIimp4, InstRTIimp5, InstRTIimp6, // RTI implied

    InstRTSimp2, InstRTSimp3, InstRTSimp4, InstRTSimp5, InstRTSimp6, // RTS implied

    InstCLCimp2, // CLC implied
    InstCLDimp2, // CLD implied
    InstCLIimp2, // CLI implied
    InstCLVimp2, // CLV implied
    InstSECimp2, // SEC implied
    InstSEDimp2, // SED implied
    InstSEIimp2, // SEI implied

    InstBCCrel2, InstBCCrel3, InstBCCrel4, // BCC relative
    InstBCSrel2, InstBCSrel3, InstBCSrel4, // BCS relative
    InstBEQrel2, InstBEQrel3, InstBEQrel4, // BEQ relative
    InstBMIrel2, InstBMIrel3, InstBMIrel4, // BMI relative
    InstBNErel2, InstBNErel3, InstBNErel4, // BNE relative
    InstBPLrel2, InstBPLrel3, InstBPLrel4, // BPL relative
    InstBVCrel2, InstBVCrel3, InstBVCrel4, // BVC relative
    InstBVSrel2, InstBVSrel3, InstBVSrel4, // BVS relative

    InstSTPimp2, InstSTPimp3, // STP implied

    InstWAIimp2, InstWAIimp3, // WAI implied

    InstNOP2, // NOP implied
    InstNOPimm2, // reserved NOP, 2 bytes 2 cycles
    InstNOPzpg2, InstNOPzpg3, // reserved NOP, 2 bytes 3 cycles
    InstNOPzpgx2, InstNOPzpgx3, InstNOPzpgx4, // reserved NOP, 2 bytes 4 cycles
    InstNOPabsx2, InstNOPabsx3, InstNOPabsx4, // reserved NOP, 3 bytes 4 cycles
    InstNOP5C2, InstNOP5C3, InstNOP5C4, InstNOP5C5, InstNOP5C6, InstNOP5C7, InstNOP5C8 // reserved NOP $5C
}

internal class StateExtensions
{
    public static States FromValue(int value)
    {
        if (Enum.IsDefined(typeof(States), value))
        {
            return (States)value;
        }

        return States.WarmUp0;
    }
}
