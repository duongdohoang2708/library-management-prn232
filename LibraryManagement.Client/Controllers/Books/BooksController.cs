using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.DTO.Book;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Client.Controllers.Books
{
    public class BooksController : Controller
    {
        private const int PageSize = 10;
        private readonly BookRepository bookRepository;

        public BooksController(BookRepository _bookRepository)
        {
            bookRepository = _bookRepository;
        }

        [HttpGet]
        public async Task<IActionResult> AllBooks(
            string? keyword,
            string? category,
            int? publisherId,
            int? publishYear,
            bool? isActive,
            string? sort,
            int page = 1)
        {
            page = page < 1 ? 1 : page;

            var query = bookRepository.QueryBooks();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var key = keyword.Trim();
                query = query.Where(b =>
                    b.Title.Contains(key) ||
                    b.ISBN.Contains(key) ||
                    b.Author.Name.Contains(key));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(b => b.Category.CategoryName == category);
            }

            if (publisherId.HasValue)
            {
                query = query.Where(b => b.PublisherId == publisherId.Value);
            }

            if (publishYear.HasValue)
            {
                query = query.Where(b => b.PublishYear == publishYear.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(b => b.IsActive == isActive.Value);
            }

            query = sort switch
            {
                "newest" => query.OrderByDescending(b => b.CreatedAt),
                "year" => query.OrderByDescending(b => b.PublishYear),
                "az" => query.OrderBy(b => b.Title),
                _ => query.OrderBy(b => b.Title)
            };

            var totalItems = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
            page = Math.Min(page, totalPages);

            var books = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            await LoadSearchOptionsAsync();
            ViewBag.Keyword = keyword;
            ViewBag.Category = category;
            ViewBag.PublisherId = publisherId;
            ViewBag.PublishYear = publishYear;
            ViewBag.IsActive = isActive;
            ViewBag.Sort = sort;
            ViewBag.TotalPages = totalPages;
            ViewBag.Request = new { Page = page };

            return View(books);
        }

        [HttpGet]
        public async Task<IActionResult> ViewBooks(
            string? keyword,
            string? category,
            int? publisherId,
            int? publishYear,
            bool? isActive,
            string? sort,
            int page = 1)
        {
            return await AllBooks(keyword, category, publisherId, publishYear, isActive, sort, page);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var book = await bookRepository.GetBookDetailsAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpGet]
        public async Task<IActionResult> AddBook(string mode = "single")
        {
            await LoadBookOptionsAsync();
            ViewBag.Mode = string.IsNullOrWhiteSpace(mode) ? "single" : mode;
            return View(new BookCreateModelRequest { IsActive = true, EditionNumber = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

            var authorId = model.AuthorId;
            if (!authorId.HasValue && !string.IsNullOrWhiteSpace(model.AuthorName))
            {
                var author = await bookRepository.CreateAuthorAsync(model.AuthorName);
                authorId = author.AuthorId;
            }

            if (!authorId.HasValue || !model.CategoryId.HasValue || !model.PublisherId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Please select author, category, and publisher.");
                return View(model);
            }

            var book = new Book
            {
                Title = model.Title.Trim(),
                Description = model.Description,
                PublishYear = model.PublishYear,
                EditionNumber = model.EditionNumber <= 0 ? 1 : model.EditionNumber,
                AuthorId = authorId.Value,
                CategoryId = model.CategoryId.Value,
                PublisherId = model.PublisherId.Value,
                ISBN = string.IsNullOrWhiteSpace(model.ISBN) ? GenerateIsbn() : model.ISBN.Trim(),
                ImageUrl = await SaveCoverImageAsync(imageFile),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var bookId = await bookRepository.CreateBookAsync(book);
            TempData["NewBookId"] = bookId;
            return RedirectToAction(nameof(AllBooks));
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var book = await bookRepository.GetBookByIdAsync(id);
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

            var book = await bookRepository.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            model.ImageUrl = book.ImageUrl ?? string.Empty;

            book.Title = model.Title.Trim();
            book.Description = model.Description;
            book.PublishYear = model.PublishYear;
            book.EditionNumber = model.EditionNumber <= 0 ? 1 : model.EditionNumber;
            book.AuthorId = model.AuthorId ?? book.AuthorId;
            book.CategoryId = model.CategoryId ?? book.CategoryId;
            book.PublisherId = model.PublisherId ?? book.PublisherId;
            book.ISBN = string.IsNullOrWhiteSpace(model.ISBN) ? book.ISBN : model.ISBN.Trim();
            book.IsActive = model.IsActive;
            book.UpdatedAt = DateTime.UtcNow;

            var imageUrl = await SaveCoverImageAsync(imageFile);
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                book.ImageUrl = imageUrl;
            }

            await bookRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = book.BookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var book = await bookRepository.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            book.IsActive = !book.IsActive;
            book.UpdatedAt = DateTime.UtcNow;
            await bookRepository.SaveChangesAsync();

            return RedirectToAction(nameof(AllBooks));
        }

        private async Task LoadSearchOptionsAsync()
        {
            ViewBag.Categories = await bookRepository.GetCategoriesAsync();
            ViewBag.Publishers = await bookRepository.GetPublishersAsync();
            ViewBag.PublishYears = await bookRepository.GetPublishYearsAsync();
        }

        private async Task LoadBookOptionsAsync()
        {
            ViewBag.Authors = await bookRepository.GetAuthorsAsync();
            ViewBag.Categories = await bookRepository.GetCategoriesAsync();
            ViewBag.Publishers = await bookRepository.GetPublishersAsync();
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

        private static string GenerateIsbn()
        {
            return DateTime.UtcNow.Ticks.ToString();
        }
    }
}
