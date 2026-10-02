using System.Diagnostics.CodeAnalysis;

namespace W65C22.Engine.Tests;

[ExcludeFromCodeCoverage]
public class RegisterAccessTests : ViaTestBase
{
    /*
       TITLE: Every selected read and write raises RegisterAccessed with the register reached
       GIVEN: a reset VIA with PA/PB pins reading 0x5A and a RegisterAccessed handler
       WHEN: one read or write of the given register runs
       THEN: one RegisterAccess with the expected register name, direction and value is raised
     */
    [Theory]
    [InlineData(Orb, true, "IRB")]
    [InlineData(Orb, false, "ORB")]
    [InlineData(Ora, true, "IRA")]
    [InlineData(Ora, false, "ORA")]
    [InlineData(Ddrb, false, "DDRB")]
    [InlineData(Ddra, false, "DDRA")]
    [InlineData(T1CL, false, "T1L-L")]
    [InlineData(T1CH, false, "T1C-H")]
    [InlineData(T2CL, false, "T2L-L")]
    [InlineData(T2CH, false, "T2C-H")]
    [InlineData(Sr, false, "SR")]
    [InlineData(Acr, false, "ACR")]
    [InlineData(Pcr, false, "PCR")]
    [InlineData(OraNoHandshake, true, "IRA-NH")]
    [InlineData(OraNoHandshake, false, "ORA-NH")]
    public void AccessRaisesEvent(int register, bool read, string expected)
    {
        // ARRANGE:
        Pins.PAIn = 0x5A;
        Pins.PBIn = 0x5A;
        var accesses = new List<RegisterAccess>();
        Engine.RegisterAccessed += accesses.Add;

        // ACT:
        if (read)
            Read(register);
        else
            Write(register, 0x5A);

        // ASSERT:
        var access = Assert.Single(accesses);
        Assert.Equal(new RegisterAccess(expected, !read, 0x5A), access);
    }

    /*
       TITLE: Reads of timer, IFR and IER registers are reported under their read names
       GIVEN: a reset VIA with a RegisterAccessed handler
       WHEN: the given register is read
       THEN: one read access with the expected name is raised
     */
    [Theory]
    [InlineData(T1CL, "T1C-L")]
    [InlineData(T2CL, "T2C-L")]
    [InlineData(Ifr, "IFR")]
    [InlineData(Ier, "IER")]
    public void ReadNames(int register, string expected)
    {
        // ARRANGE:
        var accesses = new List<RegisterAccess>();
        Engine.RegisterAccessed += accesses.Add;

        // ACT:
        Read(register);

        // ASSERT:
        var access = Assert.Single(accesses);
        Assert.Equal(expected, access.Register);
        Assert.False(access.IsWrite);
    }

    /*
       TITLE: Deselected cycles raise no RegisterAccessed
       GIVEN: a reset VIA with a RegisterAccessed handler
       WHEN: an idle cycle runs
       THEN: no access is raised
     */
    [Fact]
    public void IdleCycleRaisesNothing()
    {
        // ARRANGE:
        var accesses = new List<RegisterAccess>();
        Engine.RegisterAccessed += accesses.Add;

        // ACT:
        IdleCycle();

        // ASSERT:
        Assert.Empty(accesses);
    }
}
