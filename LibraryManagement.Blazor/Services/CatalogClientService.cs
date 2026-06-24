using System.Net.Http.Json;
using LibraryManagement.Blazor.Models;
using LibraryManagementDAL.DTO.Catalog;

namespace LibraryManagement.Blazor.Services;

public sealed class CatalogClientService : ApiClientBase
{
    public CatalogClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<CatalogIndexResult> GetIndexAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<CatalogIndexResult>("api/catalog", cancellationToken)
            ?? new CatalogIndexResult();
    }

    public Task<OperationResult<int?>> CreateAuthorAsync(AuthorSaveRequest request, CancellationToken cancellationToken = default)
        => SendCatalogActionAsync(() => client => client.PostAsJsonAsync(BuildApiUrl("api/catalog/authors"), request, cancellationToken), "Author created.");

    public Task<OperationResult<int?>> UpdateAuthorAsync(int id, AuthorSaveRequest request, CancellationToken cancellationToken = default)
        => SendCatalogActionAsync(() => client => client.PutAsJsonAsync(BuildApiUrl($"api/catalog/authors/{id}"), request, cancellationToken), "Author updated.");

    public Task<OperationResult<int?>> DeleteAuthorAsync(int id, CancellationToken cancellationToken = default)
        => SendCatalogActionAsync(() => client => client.DeleteAsync(BuildApiUrl($"api/catalog/authors/{id}"), cancellationToken), "Author deleted.");

    public Task<OperationResult<int?>> CreateCategoryAsync(CategorySaveRequest request, CancellationToken cancellationToken = default)
        => SendCatalogActionAsync(() => client => client.PostAsJsonAsync(BuildApiUrl("api/catalog/categories"), request, cancellationToken), "Category created.");

    public Task<OperationResult<int?>> UpdateCategoryAsync(int id, CategorySaveRequest request, CancellationToken cancellationToken = default)
        => SendCatalogActionAsync(() => client => client.PutAsJsonAsync(BuildApiUrl($"api/catalog/categories/{id}"), request, cancellationToken), "Category updated.");

    public Task<OperationResult<int?>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
        => SendCatalogActionAsync(() => client => client.DeleteAsync(BuildApiUrl($"api/catalog/categories/{id}"), cancellationToken), "Category deleted.");

    public Task<OperationResult<int?>> CreatePublisherAsync(PublisherSaveRequest request, CancellationToken cancellationToken = default)
        => SendCatalogActionAsync(() => client => client.PostAsJsonAsync(BuildApiUrl("api/catalog/publishers"), request, cancellationToken), "Publisher created.");

    public Task<OperationResult<int?>> UpdatePublisherAsync(int id, PublisherSaveRequest request, CancellationToken cancellationToken = default)
        => SendCatalogActionAsync(() => client => client.PutAsJsonAsync(BuildApiUrl($"api/catalog/publishers/{id}"), request, cancellationToken), "Publisher updated.");

    public Task<OperationResult<int?>> DeletePublisherAsync(int id, CancellationToken cancellationToken = default)
        => SendCatalogActionAsync(() => client => client.DeleteAsync(BuildApiUrl($"api/catalog/publishers/{id}"), cancellationToken), "Publisher deleted.");

    private async Task<OperationResult<int?>> SendCatalogActionAsync(
        Func<Func<HttpClient, Task<HttpResponseMessage>>> actionFactory,
        string fallbackSuccess)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var response = await actionFactory()(client);
        var payload = await response.Content.ReadFromJsonAsync<CatalogActionResponse>();

        return response.IsSuccessStatusCode
            ? OperationResult<int?>.Success(payload?.Id, payload?.Message ?? fallbackSuccess)
            : OperationResult<int?>.Failure(payload?.Message ?? "Catalog action failed.");
    }
}
