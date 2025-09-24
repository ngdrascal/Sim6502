using Microsoft.Extensions.Logging;
using Sim6502.types;
using Sim6502.Tests.Logging;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests;

public class UnitTestBase
{
    private readonly Context _ctx;
    private readonly W65C02SEngine _cpu;
    private byte _phi2;

    protected readonly Pins Pins;
    protected readonly Registers Regs;

    protected readonly BitFlag Low = new(false);
    protected readonly BitFlag High = new(true);
    protected readonly UInt16 BootAddr = new(0x1000);

    protected UnitTestBase()
    {
        Pins = new Pins();
        var statusReg = new StatusRegister();
        Regs = new Registers(statusReg);
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsoleIndentLogger(_ => { })
                .SetMinimumLevel(LogLevel.Trace);
        });
        var instLogger = loggerFactory.CreateLogger("W65c02s.inst_");
        var stateLogger = loggerFactory.CreateLogger("W65c02s.state");

        _ctx = new Context(Pins, Regs);
        _ctx.StateChanging += (_, args) =>
        {
            if (args.NewState == States.Fetch && _ctx.DbgOpCode != null)
            {
                instLogger.LogDebug("{Disassemble,-24} {DebugRegisters}", Disassembler.Disassemble(_ctx),
                    _ctx.DebugCapture?.ToString());

                instLogger.LogDebug("---------------------------------------------------------------");
            }
            stateLogger.LogDebug("advanceState(): {newState}", args.NewState);
        };

        _cpu = new W65C02SEngine(_ctx);
        _cpu.SubstepChanging += (_, args) =>
        {
            if (_ctx.GetSubStep() == Constants.P2LastSubstep)
                _ctx.CaptureRegs();
            stateLogger.LogTrace("   state: {CtxState} substep: {SubStep} regs: {regs} addr-bus: {addrBus}", args.State, args.Substep, _ctx.Regs, _ctx.Pins.AddrBus);
        };

        _phi2 = (byte)Low.ToInt();

        // set the inputs
        Pins.IRQB = (byte)High.ToInt();
        Pins.NMIB = (byte)High.ToInt();
        Pins.BE = (byte)High.ToInt();
        Pins.PHI2 = (byte)Low.ToInt();
        Pins.SOB = (byte)High.ToInt();
        Pins.RESB = (byte)High.ToInt();
        Pins.RDY = (byte)High.ToInt();
    }

    protected void BootToAddress(UInt16 addr)
    {
        _phi2 = 1;

        while (_ctx.State == States.WarmUp0)
            ExecuteMicroSteps(1);

        ExecuteClockCycles(1); // Warmup1
        ExecuteClockCycles(1); // Warmup2

        Pins.DataBus = addr.Lsb();
        ExecuteClockCycles(1); // Boot1

        Pins.DataBus = addr.Msb();
        ExecuteClockCycles(1); // Boot2
    }

    protected void ExecuteClockCycles(int count)
    {
        for (int i = 0; i < count; i++)
        {
            ExecuteMicroSteps(Constants.P2LastSubstep);
        }
    }

    protected void ExecuteMicroSteps(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Pins.PHI2 = _phi2;
            _cpu.Step();

            _phi2 = _phi2 == 0 ? (byte)1 : (byte)0;
        }
    }

    protected readonly byte[] Memory = new byte[64 * 1024];

    protected void ExecuteProgram(byte[] program, UInt16 startAddr)
    {
        // ARRANGE:
        const byte read = 1;

        var startAddrInt = startAddr.ToInt();
        for (var i = 0; i < program.Length; i++)
            Memory[startAddrInt + i] = program[i];

        BootToAddress(startAddr);

        // ACT:
        while (true)
        {
            ExecuteMicroSteps(1);

            var addr = Pins.AddrBus.ToInt();
            if (_ctx.GetSubStep() == Constants.P2LastSubstep)
            {
                if (Pins.RWB == read)
                {
                    if (addr == 0xFFFE)
                        break;

                    Pins.DataBusMode = DataBusMode.Input;
                    Pins.DataBus = new UInt8(Memory[addr]);
                }
                else
                {
                    Memory[addr] = (byte)Pins.DataBus.ToInt();
                }
            }
        }
    }

    protected BitFlag FlagValue(string nvzc, char flag)
    {
        return flag switch
        {
            'N' or 'n' => nvzc.ToCharArray()[0] == 'N' ? BitFlag.High() : BitFlag.Low(),
            'V' or 'v' => nvzc.ToCharArray()[1] == 'V' ? BitFlag.High() : BitFlag.Low(),
            'Z' or 'z' => nvzc.ToCharArray()[2] == 'Z' ? BitFlag.High() : BitFlag.Low(),
            'C' or 'c' => nvzc.ToCharArray()[3] == 'C' ? BitFlag.High() : BitFlag.Low(),
            _ => BitFlag.Low()
        };
    }
}
