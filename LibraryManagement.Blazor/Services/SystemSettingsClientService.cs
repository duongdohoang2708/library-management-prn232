using System.Net.Http.Json;
using LibraryManagement.Blazor.DTO.Admin;
using LibraryManagement.Blazor.Models;

namespace LibraryManagement.Blazor.Services;

public sealed class SystemSettingsClientService : ApiClientBase
{
    public SystemSettingsClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<LibraryPolicySettingsDto> GetPolicyAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<LibraryPolicySettingsDto>("api/settings/policy", cancellationToken)
            ?? new LibraryPolicySettingsDto();
    }

    public async Task<OperationResult> UpdatePolicyAsync(LibraryPolicySettingsDto model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PutAsJsonAsync(BuildApiUrl("api/settings/policy"), model, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<ActionResponseDto>(cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult.Success(payload?.Message ?? "Settings updated.")
            : OperationResult.Failure(payload?.Message ?? "Update settings failed.");
    }

    public async Task<OperationResult<ReminderRunResultDto>> RunDueRemindersAsync(CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await client.PostAsync(BuildApiUrl("api/reminders/due"), null, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<ReminderRunResultDto>(cancellationToken: cancellationToken);

        return response.IsSuccessStatusCode
            ? OperationResult<ReminderRunResultDto>.Success(payload, payload?.Message ?? "Due reminder job executed.")
            : OperationResult<ReminderRunResultDto>.Failure(payload?.Message ?? "Due reminder job failed.");
    }
}
