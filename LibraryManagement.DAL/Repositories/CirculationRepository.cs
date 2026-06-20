using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class CirculationRepository
    {
        private readonly ApplicationDbContext db;

        public CirculationRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public IQueryable<BorrowTransaction> QueryTransactions()
        {
            return db.BorrowTransactions
                .Include(x => x.Account)
                .Include(x => x.BorrowDetails)
                    .ThenInclude(x => x.BookCopy)
                        .ThenInclude(x => x!.Book)
                            .ThenInclude(x => x.Author)
                .AsSplitQuery()
                .AsQueryable();
        }

        public IQueryable<BorrowTransaction> QueryTransactionsForStatusUpdate()
        {
            return db.BorrowTransactions
                .Include(x => x.BorrowDetails)
                .AsQueryable();
        }

        public async Task<Account?> GetAccountAsync(int userId)
        {
            return await db.Accounts.FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);
        }

        public async Task<Account?> GetMemberAccountAsync(int userId)
        {
            return await db.Accounts
                .Include(x => x.Member)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);
        }

        public async Task<int> CountOpenBorrowedBooksAsync(int userId)
        {
            return await db.BorrowDetails
                .Where(x =>
                    x.BorrowTransaction.UserId == userId &&
                    x.ActualReturnDate == null)
                .CountAsync();
        }

        public async Task<BookCopy?> GetFirstAvailableCopyByBookIdAsync(int bookId)
        {
            return await db.BookCopies
                .Include(x => x.Book)
                .Where(x => x.BookId == bookId && x.Status == BookCopyStatus.Available)
                .OrderBy(x => x.Barcode)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Account>> SearchAccountsAsync(string query)
        {
            query = query.Trim();

            var accounts = db.Accounts.Where(x => x.IsActive);
            if (int.TryParse(query, out var userId))
            {
                accounts = accounts.Where(x => x.UserId == userId);
            }
            else
            {
                accounts = accounts.Where(x =>
                    x.FullName.Contains(query) ||
                    x.Email.Contains(query) ||
                    x.Username.Contains(query));
            }

            return await accounts
                .OrderBy(x => x.FullName)
                .Take(10)
                .ToListAsync();
        }

        public async Task<List<BookCopy>> GetAvailableCopiesAsync()
        {
            return await db.BookCopies
                .Include(x => x.Book)
                    .ThenInclude(x => x.Author)
                .Where(x => x.Status == BookCopyStatus.Available)
                .OrderBy(x => x.Book.Title)
                .ThenBy(x => x.Barcode)
                .ToListAsync();
        }

        public async Task<List<BookCopy>> GetCopiesByBarcodesAsync(IEnumerable<string> barcodes)
        {
            var barcodeList = barcodes.Select(x => x.Trim()).Where(x => x != string.Empty).Distinct().ToList();

            return await db.BookCopies
                .Include(x => x.Book)
                .Where(x => barcodeList.Contains(x.Barcode))
                .ToListAsync();
        }

        public async Task<BorrowTransaction?> GetTransactionAsync(int transactionId)
        {
            return await QueryTransactions()
                .FirstOrDefaultAsync(x => x.BorrowTransactionId == transactionId);
        }

        public async Task<BorrowDetail?> GetBorrowDetailAsync(int borrowDetailId)
        {
            return await db.BorrowDetails
                .Include(x => x.BorrowTransaction)
                    .ThenInclude(x => x.BorrowDetails)
                .Include(x => x.BookCopy)
                    .ThenInclude(x => x!.Book)
                .FirstOrDefaultAsync(x => x.BorrowDetailId == borrowDetailId);
        }

        public void AddTransaction(BorrowTransaction transaction)
        {
            db.BorrowTransactions.Add(transaction);
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
