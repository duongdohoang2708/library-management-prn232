using System.Net.Http.Headers;
using System.Net.Http.Json;
using LibraryManagement.Blazor.Helpers;

namespace LibraryManagement.Blazor.Services;

public abstract class ApiClientBase
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IConfiguration configuration;
    private readonly IHttpContextAccessor httpContextAccessor;

    protected ApiClientBase(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        this.httpClientFactory = httpClientFactory;
        this.configuration = configuration;
        this.httpContextAccessor = httpContextAccessor;
    }

    protected string GetApiBaseUrl()
    {
        return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
    }

    protected HttpClient CreateClient(bool includeActorHeaders = false, bool includeBearerToken = false)
    {
        var client = httpClientFactory.CreateClient();

        if (includeActorHeaders)
        {
            ApiActorHeaderHelper.AddActorHeaders(client, httpContextAccessor.HttpContext?.User);
        }

        if (includeBearerToken)
        {
            var token = httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return client;
    }

    protected async Task<T?> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<T>(BuildApiUrl(relativeUrl), cancellationToken);
    }

    protected string BuildApiUrl(string relativeUrl)
    {
        return $"{GetApiBaseUrl()}/{relativeUrl.TrimStart('/')}";
    }

    protected HttpContext? HttpContext => httpContextAccessor.HttpContext;
}
