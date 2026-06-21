using System.Net.Http.Json;
using LibraryManagement.Client.DTO.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class SystemSettingsController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public SystemSettingsController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = httpClientFactory.CreateClient();
            var policy = await client.GetFromJsonAsync<LibraryPolicySettingsDto>($"{GetApiBaseUrl()}/api/settings/policy")
                ?? new LibraryPolicySettingsDto();
            return View(policy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LibraryPolicySettingsDto model)
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.PutAsJsonAsync($"{GetApiBaseUrl()}/api/settings/policy", model);
            var result = await response.Content.ReadFromJsonAsync<ActionResponseDto>();
            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? "Settings updated." : "Update settings failed.");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunDueReminders()
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsync($"{GetApiBaseUrl()}/api/reminders/due", null);
            var result = await response.Content.ReadFromJsonAsync<ReminderRunResultDto>();
            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? "Due reminder job executed.";

            return RedirectToAction(nameof(Index));
        }

        private string GetApiBaseUrl()
        {
            return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }
    }
}
