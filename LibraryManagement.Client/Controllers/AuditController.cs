using System.Net.Http.Json;
using LibraryManagement.Client.DTO.Admin;
using LibraryManagementDAL.DTO.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class AuditController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public AuditController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var query = new List<string> { $"page={page}" };
            if (!string.IsNullOrWhiteSpace(search))
            {
                query.Add($"search={Uri.EscapeDataString(search)}");
            }

            var client = httpClientFactory.CreateClient();
            var result = await client.GetFromJsonAsync<PaginationResponseModel<AuditLogItemDto>>(
                $"{GetApiBaseUrl()}/api/auditlogs?{string.Join("&", query)}")
                ?? new PaginationResponseModel<AuditLogItemDto>();

            ViewBag.Search = search;
            return View(result);
        }

        private string GetApiBaseUrl()
        {
            return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }
    }
}
