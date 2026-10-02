namespace W65C22.Engine;

/// <summary>
/// A selected bus cycle: the register reached (datasheet designation, e.g. IRB, ORA, T1C-L, IFR;
/// IRA-NH/ORA-NH for register $F), its direction and the byte transferred. Raised at the PHI2
/// rise for reads and the fall for writes.
/// </summary>
public readonly record struct RegisterAccess(string Register, bool IsWrite, byte Value);
