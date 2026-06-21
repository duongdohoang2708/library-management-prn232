using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class CatalogManagementRepository
    {
        private readonly ApplicationDbContext db;

        public CatalogManagementRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public async Task<List<Author>> GetAuthorsAsync()
        {
            return await db.Authors.OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await db.Categories.OrderBy(x => x.CategoryName).ToListAsync();
        }

        public async Task<List<Publisher>> GetPublishersAsync()
        {
            return await db.Publishers.OrderBy(x => x.PublisherName).ToListAsync();
        }

        public async Task<Author?> GetAuthorAsync(int id)
        {
            return await db.Authors.FirstOrDefaultAsync(x => x.AuthorId == id);
        }

        public async Task<Category?> GetCategoryAsync(int id)
        {
            return await db.Categories.FirstOrDefaultAsync(x => x.CategoryId == id);
        }

        public async Task<Publisher?> GetPublisherAsync(int id)
        {
            return await db.Publishers.FirstOrDefaultAsync(x => x.PublisherId == id);
        }

        public async Task<bool> AuthorNameExistsAsync(string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await db.Authors.AnyAsync(x => x.Name.ToLower() == normalized && (!excludeId.HasValue || x.AuthorId != excludeId.Value));
        }

        public async Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await db.Categories.AnyAsync(x => x.CategoryName.ToLower() == normalized && (!excludeId.HasValue || x.CategoryId != excludeId.Value));
        }

        public async Task<bool> PublisherNameExistsAsync(string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await db.Publishers.AnyAsync(x => x.PublisherName.ToLower() == normalized && (!excludeId.HasValue || x.PublisherId != excludeId.Value));
        }

        public async Task<bool> AuthorHasBooksAsync(int id)
        {
            return await db.Books.AnyAsync(x => x.AuthorId == id);
        }

        public async Task<bool> CategoryHasBooksAsync(int id)
        {
            return await db.Books.AnyAsync(x => x.CategoryId == id);
        }

        public async Task<bool> PublisherHasBooksAsync(int id)
        {
            return await db.Books.AnyAsync(x => x.PublisherId == id);
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

        public void RemoveAuthor(Author author)
        {
            db.Authors.Remove(author);
        }

        public void RemoveCategory(Category category)
        {
            db.Categories.Remove(category);
        }

        public void RemovePublisher(Publisher publisher)
        {
            db.Publishers.Remove(publisher);
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
