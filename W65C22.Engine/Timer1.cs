namespace W65C22.Engine;

/// <summary>
/// Interval Timer 1: a 16-bit latch and counter plus the PB7 output level. The counter
/// decrements at each PHI2 fall; the fall after it passes 0 to $FFFF reloads it from the latch
/// (one-shot and free-run alike, Figure 2-3). A time-out is signalled at the PHI2 rise after the
/// 0 to $FFFF transition.
/// </summary>
internal sealed class Timer1
{
    private bool _running;
    private bool _armed;
    private bool _reloadPending;
    private bool _timeoutPending;

    public ushort Counter { get; private set; }

    public ushort Latch { get; private set; }

    /// <summary>The PB7 level driven when ACR bit 7 is set.</summary>
    public bool Pb7 { get; private set; } = true;

    /// <summary>Stops counting and disarms until the next T1C-H write. Counter and latch keep their values.</summary>
    public void Reset()
    {
        _running = false;
        _armed = false;
        _reloadPending = false;
        _timeoutPending = false;
        Pb7 = true;
    }

    public void WriteLatchLow(byte value)
    {
        Latch = (ushort)((Latch & 0xFF00) | value);
    }

    public void WriteLatchHigh(byte value)
    {
        Latch = (ushort)((Latch & 0x00FF) | (value << 8));
    }

    /// <summary>T1C-H write: transfers the latch into the counter, arms the interrupt and takes PB7 low.</summary>
    public void Load()
    {
        Counter = Latch;
        _running = true;
        _armed = true;
        _reloadPending = false;
        _timeoutPending = false;
        Pb7 = false;
    }

    /// <summary>Counts one PHI2 fall.</summary>
    public void ClockFall()
    {
        if (!_running)
            return;

        if (_reloadPending)
        {
            _reloadPending = false;
            Counter = Latch;
            return;
        }

        Counter--;
        if (Counter != 0xFFFF)
            return;

        _reloadPending = true;
        _timeoutPending = true;
    }

    /// <summary>
    /// Applies a time-out due at this PHI2 rise. Returns true when it sets the T1 Interrupt Flag:
    /// always in free-run (PB7 toggles), once per load in one-shot (PB7 returns high).
    /// </summary>
    public bool ClockRise(bool freeRun)
    {
        if (!_timeoutPending)
            return false;

        _timeoutPending = false;
        if (freeRun)
        {
            Pb7 = !Pb7;
            return true;
        }

        if (!_armed)
            return false;

        _armed = false;
        Pb7 = true;
        return true;
    }
}
