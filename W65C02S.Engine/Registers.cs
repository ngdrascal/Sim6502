using System.Diagnostics.CodeAnalysis;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Engine;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Registers
{
    public Registers(StatusRegister statusReg)
    {
        P = statusReg;
        S = new UInt8(0xFD);
        Inst = OpCodes.NOP.ToUInt8();
    }

    public UInt8 A { get; set; }

    public void UpdateAUpdateFlags(UInt8 value)
    {
        A = value;
        UpdateNZ(value);
    }

    public UInt8 X { get; set; }

    public void SetXUpdateFlags(UInt8 value)
    {
        X = value;
        UpdateNZ(value);
    }

    public UInt8 Y { get; set; }

    public void SetYUpdateFlags(UInt8 value)
    {
        Y = value;
        UpdateNZ(value);
    }

    public UInt16 PC { get; set; }

    public UInt8 S { get; set; }

    public StatusRegister P { get; }

    public UInt8 Inst { get; set; }

    public void IncEALWithX() => EA = EA.WithLsb(EA.Lsb().AddWithWrapAround(X));

    public void IncEALWithY() => EA = EA.WithLsb(EA.Lsb().AddWithWrapAround(Y));

    public UInt16 EA { get; set; }

    public void IncEAWithX() => EA = EA.AddUnsigned(X);

    public void IncEAWithY() => EA = EA.AddUnsigned(Y);

    public UInt16 EA2 { get; set; }

    public void CopyEA2ToEA() => EA = EA2;

    public UInt8 Temp { get; set; }

    public override string ToString()
    {
        return $"A:{A.ToInt():X2} P:{P} X:{X.ToInt():X2} Y:{Y.ToInt():X2} S:{S.ToInt():X2} PC:{PC.ToInt():X4}";
    }

    private void UpdateNZ(UInt8 value)
    {
        if (value.EqualsZero())
        {
            P.SetZero();
            P.ClearNegative();
        }
        else if (value.IsBitSet(7))
        {
            P.ClearZero();
            P.SetNegative();
        }
        else
        {
            P.ClearZero();
            P.ClearNegative();
        }
    }
}
