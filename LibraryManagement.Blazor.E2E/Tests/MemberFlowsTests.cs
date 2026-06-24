using LibraryManagement.Blazor.E2E.Support;
using NUnit.Framework;

namespace LibraryManagement.Blazor.E2E.Tests;

[TestFixture]
public class MemberFlowsTests : ScenarioBase
{
    [Test]
    public async Task PublicBooksBrowseAndDetails()
    {
        await OpenAsync("/Books/ViewBooks");
        PendingSeededFlow(nameof(PublicBooksBrowseAndDetails));
    }

    [Test]
    public async Task ReservationCreateCancelAndBorrowHistory()
    {
        await OpenAsync("/Reservation/MyReservations");
        PendingSeededFlow(nameof(ReservationCreateCancelAndBorrowHistory));
    }
}
