using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagement.Blazor.Models;
using LibraryManagementDAL.DTO.Reservation;

namespace LibraryManagement.Blazor.Services;

public sealed class ReservationClientService : ApiClientBase
{
    public ReservationClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<List<ReservationItem>> GetReservationsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<ReservationItem>>("api/reservations", cancellationToken)
            ?? new List<ReservationItem>();
    }

    public async Task<List<ReservationItem>> GetCurrentUserReservationsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return new List<ReservationItem>();
        }

        return await GetAsync<List<ReservationItem>>($"api/reservations/users/{userId}", cancellationToken)
            ?? new List<ReservationItem>();
    }

    public async Task<OperationResult> ApproveAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PostAsync(BuildApiUrl($"api/reservations/{reservationId}/approve"), null, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<ReservationActionResponse>(cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult.Success(payload?.Message ?? "Reservation approved.")
            : OperationResult.Failure(payload?.Message ?? "Approve reservation failed.");
    }

    public async Task<OperationResult> CancelAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PostAsync(BuildApiUrl($"api/reservations/{reservationId}/cancel"), null, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<ReservationActionResponse>(cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult.Success(payload?.Message ?? "Reservation cancelled.")
            : OperationResult.Failure(payload?.Message ?? "Cancel reservation failed.");
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdText = HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdText, out userId);
    }
}
