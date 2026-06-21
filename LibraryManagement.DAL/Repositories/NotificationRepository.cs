using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class NotificationRepository
    {
        private readonly ApplicationDbContext db;

        public NotificationRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public IQueryable<Notification> QueryNotifications()
        {
            return db.Notification
                .Include(x => x.Account)
                .AsQueryable();
        }

        public async Task<Account?> GetAccountAsync(int userId)
        {
            return await db.Accounts.FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);
        }

        public void Add(Notification notification)
        {
            db.Notification.Add(notification);
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await db.Notification
                .Where(x => x.UserId == userId && !x.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
