using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine;
using UInt16 = W65C02S.Engine.Types.UInt16;
using UInt8 = W65C02S.Engine.Types.UInt8;

namespace Sim6502.ValidationSuites;

[ExcludeFromCodeCoverage]
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
        _pinsExt.PHI2 = 0;
        _pinsExt.SOB = 1;
        _pinsExt.RESB = 1;
        _pinsExt.RDY = 1;

        _memory = new byte[MaxMemory];

        _phi2 = 0;

        _engine.OnInstructionComplete += (_, args) =>
        {
            Console.WriteLine(string.Format("{0,-24} --> {1}",
                Disassembler.Disassemble(args.Address, args.OpCode, args.Operand1, args.Operand2), args.Registers));
        };

        // _ctx.OnStateChanging += (_, args) =>
        // {
        //     if (args.NewState == States.Fetch && _ctx.DbgOpCode != null)
        //     {
        //         Console.WriteLine($"{Disassembler.Disassemble(_ctx),-24} {_ctx.Regs}");
        //     }
        // };
        //
        // _engine.OnSubstepChanging += (_, args) =>
        // {
        //     Console.WriteLine($"   state: {args.State} substep: {args.Substep} regs: {_ctx.Regs} addr-bus: {_ctx.Pins.AddrBus}");
        // };
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
