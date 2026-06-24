using System.Net.Http.Json;
using LibraryManagement.Blazor.Models;
using LibraryManagementDAL.DTO.Circulation;
using LibraryManagementDAL.Models;

namespace LibraryManagement.Blazor.Services;

public sealed class CirculationClientService : ApiClientBase
{
    public CirculationClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<CirculationListResult> GetTransactionsAsync(string? searchQuery, string? statusFilter, int page, CancellationToken cancellationToken = default)
    {
        var query = new List<string> { $"page={Math.Max(1, page)}" };
        AddQuery(query, "searchQuery", searchQuery);
        AddQuery(query, "statusFilter", statusFilter);

        return await GetAsync<CirculationListResult>($"api/circulation?{string.Join("&", query)}", cancellationToken)
            ?? new CirculationListResult();
    }

    public async Task<List<UserSearchResult>> SearchUsersAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<UserSearchResult>();
        }

        return await GetAsync<List<UserSearchResult>>($"api/circulation/users?query={Uri.EscapeDataString(query.Trim())}", cancellationToken)
            ?? new List<UserSearchResult>();
    }

    public async Task<List<AvailableCopyResult>> GetAvailableCopiesAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<AvailableCopyResult>>("api/circulation/available-copies", cancellationToken)
            ?? new List<AvailableCopyResult>();
    }

    public async Task<OperationResult> BorrowAsync(BorrowRequest model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PostAsJsonAsync(BuildApiUrl("api/circulation/borrow"), model, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<CirculationActionResponse>(cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult.Success(result?.Message ?? "Borrow transaction created successfully.")
            : OperationResult.Failure(result?.Message ?? "Create loan failed.");
    }

    public async Task<ReturnDetailsResult?> GetReturnDetailsAsync(int transactionId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<ReturnDetailsResult>($"api/circulation/transactions/{transactionId}/return-details", cancellationToken);
    }

    public async Task<OperationResult> ReturnAsync(int transactionId, ReturnRequest model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PostAsJsonAsync(BuildApiUrl($"api/circulation/transactions/{transactionId}/return"), model, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<CirculationActionResponse>(cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult.Success(result?.Message ?? "Books returned successfully.")
            : OperationResult.Failure(result?.Message ?? "Return books failed.");
    }

    public async Task<OperationResult> RenewAsync(int borrowDetailId, int extraDays = 7, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PostAsJsonAsync(BuildApiUrl("api/circulation/renew"), new RenewRequest
        {
            BorrowDetailId = borrowDetailId,
            ExtraDays = extraDays
        }, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<CirculationActionResponse>(cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult.Success(result?.Message ?? "Book renewed successfully.")
            : OperationResult.Failure(result?.Message ?? "Renew book failed.");
    }

    public async Task<OperationResult> ReportIssueAsync(int borrowDetailId, BookCopyStatus status, decimal fineAmount, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PostAsJsonAsync(BuildApiUrl("api/circulation/report-issue"), new ReportIssueRequest
        {
            BorrowDetailId = borrowDetailId,
            Status = status,
            FineAmount = fineAmount
        }, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<CirculationActionResponse>(cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult.Success(result?.Message ?? "Issue reported successfully.")
            : OperationResult.Failure(result?.Message ?? "Report issue failed.");
    }

    private static void AddQuery(List<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
