using System.Text.RegularExpressions;

namespace DentalClinic.SharedKernel.Security;

/// <summary>
/// Password policy enforcement
/// </summary>
public class PasswordPolicy
{
    public int MinimumLength { get; set; } = 8;
    public int MaximumLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = true;
    public int MinimumUniqueCharacters { get; set; } = 4;

    public static PasswordPolicy Default => new PasswordPolicy();

    public static PasswordPolicy Strict => new PasswordPolicy
    {
        MinimumLength = 12,
        MinimumUniqueCharacters = 6,
        RequireUppercase = true,
        RequireLowercase = true,
        RequireDigit = true,
        RequireSpecialCharacter = true
    };
}

/// <summary>
/// Password validation result
/// </summary>
public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();

    public static PasswordValidationResult Success() => new PasswordValidationResult { IsValid = true };
    
    public static PasswordValidationResult Failure(params string[] errors) => new PasswordValidationResult 
    { 
        IsValid = false, 
        Errors = errors.ToList() 
    };
}

/// <summary>
/// Password validator with configurable policies
/// </summary>
public class PasswordValidator
{
    private readonly PasswordPolicy _policy;

    public PasswordValidator(PasswordPolicy? policy = null)
    {
        _policy = policy ?? PasswordPolicy.Default;
    }

    /// <summary>
    /// Validates password against policy
    /// </summary>
    public PasswordValidationResult Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password cannot be empty");
            return PasswordValidationResult.Failure(errors.ToArray());
        }

        // Length validation
        if (password.Length < _policy.MinimumLength)
            errors.Add($"Password must be at least {_policy.MinimumLength} characters long");

        if (password.Length > _policy.MaximumLength)
            errors.Add($"Password cannot exceed {_policy.MaximumLength} characters");

        // Character type requirements
        if (_policy.RequireUppercase && !password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter");

        if (_policy.RequireLowercase && !password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter");

        if (_policy.RequireDigit && !password.Any(char.IsDigit))
            errors.Add("Password must contain at least one digit");

        if (_policy.RequireSpecialCharacter && !Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]"))
            errors.Add("Password must contain at least one special character");

        // Unique characters
        var uniqueChars = password.Distinct().Count();
        if (uniqueChars < _policy.MinimumUniqueCharacters)
            errors.Add($"Password must contain at least {_policy.MinimumUniqueCharacters} unique characters");

        // Common password patterns
        if (IsCommonPassword(password))
            errors.Add("Password is too common. Please choose a stronger password");

        return errors.Count == 0 
            ? PasswordValidationResult.Success() 
            : PasswordValidationResult.Failure(errors.ToArray());
    }

    /// <summary>
    /// Checks against common weak passwords
    /// </summary>
    private bool IsCommonPassword(string password)
    {
        var commonPasswords = new[] 
        { 
            "password", "123456", "12345678", "qwerty", "abc123", 
            "password1", "111111", "123123", "admin", "letmein",
            "welcome", "monkey", "dragon", "master", "sunshine"
        };

        return commonPasswords.Contains(password.ToLower());
    }

    /// <summary>
    /// Generates a random strong password
    /// </summary>
    public static string GenerateStrongPassword(int length = 16)
    {
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        var random = new Random();
        var password = new List<char>();

        // Ensure at least one of each required type
        password.Add(lowercase[random.Next(lowercase.Length)]);
        password.Add(uppercase[random.Next(uppercase.Length)]);
        password.Add(digits[random.Next(digits.Length)]);
        password.Add(special[random.Next(special.Length)]);

        // Fill remaining with random characters
        var allChars = lowercase + uppercase + digits + special;
        for (int i = 4; i < length; i++)
        {
            password.Add(allChars[random.Next(allChars.Length)]);
        }

        // Shuffle
        return new string(password.OrderBy(_ => random.Next()).ToArray());
    }
}
