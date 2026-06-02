using LibraryManagement.BLL.DTO.Auth;
using LibraryManagement.BLL.Services.Interface;
using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext db;
        private readonly IConfiguration configuration;
        private readonly PasswordHasher<User> passwordHashder;
        public AuthService(ApplicationDbContext _db, IConfiguration _configuration, PasswordHasher<User> _passwordHashder)
        {
            db = _db;
            configuration = _configuration;
            passwordHashder = _passwordHashder;
        }
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await db.Users.Include(u => u.UserRoles!).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Username == request.UserNameOrEmail || u.Email == request.UserNameOrEmail);
            if(user == null)
            {
                return null;
            }
            if(!user.IsActive)
            {
                return null;
            }

            var passwordResult = passwordHashder.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if(passwordResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var roles = user.UserRoles?.Select(ur => ur.Role.RoleName).ToList() ?? new List<string>();

            var accessToken = GenerateJwtToken(user, roles);

            user.LastLoginAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

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
            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience : audience,
                claims : claims,
                expires : DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
