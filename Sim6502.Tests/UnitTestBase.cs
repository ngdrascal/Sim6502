using Microsoft.Extensions.Logging;
using Sim6502.types;
using Sim6502.Tests.Logging;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests;

public class UnitTestBase
{
    protected readonly BitFlag Low = new(false);
    protected readonly BitFlag High = new(true);
    protected readonly UInt16 BootAddr = new(0x1000);

    protected readonly Pins Pins;
    protected readonly Registers Regs;
    protected readonly Context Ctx;
    protected readonly W65C02SEngine Cpu;
    protected byte Phi2;
    
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

        Ctx = new Context(Pins, Regs);
        Ctx.StateChanging += (_, args) =>
        {
            if (args.NewState == States.Fetch && Ctx.DbgOpCode != null)
            {
                instLogger.LogDebug("{Disassemble,-24} {DebugRegisters}", Disassembler.Disassemble(Ctx),
                    Ctx.DebugCapture.ToString());
            }
            stateLogger.LogDebug($"advanceState(): {args.NewState}");
        };

        Cpu = new W65C02SEngine(Ctx);
        Cpu.SubstepChanging+= (_, args) =>
        {
            if (Ctx.GetSubStep()==Constants.P2LastSubstep)
                Ctx.CaptureRegs();
            stateLogger.LogTrace("Executing state: {CtxState}, substep: {SubStep}", args.State, args.Substep);
        };

        Phi2 = (byte)Low.ToInt();

        // set the inputs
        Pins.SetIRQB((byte)High.ToInt());
        Pins.SetNMIB((byte)High.ToInt());
        Pins.SetBE((byte)High.ToInt());
        Pins.SetPHI2((byte)Low.ToInt());
        Pins.SetSOB((byte)High.ToInt());
        Pins.SetRESB((byte)High.ToInt());
        Pins.SetRDY((byte)High.ToInt());
    }

    protected void BootToAddress(UInt16 addr)
    {
        Phi2 = 1;

        while (Ctx.State == States.WarmUp0)
            ExecuteMicroSteps(1);

        ExecuteClockCycles(1); // Warmup1
        ExecuteClockCycles(1); // Warmup2

        Pins.SetDataBusPins(addr.Lsb());
        ExecuteClockCycles(1); // Boot1

        Pins.SetDataBusPins(addr.Msb());
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
            Pins.SetPHI2(Phi2);
            Cpu.Step();

            Phi2 = Phi2 == 0 ? (byte)1 : (byte)0;
        }
        // phi2 = (byte)HIGH.ToInt();
    }

    protected readonly byte[] Memory = new byte[64 * 1024];

    protected void ExecuteProgram(byte[] program, UInt16 startAddr)
    {
        // ARRANGE:
        const byte read = 1;

        for (var i = 0; i < program.Length; i++)
            Memory[i] = program[i];

        BootToAddress(startAddr);
        // regs.S.UpdateValue(new UInt8(0xFF));

        // ACT:
        while (true)
        {
            ExecuteMicroSteps(1);

            var addr = Pins.GetAddrBusPins().ToInt();
            if (Ctx.GetSubStep() == Constants.P2LastSubstep)
            {
                if (Pins.GetRWB() == read)
                {
                    if (addr == 0xFFFE)
                        break;

                    Pins.SetDataBusMode(DataBusMode.Input);
                    Pins.SetDataBusPins(new UInt8(Memory[addr]));
                }
                else
                {
                    Memory[addr] = (byte)Pins.GetAddrBusPins().ToInt();
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
