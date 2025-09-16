// ReSharper disable InconsistentNaming
namespace Sim6502;

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

    InstSTZzpg2, InstSTZzpg3, // STY zeropage
    InstSTZzpgx2, InstSTZzpgx3, InstSTZzpgx4, // STY zeropage,X
    InstSTZabs2, InstSTZabs3, InstSTZabs4, // STX absolute
    InstSTZabsx2, InstSTZabsx3, InstSTZabsx4, InstSTZabsx5, // STX absolute,X

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

    InstNOP2, InstNOP3, InstNOP4, InstNOP5, InstNOP6, InstNOP7, InstNOP8 // NOP
}

public class StateExtensions
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
