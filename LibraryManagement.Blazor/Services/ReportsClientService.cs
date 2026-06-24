using LibraryManagement.Blazor.Models;
using LibraryManagementDAL.DTO.Reports;

namespace LibraryManagement.Blazor.Services;

public sealed class ReportsClientService : ApiClientBase
{
    public ReportsClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<AdvancedReportResult> GetReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        return await GetAsync<AdvancedReportResult>($"api/reports/advanced{BuildDateQuery(from, to)}", cancellationToken)
            ?? new AdvancedReportResult();
    }

    public async Task<OperationResult<DownloadedFileResult>> ExportAsync(string format, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var response = await client.GetAsync(BuildApiUrl($"api/reports/advanced/export{BuildDateQuery(from, to, format)}"), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return OperationResult<DownloadedFileResult>.Failure("Export report failed.");
        }

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase)
                ? "advanced-report.pdf"
                : "advanced-report.xlsx");

        return OperationResult<DownloadedFileResult>.Success(new DownloadedFileResult
        {
            FileName = fileName,
            ContentType = contentType,
            Content = content
        }, "Report exported.");
    }

    private static string BuildDateQuery(DateTime? from, DateTime? to, string? format = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(format))
        {
            query.Add($"format={Uri.EscapeDataString(format)}");
        }

        if (from.HasValue)
        {
            query.Add($"from={from:yyyy-MM-dd}");
        }

        if (to.HasValue)
        {
            query.Add($"to={to:yyyy-MM-dd}");
        }

        return query.Count == 0 ? string.Empty : "?" + string.Join("&", query);
    }
}
