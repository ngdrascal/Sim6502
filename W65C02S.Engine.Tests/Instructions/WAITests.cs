// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using W65C02S.Engine;
using UInt16 = W65C02S.Engine.Types.UInt16;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class WAITests : UnitTestBase
{
    // -------------------------------------------------------------------------
    // WAI implied
    // -------------------------------------------------------------------------
    [Fact]
    public void TestWAIimp()
    {
        // ARRANGE:
        var opCode = OpCodes.WAIimp.ToUInt8();

        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode
        ExecuteClockCycles(1); // WAIimp2
        ExecuteClockCycles(1); // WAIimp3

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
        Assert.Equal(new UInt16(0x1001), Regs.PC);
    }
}
