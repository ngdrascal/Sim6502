// Converted from OpCodes.java
// Represents opcodes for W65c02s
namespace Us.Retrocpu.W65c02s {
    public enum OpCodes {
        TSXimp = 0xBA,
        TXAimp = 0x8A,
        TXSimp = 0x9A,
        TYAimp = 0x98,

        // Stack
        PHAimp = 0x48,
        PHPimp = 0x08,
        PHXimp = 0xDA,
        PHYimp = 0x5A,
        PLAimp = 0x68,
        PLPimp = 0x28,
        PLXimp = 0xFA,
        PLYimp = 0x7A,

        // ASL
        ASLacc = 0x0A,
        ASLzpg = 0x06,
        ASLzpgx = 0x16,
        ASLabs = 0x0E,
        ASLabsx = 0x1E,

        // LSR
        LSRacc = 0x4A,
        LSRzpg = 0x46,
        LSRzpgx = 0x56,
        LSRabs = 0x4E,
        LSRabsx = 0x5E,

        // ROL
        ROLacc = 0x2A,
        ROLzpg = 0x26,
        ROLzpgx = 0x36,
        ROLabs = 0x2E,
        ROLabsx = 0x3E,

        // ROR
        RORacc = 0x6A,
        RORzpg = 0x66,
        RORzpgx = 0x76,
        RORabs = 0x6E,
        RORabsx = 0x7E,

        // AND
        ANDimm = 0x29,
        ANDzpg = 0x25,
        ANDzpgx = 0x35,
        ANDabs = 0x2D,
        ANDabsx = 0x3D,
        ANDabsy = 0x39,
        ANDindx = 0x21,
        ANDindy = 0x31,
        ANDind = 0x32,

        // BIT
        BITimm = 0x89,
        BITzpg = 0x24,
        BITzpgx = 0x34,
        BITabs = 0x2C,
        BITabsx = 0x3C,

        // EOR
        EORimm = 0x49,
        EORzpg = 0x45,
        EORzpgx = 0x55,
        EORabs = 0x4D,
        EORabsx = 0x5D,
        EORabsy = 0x59,
        EORindx = 0x41,
        EORindy = 0x51,
        EORind = 0x52,

        // ORA
        ORAimm = 0x09,
        ORAzpg = 0x05,
        ORAzpgx = 0x15,
        ORAabs = 0x0D,
        ORAabsx = 0x1D,
        ORAabsy = 0x19,
        ORAindx = 0x01,
        ORAindy = 0x11,
        ORAind = 0x12,

        // TRB
        TRBzpg = 0x14,
        TRBabs = 0x1C,

        // TSB
        TSBzpg = 0x04,
        TSBabs = 0x0C,

        // ADC
        ADCimm = 0x69,
        ADCzpg = 0x65,
        ADCzpgx = 0x75,
        ADCabs = 0x6D,
        ADCabsx = 0x7D,
        ADCabsy = 0x79,
        ADCindx = 0x61,
        ADCindy = 0x71,
        ADCind = 0x72,

        // CMP
        CMPimm = 0xC9,
        CMPzpg = 0xC5,
        CMPzpgx = 0xD5,
        CMPabs = 0xCD,
        CMPabsx = 0xDD,
        CMPabsy = 0xD9,
        CMPindx = 0xC1,
        CMPindy = 0xD1,
        CMPind = 0xD2,

        // CPX
        CPXimm = 0xE0,
        CPXzpg = 0xE4,
        CPXabs = 0xEC,

        // CPY
        CPYimm = 0xC0,
        CPYzpg = 0xC4,
        CPYabs = 0xCC,

        // SBC
        SBCimm = 0xE9,
        SBCzpg = 0xE5,
        SBCzpgx = 0xF5,
        SBCabs = 0xED,
        SBCabsx = 0xFD,
        SBCabsy = 0xF9,
        SBCindx = 0xE1,
        SBCindy = 0xF1,
        SBCind = 0xF2,

        // Decrement
        DECacc = 0x3A,
        DECzpg = 0xC6,
        DECzpgx = 0xD6,
        DECabs = 0xCE,
        DECabsx = 0xDE,
        DEXimp = 0xCA,
        DEYimp = 0x88,

        // Increment
        INCacc = 0x1A,
        INCzpg = 0xE6,
        INCzpgx = 0xF6,
        INCabs = 0xEE,
        INCabsx = 0xFE,
        INXimp = 0xE8,
        INYimp = 0xC8,

        // Control
        BRKimp = 0x00,
        JMPabs = 0x4C,
        JMPind = 0x6C,
        JSRabs = 0x20,
        RTIimp = 0x40,
        RTSimp = 0x60,

        // Branching
        BRArel = 0x80,
        BCCrel = 0x90,
        BCSrel = 0xB0,
        BEQrel = 0xF0,
        BMIrel = 0x30,
        BNErel = 0xD0,
        BPLrel = 0x10,
        BVCrel = 0x50,
        BVSrel = 0x70,

        // Flags
        CLCimp = 0x18,
        CLDimp = 0xD8,
        CLIimp = 0x58,
        CLVimp = 0xB8,
        SECimp = 0x38,
        SEDimp = 0xF8,
        SEIimp = 0x78,

        // STP
        STPimp = 0xDB,

        // WAI
        WAIimp = 0xCB,

        // NOP
        NOP02 = 0x02,
        NOP03 = 0x03,
        NOP0B = 0x0B,
        NOP13 = 0x13,
        NOP1B = 0x1B,
        NOP22 = 0x22,
        NOP23 = 0x23,
        NOP2B = 0x2B,
        NOP33 = 0x33,
        NOP3B = 0x3B,
        NOP42 = 0x42,
        NOP43 = 0x43,
        NOP44 = 0x44,
        NOP4B = 0x4B,
        NOP53 = 0x53,
        NOP54 = 0x54,
        NOP5B = 0x5B,
        NOP5C = 0x5C,
        NOP62 = 0x62,
        NOP63 = 0x63,
        NOP6B = 0x6B,
        NOP73 = 0x73,
        NOP7B = 0x7B,
        NOP82 = 0x82,
        NOP83 = 0x83,
        NOP8B = 0x8B,
        NOP93 = 0x93,
        NOP9B = 0x9B,
        NOPA3 = 0xA3,
        NOPAB = 0xAB,
        NOPB3 = 0xB3,
        NOPBB = 0xBB,
        NOPC2 = 0xC2,
        NOPC3 = 0xC3,
        NOPD3 = 0xD3,
        NOPD4 = 0xD4,
        NOPDC = 0xDC,
        NOPE2 = 0xE2,
        NOPE3 = 0xE3,
        NOP = 0xEA,
        NOPEB = 0xEB,
        NOPF3 = 0xF3,
        NOPF4 = 0xF4,
        NOPFB = 0xFB,
        NOPFC = 0xFC
    }

    public static class OpCodesExtensions
    {
        public static byte ToByte(this OpCodes code)
        {
            return (byte)code;
        }

        public static int ToInt(this OpCodes code)
        {
            return (int)code;
        }

        public static OpCodes FromValue(int value)
        {
            if (value < 0 || value > 255)
                throw new ArgumentOutOfRangeException("param out of range: value");

            foreach (OpCodes code in Enum.GetValues(typeof(OpCodes)))
            {
                if ((int)code == value)
                    return code;
            }
            throw new ArgumentException($"no matching OpCode for {value}");
        }
    }
}
