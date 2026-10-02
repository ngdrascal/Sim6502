namespace W65C22.Engine;

/// <summary>
/// The Shift Register and its modulo-8 bit counter, configured by ACR bits 4-2. Internal clock
/// modes generate the CB1 shift clock (toggling at PHI2 falls, either every fall or on T2
/// low-byte underflows); external modes take it from CB1 edges. Shifting in samples CB2 at the
/// first PHI2 rise after a CB1 rise; shifting out puts SR7 on CB2 at the first PHI2 fall after a
/// CB1 fall and rotates it into bit 0. The bit counter advances on each CB1 rise.
/// </summary>
internal sealed class ShiftRegister
{
    private const int Disabled = 0;
    private const int InT2 = 1;
    private const int InPhi2 = 2;
    private const int InExternal = 3;
    private const int OutFreeRun = 4;
    private const int OutT2 = 5;
    private const int OutPhi2 = 6;
    private const int OutExternal = 7;

    private int _mode;
    private int _bits;
    private bool _active;
    private bool _sampleAtRise;
    private bool _shiftOutAtFall;
    private bool _prevCb1 = true;

    public byte Value { get; set; }

    /// <summary>The internal shift clock level, driven on CB1 when <see cref="DrivesCb1"/>.</summary>
    public bool Cb1Level { get; private set; } = true;

    /// <summary>The shifted-out data level, driven on CB2 when <see cref="DrivesCb2"/>.</summary>
    public bool Cb2Level { get; private set; } = true;

    public bool IsEnabled => _mode != Disabled;

    public bool DrivesCb1 => IsInternalClock;

    public bool DrivesCb2 => IsShiftOut;

    /// <summary>The mode takes its shift clock from T2's low-order counter.</summary>
    public bool UsesT2 => _mode is InT2 or OutFreeRun or OutT2;

    /// <summary>A transfer clocked by T2 is running, so T2's low byte is the shift clock.</summary>
    public bool ClocksFromT2 => UsesT2 && _active;

    private bool IsInternalClock => _mode is InT2 or InPhi2 or OutFreeRun or OutT2 or OutPhi2;

    private bool IsExternalClock => _mode is InExternal or OutExternal;

    private bool IsShiftOut => _mode >= OutFreeRun;

    private bool StopsAfterEight => _mode is InT2 or InPhi2 or OutT2 or OutPhi2;

    /// <summary>RESB: back to mode 000 (ACR is cleared). The register contents are kept.</summary>
    public void Reset()
    {
        _mode = Disabled;
        Stop();
    }

    /// <summary>ACR write: a change of mode abandons any transfer in progress.</summary>
    public void SetMode(int mode)
    {
        if (mode == _mode)
            return;

        _mode = mode;
        Stop();
    }

    /// <summary>
    /// SR read or write: resets the bit counter and, in the internal clock modes, starts a
    /// transfer whose first CB1 edge comes at a later PHI2 fall.
    /// </summary>
    public void Access()
    {
        _bits = 0;
        _sampleAtRise = false;
        _shiftOutAtFall = false;
        Cb1Level = true;
        _active = IsInternalClock;
    }

    /// <summary>
    /// Handles a PHI2 fall: a shift out due from an earlier CB1 fall, then the internal clock.
    /// <paramref name="t2Underflow"/> reports a T2 low-byte underflow at this fall.
    /// </summary>
    public bool ClockFall(bool t2Underflow)
    {
        if (_shiftOutAtFall)
        {
            _shiftOutAtFall = false;
            Cb2Level = (Value & 0x80) != 0;
            Value = (byte)((Value << 1) | (Cb2Level ? 1 : 0));
        }

        if (!_active)
            return false;

        var toggle = _mode is InPhi2 or OutPhi2 || t2Underflow;
        if (!toggle)
            return false;

        Cb1Level = !Cb1Level;
        return Cb1Edge(Cb1Level);
    }

    /// <summary>Handles a PHI2 rise: samples CB2 when a shift in is due. Returns true when IFR2 must be set.</summary>
    public bool ClockRise(bool cb2In)
    {
        if (!_sampleAtRise)
            return false;

        _sampleAtRise = false;
        Value = (byte)((Value << 1) | (cb2In ? 1 : 0));
        return CountBit();
    }

    /// <summary>
    /// Watches the CB1 net; in the external clock modes its edges clock the register. Internal
    /// modes ignore the net (CB1 may be overdriven, section 5.2). Returns true when IFR2 must be set.
    /// </summary>
    public bool SampleCb1(bool cb1)
    {
        var changed = cb1 != _prevCb1;
        _prevCb1 = cb1;

        if (!changed || !IsExternalClock)
            return false;

        return Cb1Edge(cb1);
    }

    private bool Cb1Edge(bool rising)
    {
        if (!rising)
        {
            if (IsShiftOut)
                _shiftOutAtFall = true;
            return false;
        }

        if (!IsShiftOut)
        {
            _sampleAtRise = true;
            return false;
        }

        return _mode != OutFreeRun && CountBit();
    }

    private bool CountBit()
    {
        _bits++;
        if (_bits < 8)
            return false;

        _bits = 0;
        if (StopsAfterEight)
            _active = false;
        return true;
    }

    private void Stop()
    {
        _active = false;
        _bits = 0;
        _sampleAtRise = false;
        _shiftOutAtFall = false;
        Cb1Level = true;
    }
}
