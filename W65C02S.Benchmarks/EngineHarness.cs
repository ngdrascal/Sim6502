using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace W65C02S.Benchmarks;

/// <summary>
/// Drives the engine against a flat 64KB memory the same way the validation suite does,
/// but with no tracing subscribers attached.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class EngineHarness
{
    private const byte Read = 1;

    private readonly IPinsExternal _pinsExt;
    private readonly Context _ctx;
    private readonly W65C02SEngine _engine;
    private readonly byte[] _memory = new byte[64 * 1024];

    private byte _phi2;
    private int _lastFetchAddr = -1;

    public EngineHarness(byte[] program, ushort loadAddr, ushort startAddr)
    {
        var pins = new Pins();
        _pinsExt = pins;
        _ctx = new Context(pins, new Registers(new StatusRegister()));
        _engine = new W65C02SEngine(_ctx);

        _pinsExt.IRQB = 1;
        _pinsExt.NMIB = 1;
        _pinsExt.BE = 1;
        _pinsExt.PHI2 = 0;
        _pinsExt.SOB = 1;
        _pinsExt.RESB = 1;
        _pinsExt.RDY = 1;

        program.CopyTo(_memory, loadAddr);

        ColdBootToAddress(startAddr);
    }

    public byte[] Memory => _memory;

    /// <summary>
    /// Address of the self-looping instruction that stopped <see cref="RunUntilTrapped"/>, or
    /// <see cref="BrkVector"/> when it stopped on a vector read.
    /// </summary>
    public int TrapAddress { get; private set; } = -1;

    public const int BrkVector = 0xFFFE;

    /// <summary>Also stop <see cref="RunUntilTrapped"/> when the IRQ/BRK vector is read (a test ending in BRK).</summary>
    public bool StopOnBrkVector { get; init; }

    /// <summary>Runs exactly <paramref name="cycles"/> CPU cycles.</summary>
    public void RunCycles(long cycles)
    {
        for (long i = 0; i < cycles; i++)
        {
            RunCycle();
        }
    }

    /// <summary>
    /// Runs until an instruction fetch repeats the previous fetch address (a "jmp *" or
    /// "bxx *" trap) or <paramref name="maxCycles"/> elapse. Returns the cycles executed.
    /// </summary>
    public long RunUntilTrapped(long maxCycles)
    {
        for (long i = 0; i < maxCycles; i++)
        {
            if (RunCycle())
            {
                return i + 1;
            }
        }

        return maxCycles;
    }

    // Returns true when the fetch in this cycle repeats the previous fetch address.
    private bool RunCycle()
    {
        var trapped = false;

        for (var substep = 0; substep < Constants.P2LastSubstep; substep++)
        {
            Toggle();

            if (_ctx.GetSubStep() != Constants.P2LastSubstep)
            {
                continue;
            }

            var addr = _ctx.Pins.AddrBus.ToInt();
            if (_pinsExt.RWB == Read)
            {
                if (_pinsExt.SYNC == 1)
                {
                    trapped = addr == _lastFetchAddr;
                    _lastFetchAddr = addr;
                    if (trapped)
                    {
                        TrapAddress = addr;
                    }
                }
                else if (StopOnBrkVector && addr == BrkVector)
                {
                    trapped = true;
                    TrapAddress = addr;
                }

                _ctx.Pins.DataBusMode = DataBusMode.Input;
                _ctx.Pins.DataBus = new UInt8(_memory[addr]);
            }
            else
            {
                _memory[addr] = (byte)_ctx.Pins.DataBus.ToInt();
            }
        }

        return trapped;
    }

    private void ColdBootToAddress(ushort startAddr)
    {
        var addr = new UInt16(startAddr);
        _phi2 = 1;

        while (_ctx.State == States.WarmUp0)
        {
            Toggle();
        }

        ToggleCycle(); // WarmUp1
        ToggleCycle(); // WarmUp2

        _ctx.Pins.DataBus = addr.Lsb();
        ToggleCycle(); // Boot1

        _ctx.Pins.DataBus = addr.Msb();
        ToggleCycle(); // Boot2
    }

    private void ToggleCycle()
    {
        for (var i = 0; i < Constants.P2LastSubstep; i++)
        {
            Toggle();
        }
    }

    private void Toggle()
    {
        _pinsExt.PHI2 = _phi2;
        _engine.Step();
        _phi2 = _phi2 == 0 ? (byte)1 : (byte)0;
    }
}
