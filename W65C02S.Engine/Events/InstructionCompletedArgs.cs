using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine;

public class RegisterArgs
{
    public RegisterArgs(Registers regs)
    {
        A = regs.A.Copy();
        X = regs.X.Copy();
        Y = regs.Y.Copy();
        PC = regs.PC.Copy();
        S = regs.S.Copy();
        P = regs.P.Copy();
    }

    public UInt8 A { get; }

    public UInt8 X { get; }

    public UInt8 Y { get; }

    public UInt16 PC { get; }

    public UInt8 S { get; }

    public StatusRegister P { get; }

    public override string ToString()
    {
        return $"A:{A.ToInt():X2} P:{P} X:{X.ToInt():X2} Y:{Y.ToInt():X2} S:{S.ToInt():X2} PC:{PC.ToInt():X4}";
    }
}

public class InstructionCompletedArgs : EventArgs
{
    public InstructionCompletedArgs(UInt16 address, OpCodes opCode, UInt8 operand1, UInt8 operand2, Registers regs)
    {
        Address = address.Copy();
        OpCode = opCode;
        Registers = new RegisterArgs(regs);
        Operand1 = operand1;
        Operand2 = operand2;
    }
    public UInt16 Address { get; }

    public OpCodes OpCode { get; }

    public UInt8 Operand1 { get; }

    public UInt8 Operand2 { get; }

    public RegisterArgs Registers { get; }
}