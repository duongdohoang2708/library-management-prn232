using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryManagement.BLL.DTO.Auth;
using LibraryManagement.BLL.Services.Interface;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LibraryManagement.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthenRepository authenRepository;
        private readonly IConfiguration configuration;
        private readonly PasswordHasher<User> passwordHasher;

        public AuthService(AuthenRepository _authenRepository, IConfiguration _configuration, PasswordHasher<User> _passwordHasher)
        {
            authenRepository = _authenRepository;
            configuration = _configuration;
            passwordHasher = _passwordHasher;
        }

        public async Task<ChangePasswordResponse> ChangePasswordAsync(ChangePasswordRequest request)
        {
            if(request.NewPassword != request.ConfirmNewPassword)
            {
                return new ChangePasswordResponse
                {
                    IsSuccess = false,
                    Message = "Mat khau xac nhan khong khop"
                };             
            }
            var user = await authenRepository.GetUserById(request.UserId);
            if (user == null || !user.IsActive)
            {
                return new ChangePasswordResponse
                {
                    IsSuccess = false,
                    Message = "Khong tim thay tai khoan"
                };
            }
            var passwordResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return new ChangePasswordResponse
                {
                    IsSuccess = false,
                    Message = "Mat khau hien tai khong dung"
                };
            }
            user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
            await authenRepository.UpdatePasswordAsync(user);

            return new ChangePasswordResponse
            {
                IsSuccess = true,
                Message = "Doi mat khau thanh cong"
            };
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await authenRepository.GetUserByUsernameOrEmailAsync(request.UserNameOrEmail);
            if (user == null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            var passwordResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var roles = user.UserRoles?
                .Select(ur => ur.Role.RoleName)
                .ToList() ?? new List<string>();

            var accessToken = GenerateJwtToken(user, roles);

            await authenRepository.UpdateLastLoginAsync(user);

            return new LoginResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Fullname = user.FullName,
                Email = user.Email,
                Roles = roles,
                AccessToken = accessToken
            };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName))
            {
                return new RegisterResponse
                {
                    IsSuccess = false,
                    Message = "Vui long nhap day du thong tin bat buoc"
                };
            }

            if (request.Password != request.ConfirmPassword)
            {
                return new RegisterResponse
                {
                    IsSuccess = false,
                    Message = "Mat khau xac nhan khong khop"
                };
            }

            if (await authenRepository.UsernameExistsAsync(request.Username))
            {
                return new RegisterResponse
                {
                    IsSuccess = false,
                    Message = "Username da ton tai"
                };
            }

            if (await authenRepository.EmailExistsAsync(request.Email))
            {
                return new RegisterResponse
                {
                    IsSuccess = false,
                    Message = "Email da ton tai"
                };
            }

            var user = new User
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim(),
                FullName = request.FullName.Trim(),
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
                DateOfBirth = request.DateOfBirth,
                IsActive = true,
                IsPasswordSet = true,
                PasswordHash = string.Empty
            };

            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

            var createdUser = await authenRepository.CreateUserWithRoleAsync(user, "Member");
            if (createdUser == null)
            {
                return new RegisterResponse
                {
                    IsSuccess = false,
                    Message = "Khong tim thay role Member"
                };
            }

            return new RegisterResponse
            {
                IsSuccess = true,
                Message = "Dang ky tai khoan thanh cong",
                UserId = createdUser.UserId
            };
        }

        private string GenerateJwtToken(User user, List<string> roles)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");

            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = int.Parse(jwtSettings["AccessTokenExpiryMinutes"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
