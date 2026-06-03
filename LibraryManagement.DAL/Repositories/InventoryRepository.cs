using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class InventoryRepository
    {
        private readonly ApplicationDbContext db;

        public InventoryRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public IQueryable<BookCopy> QueryCopies()
        {
            return db.BookCopies
                .Include(c => c.Book)
                .AsQueryable();
        }

        public async Task<Book?> GetBookByIdAsync(int bookId)
        {
            return await db.Books.FirstOrDefaultAsync(b => b.BookId == bookId);
        }

        public async Task<List<BookCopy>> GetCopiesByBookIdAsync(int bookId)
        {
            return await db.BookCopies
                .Where(c => c.BookId == bookId)
                .OrderBy(c => c.Barcode)
                .ToListAsync();
        }

        public async Task<BookCopy?> GetCopyByIdAsync(int copyId)
        {
            return await db.BookCopies.FirstOrDefaultAsync(c => c.BookCopyId == copyId);
        }

        public void AddCopy(BookCopy copy)
        {
            db.BookCopies.Add(copy);
        }

        public async Task<int> GetNextCopyNumberAsync()
        {
            var lastBarcode = await db.BookCopies
                .Where(c => c.Barcode.StartsWith("BC"))
                .OrderByDescending(c => c.BookCopyId)
                .Select(c => c.Barcode)
                .FirstOrDefaultAsync();

            if (lastBarcode != null && int.TryParse(lastBarcode[2..], out var number))
            {
                return number + 1;
            }

            return 1;
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
