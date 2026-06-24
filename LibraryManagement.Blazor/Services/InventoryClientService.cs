using System.Net.Http.Json;
using LibraryManagement.Blazor.DTO.Inventory;
using LibraryManagement.Blazor.Models;
using LibraryManagementDAL.Models;

namespace LibraryManagement.Blazor.Services;

public sealed class InventoryClientService : ApiClientBase
{
    public InventoryClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<InventoryListApiResult> GetCopiesAsync(
        string? searchBarcode,
        BookCopyStatus? status,
        BookCondition? condition,
        string? location,
        int page,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string> { $"page={Math.Max(1, page)}" };
        AddQuery(query, "searchBarcode", searchBarcode);
        AddQuery(query, "status", status.HasValue ? ((int)status.Value).ToString() : null);
        AddQuery(query, "condition", condition.HasValue ? ((int)condition.Value).ToString() : null);
        AddQuery(query, "location", location);

        return await GetAsync<InventoryListApiResult>($"api/inventory?{string.Join("&", query)}", cancellationToken)
            ?? new InventoryListApiResult();
    }

    public async Task<BookCopiesApiResult?> GetBookCopiesAsync(int bookId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<BookCopiesApiResult>($"api/inventory/book/{bookId}", cancellationToken);
    }

    public async Task<OperationResult> AddCopiesAsync(int bookId, int numberOfCopies, string? location, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var response = await client.PostAsJsonAsync(BuildApiUrl($"api/inventory/book/{bookId}/copies"), new
        {
            numberOfCopies,
            location
        }, cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult.Success($"Added {numberOfCopies} copies.")
            : OperationResult.Failure("Add copies failed.");
    }

    public async Task<OperationResult> UpdateCopyAsync(int copyId, int bookId, BookCopyStatus status, BookCondition condition, string? location, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var response = await client.PutAsJsonAsync(BuildApiUrl($"api/inventory/copies/{copyId}"), new
        {
            bookId,
            status,
            condition,
            location
        }, cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult.Success("Copy updated.")
            : OperationResult.Failure("Update copy failed.");
    }

    private static void AddQuery(List<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
