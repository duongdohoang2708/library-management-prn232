using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class RenewalRequestRepository
    {
        private readonly ApplicationDbContext db;

        public RenewalRequestRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public IQueryable<RenewalRequest> QueryRequests()
        {
            return db.RenewalRequests
                .Include(x => x.Account)
                .Include(x => x.ReviewedBy)
                .Include(x => x.BorrowDetail)
                    .ThenInclude(x => x.BookCopy)
                        .ThenInclude(x => x!.Book)
                .AsQueryable();
        }

        public async Task<RenewalRequest?> GetRequestAsync(int renewalRequestId)
        {
            return await QueryRequests()
                .FirstOrDefaultAsync(x => x.RenewalRequestId == renewalRequestId);
        }

        public async Task<bool> HasPendingRequestAsync(int borrowDetailId)
        {
            return await db.RenewalRequests.AnyAsync(x =>
                x.BorrowDetailId == borrowDetailId &&
                x.Status == RenewalRequestStatus.Pending);
        }

        public async Task<RenewalRequest?> GetPendingRequestForDetailAsync(int borrowDetailId)
        {
            return await QueryRequests()
                .Where(x => x.BorrowDetailId == borrowDetailId && x.Status == RenewalRequestStatus.Pending)
                .OrderByDescending(x => x.RequestedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<int>> GetStaffUserIdsAsync()
        {
            return await db.Staffs
                .Include(x => x.Role)
                .Where(x =>
                    x.Account.IsActive &&
                    (x.Role.RoleName == "Librarian" ||
                     x.Role.RoleName == "Manager" ||
                     x.Role.RoleName == "Admin"))
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();
        }

        public void Add(RenewalRequest request)
        {
            db.RenewalRequests.Add(request);
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
