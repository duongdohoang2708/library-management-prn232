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

        public async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await db.Users
                .Include(u => u.UserRoles!)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u =>
                    u.Username == usernameOrEmail ||
                    u.Email == usernameOrEmail);
        }

        public async Task UpdateLastLoginAsync(User user)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await db.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await db.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User?> CreateUserWithRoleAsync(User user, string roleName)
        {
            var role = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role == null)
            {
                return null;
            }

            user.CreatedAt = DateTime.UtcNow;
            db.Users.Add(user);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            return user;
        }
        public async Task<User?> GetUserById(int userId)
        {
            return await db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }
        public async Task UpdatePasswordAsync(User user)
        {
            user.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }
    }
}
