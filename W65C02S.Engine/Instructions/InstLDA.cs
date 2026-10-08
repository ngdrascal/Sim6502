namespace W65C02S.Engine;

internal class InstLDA : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.LDAimm, States.InstLDAimm2);
        registry.Map(OpCodes.LDAzpg, States.InstLDAzpg2);
        registry.Map(OpCodes.LDAzpgx, States.InstLDAzpgx2);
        registry.Map(OpCodes.LDAabs, States.InstLDAabs2);
        registry.Map(OpCodes.LDAabsx, States.InstLDAabsx2);
        registry.Map(OpCodes.LDAabsy, States.InstLDAabsy2);
        registry.Map(OpCodes.LDAindx, States.InstLDAindx2);
        registry.Map(OpCodes.LDAindy, States.InstLDAindy2);
        registry.Map(OpCodes.LDAind, States.InstLDAind2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstLDAimm2, Imm2);
        stateRegistry.Map(States.InstLDAzpg2, Zpg2);
        stateRegistry.Map(States.InstLDAzpg3, Zpg3);
        stateRegistry.Map(States.InstLDAzpgx2, Zpgx2);
        stateRegistry.Map(States.InstLDAzpgx3, Zpgx3);
        stateRegistry.Map(States.InstLDAzpgx4, Zpgx4);
        stateRegistry.Map(States.InstLDAabs2, Abs2);
        stateRegistry.Map(States.InstLDAabs3, Abs3);
        stateRegistry.Map(States.InstLDAabs4, Abs4);
        stateRegistry.Map(States.InstLDAabsx2, Absx2);
        stateRegistry.Map(States.InstLDAabsx3, Absx3);
        stateRegistry.Map(States.InstLDAabsx4, Absx4);
        stateRegistry.Map(States.InstLDAabsx5, Absx5);
        stateRegistry.Map(States.InstLDAabsy2, Absy2);
        stateRegistry.Map(States.InstLDAabsy3, Absy3);
        stateRegistry.Map(States.InstLDAabsy4, Absy4);
        stateRegistry.Map(States.InstLDAabsy5, Absy5);
        stateRegistry.Map(States.InstLDAindx2, Indx2);
        stateRegistry.Map(States.InstLDAindx3, Indx3);
        stateRegistry.Map(States.InstLDAindx4, Indx4);
        stateRegistry.Map(States.InstLDAindx5, Indx5);
        stateRegistry.Map(States.InstLDAindx6, Indx6);
        stateRegistry.Map(States.InstLDAindy2, Indy2);
        stateRegistry.Map(States.InstLDAindy3, Indy3);
        stateRegistry.Map(States.InstLDAindy4, Indy4);
        stateRegistry.Map(States.InstLDAindy5, Indy5);
        stateRegistry.Map(States.InstLDAindy6, Indy6);
        stateRegistry.Map(States.InstLDAind2, Ind2);
        stateRegistry.Map(States.InstLDAind3, Ind3);
        stateRegistry.Map(States.InstLDAind4, Ind4);
        stateRegistry.Map(States.InstLDAind5, Ind5);

        return this;
    }

    protected void LoadAFromEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.EA;
            ctx.Pins.RWB = Read;
        }
        else if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.UpdateAUpdateFlags(data);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // LDA - Load Accumulator with Memory
    // M -> A
    // N V B D I Z C
    // + - - - - + -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // immediate      LDA #oper     A9      2      2  
    // zeropage       LDA oper      A5      2      3  
    // zeropage,X     LDA oper,X    B5      2      4  
    // absolute       LDA oper      AD      3      4  
    // absolute,X     LDA oper,X    BD      3      4(+1) 
    // absolute,Y     LDA oper,Y    B9      3      4(+1) 
    // (indirect,X)   LDA (oper,X)  A1      2      6  
    // (indirect),Y   LDA (oper),Y  B1      2      5(+1) 
    // (indirect)     LDA (oper)    B2      2      5 
    ///////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0xA9] LDA immediate
    // -------------------------------------------------------------------------
    private void Imm2(Context ctx)
    {
        Imm2SetAddrBus(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            ctx.Regs.UpdateAUpdateFlags(data);
            ctx.DbgOperand1 = data;
        }
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xA5] LDA zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLDAzpg3);
    }

    private void Zpg3(Context ctx)
    {
        LoadAFromEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xB5] LDA zeropage,X
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLDAzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        IndexZpWithX(ctx);
        ctx.AdvanceState(States.InstLDAzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        LoadAFromEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xAD] LDA absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLDAabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstLDAabs4);
    }

    private void Abs4(Context ctx)
    {
        LoadAFromEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xBD] LDA absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLDAabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstLDAabsx4);
    }

    /*
    The 6502 does a fetch during the Cycle 4, before it checks to see if
    there was any carry; if there is no carry into the high byte of the address,
    as is often true, then the address fetched from was correct and there is
    no cycle five; the operation is a four-cycle operation in this case. Absolute
    indexed writes, however require five cycles.

    From: Programming the 65816: including the 6502, 65c02, and 65802
    By:   David Eyes, Ron Lichty
    */

    private void Absx4(Context ctx)
    {
        var nextState = States.Fetch;
        IndexEffAddr(ctx, ctx.Regs.X);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            if (!ctx.CrossedPageBoundary)
                ctx.Regs.UpdateAUpdateFlags(data);
            else
                nextState = States.InstLDAabsx5;
        }
        ctx.AdvanceState(nextState);
    }

    private void Absx5(Context ctx)
    {
        LoadAFromEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xB9] LDA absolute,Y
    // -------------------------------------------------------------------------
    private void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLDAabsy3);
    }

    private void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstLDAabsy4);
    }

    private void Absy4(Context ctx)
    {
        var nextState = States.Fetch;
        IndexEffAddr(ctx, ctx.Regs.Y);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            if (!ctx.CrossedPageBoundary)
                ctx.Regs.UpdateAUpdateFlags(data);
            else
                nextState = States.InstLDAabsy5;
        }
        ctx.AdvanceState(nextState);
    }

    private void Absy5(Context ctx)
    {
        LoadAFromEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xA1] LDA (indirect,X)
    // -------------------------------------------------------------------------
    private void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLDAindx3);
    }

    private void Indx3(Context ctx)
    {
        IndexZpWithX(ctx);
        ctx.AdvanceState(States.InstLDAindx4);
    }

    private void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstLDAindx5);
    }

    private void Indx5(Context ctx)
    {
        FetchEA2HighIndirectZp(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstLDAindx6);
    }

    private void Indx6(Context ctx)
    {
        LoadAFromEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xB1] LDA (indirect),Y
    // -------------------------------------------------------------------------
    private void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLDAindy3);
    }

    private void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstLDAindy4);
    }

    private void Indy4(Context ctx)
    {
        FetchEA2HighIndirectZp(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstLDAindy5);
    }

    private void Indy5(Context ctx)
    {
        var nextState = States.Fetch;
        IndexEffAddr(ctx, ctx.Regs.Y);
        if (ctx.GetSubStep() == P2LastSubstep)
        {
            var data = ctx.Pins.DataBus;
            if (!ctx.CrossedPageBoundary)
                ctx.Regs.UpdateAUpdateFlags(data);
            else
                nextState = States.InstLDAindy6;
        }
        ctx.AdvanceState(nextState);
    }
    private void Indy6(Context ctx)
    {
        LoadAFromEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0xB2] LDA (indirect)
    // -------------------------------------------------------------------------
    private void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstLDAind3);
    }

    private void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstLDAind4);
    }

    private void Ind4(Context ctx)
    {
        FetchEA2HighIndirectZp(ctx);
        if (ctx.GetSubStep() == P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstLDAind5);
    }

    private void Ind5(Context ctx)
    {
        LoadAFromEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
