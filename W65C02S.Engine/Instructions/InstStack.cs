// ReSharper disable InconsistentNaming

namespace W65C02S.Engine;

internal class InstStack : InstBase, IInstruction
{
    /***********************************************************************************************
      The 65C02 microprocessor, like its predecessor the 6502, utilizes a hardware stack located on
      memory page #1, specifically within the address range $0100-$01FF. This 256-byte area 
      functions as a Last-In, First-Out (LIFO) stack. The stack pointer (S or SP) is an 8-bit 
      register that holds the low-byte of the current stack address. The high-byte of the stack 
      address is implicitly $01. The stack grows downward in memory. This means that when a byte is
      pushed onto the stack, the stack pointer is decremented before the data is stored. Conversely,
      when a byte is pulled from the stack, the data is retrieved from the address pointed to by the
      stack pointer, and then the stack pointer is incremented. Therefore, the top of the stack 
      refers to the memory location pointed to by the stack pointer (S). As the stack grows downward,
      a lower value in the stack pointer indicates a "higher" or more recent item on the stack. The 
      initial value of the stack pointer is typically $FF, pointing to $01FF, which is the highest 
      address in the stack page. When the first item is pushed, S becomes $FE, and the item is stored
      at $01FE, and so on.     
     ***********************************************************************************************/

    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.PHAimp, States.InstPHAimp2);
        registry.Map(OpCodes.PHPimp, States.InstPHPimp2);
        registry.Map(OpCodes.PHXimp, States.InstPHXimp2);
        registry.Map(OpCodes.PHYimp, States.InstPHYimp2);
        registry.Map(OpCodes.PLAimp, States.InstPLAimp2);
        registry.Map(OpCodes.PLPimp, States.InstPLPimp2);
        registry.Map(OpCodes.PLXimp, States.InstPLXimp2);
        registry.Map(OpCodes.PLYimp, States.InstPLYimp2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry registry)
    {
        registry.Map(States.InstPHAimp2, InstPHAimp2);
        registry.Map(States.InstPHAimp3, InstPHAimp3);

        registry.Map(States.InstPHPimp2, InstPHPimp2);
        registry.Map(States.InstPHPimp3, InstPHPimp3);

        registry.Map(States.InstPHXimp2, InstPHXimp2);
        registry.Map(States.InstPHXimp3, InstPHXimp3);

        registry.Map(States.InstPHYimp2, InstPHYimp2);
        registry.Map(States.InstPHYimp3, InstPHYimp3);

        registry.Map(States.InstPLAimp2, InstPLAimp2);
        registry.Map(States.InstPLAimp3, InstPLAimp3);
        registry.Map(States.InstPLAimp4, InstPLAimp4);

        registry.Map(States.InstPLPimp2, InstPLPimp2);
        registry.Map(States.InstPLPimp3, InstPLPimp3);
        registry.Map(States.InstPLPimp4, InstPLPimp4);

        registry.Map(States.InstPLXimp2, InstPLXimp2);
        registry.Map(States.InstPLXimp3, InstPLXimp3);
        registry.Map(States.InstPLXimp4, InstPLXimp4);

        registry.Map(States.InstPLYimp2, InstPLYimp2);
        registry.Map(States.InstPLYimp3, InstPLYimp3);
        registry.Map(States.InstPLYimp4, InstPLYimp4);

        return this;
    }

    /////////////////////////////////////////////////////////////////////////////
    // PHA - Push Accumulator On Stack
    // push A
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        PHA           48      1      3
    /////////////////////////////////////////////////////////////////////////////
    private void InstPHAimp2(Context ctx)
    {
        ReadAndDiscard(ctx);
        ctx.AdvanceState(States.InstPHAimp3);
    }

    private void InstPHAimp3(Context ctx)
    {
        PrepareStackWrite(ctx);
        if (ctx.Substep == Constants.P2MiddleStep)
        {
            var value = ctx.Regs.A;
            ctx.Pins.DataBus = value;
        }
        else if (ctx.Substep == Constants.P2LastSubstep)
        {
            ctx.Regs.S = ctx.Regs.S.Dec();
        }

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // PHP - Push Processor Status On Stack
    // push P
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        PHP           08      1       3
    /////////////////////////////////////////////////////////////////////////////
    private void InstPHPimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPHPimp3);
    }

    private void InstPHPimp3(Context ctx)
    {
        PrepareStackWrite(ctx);
        if (ctx.Substep == Constants.P2MiddleStep)
        {
            var data = ctx.Regs.P.ToUInt8();
            ctx.Pins.DataBus = data;
        }
        else if (ctx.Substep == Constants.P2LastSubstep)
        {
            ctx.Regs.S = ctx.Regs.S.Dec();
        }

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // PHX - Push Index Register X On Stack
    // push X
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        PHX           DA      1      3
    /////////////////////////////////////////////////////////////////////////////
    private void InstPHXimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPHXimp3);
    }

    private void InstPHXimp3(Context ctx)
    {
        PrepareStackWrite(ctx);
        if (ctx.Substep == Constants.P2MiddleStep)
        {
            var data = ctx.Regs.X;
            ctx.Pins.DataBus = data;
        }
        else if (ctx.Substep == Constants.P2LastSubstep)
        {
            ctx.Regs.S = ctx.Regs.S.Dec();
        }

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // PHY - Push Index Register Y On Stack
    // push Y
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        PHY           5A      1      3
    /////////////////////////////////////////////////////////////////////////////
    private void InstPHYimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPHYimp3);
    }

    private void InstPHYimp3(Context ctx)
    {
        PrepareStackWrite(ctx);
        if (ctx.Substep == Constants.P2MiddleStep)
        {
            var data = ctx.Regs.Y;
            ctx.Pins.DataBus = data;
        }
        else if (ctx.Substep == Constants.P2LastSubstep)
        {
            ctx.Regs.S = ctx.Regs.S.Dec();
        }

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // PLA - Pull Accumulator From Stack
    // pull A
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        PLA           68      1      4
    /////////////////////////////////////////////////////////////////////////////
    private void InstPLAimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPLAimp3);
    }

    private void InstPLAimp3(Context ctx)
    {
        if (ctx.Substep == Constants.P2LastSubstep)
        {
            ctx.Regs.S = ctx.Regs.S.Inc();
        }

        ctx.AdvanceState(States.InstPLAimp4);
    }

    private void InstPLAimp4(Context ctx)
    {
        PrepareStackRead(ctx);
        if (ctx.Substep == Constants.P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.UpdateAUpdateFlags(data);
        }

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // PLP - Pull Processor Status From Stack
    // pull P
    // N V B D I Z C
    // + + - + + + +
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        PLP           28      1      4
    /////////////////////////////////////////////////////////////////////////////
    private void InstPLPimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPLPimp3);
    }

    private void InstPLPimp3(Context ctx)
    {
        if (ctx.Substep == Constants.P2LastSubstep)
        {
            ctx.Regs.S = ctx.Regs.S.Inc();
        }
        ctx.AdvanceState(States.InstPLPimp4);
    }

    private void InstPLPimp4(Context ctx)
    {
        PrepareStackRead(ctx);
        if (ctx.Substep == Constants.P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.P.SetFlags(data);
        }

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // PLX - Pull Index Register X From Stack
    // pull X
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        PLX           FA      1      4
    /////////////////////////////////////////////////////////////////////////////
    private void InstPLXimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPLXimp3);
    }

    private void InstPLXimp3(Context ctx)
    {
        if (ctx.Substep == Constants.P2LastSubstep)
        {
            ctx.Regs.S = ctx.Regs.S.Inc();
        }

        ctx.AdvanceState(States.InstPLXimp4);
    }

    private void InstPLXimp4(Context ctx)
    {
        PrepareStackRead(ctx);
        if (ctx.Substep == Constants.P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.SetXUpdateFlags(data);
        }

        ctx.AdvanceState(States.Fetch);
    }

    /////////////////////////////////////////////////////////////////////////////
    // PLY - Pull Index Register Y From Stack
    // pull Y
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // implied        PLY           7A      1      4
    /////////////////////////////////////////////////////////////////////////////
    private void InstPLYimp2(Context ctx)
    {
        ReadAndDiscard(ctx);

        ctx.AdvanceState(States.InstPLYimp3);
    }

    private void InstPLYimp3(Context ctx)
    {
        if (ctx.Substep == Constants.P2LastSubstep)
        {
            ctx.Regs.S = ctx.Regs.S.Inc();
        }

        ctx.AdvanceState(States.InstPLYimp4);
    }

    private void InstPLYimp4(Context ctx)
    {
        PrepareStackRead(ctx);
        if (ctx.Substep == Constants.P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.SetYUpdateFlags(data);
        }

        ctx.AdvanceState(States.Fetch);
    }
}
