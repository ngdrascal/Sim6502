using System.Diagnostics.CodeAnalysis;

namespace W65C22.Engine.Tests;

[ExcludeFromCodeCoverage]
public class InputLatchTests : ViaTestBase
{
    /*
       TITLE: With PA latching enabled IRA holds the PA levels at the CA1 active transition
       GIVEN: a VIA with ACR bit 0 set and PA pins reading 0x11
       WHEN: CA1 falls, the PA pins change to 0x22, and IRA is read
       THEN: the read returns 0x11
     */
    [Fact]
    public void PortALatchedOnCa1()
    {
        // ARRANGE:
        Write(Acr, PaLatch);
        Pins.PAIn = 0x11;

        // ACT:
        SetCa1(false);
        Pins.PAIn = 0x22;
        var value = Read(Ora);

        // ASSERT:
        Assert.Equal(0x11, value);
    }

    /*
       TITLE: After IRA has been read it is transparent until the next CA1 active transition
       GIVEN: a VIA with PA latched at 0x11 by CA1 and then read once
       WHEN: the PA pins read 0x33 and IRA is read through register $F
       THEN: the read returns 0x33
     */
    [Fact]
    public void PortALatchTransparentAfterRead()
    {
        // ARRANGE:
        Write(Acr, PaLatch);
        Pins.PAIn = 0x11;
        SetCa1(false);
        Read(Ora);
        Pins.PAIn = 0x33;

        // ACT:
        var value = Read(OraNoHandshake);

        // ASSERT:
        Assert.Equal(0x33, value);
    }

    /*
       TITLE: Before any CA1 active transition IRA is transparent even with latching enabled
       GIVEN: a VIA with ACR bit 0 set and PA pins reading 0x44
       WHEN: IRA is read
       THEN: the read returns 0x44
     */
    [Fact]
    public void PortALatchTransparentBeforeTransition()
    {
        // ARRANGE:
        Write(Acr, PaLatch);
        Pins.PAIn = 0x44;

        // ACT:
        var value = Read(Ora);

        // ASSERT:
        Assert.Equal(0x44, value);
    }

    /*
       TITLE: Without latching enabled CA1 does not capture PA
       GIVEN: a reset VIA with PA pins reading 0x11
       WHEN: CA1 falls, the PA pins change to 0x22, and IRA is read
       THEN: the read returns 0x22
     */
    [Fact]
    public void NoLatchWhenDisabled()
    {
        // ARRANGE:
        Pins.PAIn = 0x11;

        // ACT:
        SetCa1(false);
        Pins.PAIn = 0x22;
        var value = Read(Ora);

        // ASSERT:
        Assert.Equal(0x22, value);
    }

    /*
       TITLE: Disabling latching discards a held IRA value
       GIVEN: a VIA with PA latched at 0x11 by CA1
       WHEN: ACR bit 0 is cleared, the PA pins read 0x22, and IRA is read
       THEN: the read returns 0x22
     */
    [Fact]
    public void DisablingLatchReleasesValue()
    {
        // ARRANGE:
        Write(Acr, PaLatch);
        Pins.PAIn = 0x11;
        SetCa1(false);

        // ACT:
        Write(Acr, 0x00);
        Pins.PAIn = 0x22;
        var value = Read(Ora);

        // ASSERT:
        Assert.Equal(0x22, value);
    }

    /*
       TITLE: With PB latching enabled IRB holds input bits captured at CB1, output bits read ORB
       GIVEN: a VIA with ACR bit 1 set, DDRB=0xF0, ORB=0xA0 and PB pins reading 0x05
       WHEN: CB1 falls, the PB pins change to 0x0A, and IRB is read, then read again
       THEN: the first read returns 0xA5 and the second 0xAA
     */
    [Fact]
    public void PortBLatchedOnCb1()
    {
        // ARRANGE:
        Write(Acr, PbLatch);
        Write(Ddrb, 0xF0);
        Write(Orb, 0xA0);
        Pins.PBIn = 0x05;

        // ACT:
        SetCb1(false);
        Pins.PBIn = 0x0A;
        var first = Read(Orb);
        var second = Read(Orb);

        // ASSERT:
        Assert.Equal(0xA5, first);
        Assert.Equal(0xAA, second);
    }

    /*
       TITLE: Disabling PB latching discards a held IRB value
       GIVEN: a VIA with PB latched at 0x11 by CB1
       WHEN: ACR bit 1 is cleared, the PB pins read 0x22, and IRB is read
       THEN: the read returns 0x22
     */
    [Fact]
    public void DisablingPbLatchReleasesValue()
    {
        // ARRANGE:
        Write(Acr, PbLatch);
        Pins.PBIn = 0x11;
        SetCb1(false);

        // ACT:
        Write(Acr, 0x00);
        Pins.PBIn = 0x22;
        var value = Read(Orb);

        // ASSERT:
        Assert.Equal(0x22, value);
    }
}
