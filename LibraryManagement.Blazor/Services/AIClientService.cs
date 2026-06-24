using System.Net.Http.Json;
using LibraryManagement.Blazor.Models;

namespace LibraryManagement.Blazor.Services;

public sealed class AIClientService : ApiClientBase
{
    public AIClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<OperationResult<string>> SendChatAsync(string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return OperationResult<string>.Failure("Please enter a message.");
        }

        using var client = CreateClient(includeBearerToken: true);
        using var response = await client.PostAsJsonAsync(BuildApiUrl("api/ai/chat"), new
        {
            message = message.Trim()
        }, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: cancellationToken);
        return response.IsSuccessStatusCode
            ? OperationResult<string>.Success(payload?.ResponseText ?? string.Empty, "AI response received.")
            : OperationResult<string>.Failure(payload?.Message ?? "Could not reach the AI assistant.");
    }

    private sealed class ChatResponse
    {
        public string ResponseText { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
