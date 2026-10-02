using System.Diagnostics.CodeAnalysis;

namespace W65C22.Engine.Tests;

[ExcludeFromCodeCoverage]
public class ShiftRegisterTests : ViaTestBase
{
    // ACR bits 4-2.
    private const byte Disabled = 0x00;
    private const byte InT2 = 0x04;
    private const byte InPhi2 = 0x08;
    private const byte InExternal = 0x0C;
    private const byte OutFreeRun = 0x10;
    private const byte OutT2 = 0x14;
    private const byte OutPhi2 = 0x18;
    private const byte OutExternal = 0x1C;

    /*
       TITLE: With the shift register disabled SR is plain storage and PCR owns CB1/CB2
       GIVEN: a VIA with ACR=0 and PCR selecting CB2 manual low
       WHEN: 0x5A is written to SR, ten cycles run, and SR is read
       THEN: the read returns 0x5A, IFR2 is clear, CB1 is not driven and CB2 is driven low
     */
    [Fact]
    public void DisabledIsStorage()
    {
        // ARRANGE:
        Write(Pcr, C2ManualLow << 4);

        // ACT:
        Write(Sr, 0x5A);
        IdleCycles(10);
        var value = Read(Sr);

        // ASSERT:
        Assert.Equal(0x5A, value);
        Assert.Equal(0x00, Engine.IFR & SrFlag);
        Assert.False(Pins.CB1Drive);
        Assert.True(Pins.CB2Drive);
        Assert.False(Pins.CB2Out);
    }

    /*
       TITLE: Each shift register mode drives the expected control lines (Q19)
       GIVEN: a VIA with PCR selecting CB2 manual low
       WHEN: ACR selects the given shift register mode
       THEN: CB1 and CB2 are driven as listed
     */
    [Theory]
    [InlineData(InT2, true, false)]
    [InlineData(InPhi2, true, false)]
    [InlineData(InExternal, false, false)]
    [InlineData(OutFreeRun, true, true)]
    [InlineData(OutT2, true, true)]
    [InlineData(OutPhi2, true, true)]
    [InlineData(OutExternal, false, true)]
    public void ModeDrivesControlLines(byte mode, bool cb1Drive, bool cb2Drive)
    {
        // ARRANGE:
        Write(Pcr, C2ManualLow << 4);

        // ACT:
        Write(Acr, mode);

        // ASSERT:
        Assert.Equal(cb1Drive, Pins.CB1Drive);
        Assert.Equal(cb2Drive, Pins.CB2Drive);
    }

    /*
       TITLE: Shift out under PHI2 sends SR7 first, one bit per two cycles, then sets IFR2 (Figure 2-11)
       GIVEN: a VIA with ACR in mode 110 and the SR interrupt enabled
       WHEN: 0xA5 is written to SR and twenty cycles run, recording CB2 at each CB1 rise
       THEN: CB2 carries 1,0,1,0,0,1,0,1; IFR2 sets at the 16th fall; SR is back to 0xA5; CB1 rests high
     */
    [Fact]
    public void ShiftOutPhi2()
    {
        // ARRANGE:
        Write(Acr, OutPhi2);
        Write(Ier, IerSet | SrFlag);

        // ACT:
        Write(Sr, 0xA5);
        var trace = Run(20);

        // ASSERT:
        Assert.Equal([true, false, true, false, false, true, false, true], trace.Cb2AtCb1Rise);
        Assert.Equal(16, trace.FirstSrFlagFall);
        Assert.Equal(0xA5, Engine.SR);
        Assert.True(Pins.CB1Out);
        Assert.False(Pins.IRQB);
    }

    /*
       TITLE: Shift out puts each bit on CB2 at the first PHI2 fall after the CB1 fall (Figure 4-8)
       GIVEN: a VIA with ACR in mode 110 and 0x80 just written to SR
       WHEN: one cycle runs (CB1 falls), then PHI2 rises, then falls
       THEN: CB2 is unchanged after the CB1 fall and through the rise, and high after the next fall
     */
    [Fact]
    public void ShiftOutBitAtFallAfterCb1Fall()
    {
        // ARRANGE:
        Write(Acr, OutPhi2);
        Write(Sr, 0x00);
        Run(20);
        Write(Sr, 0x80);

        // ACT:
        IdleCycle();
        var cb1 = Pins.CB1Out;
        var afterCb1Fall = Pins.CB2Out;
        Rise();
        var afterRise = Pins.CB2Out;
        Fall();

        // ASSERT:
        Assert.False(cb1);
        Assert.False(afterCb1Fall);
        Assert.False(afterRise);
        Assert.True(Pins.CB2Out);
    }

    /*
       TITLE: Shift in under PHI2 samples CB2 after each CB1 rise and sets IFR2 after 8 bits (Figure 2-7)
       GIVEN: a VIA with ACR in mode 010 and a peripheral that presents the next bit of 0x96 on each CB1 fall
       WHEN: SR is read and twenty cycles run
       THEN: SR holds 0x96, CB1 fell exactly 8 times, and IFR2 is set
     */
    [Fact]
    public void ShiftInPhi2()
    {
        // ARRANGE:
        Write(Acr, InPhi2);

        // ACT:
        Read(Sr);
        var trace = Run(20, cb2Data: 0x96);

        // ASSERT:
        Assert.Equal(0x96, Engine.SR);
        Assert.Equal(8, trace.Cb1Falls);
        Assert.Equal(SrFlag, Engine.IFR & SrFlag);
    }

    /*
       TITLE: Shifting in sets IFR2 at the PHI2 rise that samples the 8th bit
       GIVEN: a VIA with ACR in mode 010, SR just read
       WHEN: cycles run until CB1 has risen 8 times, then PHI2 rises
       THEN: IFR2 is clear before that rise and set after it
     */
    [Fact]
    public void ShiftInFlagAtEighthSample()
    {
        // ARRANGE:
        Write(Acr, InPhi2);
        Read(Sr);
        var rises = 0;
        while (rises < 8)
        {
            var before = Pins.CB1Out;
            IdleCycle();
            if (!before && Pins.CB1Out)
                rises++;
        }

        // ACT:
        var flagBefore = Engine.IFR & SrFlag;
        Rise();

        // ASSERT:
        Assert.Equal(0, flagBefore);
        Assert.Equal(SrFlag, Engine.IFR & SrFlag);
    }

    /*
       TITLE: Under T2 control CB1 toggles every N+2 cycles and 8 bits shift out (Figure 2-10)
       GIVEN: a VIA with T2L-L=2 and ACR in mode 101
       WHEN: 0xC3 is written to SR and seventy cycles run
       THEN: CB1 edges come at falls 3, 7, 11, ... (spacing 4), there are 16 of them, CB2 carries
             1,1,0,0,0,0,1,1 and IFR2 sets at fall 63
     */
    [Fact]
    public void ShiftOutT2()
    {
        // ARRANGE:
        Write(T2CL, 2);
        Write(Acr, OutT2);

        // ACT:
        Write(Sr, 0xC3);
        var trace = Run(70);

        // ASSERT:
        Assert.Equal(Enumerable.Range(0, 16).Select(i => 3 + (4 * i)), trace.Cb1EdgeFalls);
        Assert.Equal([true, true, false, false, false, false, true, true], trace.Cb2AtCb1Rise);
        Assert.Equal(63, trace.FirstSrFlagFall);
    }

    /*
       TITLE: Under T2 control 8 bits shift in and the transfer stops
       GIVEN: a VIA with T2L-L=1, ACR in mode 001 and a peripheral presenting 0x3C bit by bit on CB1 falls
       WHEN: SR is read and sixty cycles run
       THEN: SR holds 0x3C, CB1 fell 8 times, IFR2 is set and CB1 rests high
     */
    [Fact]
    public void ShiftInT2()
    {
        // ARRANGE:
        Write(T2CL, 1);
        Write(Acr, InT2);

        // ACT:
        Read(Sr);
        var trace = Run(60, cb2Data: 0x3C);

        // ASSERT:
        Assert.Equal(0x3C, Engine.SR);
        Assert.Equal(8, trace.Cb1Falls);
        Assert.Equal(SrFlag, Engine.IFR & SrFlag);
        Assert.True(Pins.CB1Out);
    }

    /*
       TITLE: While a shift mode uses T2, the T2 timer itself is held and sets no IFR5 (Q22)
       GIVEN: a VIA with T2 loaded with 2 and then ACR switched to mode 101
       WHEN: SR is written and forty cycles run
       THEN: the T2 high byte is still 0 and IFR5 is clear
     */
    [Fact]
    public void T2HeldWhileShifting()
    {
        // ARRANGE:
        Write(T2CL, 2);
        Write(T2CH, 0);
        Write(Acr, OutT2);

        // ACT:
        Write(Sr, 0xFF);
        Run(40);

        // ASSERT:
        Assert.Equal(0, Engine.T2Counter >> 8);
        Assert.Equal(0, Engine.IFR & T2Flag);
    }

    /*
       TITLE: Free-running shift out recirculates forever and never sets IFR2 (Figure 2-9)
       GIVEN: a VIA with T2L-L=0 and ACR in mode 100
       WHEN: 0x81 is written to SR and two hundred cycles run
       THEN: the CB2 bit sequence repeats 1,0,0,0,0,0,1,0 with period 8 and IFR2 stays clear
     */
    [Fact]
    public void FreeRunRecirculates()
    {
        // ARRANGE:
        Write(T2CL, 0);
        Write(Acr, OutFreeRun);

        // ACT:
        Write(Sr, 0x81);
        var trace = Run(200);

        // ASSERT:
        var bits = trace.Cb2AtCb1Rise;
        Assert.True(bits.Count > 16);
        Assert.Equal([true, false, false, false, false, false, false, true], bits.Take(8));
        Assert.Equal(bits.Take(8), bits.Skip(8).Take(8));
        Assert.Equal(0, Engine.IFR & SrFlag);
    }

    /*
       TITLE: External clock shift in samples CB2 at the PHI2 rise after each CB1 rise and keeps going
       GIVEN: a VIA with ACR in mode 011, SR just read
       WHEN: eight CB1 pulses carry 0xA5 on CB2, IFR2 is cleared by an SR read, and four more pulses carry 1s
       THEN: CB1 is not driven, SR is 0xA5 with IFR2 set after eight, and the next four bits shift in
     */
    [Fact]
    public void ShiftInExternal()
    {
        // ARRANGE:
        Write(Acr, InExternal);
        Read(Sr);

        // ACT:
        for (var bit = 7; bit >= 0; bit--)
            ExternalPulse(cb2: ((0xA5 >> bit) & 1) != 0);
        var afterEight = Engine.SR;
        var flagAfterEight = Engine.IFR & SrFlag;
        Read(Sr);
        for (var i = 0; i < 4; i++)
            ExternalPulse(cb2: true);

        // ASSERT:
        Assert.False(Pins.CB1Drive);
        Assert.Equal(0xA5, afterEight);
        Assert.Equal(SrFlag, flagAfterEight);
        Assert.Equal(0x5F, Engine.SR);
        Assert.Equal(0, Engine.IFR & SrFlag);
    }

    /*
       TITLE: External clock shift in sets IFR2 every 8 bits without stopping
       GIVEN: a VIA with ACR in mode 011, SR just read and 8 pulses shifted with IFR2 cleared through IFR
       WHEN: eight more pulses run
       THEN: IFR2 is set again
     */
    [Fact]
    public void ExternalCountsEveryEight()
    {
        // ARRANGE:
        Write(Acr, InExternal);
        Read(Sr);
        for (var i = 0; i < 8; i++)
            ExternalPulse(cb2: false);
        Write(Ifr, SrFlag);

        // ACT:
        for (var i = 0; i < 8; i++)
            ExternalPulse(cb2: false);

        // ASSERT:
        Assert.Equal(SrFlag, Engine.IFR & SrFlag);
    }

    /*
       TITLE: External clock shift out moves a bit to CB2 at the first PHI2 fall after a CB1 fall
       GIVEN: a VIA with ACR in mode 111, CB2 resting high and 0x00 written to SR
       WHEN: CB1 falls with PHI2 low, PHI2 rises, then falls
       THEN: CB2 is still high after the CB1 fall and the rise, and low after the fall
     */
    [Fact]
    public void ShiftOutExternal()
    {
        // ARRANGE:
        Write(Acr, OutExternal);
        Write(Sr, 0x00);

        // ACT:
        SetCb1(false);
        var afterCb1 = Pins.CB2Out;
        Rise();
        var afterRise = Pins.CB2Out;
        Fall();

        // ASSERT:
        Assert.True(afterCb1);
        Assert.True(afterRise);
        Assert.True(Pins.CB2Drive);
        Assert.False(Pins.CB2Out);
    }

    /*
       TITLE: External clock shift out sets IFR2 on the 8th CB1 rise
       GIVEN: a VIA with ACR in mode 111 and SR written
       WHEN: eight CB1 pulses run
       THEN: IFR2 is clear after seven and set after eight
     */
    [Fact]
    public void ShiftOutExternalFlag()
    {
        // ARRANGE:
        Write(Acr, OutExternal);
        Write(Sr, 0x55);

        // ACT:
        for (var i = 0; i < 7; i++)
            ExternalPulse(cb2: true);
        var afterSeven = Engine.IFR & SrFlag;
        ExternalPulse(cb2: true);

        // ASSERT:
        Assert.Equal(0, afterSeven);
        Assert.Equal(SrFlag, Engine.IFR & SrFlag);
    }

    /*
       TITLE: Internal clock modes ignore edges on the CB1 net (section 5.2)
       GIVEN: a VIA with ACR in mode 110 and SR written
       WHEN: twenty cycles run while the CB1 net is toggled externally every half cycle
       THEN: the transfer still takes exactly 16 falls to set IFR2
     */
    [Fact]
    public void InternalClockIgnoresCb1Net()
    {
        // ARRANGE:
        Write(Acr, OutPhi2);

        // ACT:
        Write(Sr, 0x0F);
        var trace = Run(20, toggleCb1Net: true);

        // ASSERT:
        Assert.Equal(16, trace.FirstSrFlagFall);
    }

    /*
       TITLE: Reading SR clears IFR2 and restarts the transfer
       GIVEN: a VIA in mode 110 whose 8-bit transfer has finished
       WHEN: SR is read and three cycles run
       THEN: IFR2 is clear and CB1 is toggling again
     */
    [Fact]
    public void ReadRestarts()
    {
        // ARRANGE:
        Write(Acr, OutPhi2);
        Write(Sr, 0x00);
        Run(20);

        // ACT:
        Read(Sr);
        var trace = Run(3);

        // ASSERT:
        Assert.Equal(0, Engine.IFR & SrFlag);
        Assert.True(trace.Cb1Falls > 0);
    }

    /*
       TITLE: Changing the shift register mode abandons a transfer
       GIVEN: a VIA in mode 110 two cycles into a transfer
       WHEN: ACR switches to mode 010 and twenty cycles run
       THEN: CB1 stays high and IFR2 is never set
     */
    [Fact]
    public void ModeChangeStops()
    {
        // ARRANGE:
        Write(Acr, OutPhi2);
        Write(Sr, 0x00);
        IdleCycles(2);

        // ACT:
        Write(Acr, InPhi2);
        var trace = Run(20);

        // ASSERT:
        Assert.Equal(0, trace.Cb1Falls);
        Assert.True(Pins.CB1Out);
        Assert.Equal(0, Engine.IFR & SrFlag);
    }

    /*
       TITLE: Rewriting ACR with the same shift mode keeps the transfer running
       GIVEN: a VIA in mode 110 two cycles into a transfer
       WHEN: ACR is written with mode 110 again and twenty cycles run
       THEN: IFR2 is set
     */
    [Fact]
    public void SameModeKeepsTransfer()
    {
        // ARRANGE:
        Write(Acr, OutPhi2);
        Write(Sr, 0x00);
        IdleCycles(2);

        // ACT:
        Write(Acr, OutPhi2);
        Run(20);

        // ASSERT:
        Assert.Equal(SrFlag, Engine.IFR & SrFlag);
    }

    /*
       TITLE: Selecting mode 000 clears IFR2
       GIVEN: a VIA in mode 110 whose transfer has set IFR2
       WHEN: ACR selects mode 000
       THEN: IFR2 is clear
     */
    [Fact]
    public void DisablingClearsFlag()
    {
        // ARRANGE:
        Write(Acr, OutPhi2);
        Write(Sr, 0x00);
        Run(20);

        // ACT:
        Write(Acr, Disabled);

        // ASSERT:
        Assert.Equal(0, Engine.IFR & SrFlag);
    }

    /*
       TITLE: RESB stops the shift register and keeps its contents
       GIVEN: a VIA with ACR in mode 110 and 0x81 written to SR
       WHEN: RESB pulses low and ten cycles run
       THEN: SR still holds 0x81, CB1 is not driven and IFR2 is clear
     */
    [Fact]
    public void ResetStops()
    {
        // ARRANGE:
        Write(Acr, OutPhi2);
        Write(Sr, 0x81);

        // ACT:
        Pins.RESB = false;
        Engine.Evaluate();
        Pins.RESB = true;
        Engine.Evaluate();
        IdleCycles(10);

        // ASSERT:
        Assert.Equal(0x81, Engine.SR);
        Assert.False(Pins.CB1Drive);
        Assert.Equal(0, Engine.IFR & SrFlag);
    }

    private sealed class Trace
    {
        public List<bool> Cb2AtCb1Rise { get; } = [];
        public List<int> Cb1EdgeFalls { get; } = [];
        public int Cb1Falls { get; set; }
        public int FirstSrFlagFall { get; set; }
    }

    /// <summary>
    /// Runs idle cycles and records CB1/CB2 activity, numbering the falls from 1. With
    /// <paramref name="cb2Data"/> a peripheral presents the next bit (MSB first) on CB2 at each CB1
    /// fall. With <paramref name="toggleCb1Net"/> the CB1 net is flipped on every edge.
    /// </summary>
    private Trace Run(int cycles, int? cb2Data = null, bool toggleCb1Net = false)
    {
        var trace = new Trace();
        var bit = 7;

        // a CB1 fall already made by the access cycle that started the transfer
        if (cb2Data is { } first && !Pins.CB1Out)
        {
            trace.Cb1Falls++;
            Pins.CB2In = ((first >> bit--) & 1) != 0;
        }

        for (var fall = 1; fall <= cycles; fall++)
        {
            if (toggleCb1Net)
                Pins.CB1In = !Pins.CB1In;
            Rise();
            var before = Pins.CB1Out;
            if (toggleCb1Net)
                Pins.CB1In = !Pins.CB1In;
            Fall();

            if (before != Pins.CB1Out)
            {
                trace.Cb1EdgeFalls.Add(fall);
                if (Pins.CB1Out)
                {
                    trace.Cb2AtCb1Rise.Add(Pins.CB2Out);
                }
                else
                {
                    trace.Cb1Falls++;
                    if (cb2Data is { } data && bit >= 0)
                        Pins.CB2In = ((data >> bit--) & 1) != 0;
                }
            }

            if (trace.FirstSrFlagFall == 0 && (Engine.IFR & SrFlag) != 0)
                trace.FirstSrFlagFall = fall;
        }

        return trace;
    }

    // one external CB1 clock pulse with PHI2 low: data set up, CB1 low for a cycle, high for a cycle
    private void ExternalPulse(bool cb2)
    {
        Pins.CB2In = cb2;
        SetCb1(false);
        IdleCycle();
        SetCb1(true);
        IdleCycle();
    }
}
