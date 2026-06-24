using LibraryManagement.Blazor.E2E.Support;
using NUnit.Framework;

namespace LibraryManagement.Blazor.E2E.Tests;

[TestFixture]
public class PaymentFlowsTests : ScenarioBase
{
    [Test]
    public async Task PaymentCheckoutResultAndCashHappyPath()
    {
        await OpenAsync("/Payment/Index");
        PendingSeededFlow(nameof(PaymentCheckoutResultAndCashHappyPath));
    }
}
