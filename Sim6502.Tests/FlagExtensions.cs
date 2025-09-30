namespace Sim6502.Tests
{
    internal static class FlagExtensions
    {
        public static void LoadFlags(this StatusRegister sr, string nvbdizc)
        {
            if (nvbdizc == null || nvbdizc.ToLower() != "nvbdizc")
                throw new ArgumentException("Invalid flag string. Must be in the format 'nvbdizc' with appropriate letters uppercase to indicate set flags.");

            if (nvbdizc[0] == 'N') sr.SetNegative(); else sr.ClearNegative();
            if (nvbdizc[1] == 'V') sr.SetOverflow(); else sr.ClearOverflow();
            if (nvbdizc[2] == 'B') sr.SetBreak(); else sr.ClearBreak();
            if (nvbdizc[3] == 'D') sr.SetDecimal(); else sr.ClearDecimal();
            if (nvbdizc[4] == 'I') sr.SetIRQDisabled(); else sr.ClearIRQDisabled();
            if (nvbdizc[5] == 'Z') sr.SetZero(); else sr.ClearZero();
            if (nvbdizc[6] == 'C') sr.SetCarry(); else sr.ClearCarry();

        }
    }
}
