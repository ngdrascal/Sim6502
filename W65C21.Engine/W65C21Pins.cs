namespace W65C21.Engine;

/// <summary>
/// Pin levels shared between the host and the engine (true = high). The host sets the inputs
/// and calls <see cref="W65C21Engine.Evaluate"/>; the engine sets the outputs. An output is only
/// on the pin while its drive flag/mask bit is set, otherwise the pin is high-Z.
/// Inputs default high, matching floating inputs.
/// </summary>
public class W65C21Pins
{
    // Bus side inputs.
    public bool PHI2 { get; set; }
    public bool RESB { get; set; } = true;
    public bool CS0 { get; set; } = true;
    public bool CS1 { get; set; } = true;
    public bool CS2B { get; set; } = true;
    public bool RS0 { get; set; } = true;
    public bool RS1 { get; set; } = true;
    public bool RWB { get; set; } = true;
    public byte DataIn { get; set; } = 0xFF;

    // Peripheral side inputs. Floating port bits must be presented as 1.
    public byte PAIn { get; set; } = 0xFF;
    public byte PBIn { get; set; } = 0xFF;
    public bool CA1 { get; set; } = true;
    public bool CA2In { get; set; } = true;
    public bool CB1 { get; set; } = true;
    public bool CB2In { get; set; } = true;

    // Outputs.
    public byte DataOut { get; internal set; }
    public byte DataDrive { get; internal set; }
    public byte PAOut { get; internal set; }
    public byte PADrive { get; internal set; }
    public byte PBOut { get; internal set; }
    public byte PBDrive { get; internal set; }
    public bool CA2Out { get; internal set; }
    public bool CA2Drive { get; internal set; }
    public bool CB2Out { get; internal set; }
    public bool CB2Drive { get; internal set; }

    // Open drain: when driven the level is low, otherwise high-Z.
    public bool IRQABDrive { get; internal set; }
    public bool IRQBBDrive { get; internal set; }
}
