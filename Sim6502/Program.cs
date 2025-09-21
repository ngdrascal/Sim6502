using Microsoft.Extensions.Logging;

namespace Sim6502;

internal class Program
{
    static void Main(string[] args)
    {
        var pins = new Pins();
        var statusReg = new StatusRegister();
        var regs = new Registers(statusReg);
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var ctx = new Context(pins, regs);
        var cpu = new W65C02SEngine(ctx, loggerFactory);
        while (true)
        {
            cpu.Step();
        }

    }
}
