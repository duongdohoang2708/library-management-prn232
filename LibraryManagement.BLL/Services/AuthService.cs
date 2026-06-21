using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LibraryManagement.BLL.DTO.Auth;
using LibraryManagement.BLL.Services.Interface;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;

namespace LibraryManagement.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthenRepository authenRepository;
        private readonly IConfiguration configuration;
        private readonly PasswordHasher<Account> passwordHasher;

        public AuthService(AuthenRepository _authenRepository, IConfiguration _configuration, PasswordHasher<Account> _passwordHasher)
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

            var roles = new List<string>();
            if (user.Staff?.Role != null)
            {
                roles.Add(user.Staff.Role.RoleName);
            }
            else if (user.Member != null)
            {
                roles.Add("Member");
            }

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

            var user = new Account
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

        public async Task<AuthActionResponse> SendPasswordResetCodeAsync(ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return new AuthActionResponse
                {
                    IsSuccess = false,
                    Message = "Vui long nhap email"
                };
            }

            var user = await authenRepository.GetUserByEmailAsync(request.Email.Trim());
            if (user == null || !user.IsActive)
            {
                return new AuthActionResponse
                {
                    IsSuccess = false,
                    Message = "Khong tim thay tai khoan"
                };
            }

            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            user.PasswordResetCode = code;
            user.PasswordResetCodeExpiresAt = DateTime.UtcNow.AddMinutes(10);
            user.PasswordResetCodeVerifiedAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await authenRepository.SaveChangesAsync();

            try
            {
                await SendResetCodeEmailAsync(user.Email, user.FullName, code);
            }
            catch (Exception ex)
            {
                user.PasswordResetCode = null;
                user.PasswordResetCodeExpiresAt = null;
                user.PasswordResetCodeVerifiedAt = null;
                user.UpdatedAt = DateTime.UtcNow;
                await authenRepository.SaveChangesAsync();

                return new AuthActionResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

            return new AuthActionResponse
            {
                IsSuccess = true,
                Message = "Ma xac nhan da duoc gui ve email"
            };
        }

        public async Task<AuthActionResponse> VerifyPasswordResetCodeAsync(VerifyResetCodeRequest request)
        {
            var user = await authenRepository.GetUserByEmailAsync(request.Email.Trim());
            if (user == null || !IsValidResetCode(user, request.Code))
            {
                return new AuthActionResponse
                {
                    IsSuccess = false,
                    Message = "Ma xac nhan khong dung hoac da het han"
                };
            }

            user.PasswordResetCodeVerifiedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await authenRepository.SaveChangesAsync();

            return new AuthActionResponse
            {
                IsSuccess = true,
                Message = "Xac nhan ma thanh cong"
            };
        }

        public async Task<AuthActionResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return new AuthActionResponse
                {
                    IsSuccess = false,
                    Message = "Mat khau xac nhan khong khop"
                };
            }

            var user = await authenRepository.GetUserByEmailAsync(request.Email.Trim());
            if (user == null || !IsValidResetCode(user, request.Code))
            {
                return new AuthActionResponse
                {
                    IsSuccess = false,
                    Message = "Ma xac nhan khong dung hoac da het han"
                };
            }

            user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
            user.IsPasswordSet = true;
            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiresAt = null;
            user.PasswordResetCodeVerifiedAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await authenRepository.SaveChangesAsync();

            return new AuthActionResponse
            {
                IsSuccess = true,
                Message = "Doi mat khau thanh cong"
            };
        }

        private static bool IsValidResetCode(Account user, string code)
        {
            return !string.IsNullOrWhiteSpace(code)
                && user.PasswordResetCode == code.Trim()
                && user.PasswordResetCodeExpiresAt.HasValue
                && user.PasswordResetCodeExpiresAt.Value >= DateTime.UtcNow;
        }

        private async Task SendResetCodeEmailAsync(string email, string fullName, string code)
        {
            var smtpServer = configuration["EmailSettings:SmtpServer"];
            var smtpPortText = configuration["EmailSettings:SmtpPort"];
            var senderEmail = configuration["EmailSettings:SenderEmail"];
            var senderPassword = configuration["EmailSettings:SenderPassword"];
            var senderName = configuration["EmailSettings:DisplaySenderName"] ?? "LMS System";

            if (string.IsNullOrWhiteSpace(smtpServer)
                || string.IsNullOrWhiteSpace(smtpPortText)
                || string.IsNullOrWhiteSpace(senderEmail)
                || string.IsNullOrWhiteSpace(senderPassword)
                || !int.TryParse(smtpPortText, out var smtpPort))
            {
                throw new InvalidOperationException("Chua cau hinh EmailSettings SMTP de gui ma xac nhan.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "LMS password reset code";
            message.Body = new BodyBuilder
            {
                TextBody = $"Xin chao {fullName},\n\nMa xac nhan dat lai mat khau cua ban la: {code}\nMa co hieu luc trong 10 phut.\n\nLMS System",
                HtmlBody = $"""
                    <div style="margin:0;padding:32px;background:#071f1c;font-family:Arial,sans-serif;color:#f8fafc">
                      <div style="max-width:560px;margin:0 auto;background:#0b1220;border:1px solid rgba(255,255,255,.12);border-radius:18px;overflow:hidden">
                        <div style="padding:24px 28px;background:#0f302b;border-bottom:1px solid rgba(255,255,255,.1)">
                          <div style="font-size:20px;font-weight:800;color:#ffea00">LMS Standard</div>
                        </div>
                        <div style="padding:28px">
                          <p style="margin:0 0 12px;color:#94a3b8">Xin chao {System.Net.WebUtility.HtmlEncode(fullName)},</p>
                          <h1 style="margin:0 0 16px;font-size:24px;line-height:1.25;color:#ffffff">Password reset code</h1>
                          <div style="letter-spacing:8px;font-size:34px;font-weight:900;color:#ffea00;background:#111827;border:1px solid rgba(255,234,0,.35);border-radius:14px;padding:18px 20px;text-align:center">{code}</div>
                          <p style="margin:18px 0 0;color:#cbd5e1;font-size:15px;line-height:1.7">Ma co hieu luc trong 10 phut. Khong chia se ma nay voi nguoi khac.</p>
                        </div>
                        <div style="padding:18px 28px;color:#64748b;font-size:12px;border-top:1px solid rgba(255,255,255,.1)">
                          This email was sent automatically by LMS Standard.
                        </div>
                      </div>
                    </div>
                    """
            }.ToMessageBody();

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(senderEmail, senderPassword);
            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
        }

        private string GenerateJwtToken(Account user, List<string> roles)
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
