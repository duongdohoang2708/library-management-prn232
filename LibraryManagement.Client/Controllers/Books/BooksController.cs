using System.Net.Http.Json;
using LibraryManagement.Client.DTO.Books;
using LibraryManagementDAL.DTO.Book;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers.Books
{
    public class BooksController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public BooksController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public async Task<IActionResult> AllBooks(
            string? keyword,
            string? category,
            int? publisherId,
            int? publishYear,
            bool? isActive,
            string? availability,
            int? minRating,
            string? sort,
            int page = 1)
        {
            var result = await GetBookListAsync(keyword, category, publisherId, publishYear, isActive, availability, minRating, sort, page);

            ViewBag.Keyword = keyword;
            ViewBag.Category = category;
            ViewBag.PublisherId = publisherId;
            ViewBag.PublishYear = publishYear;
            ViewBag.IsActive = isActive;
            ViewBag.Availability = availability;
            ViewBag.MinRating = minRating;
            ViewBag.Sort = sort;
            ViewBag.Categories = result.Categories;
            ViewBag.Publishers = result.Publishers;
            ViewBag.PublishYears = result.PublishYears;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.Request = new
            {
                Page = result.Page,
                Keyword = keyword,
                Category = category,
                PublisherId = publisherId,
                PublishYear = publishYear,
                IsActive = isActive,
                Availability = availability,
                MinRating = minRating,
                Sort = sort
            };

            return View(result.Items);
        }

        [HttpGet]
        public async Task<IActionResult> ViewBooks(
            string? keyword,
            string? category,
            int? publisherId,
            int? publishYear,
            bool? isActive,
            string? availability,
            int? minRating,
            string? sort,
            int page = 1)
        {
            return await AllBooks(keyword, category, publisherId, publishYear, isActive, availability, minRating, sort, page);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = httpClientFactory.CreateClient();
            var book = await client.GetFromJsonAsync<Book>($"{GetApiBaseUrl()}/api/books/{id}");
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public async Task<IActionResult> AddBook(string mode = "single")
        {
            await LoadBookOptionsAsync();
            ViewBag.Mode = string.IsNullOrWhiteSpace(mode) ? "single" : mode;
            return View(new BookCreateModelRequest { IsActive = true, EditionNumber = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public async Task<IActionResult> AddBook(BookCreateModelRequest model, IFormFile? imageFile)
        {
            await LoadBookOptionsAsync();
            ViewBag.Mode = "single";
            ModelState.Remove(nameof(BookCreateModelRequest.ImageUrl));
            ModelState.Remove(nameof(BookCreateModelRequest.ISBN));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.ImageUrl = await SaveCoverImageAsync(imageFile) ?? string.Empty;

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/books", model);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Create book failed.");
                return View(model);
            }

            var result = await response.Content.ReadFromJsonAsync<BookSaveResult>();
            TempData["NewBookId"] = result?.BookId;
            return RedirectToAction(nameof(AllBooks));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public async Task<IActionResult> Update(int id)
        {
            var client = httpClientFactory.CreateClient();
            var book = await client.GetFromJsonAsync<Book>($"{GetApiBaseUrl()}/api/books/{id}");
            if (book == null)
            {
                return NotFound();
            }

            await LoadBookOptionsAsync();
            return View(new BookUpdateModelRequest
            {
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description ?? string.Empty,
                PublishYear = book.PublishYear ?? 0,
                EditionNumber = book.EditionNumber,
                AuthorId = book.AuthorId,
                CategoryId = book.CategoryId,
                PublisherId = book.PublisherId,
                ISBN = book.ISBN,
                ImageUrl = book.ImageUrl ?? string.Empty,
                IsActive = book.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public async Task<IActionResult> Update(int id, BookUpdateModelRequest model, IFormFile? imageFile)
        {
            if (id != model.BookId)
            {
                return BadRequest();
            }

            await LoadBookOptionsAsync();
            ModelState.Remove(nameof(BookUpdateModelRequest.ImageUrl));
            ModelState.Remove(nameof(BookUpdateModelRequest.AuthorName));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var imageUrl = await SaveCoverImageAsync(imageFile);
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                model.ImageUrl = imageUrl;
            }

            var client = httpClientFactory.CreateClient();
            var response = await client.PutAsJsonAsync($"{GetApiBaseUrl()}/api/books/{id}", model);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Update book failed.");
                return View(model);
            }

            return RedirectToAction(nameof(Details), new { id = model.BookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var client = httpClientFactory.CreateClient();
            await client.PostAsync($"{GetApiBaseUrl()}/api/books/{id}/toggle-status", null);
            return RedirectToAction(nameof(AllBooks));
        }

        private async Task<BookListApiResult> GetBookListAsync(
            string? keyword,
            string? category,
            int? publisherId,
            int? publishYear,
            bool? isActive,
            string? availability,
            int? minRating,
            string? sort,
            int page)
        {
            var query = new List<string>
            {
                $"page={page}"
            };

            AddQuery(query, "keyword", keyword);
            AddQuery(query, "category", category);
            AddQuery(query, "publisherId", publisherId?.ToString());
            AddQuery(query, "publishYear", publishYear?.ToString());
            AddQuery(query, "isActive", isActive?.ToString().ToLowerInvariant());
            AddQuery(query, "availability", availability);
            AddQuery(query, "minRating", minRating?.ToString());
            AddQuery(query, "sort", sort);

            var client = httpClientFactory.CreateClient();
            return await client.GetFromJsonAsync<BookListApiResult>($"{GetApiBaseUrl()}/api/books?{string.Join("&", query)}")
                ?? new BookListApiResult();
        }

        private async Task LoadBookOptionsAsync()
        {
            var client = httpClientFactory.CreateClient();
            var options = await client.GetFromJsonAsync<BookOptionsApiResult>($"{GetApiBaseUrl()}/api/books/options")
                ?? new BookOptionsApiResult();

            ViewBag.Authors = options.Authors;
            ViewBag.Categories = options.Categories;
            ViewBag.Publishers = options.Publishers;
        }

        private async Task<string?> SaveCoverImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(imageFile.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var relativeFolder = Path.Combine("uploads", "books");
            var absoluteFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeFolder);
            Directory.CreateDirectory(absoluteFolder);

            var absolutePath = Path.Combine(absoluteFolder, fileName);
            await using var stream = System.IO.File.Create(absolutePath);
            await imageFile.CopyToAsync(stream);

            return "/" + relativeFolder.Replace("\\", "/") + "/" + fileName;
        }

        private string GetApiBaseUrl()
        {
            return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }

        private static void AddQuery(List<string> query, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                query.Add($"{name}={Uri.EscapeDataString(value)}");
            }
        }

    }
}
