using System.Diagnostics.CodeAnalysis;

namespace W65C22.Engine.Tests;

/// <summary>
/// Builds a reset W65C22Engine with PHI2 low and the part deselected, and drives whole bus
/// cycles or single PHI2 edges through its pins.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class ViaTestBase
{
    protected const int Orb = 0x0;
    protected const int Ora = 0x1;
    protected const int Ddrb = 0x2;
    protected const int Ddra = 0x3;
    protected const int T1CL = 0x4;
    protected const int T1CH = 0x5;
    protected const int T1LL = 0x6;
    protected const int T1LH = 0x7;
    protected const int T2CL = 0x8;
    protected const int T2CH = 0x9;
    protected const int Sr = 0xA;
    protected const int Acr = 0xB;
    protected const int Pcr = 0xC;
    protected const int Ifr = 0xD;
    protected const int Ier = 0xE;
    protected const int OraNoHandshake = 0xF;

    // IFR / IER bits.
    protected const byte Ca2Flag = 0x01;
    protected const byte Ca1Flag = 0x02;
    protected const byte SrFlag = 0x04;
    protected const byte Cb2Flag = 0x08;
    protected const byte Cb1Flag = 0x10;
    protected const byte T2Flag = 0x20;
    protected const byte T1Flag = 0x40;
    protected const byte IrqBit = 0x80;
    protected const byte IerSet = 0x80;

    // ACR bits.
    protected const byte PaLatch = 0x01;
    protected const byte PbLatch = 0x02;

    // PCR nibble values (shift left 4 for CB1/CB2): bit 0 = C1 rising edge, bits 3-1 = C2 mode.
    protected const byte C1Rising = 0x01;
    protected const byte C2InFalling = 0x00;
    protected const byte C2IndependentFalling = 0x02;
    protected const byte C2InRising = 0x04;
    protected const byte C2IndependentRising = 0x06;
    protected const byte C2Handshake = 0x08;
    protected const byte C2Pulse = 0x0A;
    protected const byte C2ManualLow = 0x0C;
    protected const byte C2ManualHigh = 0x0E;

    protected ViaTestBase()
    {
        Engine = new W65C22Engine();
        Pins.PHI2 = false;
        Pins.CS1 = true;
        Pins.CS2B = true;
        Engine.Evaluate();
    }

    protected W65C22Engine Engine { get; }

    protected W65C22Pins Pins => Engine.Pins;

    /// <summary>Selects the part and sets RS/RWB ahead of the PHI2 rise.</summary>
    protected void Address(int register, bool read)
    {
        Pins.CS1 = true;
        Pins.CS2B = false;
        Pins.RS0 = (register & 1) != 0;
        Pins.RS1 = (register & 2) != 0;
        Pins.RS2 = (register & 4) != 0;
        Pins.RS3 = (register & 8) != 0;
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

    protected void IdleCycles(int count)
    {
        for (var i = 0; i < count; i++)
            IdleCycle();
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
        Pins.CB1In = level;
        Engine.Evaluate();
    }

    protected void SetCb2(bool level)
    {
        Pins.CB2In = level;
        Engine.Evaluate();
    }
}
