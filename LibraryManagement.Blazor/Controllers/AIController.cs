using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Blazor.Controllers;

[Route("api/ai")]
[ApiController]
public class AIController : ControllerBase
{
    private readonly HttpClient httpClient;
    private readonly string baseUrl;

    public AIController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        httpClient = httpClientFactory.CreateClient();
        baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5070";
    }

    [HttpGet("books/{bookId:int}/summary")]
    public async Task<IActionResult> GetBookSummary(int bookId)
    {
        AddAuthorizationHeader();
        var response = await httpClient.GetAsync($"{baseUrl.TrimEnd('/')}/api/ai/books/{bookId}/summary");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        AddAuthorizationHeader();
        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync($"{baseUrl.TrimEnd('/')}/api/ai/chat", jsonContent);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private void AddAuthorizationHeader()
    {
        httpClient.DefaultRequestHeaders.Authorization = null;
        var token = HttpContext.Session.GetString("AccessToken");
        if (!string.IsNullOrWhiteSpace(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public sealed class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
