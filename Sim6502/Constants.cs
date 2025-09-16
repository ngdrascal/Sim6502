namespace Sim6502;

public static class Constants {
    public const byte LOW = 0;

    public const byte HIGH = 1;

    public const byte Write = LOW;

    public const byte Read = HIGH;

    public const byte P1MIDDLESTEP = 2; // 4;

    public const byte P1LASTSTEP = 3; // 4;

    public const byte P2FIRSTSTEP = 4;

    public const byte P2MIDDLESTEP = 5; // 8;

    public const byte P2LASTSUBSTEP = 6; // 10;
}