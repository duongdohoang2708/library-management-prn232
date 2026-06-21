using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class ReminderRepository
    {
        private readonly ApplicationDbContext db;

        public ReminderRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public async Task<List<BorrowDetail>> GetOpenBorrowDetailsAsync()
        {
            return await db.BorrowDetails
                .Include(x => x.BorrowTransaction)
                    .ThenInclude(x => x.Account)
                .Include(x => x.BookCopy)
                    .ThenInclude(x => x!.Book)
                .Where(x => x.ActualReturnDate == null && x.BookCopy != null)
                .ToListAsync();
        }

        public async Task<bool> HasReminderAsync(int borrowDetailId, string reminderType, DateTime reminderDate)
        {
            return await db.ReminderLogs.AnyAsync(x =>
                x.BorrowDetailId == borrowDetailId &&
                x.ReminderType == reminderType &&
                x.ReminderDate == reminderDate.Date);
        }

        public void AddReminder(ReminderLog reminderLog)
        {
            db.ReminderLogs.Add(reminderLog);
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
