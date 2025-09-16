// ReSharper disable InconsistentNaming
using UInt16 = Sim6502.types.UInt16;

namespace Sim6502.Tests;

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
        Pins.SetDataBusPins(opCode);
        ExecuteClockCycles(1); // fetch the opcode
        ExecuteClockCycles(1); // WAIimp2
        ExecuteClockCycles(1); // WAIimp3

        // ASSERT:
        Assert.Equal(opCode, Pins.GetDBGINST());
        Assert.Equal(new UInt16(0x1001), Regs.PC);
    }
}
