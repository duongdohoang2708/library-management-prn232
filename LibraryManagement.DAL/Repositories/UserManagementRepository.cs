using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class UserManagementRepository
    {
        private readonly ApplicationDbContext db;

        public UserManagementRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public IQueryable<Account> QueryAccounts()
        {
            return db.Accounts
                .Include(x => x.Member)
                .Include(x => x.Staff!)
                    .ThenInclude(x => x.Role)
                .AsSplitQuery()
                .AsQueryable();
        }

        public async Task<Account?> GetAccountAsync(int userId)
        {
            return await QueryAccounts().FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await db.Roles.FirstOrDefaultAsync(x => x.RoleName == roleName);
        }

        public async Task<List<string>> GetRoleNamesAsync()
        {
            var staffRoles = await db.Roles
                .OrderBy(x => x.RoleName)
                .Select(x => x.RoleName)
                .ToListAsync();

            if (!staffRoles.Contains("Member"))
            {
                staffRoles.Insert(0, "Member");
            }

            return staffRoles;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await db.Accounts.AnyAsync(x => x.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await db.Accounts.AnyAsync(x => x.Email == email);
        }

        public void AddAccount(Account account)
        {
            db.Accounts.Add(account);
        }

        public void AddMember(Member member)
        {
            db.Members.Add(member);
        }

        public void AddStaff(Staff staff)
        {
            db.Staffs.Add(staff);
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
