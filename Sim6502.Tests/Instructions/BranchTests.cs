// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using Sim6502.types;
using UInt16 = Sim6502.types.UInt16;
using UInt8 = Sim6502.types.UInt8;

namespace Sim6502.Tests.Instructions;

[ExcludeFromCodeCoverage]
public class BranchTests : UnitTestBase
{
    private void ExecuteBranch(OpCodes opCode, UInt8 operand, BitFlag flag, BitFlag branchCondition,
                               BitFlag flagInitValue, UInt16 pcInitValue)
    {
        // ARRANGE:
        var opCodeValue = opCode.ToUInt8();
        var offset = flagInitValue.Equals(branchCondition) ? operand : new UInt8(0);
        var expectedPC = pcInitValue.Copy().AddUnsigned(new UInt8(2)).AddSigned(offset);

        BootToAddress(BootAddr);
        Regs.PC.UpdateValue(pcInitValue);
        flag.UpdateValue(flagInitValue);

        // ACT:
        Pins.DataBus = (opCodeValue);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstBXXrel2 - fetch operand, check flag

        if (flag.Equals(branchCondition))
        {
            ExecuteClockCycles(1); // InstBXXrel3 - add the operand to the lsb of the PC reg

            if (!expectedPC.Msb().Equals(pcInitValue.Msb()))
                ExecuteClockCycles(1); // InstBXXrel4 - add the carry to the msb of the PC reg
        }

        // ASSERT:
        Assert.Equal(opCodeValue, Pins.DBGINST);
        Assert.Equal(expectedPC, Regs.PC);
    }

    // -------------------------------------------------------------------------
    // BRA relative
    // -------------------------------------------------------------------------
    private void ExecuteBRABranch(UInt8 operand, UInt16 pcInitValue)
    {
        // ARRANGE:
        var opCodeValue = OpCodes.BRArel.ToUInt8();
        var offset = operand;
        var expectedPC = pcInitValue.Copy().AddUnsigned(new UInt8(2)).AddSigned(offset);

        BootToAddress(BootAddr);
        Regs.PC.UpdateValue(pcInitValue);

        // ACT:
        Pins.DataBus = (opCodeValue);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.DataBus = (operand);
        ExecuteClockCycles(1); // InstBRArel2 - fetch operand, check flag

        ExecuteClockCycles(1); // InstBRArel3 - add the operand to the lsb of the PC reg

        if (!expectedPC.Msb().Equals(pcInitValue.Msb()))
            ExecuteClockCycles(1); // InstBRArel4 - add the carry to the msb of the PC reg

        // ASSERT:
        Assert.Equal(opCodeValue, Pins.DBGINST);
        Assert.Equal(expectedPC, Regs.PC);
    }

    [Fact]
    public void TestBRAForward()
    {
        var operand = new UInt8(0x08);
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBRABranch(operand, pcInitValue);
    }

    [Fact]
    public void TestBRAForwardCrossPage()
    {
        var operand = new UInt8(0x10);
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBRABranch(operand, pcInitValue);
    }

    [Fact]
    public void TestBRABackward()
    {
        var operand = new UInt8(0xF0);
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBRABranch(operand, pcInitValue);
    }

    [Fact]
    public void TestBRABackwardCrossPage()
    {
        var operand = new UInt8(0xF0);
        var pcInitValue = new UInt16(0x0100);

        ExecuteBRABranch(operand, pcInitValue);
    }

    // -------------------------------------------------------------------------
    // BCC relative
    // -------------------------------------------------------------------------
    [Fact]
    public void TestBCCWhenCarryIsSet()
    {
        var opCode = OpCodes.BCCrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Carry;
        var branchCondition = Low;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBCCWhenCarryIsClrForward()
    {
        var opCode = OpCodes.BCCrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Carry;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBCCWhenCarryIsClrForwardCrossPage()
    {
        var opCode = OpCodes.BCCrel;
        var operand = new UInt8(0x10);
        var flag = Regs.P.Carry;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBCCWhenCarryIsClrBackward()
    {
        var opCode = OpCodes.BCCrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Carry;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBCCWhenCarryIsClrBackwardCrossPage()
    {
        var opCode = OpCodes.BCCrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Carry;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x0100);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    // -------------------------------------------------------------------------
    // BCS relative
    // -------------------------------------------------------------------------
    [Fact]
    public void TestBCSWhenCarryIsClr()
    {
        var opCode = OpCodes.BCSrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Carry;
        var branchCondition = High;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBCSWhenCarryIsSetForward()
    {
        var opCode = OpCodes.BCSrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Carry;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBCSWhenCarryIsSetForwardCrossPage()
    {
        var opCode = OpCodes.BCSrel;
        var operand = new UInt8(0x10);
        var flag = Regs.P.Carry;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBCSWhenCarryIsSetBackward()
    {
        var opCode = OpCodes.BCSrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Carry;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBCSWhenCarryIsSetBackwardCrossPage()
    {
        var opCode = OpCodes.BCSrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Carry;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x0100);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    // -------------------------------------------------------------------------
    // BEQ relative
    // -------------------------------------------------------------------------
    [Fact]
    public void TestBEQWhenZeroIsClr()
    {
        var opCode = OpCodes.BEQrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Zero;
        var branchCondition = High;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBEQWhenZeroIsSetForward()
    {
        var opCode = OpCodes.BEQrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Zero;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBEQWhenZeroIsSetForwardCrossPage()
    {
        var opCode = OpCodes.BEQrel;
        var operand = new UInt8(0x10);
        var flag = Regs.P.Zero;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBEQWhenZeroIsSetBackward()
    {
        var opCode = OpCodes.BEQrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Zero;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBEQWhenZeroIsSetBackwardCrossPage()
    {
        var opCode = OpCodes.BEQrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Zero;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x0100);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    // -------------------------------------------------------------------------
    // BMI relative
    // -------------------------------------------------------------------------
    [Fact]
    public void TestBMIWhenNegativeIsClr()
    {
        var opCode = OpCodes.BMIrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Negative;
        var branchCondition = High;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBMIWhenNegativeIsSetForward()
    {
        var opCode = OpCodes.BMIrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Negative;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBMIWhenNegativeIsSetForwardCrossPage()
    {
        var opCode = OpCodes.BMIrel;
        var operand = new UInt8(0x10);
        var flag = Regs.P.Negative;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBMIWhenNegativeIsSetBackward()
    {
        var opCode = OpCodes.BMIrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Negative;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBMIWhenNegativeIsSetBackwardCrossPage()
    {
        var opCode = OpCodes.BMIrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Negative;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x0100);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    // -------------------------------------------------------------------------
    // BNE relative
    // -------------------------------------------------------------------------
    [Fact]
    public void TestBNEWhenZeroIsSet()
    {
        var opCode = OpCodes.BNErel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Zero;
        var branchCondition = Low;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBNEWhenZeroIsClrForward()
    {
        var opCode = OpCodes.BNErel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Zero;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBNEWhenZeroIsClrForwardCrossPage()
    {
        var opCode = OpCodes.BNErel;
        var operand = new UInt8(0x10);
        var flag = Regs.P.Zero;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBNEWhenZeroIsClrBackward()
    {
        var opCode = OpCodes.BNErel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Zero;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBNEWhenZeroIsClrBackwardCrossPage()
    {
        var opCode = OpCodes.BNErel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Zero;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x0100);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    // -------------------------------------------------------------------------
    // BPL relative
    // -------------------------------------------------------------------------
    [Fact]
    public void TestBPLWhenNegativeIsSet()
    {
        var opCode = OpCodes.BPLrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Negative;
        var branchCondition = Low;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBPLWhenNegativeIsClrForward()
    {
        var opCode = OpCodes.BPLrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Negative;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBPLWhenNegativeIsClrForwardCrossPage()
    {
        var opCode = OpCodes.BPLrel;
        var operand = new UInt8(0x10);
        var flag = Regs.P.Negative;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBPLWhenNegativeIsClrBackward()
    {
        var opCode = OpCodes.BPLrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Negative;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBPLWhenNegativeIsClrBackwardCrossPage()
    {
        var opCode = OpCodes.BPLrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Negative;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x0100);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    // -------------------------------------------------------------------------
    // BVC relative
    // -------------------------------------------------------------------------
    [Fact]
    public void TestBVCWhenOverflowIsSet()
    {
        var opCode = OpCodes.BVCrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Overflow;
        var branchCondition = Low;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBVCWhenOverflowIsClrForward()
    {
        var opCode = OpCodes.BVCrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Overflow;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBVCWhenOverflowIsClrForwardCrossPage()
    {
        var opCode = OpCodes.BVCrel;
        var operand = new UInt8(0x10);
        var flag = Regs.P.Overflow;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBVCWhenOverflowIsClrBackward()
    {
        var opCode = OpCodes.BVCrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Overflow;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBVCWhenOverflowIsClrBackwardCrossPage()
    {
        var opCode = OpCodes.BVCrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Overflow;
        var branchCondition = Low;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x0100);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    // -------------------------------------------------------------------------
    // BVS relative
    // -------------------------------------------------------------------------
    [Fact]
    public void TestBVSWhenNegativeIsClr()
    {
        var opCode = OpCodes.BVSrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Overflow;
        var branchCondition = High;
        var flagInitValue = Low;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBVSWhenNegativeIsSetForward()
    {
        var opCode = OpCodes.BVSrel;
        var operand = new UInt8(0x08);
        var flag = Regs.P.Overflow;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBVSWhenNegativeIsSetForwardCrossPage()
    {
        var opCode = OpCodes.BVSrel;
        var operand = new UInt8(0x10);
        var flag = Regs.P.Overflow;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBVSWhenNegativeIsSetBackward()
    {
        var opCode = OpCodes.BVSrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Overflow;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x01F0);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    [Fact]
    public void TestBVSWhenNegativeIsSetBackwardCrossPage()
    {
        var opCode = OpCodes.BVSrel;
        var operand = new UInt8(0xF0);
        var flag = Regs.P.Overflow;
        var branchCondition = High;
        var flagInitValue = High;
        var pcInitValue = new UInt16(0x0100);

        ExecuteBranch(opCode, operand, flag, branchCondition, flagInitValue, pcInitValue);
    }

    // -------------------------------------------------------------------------
    // Integration
    // -------------------------------------------------------------------------
    [Fact]
    public void TestBranchIntegration()
    {
        // ARRANGE:
        byte[] program =
        [
                                // 1000:              .ORG $1000
            0xA9, 0x00,         // 1000: START:       LDA #$00
            0x48,               // 1002:              PHA
            0x28,               // 1003:              PLP
            0xB0, 0x40,         // 1004:              BCS ERROR
            0xF0, 0x3E,         // 1006:              BEQ ERROR
            0x30, 0x3C,         // 1008:              BMI ERROR
            0x70, 0x3A,         // 100A:              BVS ERROR
                                // 100C:              
                                // 100C: BCC0:        
            0x90, 0x03,         // 100C:              BCC BNE0
            0x4C, 0x46, 0x00,   // 100E:              JMP ERROR
                                // 1011: BNE0:        
            0xD0, 0x03,         // 1011:              BNE BPL0
            0x4C, 0x46, 0x00,   // 1013:              JMP ERROR
                                // 1016: BPL0:        
            0x10, 0x03,         // 1016:              BPL BVC0
            0x4C, 0x46, 0x00,   // 1018:              JMP ERROR
                                // 101B: BVC0:        
            0x50, 0x03,         // 101B:              BVC BCC1
            0x4C, 0x46, 0x00,   // 101D:              JMP ERROR
                                // 1020: BCC1:        
            0xA9, 0xFF,         // 1020:              LDA #$FF
            0x48,               // 1022:              PHA
            0x28,               // 1023:              PLP
            0x90, 0x20,         // 1024:              BCC ERROR
            0xD0, 0x1E,         // 1026:              BNE ERROR
            0x10, 0x1C,         // 1028:              BPL ERROR
            0x50, 0x1A,         // 102A:              BVC ERROR
                                // 102C:              
            0xB0, 0x03,         // 102C:              BCS BEQ1
            0x4C, 0x46, 0x00,   // 102E:              JMP ERROR
                                // 1031: BEQ1:        
            0xF0, 0x03,         // 1031:              BEQ BMI1
            0x4C, 0x46, 0x00,   // 1033:              JMP ERROR
                                // 1036: BMI1:        
            0x30, 0x03,         // 1036:              BMI BVS1
            0x4C, 0x46, 0x00,   // 1038:              JMP ERROR
                                // 103B: BVS1:        
            0x70, 0x03,         // 103B:              BVS SUCCESS
            0x4C, 0x46, 0x00,   // 103D:              JMP ERROR
                                // 1040: SUCCESS:     
            0xA9, 0x00,         // 1040:              LDA #$00
            0x8D, 0x4C, 0x10,   // 1042:              STA RESULT
            0x00,               // 1045:              BRK
                                // 1046: ERROR:       
            0xA9, 0x01,         // 1046:              LDA #$01
            0x8D, 0x4C, 0x10,   // 1048:              STA RESULT
            0x00,               // 104B:              BRK
                                // 104C: RESULT:
            0xFF                // 104C:              .DB $FF
        ];

        // ACT:
        ExecuteProgram(program, 0, 0);

        // ASSERT:
        Assert.Equal(0x00, Memory[BootAddr.ToInt() + 0x4C]);
    }
}
