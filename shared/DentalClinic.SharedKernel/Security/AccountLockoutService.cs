namespace DentalClinic.SharedKernel.Security;

/// <summary>
/// Account lockout tracker to prevent brute force attacks
/// </summary>
public interface IAccountLockoutService
{
    Task<bool> IsLockedOutAsync(string identifier);
    Task RecordFailedAttemptAsync(string identifier);
    Task ResetFailedAttemptsAsync(string identifier);
    Task<int> GetFailedAttemptsCountAsync(string identifier);
    Task<DateTime?> GetLockoutEndTimeAsync(string identifier);
}

/// <summary>
/// In-memory implementation of account lockout service
/// For production, use Redis or database-backed implementation
/// </summary>
public class AccountLockoutService : IAccountLockoutService
{
    private readonly Dictionary<string, LockoutInfo> _lockoutStore = new();
    private readonly int _maxFailedAttempts;
    private readonly TimeSpan _lockoutDuration;
    private readonly TimeSpan _failedAttemptWindow;

    public AccountLockoutService(
        int maxFailedAttempts = 5,
        TimeSpan? lockoutDuration = null,
        TimeSpan? failedAttemptWindow = null)
    {
        _maxFailedAttempts = maxFailedAttempts;
        _lockoutDuration = lockoutDuration ?? TimeSpan.FromMinutes(15);
        _failedAttemptWindow = failedAttemptWindow ?? TimeSpan.FromMinutes(10);
    }

    public Task<bool> IsLockedOutAsync(string identifier)
    {
        if (!_lockoutStore.TryGetValue(identifier, out var info))
            return Task.FromResult(false);

        // Check if lockout has expired
        if (info.LockoutEnd.HasValue && info.LockoutEnd.Value <= DateTime.UtcNow)
        {
            _lockoutStore.Remove(identifier);
            return Task.FromResult(false);
        }

        return Task.FromResult(info.IsLockedOut);
    }

    public Task RecordFailedAttemptAsync(string identifier)
    {
        if (!_lockoutStore.TryGetValue(identifier, out var info))
        {
            info = new LockoutInfo();
            _lockoutStore[identifier] = info;
        }

        // Remove old attempts outside the window
        var cutoff = DateTime.UtcNow.Subtract(_failedAttemptWindow);
        info.FailedAttempts.RemoveAll(dt => dt < cutoff);

        // Add new failed attempt
        info.FailedAttempts.Add(DateTime.UtcNow);

        // Check if should lockout
        if (info.FailedAttempts.Count >= _maxFailedAttempts)
        {
            info.IsLockedOut = true;
            info.LockoutEnd = DateTime.UtcNow.Add(_lockoutDuration);
        }

        return Task.CompletedTask;
    }

    public Task ResetFailedAttemptsAsync(string identifier)
    {
        _lockoutStore.Remove(identifier);
        return Task.CompletedTask;
    }

    public Task<int> GetFailedAttemptsCountAsync(string identifier)
    {
        if (!_lockoutStore.TryGetValue(identifier, out var info))
            return Task.FromResult(0);

        // Clean old attempts
        var cutoff = DateTime.UtcNow.Subtract(_failedAttemptWindow);
        info.FailedAttempts.RemoveAll(dt => dt < cutoff);

        return Task.FromResult(info.FailedAttempts.Count);
    }

    public Task<DateTime?> GetLockoutEndTimeAsync(string identifier)
    {
        if (!_lockoutStore.TryGetValue(identifier, out var info))
            return Task.FromResult<DateTime?>(null);

        return Task.FromResult(info.LockoutEnd);
    }

    private class LockoutInfo
    {
        public List<DateTime> FailedAttempts { get; set; } = new();
        public bool IsLockedOut { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }
}
