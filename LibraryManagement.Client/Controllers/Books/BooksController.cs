using System.Net.Http.Json;
using LibraryManagement.Client.DTO.Books;
using LibraryManagement.Client.Helpers;
using LibraryManagementDAL.DTO.Book;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

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
            ApiActorHeaderHelper.AddActorHeaders(client, User);
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
            ApiActorHeaderHelper.AddActorHeaders(client, User);
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
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            await client.PostAsync($"{GetApiBaseUrl()}/api/books/{id}/toggle-status", null);
            return RedirectToAction(nameof(AllBooks));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public IActionResult DownloadTemplate()
        {
            ExcelPackage.License.SetNonCommercialPersonal("LMS Standard");
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Books");
            var headers = new[]
            {
                "Title",
                "ISBN",
                "Description",
                "PublishYear",
                "EditionNumber",
                "AuthorName",
                "CategoryName",
                "PublisherName",
                "PublisherAddress",
                "ImageUrl",
                "IsActive"
            };

            for (var i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                sheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            sheet.Cells[2, 1].Value = "Example Book";
            sheet.Cells[2, 2].Value = "9780000000000";
            sheet.Cells[2, 3].Value = "Short description";
            sheet.Cells[2, 4].Value = DateTime.UtcNow.Year;
            sheet.Cells[2, 5].Value = 1;
            sheet.Cells[2, 6].Value = "Author Name";
            sheet.Cells[2, 7].Value = "Category Name";
            sheet.Cells[2, 8].Value = "Publisher Name";
            sheet.Cells[2, 9].Value = "Publisher Address";
            sheet.Cells[2, 10].Value = "/uploads/books/example.jpg";
            sheet.Cells[2, 11].Value = "true";
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            return File(
                package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Books_Import_Template.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public async Task<IActionResult> Import(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ImportError"] = "Please choose an Excel file.";
                return RedirectToAction(nameof(AddBook), new { mode = "import" });
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ImportError"] = "Only .xlsx files are supported.";
                return RedirectToAction(nameof(AddBook), new { mode = "import" });
            }

            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);

            using var form = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            form.Add(fileContent, "file", file.FileName);

            var response = await client.PostAsync($"{GetApiBaseUrl()}/api/books/import", form);
            var result = await response.Content.ReadFromJsonAsync<BookImportApiResult>() ?? new BookImportApiResult();

            if (!response.IsSuccessStatusCode)
            {
                TempData["ImportError"] = result.Errors.FirstOrDefault() ?? "Import books failed.";
                return RedirectToAction(nameof(AddBook), new { mode = "import" });
            }

            TempData["ImportSuccess"] = $"Imported {result.ImportedCount} books. Skipped {result.SkippedCount} rows.";
            if (result.Errors.Count > 0)
            {
                TempData["ImportErrors"] = string.Join("\n", result.Errors.Take(20));
            }

            return RedirectToAction(nameof(AddBook), new { mode = "import" });
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
                $"page={page}",
                "pageSize=10"
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
