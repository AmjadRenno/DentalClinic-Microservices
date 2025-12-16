using AuthService.API.Data;
using Microsoft.EntityFrameworkCore;

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

                // Here we set the default role
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

            // Use BCrypt.Verify to verify the password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        private string HashPassword(string password)
        {
            // BCrypt automatically generates Salt for each password
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }
    }
}
