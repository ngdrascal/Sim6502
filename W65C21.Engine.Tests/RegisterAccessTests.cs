using System.Diagnostics.CodeAnalysis;

namespace W65C21.Engine.Tests;

[ExcludeFromCodeCoverage]
public class RegisterAccessTests : PiaTestBase
{
    /*
       TITLE: Every selected read and write raises RegisterAccessed with the register reached
       GIVEN: a reset PIA with a RegisterAccessed handler and the given DDR Access bits
       WHEN: one read or write of the given location runs
       THEN: one RegisterAccess with the expected register name, direction and value is raised
     */
    [Theory]
    [InlineData(PortA, false, false, "DDRA")]
    [InlineData(PortA, true, false, "ORA")]
    [InlineData(PortA, true, true, "PA")]
    [InlineData(Cra, true, false, "CRA")]
    [InlineData(PortB, false, false, "DDRB")]
    [InlineData(PortB, true, false, "ORB")]
    [InlineData(PortB, true, true, "PB")]
    [InlineData(Crb, false, false, "CRB")]
    public void AccessRaisesEvent(int register, bool ddrAccess, bool read, string expected)
    {
        // ARRANGE:
        if (ddrAccess)
        {
            Write(Cra, DdrAccess);
            Write(Crb, DdrAccess);
        }

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
       TITLE: Deselected cycles raise no RegisterAccessed
       GIVEN: a reset PIA with a RegisterAccessed handler
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
