using LibraryManagement.DAL.Data;
using LibraryManagementDAL.DTO.Dashboard;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class DashboardRepository
    {
        private readonly ApplicationDbContext db;

        public DashboardRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public async Task<DashboardStatsResult> GetStatsAsync()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
            var result = new DashboardStatsResult
            {
                TotalBooks = await db.Books.AsNoTracking().CountAsync(),
                TotalUsers = await db.Accounts.AsNoTracking().CountAsync(),
                TotalTransactions = await db.BorrowTransactions.AsNoTracking().CountAsync(),
                TotalActiveBorrows = await db.BorrowTransactions.AsNoTracking()
                    .CountAsync(x => x.Status == "Borrowing" || x.Status == "Overdue"),
                OverdueBooksCount = await db.BorrowDetails.AsNoTracking()
                    .CountAsync(x => x.ActualReturnDate == null && x.DueDate.Date < now.Date),
                TotalUnpaidFines = await db.BorrowDetails.AsNoTracking()
                    .Where(x => (x.FineAmount ?? 0) > (x.FinePaidAmount ?? 0))
                    .SumAsync(x => (x.FineAmount ?? 0) - (x.FinePaidAmount ?? 0))
            };

            var borrowGroups = await db.BorrowTransactions.AsNoTracking()
                .Where(x => x.BorrowDate >= monthStart)
                .GroupBy(x => new { x.BorrowDate.Year, x.BorrowDate.Month })
                .Select(g => new MonthCount(g.Key.Year, g.Key.Month, g.Count()))
                .ToListAsync();

            var returnGroups = await db.BorrowDetails.AsNoTracking()
                .Where(x => x.ActualReturnDate != null && x.ActualReturnDate >= monthStart)
                .GroupBy(x => new { x.ActualReturnDate!.Value.Year, x.ActualReturnDate.Value.Month })
                .Select(g => new MonthCount(g.Key.Year, g.Key.Month, g.Count()))
                .ToListAsync();

            var registrationGroups = await db.Accounts.AsNoTracking()
                .Where(x => x.CreatedAt >= monthStart)
                .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                .Select(g => new MonthCount(g.Key.Year, g.Key.Month, g.Count()))
                .ToListAsync();

            FillMonthlySeries(result, monthStart, borrowGroups, returnGroups, registrationGroups);

            result.CategoryStats = await db.Books.AsNoTracking()
                .GroupBy(x => x.Category.CategoryName)
                .Select(g => new DashboardNameCount
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Label)
                .Take(8)
                .ToListAsync();

            result.BookStatusStats = (await db.BookCopies.AsNoTracking()
                    .GroupBy(x => x.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync())
                .OrderBy(x => (int)x.Status)
                .Select(x => new DashboardNameCount
                {
                    Label = x.Status.ToString(),
                    Count = x.Count
                })
                .ToList();

            result.RecentUsers = await db.Accounts.AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.UserId)
                .Take(5)
                .Select(x => new DashboardUserItem
                {
                    UserId = x.UserId,
                    FullName = x.FullName,
                    Email = x.Email,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            result.RecentPayments = await db.Payments.AsNoTracking()
                .Where(x => x.PaymentStatus == PaymentStatus.Success)
                .OrderByDescending(x => x.PaidAt ?? x.CreatedAt)
                .ThenByDescending(x => x.PaymentId)
                .Take(5)
                .Select(x => new DashboardPaymentItem
                {
                    PaymentId = x.PaymentId,
                    UserId = x.UserId,
                    UserFullName = x.Account.FullName,
                    Amount = x.Amount,
                    CreatedAt = x.CreatedAt,
                    PaidAt = x.PaidAt,
                    PaymentStatus = x.PaymentStatus.ToString()
                })
                .ToListAsync();

            result.RecentTransactions = await db.BorrowTransactions.AsNoTracking()
                .OrderByDescending(x => x.BorrowDate)
                .ThenByDescending(x => x.BorrowTransactionId)
                .Take(8)
                .Select(x => new DashboardTransactionItem
                {
                    BorrowTransactionId = x.BorrowTransactionId,
                    UserId = x.UserId,
                    UserFullName = x.Account.FullName,
                    BorrowDate = x.BorrowDate,
                    DueDate = x.DueDate,
                    Status = x.Status
                })
                .ToListAsync();

            result.TopDebtors = await db.BorrowDetails.AsNoTracking()
                .Where(x => (x.FineAmount ?? 0) > (x.FinePaidAmount ?? 0))
                .GroupBy(x => new
                {
                    x.BorrowTransaction.UserId,
                    x.BorrowTransaction.Account.FullName,
                    x.BorrowTransaction.Account.Email
                })
                .Select(g => new DashboardDebtorItem
                {
                    UserId = g.Key.UserId,
                    FullName = g.Key.FullName,
                    Email = g.Key.Email,
                    TotalUnpaidFine = g.Sum(x => (x.FineAmount ?? 0) - (x.FinePaidAmount ?? 0)),
                    UnpaidBookCount = g.Count()
                })
                .OrderByDescending(x => x.TotalUnpaidFine)
                .ThenBy(x => x.FullName)
                .Take(5)
                .ToListAsync();

            return result;
        }

        private static void FillMonthlySeries(
            DashboardStatsResult result,
            DateTime monthStart,
            List<MonthCount> borrowGroups,
            List<MonthCount> returnGroups,
            List<MonthCount> registrationGroups)
        {
            for (var i = 0; i < 6; i++)
            {
                var month = monthStart.AddMonths(i);
                result.MonthlyLabels.Add(month.ToString("MMM yyyy"));
                result.MonthlyBorrows.Add(FindMonthCount(borrowGroups, month));
                result.MonthlyReturns.Add(FindMonthCount(returnGroups, month));
                result.MonthlyRegistrations.Add(FindMonthCount(registrationGroups, month));
            }
        }

        private static int FindMonthCount(List<MonthCount> source, DateTime month)
        {
            return source.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Count ?? 0;
        }

        private sealed class MonthCount
        {
            public MonthCount(int year, int month, int count)
            {
                Year = year;
                Month = month;
                Count = count;
            }

            public int Year { get; }
            public int Month { get; }
            public int Count { get; }
        }
    }
}
