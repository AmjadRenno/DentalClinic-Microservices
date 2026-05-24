using System.Text.RegularExpressions;

namespace DentalClinic.SharedKernel.Security;

/// <summary>
/// Input sanitization to prevent XSS and injection attacks
/// </summary>
public static class InputSanitizer
{
    private static readonly Regex HtmlTagPattern = new Regex("<[^>]*>", RegexOptions.Compiled);
    private static readonly Regex ScriptPattern = new Regex("<script[^>]*>.*?</script>", 
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex SqlInjectionPattern = new Regex(
        @"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT( +INTO)?|MERGE|SELECT|UPDATE|UNION( +ALL)?)\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Removes HTML tags from input
    /// </summary>
    public static string RemoveHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove script tags first
        var withoutScripts = ScriptPattern.Replace(input, string.Empty);
        
        // Remove all HTML tags
        return HtmlTagPattern.Replace(withoutScripts, string.Empty).Trim();
    }

    /// <summary>
    /// Sanitizes input to prevent basic SQL injection attempts
    /// Note: This is a defense-in-depth measure. Primary protection is parameterized queries.
    /// </summary>
    public static string SanitizeSqlInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove dangerous SQL keywords
        var sanitized = SqlInjectionPattern.Replace(input, string.Empty);
        
        // Remove common SQL injection characters
        sanitized = sanitized.Replace("'", "''")  // Escape single quotes
                             .Replace("--", string.Empty) // Remove SQL comments
                             .Replace(";", string.Empty)  // Remove statement terminators
                             .Replace("/*", string.Empty) // Remove block comments
                             .Replace("*/", string.Empty);

        return sanitized.Trim();
    }

    /// <summary>
    /// General purpose sanitization for text inputs
    /// </summary>
    public static string SanitizeText(string? input, int maxLength = 500)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove HTML and scripts
        var sanitized = RemoveHtml(input);

        // Truncate to max length
        if (sanitized.Length > maxLength)
            sanitized = sanitized.Substring(0, maxLength);

        // Remove control characters except newline and tab
        sanitized = new string(sanitized.Where(c => 
            !char.IsControl(c) || c == '\n' || c == '\r' || c == '\t').ToArray());

        return sanitized.Trim();
    }

    /// <summary>
    /// Validates and sanitizes email addresses
    /// </summary>
    public static string? SanitizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        // Basic sanitization
        email = email.Trim().ToLowerInvariant();

        // Remove potentially dangerous characters
        email = Regex.Replace(email, @"[^\w@\.\-\+]", string.Empty);

        // Basic email format validation
        if (!Regex.IsMatch(email, @"^[\w\.\-\+]+@[\w\.\-]+\.\w+$"))
            return null;

        return email;
    }

    /// <summary>
    /// Validates phone number format
    /// </summary>
    public static string? SanitizePhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        // Remove all non-digit characters
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        // Basic validation (10-15 digits)
        if (digits.Length < 10 || digits.Length > 15)
            return null;

        return digits;
    }

    /// <summary>
    /// Sanitizes search queries
    /// </summary>
    public static string SanitizeSearchQuery(string? query, int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(query))
            return string.Empty;

        // Remove HTML and scripts
        var sanitized = RemoveHtml(query);

        // Remove SQL injection attempts
        sanitized = SqlInjectionPattern.Replace(sanitized, string.Empty);

        // Truncate
        if (sanitized.Length > maxLength)
            sanitized = sanitized.Substring(0, maxLength);

        // Remove special characters that could be used for injection
        sanitized = Regex.Replace(sanitized, @"[^\w\s\-]", string.Empty);

        return sanitized.Trim();
    }
}
