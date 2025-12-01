using AuthService.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.API.Services
{
    public class UserService
    {
        private readonly AuthDbContext _db;

        public UserService(AuthDbContext db)
        {
            _db = db;
        }

        public async Task<bool> RegisterUserAsync(string fullName, string email, string password)
        {
            // Check if email exists
            if (await _db.Users.AnyAsync(u => u.Email == email))
                return false;

            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordHash = HashPassword(password),

                // 🔹 هنا نضبط الدور الافتراضي
                Role = "Patient"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return true;
        }


        public async Task<UserEntity?> ValidateUserAsync(string email, string password)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user is null)
                return null;

            var hash = HashPassword(password);
            if (user.PasswordHash != hash)
                return null;

            return user;
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }
    }
}
