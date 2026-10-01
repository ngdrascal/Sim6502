namespace W65C21.Engine;

/// <summary>
/// One Side (A or B) of the PIA: its registers, control lines and IRQ state. The engine decides
/// which bus events start a CA2/CB2 strobe and on which PHI2 edge it is clocked.
/// </summary>
internal sealed class PiaSide
{
    private const byte Irq1Flag = 0x80;
    private const byte Irq2Flag = 0x40;
    private const byte DdrAccess = 0x04;
    private const byte FlagMask = Irq1Flag | Irq2Flag;
    private const byte C1IrqEnable = 0x01;
    private const byte C1RisingEdge = 0x02;
    private const byte C2IrqEnable = 0x08;
    private const byte C2RisingEdge = 0x10;
    private const byte C2Output = 0x20;
    private const byte C2ModeMask = 0x38;
    private const byte HandshakeMode = 0x20;
    private const byte PulseMode = 0x28;
    private const byte ManualLowMode = 0x30;
    private const byte ManualHighMode = 0x38;

    private bool _prevC1 = true;
    private bool _prevC2In = true;
    private bool _c2LowPending;
    private bool _c2PulseActive;

    public byte Cr { get; private set; }
    public byte Ddr { get; set; }
    public byte Or { get; set; }
    public bool C2Level { get; private set; } = true;

    public bool DataSelected => (Cr & DdrAccess) != 0;
    public bool C2IsOutput => (Cr & C2Output) != 0;

    public bool IrqAsserted =>
        ((Cr & Irq1Flag) != 0 && (Cr & C1IrqEnable) != 0) ||
        ((Cr & Irq2Flag) != 0 && (Cr & C2IrqEnable) != 0);

    private byte C2Mode => (byte)(Cr & C2ModeMask);
    private bool C2Strobed => C2Mode == HandshakeMode || C2Mode == PulseMode;

    public void Reset(bool c1, bool c2In)
    {
        Cr = 0;
        Ddr = 0;
        Or = 0;
        C2Level = true;
        _c2LowPending = false;
        _c2PulseActive = false;
        _prevC1 = c1;
        _prevC2In = c2In;
    }

    public void WriteCr(byte value)
    {
        var wasStrobed = C2Strobed;
        Cr = (byte)((Cr & FlagMask) | (value & ~FlagMask));

        if (C2IsOutput)
            Cr = (byte)(Cr & ~Irq2Flag);

        if (C2Mode == ManualLowMode)
            C2Level = false;
        else if (C2Mode == ManualHighMode)
            C2Level = true;
        else if (C2Strobed && !wasStrobed)
            C2Level = true;

        if (!C2Strobed)
        {
            _c2LowPending = false;
            _c2PulseActive = false;
        }
    }

    public void ClearFlags()
    {
        Cr = (byte)(Cr & ~FlagMask);
    }

    /// <summary>Detects active transitions on C1 and (input mode) C2.</summary>
    public void SampleControlLines(bool c1, bool c2In)
    {
        if (c1 != _prevC1 && c1 == ((Cr & C1RisingEdge) != 0))
        {
            Cr |= Irq1Flag;
            if (C2Mode == HandshakeMode)
                C2Level = true;
        }

        if (!C2IsOutput && c2In != _prevC2In && c2In == ((Cr & C2RisingEdge) != 0))
            Cr |= Irq2Flag;

        _prevC1 = c1;
        _prevC2In = c2In;
    }

    /// <summary>Requests that C2 goes low at the next strobe clock, if in handshake or pulse mode.</summary>
    public void StartStrobe()
    {
        if (C2Strobed)
            _c2LowPending = true;
    }

    /// <summary>The PHI2 edge on which C2 strobe changes take effect.</summary>
    public void ClockStrobe()
    {
        if (_c2PulseActive)
        {
            _c2PulseActive = false;
            C2Level = true;
        }

        if (_c2LowPending)
        {
            _c2LowPending = false;
            C2Level = false;
            _c2PulseActive = C2Mode == PulseMode;
        }
    }
}
