namespace Sim6502;

public static class Constants {
    public const byte Low = 0;

    public const byte High = 1;

    public const byte Write = Low;

    public const byte Read = High;

    public const byte P1MiddleStep = 2;  // 4;

    public const byte P1LastStep = 3;    // 4;

    public const byte P2FirstStep = 4;

    public const byte P2MiddleStep = 5;  // 8;

    public const byte P2LastSubstep = 6; // 10;
}