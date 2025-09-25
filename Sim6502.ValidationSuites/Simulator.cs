using static System.Console;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.ValidationSuites;

internal class Simulator
{
    private readonly IPinsExternal _pinsExt;
    private readonly Context _ctx;
    private readonly W65C02SEngine _engine;
    private readonly byte[] _memory;

    private byte _phi2;

    public const int MaxMemory = 64 * 1024;

    public byte Peek(ushort address) => _memory[address];

    public long InstCount => _engine.InstCount;

    public Simulator()
    {
        var pins = new Pins();
        _pinsExt = pins;
        var statusReg = new StatusRegister();
        var regs = new Registers(statusReg);
        _ctx = new Context(pins, regs);
        _engine = new W65C02SEngine(_ctx);

        _pinsExt.IRQB = 1;
        _pinsExt.NMIB = 1;
        _pinsExt.BE = 1;
        _pinsExt.PHI2 = 1;
        _pinsExt.SOB = 1;
        _pinsExt.RESB = 1;
        _pinsExt.RDY = 1;

        _memory = new byte[MaxMemory];

        _phi2 = 0;
    }

    public void LoadAndRunProgram(byte[] program, ushort loadAtAddress, ushort startFromAddr)
    {
        const byte read = 1;

        for (var i = 0; i < program.Length; i++)
            _memory[loadAtAddress + i] = program[i];

        ColdBootToAddress(new UInt16(startFromAddr));

        while (true)
        {
            ExecuteMicroSteps(1);

            var addr = _ctx.Pins.AddrBus.ToInt();
            if (_ctx.GetSubStep() == Constants.P2LastSubstep)
            {
                if (_pinsExt.RWB == read)
                {
                    if (addr == 0xFFFE)
                        break;

                    _ctx.Pins.DataBusMode = DataBusMode.Input;
                    _ctx.Pins.DataBus = new UInt8(_memory[addr]);
                }
                else
                {
                    _memory[addr] = (byte)_ctx.Pins.DataBus.ToInt();
                }
            }
        }
    }

    private void ColdBootToAddress(UInt16 addr)
    {
        _phi2 = 1;

        while (_ctx.State == States.WarmUp0)
            ExecuteMicroSteps(1);

        ExecuteClockCycles(1); // Warmup1
        ExecuteClockCycles(1); // Warmup2

        _ctx.Pins.DataBus = addr.Lsb();
        ExecuteClockCycles(1); // Boot1

        _ctx.Pins.DataBus = addr.Msb();
        ExecuteClockCycles(1); // Boot2
    }

    private void ExecuteClockCycles(int count)
    {
        for (var i = 0; i < count; i++)
        {
            ExecuteMicroSteps(Constants.P2LastSubstep);
        }
    }

    private void ExecuteMicroSteps(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _pinsExt.PHI2 = _phi2;
            _engine.Step();

            _phi2 = _phi2 == 0 ? (byte)1 : (byte)0;
        }
    }
}
