namespace W65C22.Engine;

/// <summary>
/// The two control lines of one Port (CA1/CA2 or CB1/CB2) as configured by a PCR nibble: active
/// transition detection and the C2 output level. The engine owns the Interrupt Flags and decides
/// which bus events start a C2 strobe and on which PHI2 edge it takes effect.
/// </summary>
internal sealed class ControlLines
{
    // PCR nibble: bit 0 = C1 active edge, bits 3-1 = C2 mode.
    private const byte C1Rising = 0x01;
    private const byte C2Independent = 0x02;
    private const byte C2InRising = 0x04;
    private const byte C2Output = 0x08;
    private const byte C2ModeMask = 0x0E;
    private const byte HandshakeMode = 0x08;
    private const byte PulseMode = 0x0A;
    private const byte ManualLowMode = 0x0C;
    private const byte ManualHighMode = 0x0E;

    private bool _prevC1 = true;
    private bool _prevC2In = true;
    private bool _lowPending;
    private bool _lowOnFall;
    private bool _pulseActive;
    private bool _pulseOnFall;

    public byte Control { get; private set; }

    public bool C2Level { get; private set; } = true;

    public bool C2IsOutput => (Control & C2Output) != 0;

    /// <summary>C2 is an independent interrupt input: ORx accesses leave its flag alone.</summary>
    public bool C2IsIndependent => !C2IsOutput && (Control & C2Independent) != 0;

    private byte C2Mode => (byte)(Control & C2ModeMask);

    private bool C2Strobed => C2Mode == HandshakeMode || C2Mode == PulseMode;

    public void Reset(bool c1, bool c2In)
    {
        Control = 0;
        C2Level = true;
        CancelStrobe();
        _prevC1 = c1;
        _prevC2In = c2In;
    }

    public void WriteControl(byte nibble)
    {
        var wasStrobed = C2Strobed;
        Control = (byte)(nibble & 0x0F);

        if (C2Mode == ManualLowMode)
            C2Level = false;
        else if (C2Mode == ManualHighMode)
            C2Level = true;
        else if (C2Strobed && !wasStrobed)
            C2Level = true;

        if (!C2Strobed)
            CancelStrobe();
    }

    /// <summary>
    /// Detects active transitions on C1 and (input mode) C2. A C1 active transition also ends a
    /// handshake by setting C2 high.
    /// </summary>
    public (bool C1Active, bool C2Active) Sample(bool c1, bool c2In)
    {
        var c1Active = c1 != _prevC1 && c1 == ((Control & C1Rising) != 0);
        var c2Active = !C2IsOutput && c2In != _prevC2In && c2In == ((Control & C2InRising) != 0);

        if (c1Active && C2Mode == HandshakeMode)
            C2Level = true;

        _prevC1 = c1;
        _prevC2In = c2In;
        return (c1Active, c2Active);
    }

    /// <summary>
    /// Requests that C2 goes low at the next PHI2 fall (<paramref name="onFall"/>) or rise, if in
    /// handshake or pulse mode. A pulse returns high on the same edge one cycle later.
    /// </summary>
    public void StartStrobe(bool onFall)
    {
        if (!C2Strobed)
            return;

        _lowPending = true;
        _lowOnFall = onFall;
    }

    /// <summary>Applies strobe changes due on this PHI2 edge.</summary>
    public void ClockStrobe(bool fall)
    {
        if (_pulseActive && _pulseOnFall == fall)
        {
            _pulseActive = false;
            C2Level = true;
        }

        if (_lowPending && _lowOnFall == fall)
        {
            _lowPending = false;
            C2Level = false;
            _pulseActive = C2Mode == PulseMode;
            _pulseOnFall = fall;
        }
    }

    private void CancelStrobe()
    {
        _lowPending = false;
        _pulseActive = false;
    }
}
