#pragma warning disable IDE0051
namespace W65C02S.Engine;

/// <summary>
/// Pin mapping for W65c02s.
/// </summary>
/// <remarks>
///      +----------------+
///    1 |  VPB      RESB  | 40
///    2 |  RDY      PHI2O | 39
///    3 |  PHI1O    SOB   | 38
///    4 |  IRQB     PHI2  | 37
///    5 |  MLB      BE    | 36
///    6 |  NMIB     NC    | 35
///    7 |  SYNC     RWB   | 34
///    8 |  VDD      D0    | 33
///    9 |  A0       D1    | 32
///   10 |  A1       D2    | 31
///   11 |  A2       D3    | 30
///   12 |  A3       D4    | 29
///   13 |  A4       D5    | 28
///   14 |  A5       D6    | 27
///   15 |  A6       D7    | 26
///   16 |  A7       A15   | 25
///   17 |  A8       A14   | 24
///   18 |  A9       A13   | 23
///   19 |  A10      A12   | 22
///   20 |  A11      VSS   | 21
///      +----------------+
/// </remarks>
public static class PinMap
{
    public const byte VPB = 1;
    public const byte RDY = 2;
    public const byte PHI1O = 3;
    public const byte IRQB = 4;
    public const byte MLB = 5;
    public const byte NMIB = 6;
    public const byte SYNC = 7;
    public const byte VDD = 8;
    public const byte A0 = 9;
    public const byte A1 = 10;
    public const byte A2 = 11;
    public const byte A3 = 12;
    public const byte A4 = 13;
    public const byte A5 = 14;
    public const byte A6 = 15;
    public const byte A7 = 16;
    public const byte A8 = 17;
    public const byte A9 = 18;
    public const byte A10 = 19;
    public const byte A11 = 20;
    public const byte VSS = 21;
    public const byte A12 = 22;
    public const byte A13 = 23;
    public const byte A14 = 24;
    public const byte A15 = 25;
    public const byte D7 = 26;
    public const byte D6 = 27;
    public const byte D5 = 28;
    public const byte D4 = 29;
    public const byte D3 = 30;
    public const byte D2 = 31;
    public const byte D1 = 32;
    public const byte D0 = 33;
    public const byte RWB = 34;
    public const byte NC = 35;
    public const byte BE = 36;
    public const byte PHI2 = 37;
    public const byte SOB = 38;
    public const byte PHI2O = 39;
    public const byte RESB = 40;

    public static readonly byte[] DataPins = [D0, D1, D2, D3, D4, D5, D6, D7];

    public static readonly byte[] AddrPins = [A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15];

}
