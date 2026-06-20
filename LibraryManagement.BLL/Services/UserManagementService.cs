using LibraryManagement.BLL.DTO.User;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.BLL.Services
{
    public class UserManagementService
    {
        private const int PageSize = 10;
        private readonly UserManagementRepository userManagementRepository;
        private readonly PasswordHasher<Account> passwordHasher;

        public UserManagementService(
            UserManagementRepository _userManagementRepository,
            PasswordHasher<Account> _passwordHasher)
        {
            userManagementRepository = _userManagementRepository;
            passwordHasher = _passwordHasher;
        }

        public async Task<PaginatedResult<Account>> GetUsersAsync(string? search, string? roleName, int page)
        {
            page = page < 1 ? 1 : page;
            var query = userManagementRepository.QueryAccounts();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = int.TryParse(keyword, out var userId)
                    ? query.Where(x => x.UserId == userId)
                    : query.Where(x =>
                        x.Username.Contains(keyword) ||
                        x.FullName.Contains(keyword) ||
                        x.Email.Contains(keyword) ||
                        (x.Phone != null && x.Phone.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(roleName))
            {
                var role = roleName.Trim();
                query = role == "Member"
                    ? query.Where(x => x.Member != null)
                    : query.Where(x => x.Staff != null && x.Staff.Role.RoleName == role);
            }

            query = query
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.CreatedAt)
                .ThenBy(x => x.FullName);

            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            page = Math.Min(page, totalPages);

            return new PaginatedResult<Account>
            {
                Items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
                PageNumber = page,
                CurrentPage = page,
                PageSize = PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };
        }

        public async Task<List<string>> GetRoleNamesAsync()
        {
            return await userManagementRepository.GetRoleNamesAsync();
        }

        public async Task<Account?> GetProfileAsync(int userId)
        {
            return await userManagementRepository.GetAccountAsync(userId);
        }

        public async Task<UserActionResponse> CreateAsync(UserCreateRequest request)
        {
            var password = string.IsNullOrWhiteSpace(request.Password)
                ? request.PasswordHash
                : request.Password;
            var confirmPassword = request.ComfirmPasswordHash;

            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(password))
            {
                return Fail("Please fill all required fields.");
            }

            if (password.Length < 6)
            {
                return Fail("Password must be at least 6 characters.");
            }

            if (!string.IsNullOrWhiteSpace(confirmPassword) && password != confirmPassword)
            {
                return Fail("Confirm password does not match.");
            }

            var username = request.Username.Trim();
            var email = request.Email.Trim();

            if (await userManagementRepository.UsernameExistsAsync(username))
            {
                return Fail("Username already exists.");
            }

            if (await userManagementRepository.EmailExistsAsync(email))
            {
                return Fail("Email already exists.");
            }

            var roleName = string.IsNullOrWhiteSpace(request.Role)
                ? "Librarian"
                : request.Role.Trim();

            Role? staffRole = null;
            if (roleName != "Member")
            {
                staffRole = await userManagementRepository.GetRoleByNameAsync(roleName);
                if (staffRole == null)
                {
                    return Fail($"Role '{roleName}' does not exist.");
                }
            }

            var now = DateTime.UtcNow;
            var account = new Account
            {
                Username = username,
                Email = email,
                FullName = request.FullName.Trim(),
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
                DateOfBirth = request.DateOfBirth,
                IsActive = true,
                IsPasswordSet = true,
                PasswordHash = string.Empty,
                CreatedAt = now
            };
            account.PasswordHash = passwordHasher.HashPassword(account, password);

            userManagementRepository.AddAccount(account);
            await userManagementRepository.SaveChangesAsync();

            if (roleName == "Member")
            {
                userManagementRepository.AddMember(new Member
                {
                    UserId = account.UserId,
                    MemberCode = $"MEM{account.UserId:00000}",
                    JoinedAt = now,
                    CreatedAt = now
                });
            }
            else
            {
                userManagementRepository.AddStaff(new Staff
                {
                    UserId = account.UserId,
                    RoleId = staffRole!.RoleId,
                    StaffCode = $"STF{account.UserId:00000}",
                    HiredAt = now,
                    CreatedAt = now
                });
            }

            await userManagementRepository.SaveChangesAsync();

            return new UserActionResponse
            {
                IsSuccess = true,
                Message = "User account created successfully.",
                UserId = account.UserId
            };
        }

        public async Task<UserActionResponse> ToggleStatusAsync(int userId)
        {
            var account = await userManagementRepository.GetAccountAsync(userId);
            if (account == null)
            {
                return Fail("User not found.");
            }

            account.IsActive = !account.IsActive;
            account.UpdatedAt = DateTime.UtcNow;
            await userManagementRepository.SaveChangesAsync();

            return new UserActionResponse
            {
                IsSuccess = true,
                Message = account.IsActive ? "User activated successfully." : "User deactivated successfully.",
                UserId = account.UserId
            };
        }

        public async Task<UserActionResponse> UpdateProfileAsync(int userId, UserProfileUpdateRequest request)
        {
            var account = await userManagementRepository.GetAccountAsync(userId);
            if (account == null)
            {
                return Fail("User not found.");
            }

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return Fail("Full name is required.");
            }

            account.FullName = request.FullName.Trim();
            account.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
            account.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
            account.DateOfBirth = request.DateOfBirth;
            account.UpdatedAt = DateTime.UtcNow;

            await userManagementRepository.SaveChangesAsync();

            return new UserActionResponse
            {
                IsSuccess = true,
                Message = "Profile updated successfully.",
                UserId = account.UserId
            };
        }

        private static UserActionResponse Fail(string message)
        {
            return new UserActionResponse
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}
