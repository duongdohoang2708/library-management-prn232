using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class AuditLogRepository
    {
        private readonly ApplicationDbContext db;

        public AuditLogRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public IQueryable<AuditLog> Query()
        {
            return db.AuditLogs.AsNoTracking().AsQueryable();
        }

        public async Task AddAsync(AuditLog auditLog)
        {
            db.AuditLogs.Add(auditLog);
            await db.SaveChangesAsync();
        }
    }
}
