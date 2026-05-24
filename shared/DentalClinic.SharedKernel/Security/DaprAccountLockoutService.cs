using Dapr.Client;
using System.Text.Json;

namespace DentalClinic.SharedKernel.Security;

/// <summary>
/// Distributed account lockout service using Dapr State Store
/// Suitable for production with multiple service instances
/// </summary>
public class DaprAccountLockoutService : IAccountLockoutService
{
    private readonly DaprClient _daprClient;
    private readonly string _stateStoreName;
    private readonly int _maxFailedAttempts;
    private readonly TimeSpan _lockoutDuration;
    private readonly TimeSpan _failedAttemptWindow;

    public DaprAccountLockoutService(
        DaprClient daprClient,
        string stateStoreName = "lockout-statestore",
        int maxFailedAttempts = 5,
        TimeSpan? lockoutDuration = null,
        TimeSpan? failedAttemptWindow = null)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _stateStoreName = stateStoreName;
        _maxFailedAttempts = maxFailedAttempts;
        _lockoutDuration = lockoutDuration ?? TimeSpan.FromMinutes(15);
        _failedAttemptWindow = failedAttemptWindow ?? TimeSpan.FromMinutes(10);
    }

    public async Task<bool> IsLockedOutAsync(string identifier)
    {
        var info = await GetLockoutInfoAsync(identifier);
        if (info == null)
            return false;

        // Check if lockout has expired
        if (info.LockoutEnd.HasValue && info.LockoutEnd.Value <= DateTime.UtcNow)
        {
            await _daprClient.DeleteStateAsync(_stateStoreName, GetStateKey(identifier));
            return false;
        }

        return info.IsLockedOut;
    }

    public async Task RecordFailedAttemptAsync(string identifier)
    {
        var info = await GetLockoutInfoAsync(identifier) ?? new LockoutInfo();

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

        await SaveLockoutInfoAsync(identifier, info);
    }

    public async Task ResetFailedAttemptsAsync(string identifier)
    {
        await _daprClient.DeleteStateAsync(_stateStoreName, GetStateKey(identifier));
    }

    public async Task<int> GetFailedAttemptsCountAsync(string identifier)
    {
        var info = await GetLockoutInfoAsync(identifier);
        if (info == null)
            return 0;

        // Clean old attempts
        var cutoff = DateTime.UtcNow.Subtract(_failedAttemptWindow);
        info.FailedAttempts.RemoveAll(dt => dt < cutoff);

        return info.FailedAttempts.Count;
    }

    public async Task<DateTime?> GetLockoutEndTimeAsync(string identifier)
    {
        var info = await GetLockoutInfoAsync(identifier);
        return info?.LockoutEnd;
    }

    private async Task<LockoutInfo?> GetLockoutInfoAsync(string identifier)
    {
        try
        {
            return await _daprClient.GetStateAsync<LockoutInfo>(_stateStoreName, GetStateKey(identifier));
        }
        catch
        {
            // If state doesn't exist or error, return null
            return null;
        }
    }

    private async Task SaveLockoutInfoAsync(string identifier, LockoutInfo info)
    {
        await _daprClient.SaveStateAsync(_stateStoreName, GetStateKey(identifier), info);
    }

    private static string GetStateKey(string identifier)
    {
        return $"lockout:{identifier}";
    }

    private class LockoutInfo
    {
        public List<DateTime> FailedAttempts { get; set; } = new();
        public bool IsLockedOut { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }
}
