using LibraryManagement.Blazor.E2E.Support;
using NUnit.Framework;

namespace LibraryManagement.Blazor.E2E.Tests;

[TestFixture]
public class AuthenticationFlowsTests : ScenarioBase
{
    [Test]
    public async Task LoginLogoutAndRoleRedirect()
    {
        await OpenAsync("/Auth/Login");
        PendingSeededFlow(nameof(LoginLogoutAndRoleRedirect));
    }

    [Test]
    public async Task AccessControlForMemberVsStaffRoutes()
    {
        await OpenAsync("/Home/Index");
        PendingSeededFlow(nameof(AccessControlForMemberVsStaffRoutes));
    }
}
