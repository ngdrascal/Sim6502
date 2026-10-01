namespace W65C21.Engine;

/// <summary>
/// A selected bus cycle: the register reached (PA, ORA, DDRA, CRA, PB, ORB, DDRB, CRB), its
/// direction and the byte transferred. Raised at the PHI2 rise for reads and the fall for writes.
/// </summary>
public readonly record struct RegisterAccess(string Register, bool IsWrite, byte Value);
