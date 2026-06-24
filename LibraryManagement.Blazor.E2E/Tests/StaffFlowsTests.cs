using LibraryManagement.Blazor.E2E.Support;
using NUnit.Framework;

namespace LibraryManagement.Blazor.E2E.Tests;

[TestFixture]
public class StaffFlowsTests : ScenarioBase
{
    [Test]
    public async Task StaffBookAddUpdateImportHappyPath()
    {
        await OpenAsync("/Books/AllBooks");
        PendingSeededFlow(nameof(StaffBookAddUpdateImportHappyPath));
    }

    [Test]
    public async Task InventoryCopyManagementHappyPath()
    {
        await OpenAsync("/Inventory/Index");
        PendingSeededFlow(nameof(InventoryCopyManagementHappyPath));
    }

    [Test]
    public async Task CirculationBorrowReturnHappyPath()
    {
        await OpenAsync("/Circulation/Index");
        PendingSeededFlow(nameof(CirculationBorrowReturnHappyPath));
    }
}
