using LibraryManagement.Blazor.DTO.Admin;
using LibraryManagementDAL.DTO.Pagination;

namespace LibraryManagement.Blazor.Services;

public sealed class AuditClientService : ApiClientBase
{
    public AuditClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<PaginationResponseModel<AuditLogItemDto>> GetAuditPageAsync(string? search, int page, CancellationToken cancellationToken = default)
    {
        var parts = new List<string> { $"page={Math.Max(1, page)}" };
        if (!string.IsNullOrWhiteSpace(search))
        {
            parts.Add($"search={Uri.EscapeDataString(search.Trim())}");
        }

        return await GetAsync<PaginationResponseModel<AuditLogItemDto>>($"api/auditlogs?{string.Join("&", parts)}", cancellationToken)
            ?? new PaginationResponseModel<AuditLogItemDto>();
    }
}
