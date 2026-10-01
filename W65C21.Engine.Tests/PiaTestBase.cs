using System.Diagnostics.CodeAnalysis;

namespace W65C21.Engine.Tests;

/// <summary>
/// Builds a reset W65C21Engine with PHI2 low and the part deselected, and drives whole bus
/// cycles or single PHI2 edges through its pins.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class PiaTestBase
{
    protected const int PortA = 0;
    protected const int Cra = 1;
    protected const int PortB = 2;
    protected const int Crb = 3;

    protected const byte DdrAccess = 0x04;
    protected const byte Irq1Flag = 0x80;
    protected const byte Irq2Flag = 0x40;

    // CR bits 0-2 for C1 control plus DDR Access, used with the C2 modes below.
    protected const byte C1IrqEnable = 0x01;
    protected const byte C1Rising = 0x02;
    protected const byte C2InIrqEnable = 0x08;
    protected const byte C2InRising = 0x10;
    protected const byte C2Handshake = 0x20;
    protected const byte C2Pulse = 0x28;
    protected const byte C2ManualLow = 0x30;
    protected const byte C2ManualHigh = 0x38;

    protected PiaTestBase()
    {
        Engine = new W65C21Engine();
        Pins.PHI2 = false;
        Pins.CS0 = true;
        Pins.CS1 = true;
        Pins.CS2B = true;
        Engine.Evaluate();
    }

    protected W65C21Engine Engine { get; }

    protected W65C21Pins Pins => Engine.Pins;

    /// <summary>Selects the part and sets RS/RWB ahead of the PHI2 rise.</summary>
    protected void Address(int register, bool read)
    {
        Pins.CS2B = false;
        Pins.RS0 = (register & 1) != 0;
        Pins.RS1 = (register & 2) != 0;
        Pins.RWB = read;
        Engine.Evaluate();
    }

    protected void Deselect()
    {
        Pins.CS2B = true;
        Pins.RWB = true;
        Engine.Evaluate();
    }

    protected void Rise()
    {
        Pins.PHI2 = true;
        Engine.Evaluate();
    }

    protected void Fall()
    {
        Pins.PHI2 = false;
        Engine.Evaluate();
    }

    protected void Write(int register, byte value)
    {
        Address(register, read: false);
        Rise();
        Pins.DataIn = value;
        Engine.Evaluate();
        Fall();
        Deselect();
    }

    protected byte Read(int register)
    {
        Address(register, read: true);
        Rise();
        var value = Pins.DataOut;
        Fall();
        Deselect();
        return value;
    }

    /// <summary>A full PHI2 cycle with the part deselected.</summary>
    protected void IdleCycle()
    {
        Rise();
        Fall();
    }

    protected void SetCa1(bool level)
    {
        Pins.CA1 = level;
        Engine.Evaluate();
    }

    protected void SetCa2(bool level)
    {
        Pins.CA2In = level;
        Engine.Evaluate();
    }

    protected void SetCb1(bool level)
    {
        Pins.CB1 = level;
        Engine.Evaluate();
    }

    protected void SetCb2(bool level)
    {
        Pins.CB2In = level;
        Engine.Evaluate();
    }
}
