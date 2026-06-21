using LibraryManagement.DAL.Data;
using LibraryManagementDAL.DTO.Reports;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class ReportRepository
    {
        private readonly ApplicationDbContext db;

        public ReportRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public async Task<List<MostBorrowedBookItem>> GetMostBorrowedBooksAsync(DateTime from, DateTime to)
        {
            return await db.BorrowDetails
                .AsNoTracking()
                .Where(x => x.BookCopy != null && x.BorrowDate >= from && x.BorrowDate <= to)
                .GroupBy(x => new
                {
                    x.BookCopy!.BookId,
                    x.BookCopy.Book.Title,
                    AuthorName = x.BookCopy.Book.Author.Name
                })
                .Select(g => new MostBorrowedBookItem
                {
                    BookId = g.Key.BookId,
                    Title = g.Key.Title,
                    AuthorName = g.Key.AuthorName,
                    BorrowCount = g.Count()
                })
                .OrderByDescending(x => x.BorrowCount)
                .ThenBy(x => x.Title)
                .Take(10)
                .ToListAsync();
        }

        public async Task<List<OverdueUserItem>> GetTopOverdueUsersAsync(DateTime from, DateTime to)
        {
            return await db.BorrowDetails
                .AsNoTracking()
                .Where(x => x.DueDate >= from && x.DueDate <= to && (x.ActualReturnDate == null || x.ActualReturnDate > x.DueDate))
                .GroupBy(x => new
                {
                    x.BorrowTransaction.UserId,
                    x.BorrowTransaction.Account.FullName,
                    x.BorrowTransaction.Account.Email
                })
                .Select(g => new OverdueUserItem
                {
                    UserId = g.Key.UserId,
                    FullName = g.Key.FullName,
                    Email = g.Key.Email,
                    OverdueCount = g.Count(),
                    TotalFine = g.Sum(x => x.FineAmount ?? 0)
                })
                .OrderByDescending(x => x.OverdueCount)
                .ThenByDescending(x => x.TotalFine)
                .Take(10)
                .ToListAsync();
        }

        public async Task<List<FineRevenueMonthItem>> GetFineRevenueByMonthAsync(DateTime from, DateTime to)
        {
            var payments = await db.Payments
                .AsNoTracking()
                .Where(x => x.PaymentStatus == PaymentStatus.Success && (x.PaidAt ?? x.CreatedAt) >= from && (x.PaidAt ?? x.CreatedAt) <= to)
                .GroupBy(x => new
                {
                    Year = (x.PaidAt ?? x.CreatedAt).Year,
                    Month = (x.PaidAt ?? x.CreatedAt).Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Amount = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return payments
                .Select(x => new FineRevenueMonthItem
                {
                    Month = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
                    Amount = x.Amount
                })
                .ToList();
        }
    }
}
