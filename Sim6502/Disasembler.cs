using Sim6502.types;

namespace Sim6502;

/// <summary>
/// Disassembler for W65c02s instructions. Converts opcode and operands to human-readable assembly.
/// </summary>
public static class Disasembler
{
    /// <summary>
    /// Disassembles the current instruction in the context.
    /// </summary>
    /// <param name="ctx">CPU context containing opcode and operands.</param>
    /// <returns>Disassembled instruction string.</returns>

    public static string Disasemble(Context ctx)
    {
        string opCodeStr = ctx.DbgOpCode.ToString().Substring(0, 3);
        string operandStr = string.Empty;

        switch (ctx.DbgOpCode)
        {
            case OpCodes.ADCimm:
            case OpCodes.ANDimm:
            case OpCodes.BITimm:
            case OpCodes.CMPimm:
            case OpCodes.CPXimm:
            case OpCodes.CPYimm:
            case OpCodes.EORimm:
            case OpCodes.LDAimm:
            case OpCodes.LDXimm:
            case OpCodes.LDYimm:
            case OpCodes.ORAimm:
            case OpCodes.SBCimm:
                operandStr = BuildImm(ctx.DbgOperand1);
                break;

            case OpCodes.ADCzpg:
            case OpCodes.ANDzpg:
            case OpCodes.ASLzpg:
            case OpCodes.BITzpg:
            case OpCodes.CMPzpg:
            case OpCodes.CPXzpg:
            case OpCodes.CPYzpg:
            case OpCodes.DECzpg:
            case OpCodes.EORzpg:
            case OpCodes.INCzpg:
            case OpCodes.LDAzpg:
            case OpCodes.LDXzpg:
            case OpCodes.LDYzpg:
            case OpCodes.LSRzpg:
            case OpCodes.ORAzpg:
            case OpCodes.ROLzpg:
            case OpCodes.RORzpg:
            case OpCodes.SBCzpg:
            case OpCodes.STAzpg:
            case OpCodes.STXzpg:
            case OpCodes.STYzpg:
            case OpCodes.STZzpg:
            case OpCodes.TRBzpg:
            case OpCodes.TSBzpg:
                operandStr = BuildZpg(ctx.DbgOperand1);
                break;

            case OpCodes.ADCzpgx:
            case OpCodes.ANDzpgx:
            case OpCodes.ASLzpgx:
            case OpCodes.BITzpgx:
            case OpCodes.CMPzpgx:
            case OpCodes.DECzpgx:
            case OpCodes.EORzpgx:
            case OpCodes.INCzpgx:
            case OpCodes.LDAzpgx:
            case OpCodes.LDYzpgx:
            case OpCodes.LSRzpgx:
            case OpCodes.ORAzpgx:
            case OpCodes.ROLzpgx:
            case OpCodes.RORzpgx:
            case OpCodes.SBCzpgx:
            case OpCodes.STAzpgx:
            case OpCodes.STYzpgx:
            case OpCodes.STZzpgx:
                operandStr = BuildZpgx(ctx.DbgOperand1);
                break;

            case OpCodes.LDXzpgy:
            case OpCodes.STXzpgy:
                operandStr = BuildZpgy(ctx.DbgOperand1);
                break;

            case OpCodes.ADCabs:
            case OpCodes.ANDabs:
            case OpCodes.ASLabs:
            case OpCodes.BITabs:
            case OpCodes.CMPabs:
            case OpCodes.CPXabs:
            case OpCodes.CPYabs:
            case OpCodes.DECabs:
            case OpCodes.EORabs:
            case OpCodes.INCabs:
            case OpCodes.JMPabs:
            case OpCodes.JSRabs:
            case OpCodes.LDAabs:
            case OpCodes.LDXabs:
            case OpCodes.LDYabs:
            case OpCodes.LSRabs:
            case OpCodes.ORAabs:
            case OpCodes.ROLabs:
            case OpCodes.RORabs:
            case OpCodes.SBCabs:
            case OpCodes.STAabs:
            case OpCodes.STXabs:
            case OpCodes.STYabs:
            case OpCodes.STZabs:
            case OpCodes.TRBabs:
            case OpCodes.TSBabs:
                operandStr = BuildAbs(ctx.DbgOperand1, ctx.DbgOperand2);
                break;

            case OpCodes.ADCabsx:
            case OpCodes.ANDabsx:
            case OpCodes.ASLabsx:
            case OpCodes.BITabsx:
            case OpCodes.CMPabsx:
            case OpCodes.DECabsx:
            case OpCodes.EORabsx:
            case OpCodes.INCabsx:
            case OpCodes.LDAabsx:
            case OpCodes.LDYabsx:
            case OpCodes.LSRabsx:
            case OpCodes.ORAabsx:
            case OpCodes.ROLabsx:
            case OpCodes.RORabsx:
            case OpCodes.SBCabsx:
            case OpCodes.STAabsx:
            case OpCodes.STZabsx:
                operandStr = BuildAbsx(ctx.DbgOperand1, ctx.DbgOperand2);
                break;

            case OpCodes.ADCabsy:
            case OpCodes.ANDabsy:
            case OpCodes.CMPabsy:
            case OpCodes.EORabsy:
            case OpCodes.LDAabsy:
            case OpCodes.LDXabsy:
            case OpCodes.ORAabsy:
            case OpCodes.SBCabsy:
            case OpCodes.STAabsy:
                operandStr = BuildAbsy(ctx.DbgOperand1, ctx.DbgOperand2);
                break;

            case OpCodes.ADCindx:
            case OpCodes.ANDindx:
            case OpCodes.CMPindx:
            case OpCodes.EORindx:
            case OpCodes.LDAindx:
            case OpCodes.ORAindx:
            case OpCodes.SBCindx:
            case OpCodes.STAindx:
                operandStr = BuildIndx(ctx.DbgOperand1);
                break;

            case OpCodes.ADCindy:
            case OpCodes.ANDindy:
            case OpCodes.CMPindy:
            case OpCodes.EORindy:
            case OpCodes.LDAindy:
            case OpCodes.ORAindy:
            case OpCodes.SBCindy:
            case OpCodes.STAindy:
                operandStr = BuildIndy(ctx.DbgOperand1);
                break;

            case OpCodes.ADCind:
            case OpCodes.ANDind:
            case OpCodes.CMPind:
            case OpCodes.EORind:
            case OpCodes.LDAind:
            case OpCodes.ORAind:
            case OpCodes.SBCind:
            case OpCodes.STAind:
                operandStr = BuildInd(ctx.DbgOperand1);
                break;

            case OpCodes.BCCrel:
            case OpCodes.BCSrel:
            case OpCodes.BEQrel:
            case OpCodes.BMIrel:
            case OpCodes.BNErel:
            case OpCodes.BPLrel:
            case OpCodes.BRArel:
            case OpCodes.BVCrel:
            case OpCodes.BVSrel:
                operandStr = BuildRel(ctx.DbgOperand1);
                break;

            case OpCodes.ASLacc:
            case OpCodes.DECacc:
            case OpCodes.INCacc:
            case OpCodes.LSRacc:
            case OpCodes.ROLacc:
            case OpCodes.RORacc:
                operandStr = "A";
                break;

            case OpCodes.BRKimp:
            case OpCodes.CLCimp:
            case OpCodes.CLDimp:
            case OpCodes.CLIimp:
            case OpCodes.CLVimp:
            case OpCodes.DEXimp:
            case OpCodes.DEYimp:
            case OpCodes.INXimp:
            case OpCodes.INYimp:
            case OpCodes.NOP:
            case OpCodes.PHAimp:
            case OpCodes.PHPimp:
            case OpCodes.PHXimp:
            case OpCodes.PHYimp:
            case OpCodes.PLAimp:
            case OpCodes.PLPimp:
            case OpCodes.PLXimp:
            case OpCodes.PLYimp:
            case OpCodes.RTIimp:
            case OpCodes.RTSimp:
            case OpCodes.SECimp:
            case OpCodes.SEDimp:
            case OpCodes.SEIimp:
            case OpCodes.STPimp:
            case OpCodes.TAXimp:
            case OpCodes.TAYimp:
            case OpCodes.TSXimp:
            case OpCodes.TXAimp:
            case OpCodes.TXSimp:
            case OpCodes.TYAimp:
            case OpCodes.WAIimp:
                operandStr = string.Empty;
                break;

            case OpCodes.NOP02:
            case OpCodes.NOP03:
            case OpCodes.NOP0B:
            case OpCodes.NOP13:
            case OpCodes.NOP1B:
            case OpCodes.NOP22:
            case OpCodes.NOP23:
            case OpCodes.NOP2B:
            case OpCodes.NOP33:
            case OpCodes.NOP3B:
            case OpCodes.NOP42:
            case OpCodes.NOP43:
            case OpCodes.NOP44:
            case OpCodes.NOP4B:
            case OpCodes.NOP53:
            case OpCodes.NOP54:
            case OpCodes.NOP5B:
            case OpCodes.NOP5C:
            case OpCodes.NOP62:
            case OpCodes.NOP63:
            case OpCodes.NOP6B:
            case OpCodes.NOP73:
            case OpCodes.NOP7B:
            case OpCodes.NOP82:
            case OpCodes.NOP83:
            case OpCodes.NOP8B:
            case OpCodes.NOP93:
            case OpCodes.NOP9B:
            case OpCodes.NOPA3:
            case OpCodes.NOPAB:
            case OpCodes.NOPB3:
            case OpCodes.NOPBB:
            case OpCodes.NOPC2:
            case OpCodes.NOPC3:
            case OpCodes.NOPD3:
            case OpCodes.NOPD4:
            case OpCodes.NOPDC:
            case OpCodes.NOPE2:
            case OpCodes.NOPE3:
            case OpCodes.NOPEB:
            case OpCodes.NOPF3:
            case OpCodes.NOPF4:
            case OpCodes.NOPFB:
            case OpCodes.NOPFC:
            default:
                operandStr = "???";
                break;
        }

        return string.Format("{0:X4}: {1} {2}", ctx.DbgPC, opCodeStr, operandStr);
    }

    private static string BuildImm(UInt8 value)
    {
        return string.Format("#$%02X", value.ToInt());
    }

    private static string BuildZpg(UInt8 value)
    {
        return string.Format("${0:X2}", value.ToInt());
    }

    private static string BuildZpgx(UInt8 value)
    {
        return string.Format("${0:X2},X", value.ToInt());
    }

    private static string BuildZpgy(UInt8 value)
    {
        return string.Format("(${0:X2}),Y", value.ToInt());
    }

    private static string BuildAbs(UInt8 low, UInt8 high)
    {
        return string.Format("${0:X2}{1:X2}", high.ToInt(), low.ToInt());
    }

    private static string BuildAbsx(UInt8 low, UInt8 high)
    {
        return string.Format("${0:X2}{1:X2},X", high.ToInt(), low.ToInt());
    }

    private static string BuildAbsy(UInt8 low, UInt8 high)
    {
        return string.Format("${0:X2}{1:X2},Y", high.ToInt(), low.ToInt());
    }

    private static string BuildIndx(UInt8 value)
    {
        return string.Format("(${0:X2},X)", value.ToInt());
    }

    private static string BuildIndy(UInt8 value)
    {
        return string.Format("(${0:X2}),Y", value.ToInt());
    }

    private static string BuildInd(UInt8 value)
    {
        return string.Format("(${0:X2})", value.ToInt());
    }

    private static string BuildRel(UInt8 value)
    {
        return string.Format("${0:X2}", value.ToInt());
    }
}