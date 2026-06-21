using System.Net.Http.Json;
using LibraryManagementDAL.DTO.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ReportsController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public ReportsController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        public async Task<IActionResult> Index(DateTime? from, DateTime? to)
        {
            var query = BuildDateQuery(from, to);
            var client = httpClientFactory.CreateClient();
            var report = await client.GetFromJsonAsync<AdvancedReportResult>($"{GetApiBaseUrl()}/api/reports/advanced{query}")
                ?? new AdvancedReportResult();

            ViewBag.From = from;
            ViewBag.To = to;
            return View(report);
        }

        public async Task<IActionResult> Export(string format = "excel", DateTime? from = null, DateTime? to = null)
        {
            var query = BuildDateQuery(from, to, format);
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{GetApiBaseUrl()}/api/reports/advanced/export{query}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Export report failed.";
                return RedirectToAction(nameof(Index), new { from, to });
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? (format == "pdf" ? "advanced-report.pdf" : "advanced-report.xlsx");

            return File(bytes, contentType, fileName);
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

            return query.Any() ? "?" + string.Join("&", query) : string.Empty;
        }

        private string GetApiBaseUrl()
        {
            return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }
    }
}
