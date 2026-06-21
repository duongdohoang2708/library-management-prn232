using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class AuthenRepository
    {
        private readonly ApplicationDbContext db;

        public AuthenRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public async Task<Account?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await db.Accounts
                .Include(u => u.Member)
                .Include(u => u.Staff!)
                .ThenInclude(s => s.Role)
                .FirstOrDefaultAsync(u =>
                    u.Username == usernameOrEmail ||
                    u.Email == usernameOrEmail);
        }

        public async Task<Account?> GetUserByEmailAsync(string email)
        {
            return await db.Accounts.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task UpdateLastLoginAsync(Account user)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await db.Accounts.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await db.Accounts.AnyAsync(u => u.Email == email);
        }

        public async Task<Account?> CreateUserWithRoleAsync(Account user, string roleName)
        {
            user.CreatedAt = DateTime.UtcNow;
            db.Accounts.Add(user);
            await db.SaveChangesAsync();

            db.Members.Add(new Member
            {
                UserId = user.UserId,
                MemberCode = $"MEM{user.UserId:00000}",
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            return user;
        }
        public async Task<Account?> GetUserById(int userId)
        {
            return await db.Accounts.FirstOrDefaultAsync(u => u.UserId == userId);
        }
        public async Task UpdatePasswordAsync(Account user)
        {
            user.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
