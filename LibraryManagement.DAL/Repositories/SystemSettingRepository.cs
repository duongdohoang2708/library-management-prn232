using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class SystemSettingRepository
    {
        private readonly ApplicationDbContext db;

        public SystemSettingRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public async Task<List<SystemSetting>> GetAllAsync()
        {
            return await db.SystemSettings.OrderBy(x => x.Key).ToListAsync();
        }

        public async Task<SystemSetting?> GetByKeyAsync(string key)
        {
            return await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == key);
        }

        public void Add(SystemSetting setting)
        {
            db.SystemSettings.Add(setting);
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
