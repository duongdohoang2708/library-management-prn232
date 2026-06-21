using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class BookRepository
    {
        private readonly ApplicationDbContext db;

        public BookRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public IQueryable<Book> QueryBooks()
        {
            return db.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .AsQueryable();
        }

        public IQueryable<Author> QueryAuthors()
        {
            return db.Authors.AsQueryable();
        }

        public IQueryable<Category> QueryCategories()
        {
            return db.Categories.AsQueryable();
        }

        public IQueryable<Publisher> QueryPublishers()
        {
            return db.Publishers.AsQueryable();
        }

        public async Task<Book?> GetBookDetailsAsync(int bookId)
        {
            return await db.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookCopies)
                .Include(b => b.BookReviews)
                .FirstOrDefaultAsync(b => b.BookId == bookId);
        }

        public async Task<Book?> GetBookByIdAsync(int bookId)
        {
            return await db.Books.FindAsync(bookId);
        }

        public async Task<List<Author>> GetAuthorsAsync()
        {
            return await db.Authors.OrderBy(a => a.Name).ToListAsync();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await db.Categories.OrderBy(c => c.CategoryName).ToListAsync();
        }

        public async Task<List<Publisher>> GetPublishersAsync()
        {
            return await db.Publishers.OrderBy(p => p.PublisherName).ToListAsync();
        }

        public async Task<List<int>> GetPublishYearsAsync()
        {
            return await db.Books
                .Where(b => b.PublishYear.HasValue)
                .Select(b => b.PublishYear!.Value)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }

        public async Task<Author> CreateAuthorAsync(string authorName)
        {
            var author = new Author
            {
                Name = authorName.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            db.Authors.Add(author);
            await db.SaveChangesAsync();
            return author;
        }

        public async Task<Author?> GetAuthorByNameAsync(string authorName)
        {
            var normalized = authorName.Trim().ToLower();
            return await db.Authors.FirstOrDefaultAsync(a => a.Name.ToLower() == normalized);
        }

        public async Task<Category?> GetCategoryByNameAsync(string categoryName)
        {
            var normalized = categoryName.Trim().ToLower();
            return await db.Categories.FirstOrDefaultAsync(c => c.CategoryName.ToLower() == normalized);
        }

        public async Task<Publisher?> GetPublisherByNameAsync(string publisherName)
        {
            var normalized = publisherName.Trim().ToLower();
            return await db.Publishers.FirstOrDefaultAsync(p => p.PublisherName.ToLower() == normalized);
        }

        public async Task<bool> IsbnExistsAsync(string isbn)
        {
            var normalized = isbn.Trim().ToLower();
            return await db.Books.AnyAsync(b => b.ISBN.ToLower() == normalized);
        }

        public void AddAuthor(Author author)
        {
            db.Authors.Add(author);
        }

        public void AddCategory(Category category)
        {
            db.Categories.Add(category);
        }

        public void AddPublisher(Publisher publisher)
        {
            db.Publishers.Add(publisher);
        }

        public void AddBook(Book book)
        {
            db.Books.Add(book);
        }

        public async Task<int> CreateBookAsync(Book book)
        {
            db.Books.Add(book);
            await db.SaveChangesAsync();
            return book.BookId;
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }

        public async Task<BookAISummary?> GetBookSummaryAsync(int bookId)
        {
            return await db.BookAISummary.FirstOrDefaultAsync(s => s.BookId == bookId);
        }

        public async Task SaveBookSummaryAsync(BookAISummary summary)
        {
            var existing = await db.BookAISummary.FirstOrDefaultAsync(s => s.BookId == summary.BookId);
            if (existing != null)
            {
                existing.SummaryText = summary.SummaryText;
                existing.ModelName = summary.ModelName;
                existing.TokensUsed = summary.TokensUsed;
                db.BookAISummary.Update(existing);
            }
            else
            {
                db.BookAISummary.Add(summary);
            }
            await db.SaveChangesAsync();
        }

        public async Task LogAIRequestAsync(AIRequestLog log)
        {
            db.AIRequestLogs.Add(log);
            await db.SaveChangesAsync();
        }
    }
}
