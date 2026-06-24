using System.Net.Http.Json;
using System.Text.Json;
using LibraryManagement.Blazor.Models;
using LibraryManagementDAL.DTO.Circulation;
using LibraryManagementDAL.DTO.Payment;
using LibraryManagementDAL.Models;

namespace LibraryManagement.Blazor.Services;

public sealed class PaymentClientService : ApiClientBase
{
    public PaymentClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<PagedUserFinesResponse> GetUsersWithFinesAsync(int page, string? searchQuery, CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"page={Math.Max(1, page)}",
            "pageSize=10"
        };

        AddQuery(query, "search", searchQuery);

        return await GetAsync<PagedUserFinesResponse>($"api/payments/users-with-fines?{string.Join("&", query)}", cancellationToken)
            ?? new PagedUserFinesResponse
            {
                CurrentPage = 1,
                TotalPages = 1,
                Users = new List<UserFinesDTO>()
            };
    }

    public async Task<List<BorrowDetail>> GetUserFinesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<BorrowDetail>>($"api/payments/user/{userId}", cancellationToken)
            ?? new List<BorrowDetail>();
    }

    public async Task<List<BorrowDetail>> GetMyFinesAsync(CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        return await client.GetFromJsonAsync<List<BorrowDetail>>(BuildApiUrl("api/payments/my-fines"), cancellationToken)
            ?? new List<BorrowDetail>();
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

    public async Task<OperationResult> CreateFineAsync(int userId, decimal amount, string reason, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PostAsJsonAsync(BuildApiUrl("api/payments/manual-fine"), new CreateManualFineRequestDTO
        {
            UserId = userId,
            Amount = amount,
            Reason = reason
        }, cancellationToken);

        var message = await ReadMessageAsync(response, response.IsSuccessStatusCode
            ? "Manual fine created successfully."
            : "Create fine failed.", cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult.Success(message)
            : OperationResult.Failure(message);
    }

    public async Task<OperationResult<string?>> CreateVnPayUrlAsync(int userId, IReadOnlyCollection<int> borrowDetailIds, decimal amount, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PostAsJsonAsync(BuildApiUrl("api/payments/vnpay-url"), new CreateVnPayUrlRequestDTO
        {
            UserId = userId,
            BorrowDetailIds = borrowDetailIds.ToList(),
            Amount = amount
        }, cancellationToken);

        VnPayUrlResponse? payload = null;
        if (response.Content.Headers.ContentLength is null || response.Content.Headers.ContentLength > 0)
        {
            payload = await response.Content.ReadFromJsonAsync<VnPayUrlResponse>(cancellationToken: cancellationToken);
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = payload?.Message;
            if (string.IsNullOrWhiteSpace(message))
            {
                message = await ReadMessageAsync(response, "Could not generate VnPay URL.", cancellationToken);
            }

            return OperationResult<string?>.Failure(message);
        }

        if (string.IsNullOrWhiteSpace(payload?.Url))
        {
            return OperationResult<string?>.Failure("Could not generate VnPay URL.");
        }

        return OperationResult<string?>.Success(payload.Url, "Redirecting to VnPay.");
    }

    public async Task<OperationResult<string?>> ProcessCashAsync(int userId, IReadOnlyCollection<int> borrowDetailIds, decimal amount, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PostAsJsonAsync(BuildApiUrl("api/payments/process-cash"), new ProcessPaymentRequestDTO
        {
            UserId = userId,
            BorrowDetailIds = borrowDetailIds.ToList(),
            AmountPaid = amount,
            PaymentMethod = "Cash"
        }, cancellationToken);

        CashPaymentResponse? payload = null;
        if (response.Content.Headers.ContentLength is null || response.Content.Headers.ContentLength > 0)
        {
            payload = await response.Content.ReadFromJsonAsync<CashPaymentResponse>(cancellationToken: cancellationToken);
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = payload?.Message;
            if (string.IsNullOrWhiteSpace(message))
            {
                message = await ReadMessageAsync(response, "Cash payment failed.", cancellationToken);
            }

            return OperationResult<string?>.Failure(message);
        }

        return OperationResult<string?>.Success(payload?.TransactionCode, "Payment successful.");
    }

    private static void AddQuery(List<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }

    private static async Task<string> ReadMessageAsync(HttpResponseMessage response, string fallbackMessage, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return fallbackMessage;
        }

        try
        {
            using var json = JsonDocument.Parse(content);
            if (json.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (json.RootElement.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString() ?? fallbackMessage;
                }

                if (json.RootElement.TryGetProperty("Message", out var upperMessage) && upperMessage.ValueKind == JsonValueKind.String)
                {
                    return upperMessage.GetString() ?? fallbackMessage;
                }
            }
        }
        catch (JsonException)
        {
        }

        return content;
    }

    private sealed class VnPayUrlResponse
    {
        public bool Success { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    private sealed class CashPaymentResponse
    {
        public bool Success { get; set; }
        public string TransactionCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
