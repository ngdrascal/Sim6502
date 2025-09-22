// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class NOPTests : UnitTestBase
{
    private void ExecuteNOP(OpCodes op, int cycles)
    {
        // ARRANGE:
        var opCode = op.ToUInt8();

        // ARRANGE:
        BootToAddress(BootAddr);

        // ACT:
        Pins.DataBus = (opCode);
        ExecuteClockCycles(1); // fetch the opcode

        ExecuteClockCycles(cycles - 1); // extra cycles needed

        // ASSERT:
        Assert.Equal(opCode, Pins.DBGINST);
    }

    // -------------------------------------------------------------------------
    // NOP (1 cycle)
    // -------------------------------------------------------------------------
    [Fact]
    public void TestNOP03()
    {
        ExecuteNOP(OpCodes.NOP03, 0);
    }

    [Fact]
    public void TestNOP0B()
    {
        ExecuteNOP(OpCodes.NOP0B, 0);
    }

    [Fact]
    public void TestNOP13()
    {
        ExecuteNOP(OpCodes.NOP13, 0);
    }

    [Fact]
    public void TestNOP1B()
    {
        ExecuteNOP(OpCodes.NOP1B, 0);
    }

    [Fact]
    public void TestNOP23()
    {
        ExecuteNOP(OpCodes.NOP23, 0);
    }

    [Fact]
    public void TestNOP2B()
    {
        ExecuteNOP(OpCodes.NOP2B, 0);
    }

    [Fact]
    public void TestNOP33()
    {
        ExecuteNOP(OpCodes.NOP33, 0);
    }

    [Fact]
    public void TestNOP3B()
    {
        ExecuteNOP(OpCodes.NOP3B, 0);
    }

    [Fact]
    public void TestNOP43()
    {
        ExecuteNOP(OpCodes.NOP43, 0);
    }

    [Fact]
    public void TestNOP4B()
    {
        ExecuteNOP(OpCodes.NOP4B, 0);
    }

    [Fact]
    public void TestNOP53()
    {
        ExecuteNOP(OpCodes.NOP53, 0);
    }

    [Fact]
    public void TestNOP5B()
    {
        ExecuteNOP(OpCodes.NOP5B, 0);
    }

    [Fact]
    public void TestNOP63()
    {
        ExecuteNOP(OpCodes.NOP63, 0);
    }

    [Fact]
    public void TestNOP6B()
    {
        ExecuteNOP(OpCodes.NOP6B, 0);
    }

    [Fact]
    public void TestNOP73()
    {
        ExecuteNOP(OpCodes.NOP73, 0);
    }

    [Fact]
    public void TestNOP7B()
    {
        ExecuteNOP(OpCodes.NOP7B, 0);
    }

    [Fact]
    public void TestNOP83()
    {
        ExecuteNOP(OpCodes.NOP83, 0);
    }

    [Fact]
    public void TestNOP8B()
    {
        ExecuteNOP(OpCodes.NOP8B, 0);
    }

    [Fact]
    public void TestNOP93()
    {
        ExecuteNOP(OpCodes.NOP93, 0);
    }

    [Fact]
    public void TestNOP9B()
    {
        ExecuteNOP(OpCodes.NOP9B, 0);
    }

    [Fact]
    public void TestNOPA3()
    {
        ExecuteNOP(OpCodes.NOPA3, 0);
    }

    [Fact]
    public void TestNOPAB()
    {
        ExecuteNOP(OpCodes.NOPAB, 0);
    }

    [Fact]
    public void TestNOPB3()
    {
        ExecuteNOP(OpCodes.NOPB3, 0);
    }

    [Fact]
    public void TestNOPBB()
    {
        ExecuteNOP(OpCodes.NOPBB, 0);
    }

    [Fact]
    public void TestNOPC3()
    {
        ExecuteNOP(OpCodes.NOPC3, 0);
    }

    [Fact]
    public void TestNOPD3()
    {
        ExecuteNOP(OpCodes.NOPD3, 0);
    }

    [Fact]
    public void TestNOPE3()
    {
        ExecuteNOP(OpCodes.NOPE3, 0);
    }

    [Fact]
    public void TestNOPEB()
    {
        ExecuteNOP(OpCodes.NOPEB, 0);
    }

    [Fact]
    public void TestNOPF3()
    {
        ExecuteNOP(OpCodes.NOPF3, 0);
    }

    [Fact]
    public void TestNOPFB()
    {
        ExecuteNOP(OpCodes.NOPFB, 0);
    }

    // -------------------------------------------------------------------------
    // NOP (2 cycles)
    // -------------------------------------------------------------------------
    [Fact]
    public void TestNOP02()
    {
        ExecuteNOP(OpCodes.NOP02, 2);
    }

    [Fact]
    public void TestNOP22()
    {
        ExecuteNOP(OpCodes.NOP22, 2);
    }

    // -------------------------------------------------------------------------
    // NOP (3 cycles)
    // -------------------------------------------------------------------------
    [Fact]
    public void TestNOP44()
    {
        ExecuteNOP(OpCodes.NOP44, 3);
    }

    // -------------------------------------------------------------------------
    // NOP (4 cycles)
    // -------------------------------------------------------------------------

    // -------------------------------------------------------------------------
    // NOP (8 cycles)
    // -------------------------------------------------------------------------
}
