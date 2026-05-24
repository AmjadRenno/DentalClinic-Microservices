using AuthService.API.Data;
using DentalClinic.SharedKernel.Security;
using Microsoft.EntityFrameworkCore;

namespace AuthService.API.Services
{
    public class UserService
    {
        private readonly AuthDbContext _db;
        private readonly PasswordValidator _passwordValidator;
        private readonly IAccountLockoutService _lockoutService;

        public UserService(
            AuthDbContext db,
            PasswordValidator passwordValidator,
            IAccountLockoutService lockoutService)
        {
            _db = db;
            _passwordValidator = passwordValidator;
            _lockoutService = lockoutService;
        }

        /// <summary>
        /// Registers a new user with password validation
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> RegisterUserAsync(string fullName, string email, string password)
        {
            // Validate password against policy
            var passwordValidation = _passwordValidator.Validate(password);
            if (!passwordValidation.IsValid)
            {
                return (false, string.Join(", ", passwordValidation.Errors));
            }

            // Check if email exists
            if (await _db.Users.AnyAsync(u => u.Email == email))
                return (false, "Email already exists");

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

            return (true, null);
        }

        /// <summary>
        /// Validates user credentials with account lockout protection
        /// </summary>
        public async Task<(UserEntity? User, string? ErrorMessage)> ValidateUserAsync(string email, string password)
        {
            // Check if account is locked out
            if (await _lockoutService.IsLockedOutAsync(email))
            {
                var lockoutEnd = await _lockoutService.GetLockoutEndTimeAsync(email);
                var remainingMinutes = lockoutEnd.HasValue 
                    ? (int)(lockoutEnd.Value - DateTime.UtcNow).TotalMinutes 
                    : 0;
                
                return (null, $"Account is locked due to multiple failed login attempts. Try again in {remainingMinutes} minutes.");
            }

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user is null)
            {
                // Record failed attempt even if user doesn't exist (prevents user enumeration timing attacks)
                await _lockoutService.RecordFailedAttemptAsync(email);
                return (null, "Invalid credentials");
            }

            // Use BCrypt.Verify to verify the password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // Record failed attempt
                await _lockoutService.RecordFailedAttemptAsync(email);
                
                var failedAttempts = await _lockoutService.GetFailedAttemptsCountAsync(email);
                var remainingAttempts = 5 - failedAttempts;
                
                if (remainingAttempts > 0)
                {
                    return (null, $"Invalid credentials. {remainingAttempts} attempts remaining before lockout.");
                }
                
                return (null, "Invalid credentials. Account has been locked.");
            }

            // Success - reset failed attempts
            await _lockoutService.ResetFailedAttemptsAsync(email);
            return (user, null);
        }

        private string HashPassword(string password)
        {
            // BCrypt automatically generates Salt for each password
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }
    }
}
