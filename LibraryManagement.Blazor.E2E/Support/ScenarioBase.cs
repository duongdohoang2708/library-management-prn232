using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace LibraryManagement.Blazor.E2E.Support;

public abstract class ScenarioBase : PageTest
{
    protected async Task OpenAsync(string path)
    {
        await Page.GotoAsync($"{TestSettings.BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}");
    }

    protected static void PendingSeededFlow(string flowName)
    {
        Assert.Ignore($"Scaffolded Playwright flow '{flowName}' requires seeded data, credentials, and selector stabilization.");
    }
}
