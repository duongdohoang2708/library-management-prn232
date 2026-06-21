using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class ReviewRepository
    {
        private readonly ApplicationDbContext db;

        public ReviewRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public async Task<Book?> GetBookAsync(int bookId)
        {
            return await db.Books
                .Include(x => x.Author)
                .FirstOrDefaultAsync(x => x.BookId == bookId && x.IsActive);
        }

        public async Task<Account?> GetMemberAccountAsync(int userId)
        {
            return await db.Accounts
                .Include(x => x.Member)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);
        }

        public async Task<List<BookReview>> GetBookReviewsAsync(int bookId)
        {
            return await db.BookReviews
                .Include(x => x.Account)
                .Where(x => x.BookId == bookId)
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ToListAsync();
        }

        public async Task<BookReview?> GetUserReviewAsync(int userId, int bookId)
        {
            return await db.BookReviews
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.BookId == bookId);
        }

        public async Task<bool> HasBorrowedBookAsync(int userId, int bookId)
        {
            return await db.BorrowDetails.AnyAsync(x =>
                x.BorrowTransaction.UserId == userId &&
                x.BookCopy != null &&
                x.BookCopy.BookId == bookId);
        }

        public void Add(BookReview review)
        {
            db.BookReviews.Add(review);
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
