using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagement.Blazor.DTO.Admin;
using LibraryManagement.Blazor.DTO.User;
using LibraryManagement.Blazor.Models;
using LibraryManagementDAL.DTO.Circulation;
using LibraryManagementDAL.DTO.Pagination;
using LibraryManagementDAL.DTO.Reservation;
using LibraryManagementDAL.DTO.User;
using LibraryManagementDAL.Models;

namespace LibraryManagement.Blazor.Services;

public sealed class UserClientService : ApiClientBase
{
    public UserClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<PaginationResponseModel<Account>> GetUsersAsync(string? search, string? roleName, int page, CancellationToken cancellationToken = default)
    {
        var query = new List<string> { $"page={Math.Max(1, page)}" };
        AddQuery(query, "search", search);
        AddQuery(query, "roleName", roleName);

        return await GetAsync<PaginationResponseModel<Account>>($"api/users?{string.Join("&", query)}", cancellationToken)
            ?? new PaginationResponseModel<Account>
            {
                CurrentPage = 1,
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1
            };
    }

    public async Task<List<string>> GetRoleNamesAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<string>>("api/users/roles", cancellationToken)
            ?? new List<string> { "Member", "Librarian", "Manager", "Admin" };
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var account = await GetAsync<Account>($"api/users/{userId}", cancellationToken);
        if (account is null)
        {
            return null;
        }

        return new UserProfileDto
        {
            UserId = account.UserId,
            Username = account.Username,
            Email = account.Email,
            FullName = account.FullName,
            Phone = account.Phone ?? string.Empty,
            Address = account.Address ?? string.Empty,
            DateOfBirth = account.DateOfBirth,
            IsActive = account.IsActive
        };
    }

    public async Task<OperationResult<int?>> CreateAsync(UserCreateRequest model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PostAsJsonAsync(BuildApiUrl("api/users"), model, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<UserActionResponse>(cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult<int?>.Success(payload?.UserId, payload?.Message ?? "User created successfully.")
            : OperationResult<int?>.Failure(payload?.Message ?? "Create user failed.");
    }

    public async Task<OperationResult> ToggleStatusAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PostAsync(BuildApiUrl($"api/users/{userId}/toggle-status"), null, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<UserActionResponse>(cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult.Success(payload?.Message ?? "User status updated.")
            : OperationResult.Failure(payload?.Message ?? "Update user status failed.");
    }

    public async Task<OperationResult> UpdateProfileAsync(int userId, UserProfileDto model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PutAsJsonAsync(BuildApiUrl($"api/users/{userId}/profile"), new
        {
            model.FullName,
            model.Phone,
            model.Address,
            model.DateOfBirth
        }, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<UserActionResponse>(cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return OperationResult.Failure(payload?.Message ?? "Profile update failed.");
        }

        if (HttpContext is not null)
        {
            HttpContext.Session.SetString("FullName", model.FullName ?? string.Empty);
        }

        return OperationResult.Success(payload?.Message ?? "Profile updated successfully.");
    }

    public async Task<MemberDashboardViewModel> GetMemberDashboardAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        var transactionsTask = client.GetFromJsonAsync<List<CirculationTransactionItem>>(BuildApiUrl($"api/circulation/users/{userId}/borrow-history"), cancellationToken);
        var reservationsTask = client.GetFromJsonAsync<List<ReservationItem>>(BuildApiUrl($"api/reservations/users/{userId}"), cancellationToken);
        var notificationsTask = client.GetFromJsonAsync<PaginationResponseModel<Notification>>(BuildApiUrl($"api/notifications/users/{userId}?page=1"), cancellationToken);

        await Task.WhenAll(transactionsTask, reservationsTask, notificationsTask);

        var transactions = transactionsTask.Result ?? new List<CirculationTransactionItem>();
        var reservations = reservationsTask.Result ?? new List<ReservationItem>();
        var notifications = notificationsTask.Result ?? new PaginationResponseModel<Notification>();

        var openDetails = transactions
            .SelectMany(x => x.BorrowDetails)
            .Where(x => x.ActualReturnDate == null)
            .ToList();

        return new MemberDashboardViewModel
        {
            BorrowTransactions = transactions.Take(5).ToList(),
            Reservations = reservations.Take(5).ToList(),
            Notifications = notifications.Items.Take(5).ToList(),
            CurrentlyBorrowedCount = openDetails.Count,
            OverdueCount = openDetails.Count(x => x.DueDate.Date < DateTime.UtcNow.Date),
            UnpaidFineAmount = transactions.SelectMany(x => x.BorrowDetails).Sum(x => x.FineAmount ?? 0),
            ActiveReservationCount = reservations.Count(x => x.Status == ReservationStatus.Pending || x.Status == ReservationStatus.Allocated)
        };
    }

    public async Task<List<CirculationTransactionItem>> GetBorrowHistoryAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<CirculationTransactionItem>>($"api/circulation/users/{userId}/borrow-history", cancellationToken)
            ?? new List<CirculationTransactionItem>();
    }

    public bool TryGetCurrentUserId(out int userId)
    {
        var userIdText = HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdText, out userId);
    }

    private static void AddQuery(List<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }

    private sealed class UserActionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
    }
}
