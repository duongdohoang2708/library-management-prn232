using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagement.Blazor.Models;
using LibraryManagementDAL.DTO.Circulation;
using LibraryManagementDAL.DTO.Notification;
using LibraryManagementDAL.DTO.Pagination;
using LibraryManagementDAL.Models;

namespace LibraryManagement.Blazor.Services;

public sealed class NotificationClientService : ApiClientBase
{
    public NotificationClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<PaginationResponseModel<Notification>> GetNotificationsAsync(int page = 1, CancellationToken cancellationToken = default)
    {
        var userIdText = HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdText, out var userId))
        {
            return new PaginationResponseModel<Notification>
            {
                CurrentPage = 1,
                TotalPages = 1,
                PageSize = 10
            };
        }

        return await GetAsync<PaginationResponseModel<Notification>>($"api/notifications/users/{userId}?page={page}", cancellationToken)
            ?? new PaginationResponseModel<Notification>
            {
                CurrentPage = 1,
                TotalPages = 1,
                PageSize = 10
            };
    }

    public async Task<OperationResult> SendNotificationAsync(NotificationCreateRequest model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync(BuildApiUrl("api/notifications"), model, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ActionResponse>(cancellationToken);
        return response.IsSuccessStatusCode
            ? OperationResult.Success(result?.Message ?? "Notification sent.")
            : OperationResult.Failure(result?.Message ?? "Send notification failed.");
    }

    public async Task<OperationResult> MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        var userIdText = HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdText, out var userId))
        {
            return OperationResult.Failure("Please log in to continue.");
        }

        using var client = CreateClient();
        var response = await client.PostAsync(BuildApiUrl($"api/notifications/users/{userId}/mark-all-read"), null, cancellationToken);
        return response.IsSuccessStatusCode
            ? OperationResult.Success("All notifications marked as read.")
            : OperationResult.Failure("Could not mark notifications as read.");
    }

    public async Task<List<UserSearchResult>> SearchUsersAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<UserSearchResult>();
        }

        return await GetAsync<List<UserSearchResult>>($"api/circulation/users?query={Uri.EscapeDataString(query)}", cancellationToken)
            ?? new List<UserSearchResult>();
    }

    private sealed class ActionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? Id { get; set; }
    }
}
