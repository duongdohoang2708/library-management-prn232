using System.Net.Http.Json;
using LibraryManagement.Client.Helpers;
using LibraryManagementDAL.DTO.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers
{
    [Authorize(Roles = "Admin,Manager,Librarian")]
    public class CatalogController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public CatalogController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = httpClientFactory.CreateClient();
            var model = await client.GetFromJsonAsync<CatalogIndexResult>($"{GetApiBaseUrl()}/api/catalog")
                ?? new CatalogIndexResult();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAuthor(AuthorSaveRequest request)
        {
            var client = CreateActorClient();
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/catalog/authors", request);
            await SetCatalogTempDataAsync(response, "Author created.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorSaveRequest request)
        {
            var client = CreateActorClient();
            var response = await client.PutAsJsonAsync($"{GetApiBaseUrl()}/api/catalog/authors/{id}", request);
            await SetCatalogTempDataAsync(response, "Author updated.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var client = CreateActorClient();
            var response = await client.DeleteAsync($"{GetApiBaseUrl()}/api/catalog/authors/{id}");
            await SetCatalogTempDataAsync(response, "Author deleted.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategorySaveRequest request)
        {
            var client = CreateActorClient();
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/catalog/categories", request);
            await SetCatalogTempDataAsync(response, "Category created.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(int id, CategorySaveRequest request)
        {
            var client = CreateActorClient();
            var response = await client.PutAsJsonAsync($"{GetApiBaseUrl()}/api/catalog/categories/{id}", request);
            await SetCatalogTempDataAsync(response, "Category updated.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var client = CreateActorClient();
            var response = await client.DeleteAsync($"{GetApiBaseUrl()}/api/catalog/categories/{id}");
            await SetCatalogTempDataAsync(response, "Category deleted.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePublisher(PublisherSaveRequest request)
        {
            var client = CreateActorClient();
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/catalog/publishers", request);
            await SetCatalogTempDataAsync(response, "Publisher created.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePublisher(int id, PublisherSaveRequest request)
        {
            var client = CreateActorClient();
            var response = await client.PutAsJsonAsync($"{GetApiBaseUrl()}/api/catalog/publishers/{id}", request);
            await SetCatalogTempDataAsync(response, "Publisher updated.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            var client = CreateActorClient();
            var response = await client.DeleteAsync($"{GetApiBaseUrl()}/api/catalog/publishers/{id}");
            await SetCatalogTempDataAsync(response, "Publisher deleted.");
            return RedirectToAction(nameof(Index));
        }

        private HttpClient CreateActorClient()
        {
            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            return client;
        }

        private async Task SetCatalogTempDataAsync(HttpResponseMessage response, string fallbackSuccess)
        {
            var result = await response.Content.ReadFromJsonAsync<CatalogActionResponse>();
            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? fallbackSuccess : "Catalog action failed.");
        }

        private string GetApiBaseUrl()
        {
            return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }
    }
}
