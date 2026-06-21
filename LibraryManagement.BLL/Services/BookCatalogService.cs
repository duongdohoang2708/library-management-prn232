using LibraryManagement.BLL.DTO.Books;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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

        public async Task<BookImportResult> ImportAsync(Stream stream)
        {
            var result = new BookImportResult();
            var importIsbns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ExcelPackage.License.SetNonCommercialPersonal("LMS Standard");

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet?.Dimension == null)
            {
                result.Errors.Add("The Excel file is empty.");
                return result;
            }

            var rowCount = worksheet.Dimension.End.Row;
            for (var row = 2; row <= rowCount; row++)
            {
                var title = GetCell(worksheet, row, 1);
                var isbn = GetCell(worksheet, row, 2);
                var description = GetCell(worksheet, row, 3);
                var publishYearText = GetCell(worksheet, row, 4);
                var editionText = GetCell(worksheet, row, 5);
                var authorName = GetCell(worksheet, row, 6);
                var categoryName = GetCell(worksheet, row, 7);
                var publisherName = GetCell(worksheet, row, 8);
                var publisherAddress = GetCell(worksheet, row, 9);
                var imageUrl = GetCell(worksheet, row, 10);
                var isActiveText = GetCell(worksheet, row, 11);

                if (string.IsNullOrWhiteSpace(title)
                    && string.IsNullOrWhiteSpace(isbn)
                    && string.IsNullOrWhiteSpace(authorName)
                    && string.IsNullOrWhiteSpace(categoryName)
                    && string.IsNullOrWhiteSpace(publisherName))
                {
                    continue;
                }

                var rowErrors = new List<string>();
                if (string.IsNullOrWhiteSpace(title))
                {
                    rowErrors.Add("Title is required");
                }

                if (string.IsNullOrWhiteSpace(authorName))
                {
                    rowErrors.Add("AuthorName is required");
                }

                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    rowErrors.Add("CategoryName is required");
                }

                if (string.IsNullOrWhiteSpace(publisherName))
                {
                    rowErrors.Add("PublisherName is required");
                }

                int? publishYear = null;
                if (!string.IsNullOrWhiteSpace(publishYearText))
                {
                    if (!int.TryParse(publishYearText, out var parsedYear) || parsedYear < 1000 || parsedYear > DateTime.UtcNow.Year + 1)
                    {
                        rowErrors.Add("PublishYear is invalid");
                    }
                    else
                    {
                        publishYear = parsedYear;
                    }
                }

                var editionNumber = 1;
                if (!string.IsNullOrWhiteSpace(editionText)
                    && (!int.TryParse(editionText, out editionNumber) || editionNumber <= 0))
                {
                    rowErrors.Add("EditionNumber must be greater than 0");
                }

                if (!string.IsNullOrWhiteSpace(isbn) && !importIsbns.Add(isbn.Trim()))
                {
                    rowErrors.Add($"ISBN \"{isbn}\" is duplicated in this file");
                }
                else if (!string.IsNullOrWhiteSpace(isbn) && await bookRepository.IsbnExistsAsync(isbn))
                {
                    rowErrors.Add($"ISBN \"{isbn}\" already exists");
                }

                if (rowErrors.Count > 0)
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Row {row}: {string.Join("; ", rowErrors)}.");
                    continue;
                }

                var author = await bookRepository.GetAuthorByNameAsync(authorName);
                if (author == null)
                {
                    author = new Author
                    {
                        Name = authorName.Trim(),
                        CreatedAt = DateTime.UtcNow
                    };
                    bookRepository.AddAuthor(author);
                }

                var category = await bookRepository.GetCategoryByNameAsync(categoryName);
                if (category == null)
                {
                    category = new Category
                    {
                        CategoryName = categoryName.Trim(),
                        Description = string.Empty,
                        CreatedAt = DateTime.UtcNow
                    };
                    bookRepository.AddCategory(category);
                }

                var publisher = await bookRepository.GetPublisherByNameAsync(publisherName);
                if (publisher == null)
                {
                    publisher = new Publisher
                    {
                        PublisherName = publisherName.Trim(),
                        Address = publisherAddress?.Trim() ?? string.Empty,
                        CreatedAt = DateTime.UtcNow
                    };
                    bookRepository.AddPublisher(publisher);
                }

                bookRepository.AddBook(new Book
                {
                    Title = title.Trim(),
                    ISBN = string.IsNullOrWhiteSpace(isbn) ? GenerateIsbn() : isbn.Trim(),
                    Description = description,
                    PublishYear = publishYear,
                    EditionNumber = editionNumber,
                    Author = author,
                    Category = category,
                    Publisher = publisher,
                    ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim(),
                    IsActive = ParseBool(isActiveText, true),
                    CreatedAt = DateTime.UtcNow
                });

                result.ImportedCount++;
            }

            if (result.ImportedCount > 0)
            {
                await bookRepository.SaveChangesAsync();
                await auditLogService.LogAsync("ImportBooks", "Book", null, $"{result.ImportedCount} books imported from Excel.");
            }

            return result;
        }

        private static string GenerateIsbn()
        {
            return DateTime.UtcNow.Ticks.ToString();
        }

        private static string GetCell(ExcelWorksheet worksheet, int row, int column)
        {
            return worksheet.Cells[row, column].Text?.Trim() ?? string.Empty;
        }

        private static bool ParseBool(string value, bool defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return value.Trim().ToLowerInvariant() switch
            {
                "true" or "1" or "yes" or "y" or "active" => true,
                "false" or "0" or "no" or "n" or "inactive" => false,
                _ => defaultValue
            };
        }
    }
}
