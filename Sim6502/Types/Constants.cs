namespace Sim6502.types;

/// <summary>
/// Common constants for logic levels and bus directions.
/// </summary>
file static class Constants
{
    public const byte Low = 0;
    public const byte High = 1;
    public const byte Write = Low;
    public const byte Read = High;
}
