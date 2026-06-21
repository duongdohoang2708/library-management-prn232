using LibraryManagement.BLL.DTO.Books;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.BLL.Services
{
    public class BookCatalogService
    {
        private const int PageSize = 10;
        private readonly BookRepository bookRepository;
        private readonly AuditLogService auditLogService;

        public BookCatalogService(BookRepository _bookRepository, AuditLogService _auditLogService)
        {
            bookRepository = _bookRepository;
            auditLogService = _auditLogService;
        }

        public async Task<BookListResult> GetBooksAsync(
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

            if (!string.IsNullOrWhiteSpace(availability))
            {
                query = availability.Trim().ToLowerInvariant() switch
                {
                    "available" => query.Where(b => b.BookCopies.Any(c => c.Status == BookCopyStatus.Available)),
                    "borrowed" => query.Where(b => b.BookCopies.Any(c => c.Status == BookCopyStatus.Borrowed)),
                    "reserved" => query.Where(b => b.BookCopies.Any(c => c.Status == BookCopyStatus.Reserved)),
                    "lost" => query.Where(b => b.BookCopies.Any(c => c.Status == BookCopyStatus.Lost)),
                    "damaged" => query.Where(b => b.BookCopies.Any(c => c.Status == BookCopyStatus.Damaged)),
                    "unavailable" => query.Where(b => !b.BookCopies.Any(c => c.Status == BookCopyStatus.Available)),
                    _ => query
                };
            }

            if (minRating.HasValue)
            {
                query = query.Where(b => b.BookReviews.Any() && b.BookReviews.Average(r => r.Rating) >= minRating.Value);
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

            return new BookListResult
            {
                Items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
                TotalPages = totalPages,
                Page = page,
                Categories = await bookRepository.GetCategoriesAsync(),
                Publishers = await bookRepository.GetPublishersAsync(),
                PublishYears = await bookRepository.GetPublishYearsAsync()
            };
        }

        public async Task<Book?> GetBookDetailsAsync(int id)
        {
            return await bookRepository.GetBookDetailsAsync(id);
        }

        public IQueryable<Book> QueryBooks()
        {
            return bookRepository.QueryBooks();
        }

        public IQueryable<Author> QueryAuthors()
        {
            return bookRepository.QueryAuthors();
        }

        public IQueryable<Category> QueryCategories()
        {
            return bookRepository.QueryCategories();
        }

        public IQueryable<Publisher> QueryPublishers()
        {
            return bookRepository.QueryPublishers();
        }

        public async Task<BookOptionsResult> GetOptionsAsync()
        {
            return new BookOptionsResult
            {
                Authors = await bookRepository.GetAuthorsAsync(),
                Categories = await bookRepository.GetCategoriesAsync(),
                Publishers = await bookRepository.GetPublishersAsync()
            };
        }

        public async Task<int?> CreateAsync(BookCreateRequest request)
        {
            var authorId = request.AuthorId;
            if (!authorId.HasValue && !string.IsNullOrWhiteSpace(request.AuthorName))
            {
                var author = await bookRepository.CreateAuthorAsync(request.AuthorName);
                authorId = author.AuthorId;
            }

            if (!authorId.HasValue || !request.CategoryId.HasValue || !request.PublisherId.HasValue)
            {
                return null;
            }

            var book = new Book
            {
                Title = request.Title.Trim(),
                Description = request.Description,
                PublishYear = request.PublishYear,
                EditionNumber = request.EditionNumber <= 0 ? 1 : request.EditionNumber,
                AuthorId = authorId.Value,
                CategoryId = request.CategoryId.Value,
                PublisherId = request.PublisherId.Value,
                ISBN = string.IsNullOrWhiteSpace(request.ISBN) ? GenerateIsbn() : request.ISBN.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var bookId = await bookRepository.CreateBookAsync(book);
            await auditLogService.LogAsync("CreateBook", "Book", bookId.ToString(), $"Book \"{book.Title}\" created.");
            return bookId;
        }

        public async Task<int?> UpdateAsync(int id, BookUpdateRequest request)
        {
            if (id != request.BookId)
            {
                return null;
            }

            var book = await bookRepository.GetBookByIdAsync(id);
            if (book == null)
            {
                return null;
            }

            book.Title = request.Title.Trim();
            book.Description = request.Description;
            book.PublishYear = request.PublishYear;
            book.EditionNumber = request.EditionNumber <= 0 ? 1 : request.EditionNumber;
            book.AuthorId = request.AuthorId ?? book.AuthorId;
            book.CategoryId = request.CategoryId ?? book.CategoryId;
            book.PublisherId = request.PublisherId ?? book.PublisherId;
            book.ISBN = string.IsNullOrWhiteSpace(request.ISBN) ? book.ISBN : request.ISBN.Trim();
            book.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? book.ImageUrl : request.ImageUrl;
            book.IsActive = request.IsActive;
            book.UpdatedAt = DateTime.UtcNow;

            await bookRepository.SaveChangesAsync();
            await auditLogService.LogAsync("UpdateBook", "Book", book.BookId.ToString(), $"Book \"{book.Title}\" updated.");
            return book.BookId;
        }

        public async Task<(int BookId, bool IsActive)?> ToggleStatusAsync(int id)
        {
            var book = await bookRepository.GetBookByIdAsync(id);
            if (book == null)
            {
                return null;
            }

            book.IsActive = !book.IsActive;
            book.UpdatedAt = DateTime.UtcNow;
            await bookRepository.SaveChangesAsync();
            await auditLogService.LogAsync("ToggleBookStatus", "Book", book.BookId.ToString(), $"Book \"{book.Title}\" status changed to {(book.IsActive ? "Active" : "Inactive")}.");

            return (book.BookId, book.IsActive);
        }

        private static string GenerateIsbn()
        {
            return DateTime.UtcNow.Ticks.ToString();
        }
    }
}
