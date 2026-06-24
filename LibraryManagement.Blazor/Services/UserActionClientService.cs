using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagement.Blazor.Models;
using LibraryManagementDAL.DTO.Circulation;
using LibraryManagementDAL.DTO.Reservation;

namespace LibraryManagement.Blazor.Services;

public sealed class UserActionClientService : ApiClientBase
{
    public UserActionClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<OperationResult<FollowUpAction>> RequestReserveAsync(int bookId, CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return OperationResult<FollowUpAction>.Failure("Please log in to continue.");
        }

        using var client = CreateClient(includeActorHeaders: true);
        var response = await client.PostAsJsonAsync(
            BuildApiUrl("api/reservations"),
            new ReservationCreateRequest
            {
                UserId = userId,
                BookId = bookId
            },
            cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ReservationActionResponse>(cancellationToken);
        return response.IsSuccessStatusCode
            ? OperationResult<FollowUpAction>.Success(
                new FollowUpAction { Text = "View My Reservations", Url = "/Reservation/MyReservations" },
                result?.Message ?? "Your reservation has been placed.")
            : OperationResult<FollowUpAction>.Failure(result?.Message ?? "Failed to create reservation.");
    }

    public async Task<OperationResult<FollowUpAction>> RequestBorrowAsync(int bookId, CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return OperationResult<FollowUpAction>.Failure("Please log in to continue.");
        }

        using var client = CreateClient(includeActorHeaders: true);
        var response = await client.PostAsJsonAsync(
            BuildApiUrl("api/circulation/member-borrow"),
            new MemberBorrowRequest
            {
                UserId = userId,
                BookId = bookId,
                LoanDays = 14
            },
            cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<CirculationActionResponse>(cancellationToken);
        return response.IsSuccessStatusCode
            ? OperationResult<FollowUpAction>.Success(
                new FollowUpAction { Text = "View Borrowed Books", Url = "/User/BorrowHistory" },
                result?.Message ?? "Book borrowed successfully.")
            : OperationResult<FollowUpAction>.Failure(result?.Message ?? "Failed to submit borrow request.");
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdText = HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdText, out userId);
    }
}
