namespace Sim6502.Instructions;

public class InstSTA : InstBase
{
    public InstSTA(IT2Registry t2Registry, IStateRegistry stateRegistry)
        : base(t2Registry, stateRegistry)
    {
    }

    protected override void RegisterT2State(IT2Registry registry)
    {
        registry.Map(OpCodes.STAzpg, States.InstSTAzpg2);
        registry.Map(OpCodes.STAzpgx, States.InstSTAzpgx2);
        registry.Map(OpCodes.STAabs, States.InstSTAabs2);
        registry.Map(OpCodes.STAabsx, States.InstSTAabsx2);
        registry.Map(OpCodes.STAabsy, States.InstSTAabsy2);
        registry.Map(OpCodes.STAindx, States.InstSTAindx2);
        registry.Map(OpCodes.STAindy, States.InstSTAindy2);
        registry.Map(OpCodes.STAind, States.InstSTAind2);
    }

    protected override void RegisterStates(IStateRegistry stateRegistry)
    {
        stateRegistry.Map(States.InstSTAzpg2, ctx => Zpg2(ctx));
        stateRegistry.Map(States.InstSTAzpg3, ctx => Zpg3(ctx));

        stateRegistry.Map(States.InstSTAzpgx2, ctx => Zpgx2(ctx));
        stateRegistry.Map(States.InstSTAzpgx3, ctx => Zpgx3(ctx));
        stateRegistry.Map(States.InstSTAzpgx4, ctx => Zpgx4(ctx));

        stateRegistry.Map(States.InstSTAabs2, ctx => Abs2(ctx));
        stateRegistry.Map(States.InstSTAabs3, ctx => Abs3(ctx));
        stateRegistry.Map(States.InstSTAabs4, ctx => Abs4(ctx));

        stateRegistry.Map(States.InstSTAabsx2, ctx => Absx2(ctx));
        stateRegistry.Map(States.InstSTAabsx3, ctx => Absx3(ctx));
        stateRegistry.Map(States.InstSTAabsx4, ctx => Absx4(ctx));
        stateRegistry.Map(States.InstSTAabsx5, ctx => Absx5(ctx));

        stateRegistry.Map(States.InstSTAabsy2, ctx => Absy2(ctx));
        stateRegistry.Map(States.InstSTAabsy3, ctx => Absy3(ctx));
        stateRegistry.Map(States.InstSTAabsy4, ctx => Absy4(ctx));
        stateRegistry.Map(States.InstSTAabsy5, ctx => Absy5(ctx));

        stateRegistry.Map(States.InstSTAindx2, ctx => Indx2(ctx));
        stateRegistry.Map(States.InstSTAindx3, ctx => Indx3(ctx));
        stateRegistry.Map(States.InstSTAindx4, ctx => Indx4(ctx));
        stateRegistry.Map(States.InstSTAindx5, ctx => Indx5(ctx));
        stateRegistry.Map(States.InstSTAindx6, ctx => Indx6(ctx));

        stateRegistry.Map(States.InstSTAindy2, ctx => Indy2(ctx));
        stateRegistry.Map(States.InstSTAindy3, ctx => Indy3(ctx));
        stateRegistry.Map(States.InstSTAindy4, ctx => Indy4(ctx));
        stateRegistry.Map(States.InstSTAindy5, ctx => Indy5(ctx));
        stateRegistry.Map(States.InstSTAindy6, ctx => Indy6(ctx));

        stateRegistry.Map(States.InstSTAind2, ctx => Ind2(ctx));
        stateRegistry.Map(States.InstSTAind3, ctx => Ind3(ctx));
        stateRegistry.Map(States.InstSTAind4, ctx => Ind4(ctx));
        stateRegistry.Map(States.InstSTAind5, ctx => Ind5(ctx));
    }

    protected void StoreAToEffAddr(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P1MiddleStep)
        {
            ctx.Pins.SetAddrBusPins(ctx.Regs.EA);
            ctx.Pins.SetRWB(Constants.Write);
            ctx.Pins.SetDataBusMode(DataBusMode.Output);
        }
        else if (ctx.GetSubStep() == Constants.P2MiddleStep)
        {
            ctx.Pins.SetDataBusPins(ctx.Regs.A);
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
    public void Zpg2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTAzpg3);
    }

    public void Zpg3(Context ctx)
    {
        StoreAToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x95] STA zeropage,X
    // -------------------------------------------------------------------------
    public void Zpgx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTAzpgx3);
    }

    public void Zpgx3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstSTAzpgx4);
    }

    public void Zpgx4(Context ctx)
    {
        StoreAToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x8D] STA absolute
    // -------------------------------------------------------------------------
    public void Abs2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTAabs3);
    }

    public void Abs3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstSTAabs4);
    }

    public void Abs4(Context ctx)
    {
        StoreAToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x9D] STA absolute,X
    // -------------------------------------------------------------------------
    public void Absx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTAabsx3);
    }

    public void Absx3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstSTAabsx4);
    }

    public void Absx4(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.IncEALWithX();
        }

        ctx.AdvanceState(States.InstSTAabsx5);
    }

    public void Absx5(Context ctx)
    {
        StoreAToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x99] STA absolute,Y
    // -------------------------------------------------------------------------
    public void Absy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTAabsy3);
    }

    public void Absy3(Context ctx)
    {
        FetchEffAddrHigh(ctx);
        ctx.AdvanceState(States.InstSTAabsy4);
    }

    public void Absy4(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
        {
            ctx.Regs.IncEAWithY();
        }

        ctx.AdvanceState(States.InstSTAabsy5);
    }

    public void Absy5(Context ctx)
    {
        StoreAToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x81] STA (indirect,X)
    // -------------------------------------------------------------------------
    public void Indx2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTAindx3);
    }

    public void Indx3(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.IncEALWithX();

        ctx.AdvanceState(States.InstSTAindx4);
    }

    public void Indx4(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstSTAindx5);
    }

    public void Indx5(Context ctx)
    {
        FetchEA2HighIndirect(ctx);

        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstSTAindx6);
    }

    public void Indx6(Context ctx)
    {
        StoreAToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x91] STA (indirect),Y
    // -------------------------------------------------------------------------
    public void Indy2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTAindy3);
    }

    public void Indy3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstSTAindy4);
    }

    public void Indy4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);
        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstSTAindy5);
    }

    public void Indy5(Context ctx)
    {
        if (ctx.GetSubStep() == Constants.P1MiddleStep)
            ctx.Regs.IncEAWithY();

        ctx.AdvanceState(States.InstSTAindy6);
    }

    public void Indy6(Context ctx)
    {
        StoreAToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }

    // -------------------------------------------------------------------------
    // [0x92] STA (indirect)
    // -------------------------------------------------------------------------
    public void Ind2(Context ctx)
    {
        FetchEffAddrLow(ctx);
        ctx.AdvanceState(States.InstSTAind3);
    }

    public void Ind3(Context ctx)
    {
        FetchEA2LowIndirect(ctx);
        ctx.AdvanceState(States.InstSTAind4);
    }

    public void Ind4(Context ctx)
    {
        FetchEA2HighIndirect(ctx);

        if (ctx.GetSubStep() == Constants.P2LastSubstep)
            ctx.Regs.CopyEA2ToEA();

        ctx.AdvanceState(States.InstSTAind5);
    }

    public void Ind5(Context ctx)
    {
        StoreAToEffAddr(ctx);
        ctx.AdvanceState(States.Fetch);
    }
}
