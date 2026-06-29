using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TrackNGo.Data;
using TrackNGo.Models;

namespace TrackNGo.Services
{
    public interface IAuthService
    {
        Task<User?> ValidateCredentialsAsync(string username, string password);
        Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
        int? GetCurrentUserId(ClaimsPrincipal principal);
        UserRole? GetCurrentUserRole(ClaimsPrincipal principal);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;

        public AuthService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<User?> ValidateCredentialsAsync(string username, string password)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => (u.Username == username || u.Email == username) && u.IsActive);

            if (user == null) return null;

            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return user;
        }

        public Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("Department", user.Department)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            return Task.FromResult(principal);
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool VerifyPassword(string password, string hash)
        {
            var computedHash = HashPassword(password);
            return computedHash == hash;
        }

        public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var userId = GetCurrentUserId(principal);
            if (userId == null) return null;
            return await _db.Users.FindAsync(userId.Value);
        }

        public int? GetCurrentUserId(ClaimsPrincipal principal)
        {
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null || !int.TryParse(idClaim.Value, out var id)) return null;
            return id;
        }

        public UserRole? GetCurrentUserRole(ClaimsPrincipal principal)
        {
            var roleClaim = principal.FindFirst(ClaimTypes.Role);
            if (roleClaim == null || !Enum.TryParse<UserRole>(roleClaim.Value, out var role)) return null;
            return role;
        }
    }
}
