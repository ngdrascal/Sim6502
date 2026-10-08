namespace W65C02S.Engine;

internal class InstSTA : InstBase, IInstruction
{
    public IInstruction RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.STAzpg, States.InstSTAzpg2);
        registry.Map(OpCodes.STAzpgx, States.InstSTAzpgx2);
        registry.Map(OpCodes.STAabs, States.InstSTAabs2);
        registry.Map(OpCodes.STAabsx, States.InstSTAabsx2);
        registry.Map(OpCodes.STAabsy, States.InstSTAabsy2);
        registry.Map(OpCodes.STAindx, States.InstSTAindx2);
        registry.Map(OpCodes.STAindy, States.InstSTAindy2);
        registry.Map(OpCodes.STAind, States.InstSTAind2);

        return this;
    }

    public IInstruction RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstSTAzpg2, Zpg2);
        stateRegistry.Map(States.InstSTAzpg3, Zpg3);
        stateRegistry.Map(States.InstSTAzpgx2, Zpgx2);
        stateRegistry.Map(States.InstSTAzpgx3, Zpgx3);
        stateRegistry.Map(States.InstSTAzpgx4, Zpgx4);
        stateRegistry.Map(States.InstSTAabs2, Abs2);
        stateRegistry.Map(States.InstSTAabs3, Abs3);
        stateRegistry.Map(States.InstSTAabs4, Abs4);
        stateRegistry.Map(States.InstSTAabsx2, Absx2);
        stateRegistry.Map(States.InstSTAabsx3, Absx3);
        stateRegistry.Map(States.InstSTAabsx4, Absx4);
        stateRegistry.Map(States.InstSTAabsx5, Absx5);
        stateRegistry.Map(States.InstSTAabsy2, Absy2);
        stateRegistry.Map(States.InstSTAabsy3, Absy3);
        stateRegistry.Map(States.InstSTAabsy4, Absy4);
        stateRegistry.Map(States.InstSTAabsy5, Absy5);
        stateRegistry.Map(States.InstSTAindx2, Indx2);
        stateRegistry.Map(States.InstSTAindx3, Indx3);
        stateRegistry.Map(States.InstSTAindx4, Indx4);
        stateRegistry.Map(States.InstSTAindx5, Indx5);
        stateRegistry.Map(States.InstSTAindx6, Indx6);
        stateRegistry.Map(States.InstSTAindy2, Indy2);
        stateRegistry.Map(States.InstSTAindy3, Indy3);
        stateRegistry.Map(States.InstSTAindy4, Indy4);
        stateRegistry.Map(States.InstSTAindy5, Indy5);
        stateRegistry.Map(States.InstSTAindy6, Indy6);
        stateRegistry.Map(States.InstSTAind2, Ind2);
        stateRegistry.Map(States.InstSTAind3, Ind3);
        stateRegistry.Map(States.InstSTAind4, Ind4);
        stateRegistry.Map(States.InstSTAind5, Ind5);

        return this;
    }

    protected void StoreAToEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P1MiddleStep)
        {
            ctx.Pins.AddrBus = ctx.Regs.EA;
            ctx.Pins.RWB = Constants.Write;
            ctx.Pins.DataBusMode = DataBusMode.Output;
        }
        else if (ctx.GetSubStep() == Constants.P2MiddleStep)
        {
            ctx.Pins.DataBus = ctx.Regs.A;
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    // STA - Store Accumulator in Memory
    // A -> M
    // N V B D I Z C
    // - - - - - - -
    //
    // addressing     assembler     opc   bytes  cycles
    // ------------------------------------------------
    // zeropage       STA oper      85      2      3  
    // zeropage,X     STA oper,X    95      2      4  
    // absolute       STA oper      8D      3      4  
    // absolute,X     STA oper,X    9D      3      5  
    // absolute,Y     STA oper,Y    99      3      5  
    // (indirect,X)   STA (oper,X)  81      2      6
    // (indirect),Y   STA (oper),Y  91      2      6 
    // (indirect)     STA (oper)    92      2      5
    /////////////////////////////////////////////////////////////////////////////

    // -------------------------------------------------------------------------
    // [0x85] STA zeropage
    // -------------------------------------------------------------------------
    private void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTAzpg3);
    }

    private void Zpg3(Context ctx)
    {
        StoreAToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x95] STA zeropage,X
    // -------------------------------------------------------------------------
    private void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTAzpgx3);
    }

    private void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstSTAzpgx4);
    }

    private void Zpgx4(Context ctx)
    {
        StoreAToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x8D] STA absolute
    // -------------------------------------------------------------------------
    private void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTAabs3);
    }

    private void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstSTAabs4);
    }

    private void Abs4(Context ctx)
    {
        StoreAToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x9D] STA absolute,X
    // -------------------------------------------------------------------------
    private void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTAabsx3);
    }

    private void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstSTAabsx4);
    }

    private void Absx4(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.IncEAWithX();
        }

        ctx.AdvanceState(States.InstSTAabsx5);
    }

    private void Absx5(Context ctx)
    {
        StoreAToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x99] STA absolute,Y
    // -------------------------------------------------------------------------
    private void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTAabsy3);
    }

    private void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);

        ctx.AdvanceState(States.InstSTAabsy4);
    }

    private void Absy4(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.IncEAWithY();
        }

        ctx.AdvanceState(States.InstSTAabsy5);
    }

    private void Absy5(Context ctx)
    {
        StoreAToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x81] STA (indirect,X)
    // -------------------------------------------------------------------------
    private void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTAindx3);
    }

    private void Indx3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstSTAindx4);
    }

    private void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstSTAindx5);
    }

    private void Indx5(Context ctx)
    {
        FetchEA2HighIndirectZp(ctx);
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstSTAindx6);
    }

    private void Indx6(Context ctx)
    {
        StoreAToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x91] STA (indirect),Y
    // -------------------------------------------------------------------------
    private void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTAindy3);
    }

    private void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstSTAindy4);
    }

    private void Indy4(Context ctx)
    {
        FetchEA2HighIndirectZp(ctx);
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();
        ctx.AdvanceState(States.InstSTAindy5);
    }

    private void Indy5(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P1MiddleStep)
            ctx.Regs.IncEAWithY();

        ctx.AdvanceState(States.InstSTAindy6);
    }

    private void Indy6(Context ctx)
    {
        StoreAToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x92] STA (indirect)
    // -------------------------------------------------------------------------
    private void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);

        ctx.AdvanceState(States.InstSTAind3);
    }

    private void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);

        ctx.AdvanceState(States.InstSTAind4);
    }

    private void Ind4(Context ctx)
    {
        FetchEA2HighIndirectZp(ctx);
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstSTAind5);
    }

    private void Ind5(Context ctx)
    {
        StoreAToEffAddr(ctx);

        ctx.AdvanceState(States.Fetch);
    }
}
