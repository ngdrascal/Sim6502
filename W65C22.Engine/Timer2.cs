namespace W65C22.Engine;

/// <summary>
/// Interval Timer 2: an 8-bit low-order latch and a 16-bit counter. As an interval timer the
/// counter decrements at each PHI2 fall, rolls over at $FFFF and keeps counting; the time-out is
/// signalled at the PHI2 rise after the 0 to $FFFF transition. As a pulse counter it decrements
/// on each falling PB6 seen between PHI2 rises and times out on reaching 0 (section 2.10).
/// Only the first time-out after a T2C-H write sets the Interrupt Flag.
/// </summary>
internal sealed class Timer2
{
    private bool _running;
    private bool _armed;
    private bool _timeoutPending;
    private bool _prevPb6 = true;
    private bool _lowReloadPending;

    public ushort Counter { get; private set; }

    public byte LatchLow { get; set; }

    /// <summary>Stops counting and disarms until the next T2C-H write. Counter and latch keep their values.</summary>
    public void Reset()
    {
        _running = false;
        _armed = false;
        _timeoutPending = false;
    }

    /// <summary>T2C-H write: loads the counter from the value and the low-order latch and arms the interrupt.</summary>
    public void Load(byte high)
    {
        Counter = (ushort)((high << 8) | LatchLow);
        _running = true;
        _armed = true;
        _timeoutPending = false;
    }

    /// <summary>SR access in a T2-clocked shift mode: reloads the low-order counter from the latch.</summary>
    public void RestartShiftClock()
    {
        Counter = (ushort)((Counter & 0xFF00) | LatchLow);
        _lowReloadPending = false;
    }

    /// <summary>
    /// Counts one PHI2 fall on the low-order counter only, as the Shift Register clock. Returns true
    /// when it passes 0 to $FF; it reloads from the latch on the following fall (N+2 cycles).
    /// </summary>
    public bool ClockShiftFall()
    {
        if (_lowReloadPending)
        {
            _lowReloadPending = false;
            Counter = (ushort)((Counter & 0xFF00) | LatchLow);
            return false;
        }

        var low = (byte)(Counter - 1);
        Counter = (ushort)((Counter & 0xFF00) | low);
        if (low != 0xFF)
            return false;

        _lowReloadPending = true;
        return true;
    }

    /// <summary>Counts one PHI2 fall in interval mode.</summary>
    public void ClockFall()
    {
        if (!_running)
            return;

        Counter--;
        if (Counter == 0xFFFF && _armed)
            _timeoutPending = true;
    }

    /// <summary>
    /// Handles one PHI2 rise: applies an interval time-out, and in pulse counting mode samples PB6
    /// and counts a high to low change. Returns true when the T2 Interrupt Flag must be set.
    /// </summary>
    public bool ClockRise(bool pulseCounting, bool pb6)
    {
        var pb6Fell = _prevPb6 && !pb6;
        _prevPb6 = pb6;

        if (_timeoutPending)
        {
            _timeoutPending = false;
            _armed = false;
            return true;
        }

        if (!pulseCounting || !_running || !pb6Fell)
            return false;

        Counter--;
        if (Counter != 0 || !_armed)
            return false;

        _armed = false;
        return true;
    }
}
