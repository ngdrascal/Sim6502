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
        Pins.SetDataBusPins(opCodeValue);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstBXXrel2 - fetch operand, check flag

        if (flag.Equals(branchCondition))
        {
            ExecuteClockCycles(1); // InstBXXrel3 - add the operand to the lsb of the PC reg

            if (!expectedPC.Msb().Equals(pcInitValue.Msb()))
                ExecuteClockCycles(1); // InstBXXrel4 - add the carry to the msb of the PC reg
        }

        // ASSERT:
        Assert.Equal(opCodeValue, Pins.GetDBGINST());
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
        Pins.SetDataBusPins(opCodeValue);
        ExecuteClockCycles(1); // fetch the opcode

        Pins.SetDataBusPins(operand);
        ExecuteClockCycles(1); // InstBRArel2 - fetch operand, check flag

        ExecuteClockCycles(1); // InstBRArel3 - add the operand to the lsb of the PC reg

        if (!expectedPC.Msb().Equals(pcInitValue.Msb()))
            ExecuteClockCycles(1); // InstBRArel4 - add the carry to the msb of the PC reg

        // ASSERT:
        Assert.Equal(opCodeValue, Pins.GetDBGINST());
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
        byte[] program = {
            //                                       0000:              .ORG $0000
            0xA9, 0x00, //                           0000: START:       LDA #$00
            0x48, //                                 0002:              PHA
            0x28, //                                 0003:              PLP
            0xB0, 0x40, //                           0004:              BCS ERROR
            0xF0, 0x3E, //                           0006:              BEQ ERROR
            0x30, 0x3C, //                           0008:              BMI ERROR
            0x70, 0x3A, //                           000A:              BVS ERROR
            //                                       000C:              
            //                                       000C: BCC0:        
            0x90, 0x03, //                           000C:              BCC BNE0
            0x4C, 0x46, 0x00, //                     000E:              JMP ERROR
            //                                       0011: BNE0:        
            0xD0, 0x03, //                           0011:              BNE BPL0
            0x4C, 0x46, 0x00, //                     0013:              JMP ERROR
            //                                       0016: BPL0:        
            0x10, 0x03, //                           0016:              BPL BVC0
            0x4C, 0x46, 0x00, //                     0018:              JMP ERROR
            //                                       001B: BVC0:        
            0x50, 0x03, //                           001B:              BVC BCC1
            0x4C, 0x46, 0x00, //                     001D:              JMP ERROR
            //                                       0020: BCC1:        
            0xA9, 0xFF, //                           0020:              LDA #$FF
            0x48, //                                 0022:              PHA
            0x28, //                                 0023:              PLP
            0x90, 0x20, //                           0024:              BCC ERROR
            0xD0, 0x1E, //                           0026:              BNE ERROR
            0x10, 0x1C, //                           0028:              BPL ERROR
            0x50, 0x1A, //                           002A:              BVC ERROR
            //                                       002C:              
            0xB0, 0x03, //                           002C:              BCS BEQ1
            0x4C, 0x46, 0x00, //                     002E:              JMP ERROR
            //                                       0031: BEQ1:        
            0xF0, 0x03, //                           0031:              BEQ BMI1
            0x4C, 0x46, 0x00, //                     0033:              JMP ERROR
            //                                       0036: BMI1:        
            0x30, 0x03, //                           0036:              BMI BVS1
            0x4C, 0x46, 0x00, //                     0038:              JMP ERROR
            //                                       003B: BVS1:        
            0x70, 0x03, //                           003B:              BVS SUCCESS
            0x4C, 0x46, 0x00, //                     003D:              JMP ERROR
            //                                       0040: SUCCESS:     
            0xA9, 0x00, //                           0040:              LDA #$00
            0x8D, 0x4C, 0x00, //                     0042:              STA RESULT
            0x00, //                                 0045:              BRK
            //                                       0046: ERROR:       
            0xA9, 0x01, //                           0046:              LDA #$01
            0x8D, 0x4C, 0x00, //                     0048:              STA RESULT
            0x00, //                                 004B:              BRK
            //                                       004C: RESULT:      
            0xFF, //                                 004C:              .DB $FF
        };

        // ACT:
        ExecuteProgram(program, new UInt16(0x0000));

        // ASSERT:
        Assert.Equal(0x00, Memory[0x4C]);
    }
}
