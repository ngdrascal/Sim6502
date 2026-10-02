using System.Diagnostics.CodeAnalysis;

namespace W65C22.Engine.Tests;

[ExcludeFromCodeCoverage]
public class RegisterTests : ViaTestBase
{
    /*
       TITLE: DDRA, DDRB, ACR and PCR read back what was written
       GIVEN: a reset VIA
       WHEN: distinct values are written to DDRA, DDRB, ACR and PCR and each is read
       THEN: each read returns its written value
     */
    [Theory]
    [InlineData(Ddra, 0x0F)]
    [InlineData(Ddrb, 0xF0)]
    [InlineData(Acr, 0x03)]
    [InlineData(Pcr, 0x5A)]
    public void RegistersReadBack(int register, byte value)
    {
        // ARRANGE:

        // ACT:
        Write(register, value);
        var read = Read(register);

        // ASSERT:
        Assert.Equal(value, read);
    }

    /*
       TITLE: ORA and ORB writes land in the Output Registers and leave the DDRs alone
       GIVEN: a reset VIA
       WHEN: 0x5A is written to ORA and 0xA5 to ORB
       THEN: ORA is 0x5A, ORB is 0xA5 and both DDRs are 0
     */
    [Fact]
    public void OutputRegistersWrite()
    {
        // ARRANGE:

        // ACT:
        Write(Ora, 0x5A);
        Write(Orb, 0xA5);

        // ASSERT:
        Assert.Equal(0x5A, Engine.ORA);
        Assert.Equal(0xA5, Engine.ORB);
        Assert.Equal(0x00, Engine.DDRA);
        Assert.Equal(0x00, Engine.DDRB);
    }

    /*
       TITLE: Register $F writes ORA like register 1
       GIVEN: a reset VIA
       WHEN: 0x3C is written to register $F
       THEN: ORA is 0x3C
     */
    [Fact]
    public void RegisterFWritesOra()
    {
        // ARRANGE:

        // ACT:
        Write(OraNoHandshake, 0x3C);

        // ASSERT:
        Assert.Equal(0x3C, Engine.ORA);
    }

    /*
       TITLE: Port lines drive the Output Register only where the Data Direction Register is 1
       GIVEN: a reset VIA
       WHEN: DDRA=0x0F, ORA=0x55, DDRB=0xF0, ORB=0xAA are written
       THEN: PA drives 0x55 under mask 0x0F and PB drives 0xAA under mask 0xF0
     */
    [Fact]
    public void PortDriveFollowsDataDirection()
    {
        // ARRANGE:

        // ACT:
        Write(Ddra, 0x0F);
        Write(Ddrb, 0xF0);
        Write(Ora, 0x55);
        Write(Orb, 0xAA);

        // ASSERT:
        Assert.Equal(0x55, Pins.PAOut);
        Assert.Equal(0x0F, Pins.PADrive);
        Assert.Equal(0xAA, Pins.PBOut);
        Assert.Equal(0xF0, Pins.PBDrive);
    }

    /*
       TITLE: Reading IRA returns the pin levels, even on output lines
       GIVEN: a VIA with DDRA=0xFF and ORA=0x00
       WHEN: the PA pins read 0x3C and IRA is read through register 1 and register $F
       THEN: both reads return 0x3C
     */
    [Fact]
    public void PortAReadReturnsPinLevels()
    {
        // ARRANGE:
        Write(Ddra, 0xFF);
        Write(Ora, 0x00);
        Pins.PAIn = 0x3C;

        // ACT:
        var value = Read(Ora);
        var noHandshake = Read(OraNoHandshake);

        // ASSERT:
        Assert.Equal(0x3C, value);
        Assert.Equal(0x3C, noHandshake);
    }

    /*
       TITLE: Reading IRB returns ORB on output lines and pin levels on inputs
       GIVEN: a VIA with DDRB=0xF0 and ORB=0xA0
       WHEN: the PB pins read 0x55 and IRB is read
       THEN: the read returns 0xA0 from ORB merged with 0x05 from the input pins
     */
    [Fact]
    public void PortBReadMergesOutputRegisterAndPins()
    {
        // ARRANGE:
        Write(Ddrb, 0xF0);
        Write(Orb, 0xA0);
        Pins.PBIn = 0x55;

        // ACT:
        var value = Read(Orb);

        // ASSERT:
        Assert.Equal(0xA5, value);
    }

    /*
       TITLE: The shift register location reads 0 and ignores writes for now
       GIVEN: a reset VIA
       WHEN: 0xFF is written to SR and SR is read
       THEN: the read returns 0
     */
    [Fact]
    public void ShiftRegisterNotYetModelled()
    {
        // ARRANGE:

        // ACT:
        Write(Sr, 0xFF);
        var value = Read(Sr);

        // ASSERT:
        Assert.Equal(0x00, value);
    }
}
