using CO4029_BE.Application.Abstractions;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace CO4029_BE.Application.Services;

/// <summary>
/// Keys under which codes are stored. The <c>otp_codes.email</c> column holds the key, so a code issued
/// for one purpose (e.g. registration) can never be used for another (e.g. password reset).
/// </summary>
public static class OtpPurpose
{
    public static string PasswordChange(string email) => Normalize(email);
    public static string PasswordReset(string email) => "reset:" + Normalize(email);
    public static string Register(string email) => "register:" + Normalize(email);
    public static string EmailChange(string userId, string newEmail) => $"email-change:{userId}:{Normalize(newEmail)}";

    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}

/// <summary>Thrown when a new code is requested before the resend cooldown has elapsed.</summary>
public class OtpCooldownException : InvalidOperationException
{
    public int RetryAfterSeconds { get; }

    public OtpCooldownException(int retryAfterSeconds)
        : base($"Vui lòng chờ {retryAfterSeconds} giây trước khi yêu cầu mã OTP mới.")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}

/// <summary>
/// Issues and verifies 4-digit one-time codes: 5-minute lifetime, 60-second resend cooldown, a new code
/// replaces the previous one, and 5 wrong guesses invalidate every active code of that key.
/// </summary>
public class OtpService
{
    public const int CodeLength = 4;
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
    public const int MaxFailedAttempts = 5;

    private readonly IOtpCodeRepository _codes;
    private readonly IOtpMailer _mailer;
    private readonly IMemoryCache _failures;
    private readonly TimeProvider _time;

    public OtpService(IOtpCodeRepository codes, IOtpMailer mailer, IMemoryCache failures, TimeProvider time)
    {
        _codes = codes;
        _mailer = mailer;
        _failures = failures;
        _time = time;
    }

    /// <summary>Creates a code under <paramref name="key"/> and emails it to <paramref name="recipient"/>.</summary>
    /// <exception cref="OtpCooldownException">The previous code was issued less than 60 seconds ago.</exception>
    public async Task IssueAsync(string key, string recipient)
    {
        var now = Now;
        var active = await ActiveCodesAsync(key, now);

        var latest = active.MaxBy(code => code.created_at);
        if (latest?.created_at is { } createdAt && now - createdAt < ResendCooldown)
        {
            var wait = (int)Math.Ceiling((ResendCooldown - (now - createdAt)).TotalSeconds);
            throw new OtpCooldownException(Math.Max(wait, 1));
        }

        var otp = OTPGenerator.GenerateOTP(CodeLength);
        // Send first: if the mail cannot be delivered no unusable code is stored.
        _mailer.SendOTPMail(recipient, otp);

        await ExpireAsync(active, now);
        await _codes.CreateAsync(new OtpCode
        {
            otp = otp,
            email = key,
            is_verified = false,
            created_at = now,
            expires_at = now + Lifetime
        });
        _failures.Remove(FailureKey(key));
    }

    /// <summary>Checks a code without using it up (for an OTP screen that precedes the final submit).</summary>
    public Task<bool> CheckAsync(string key, string code) => VerifyAsync(key, code, consume: false);

    /// <summary>Checks a code and marks it used so it cannot be replayed.</summary>
    public Task<bool> ConsumeAsync(string key, string code) => VerifyAsync(key, code, consume: true);

    private async Task<bool> VerifyAsync(string key, string code, bool consume)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var now = Now;
        var failureKey = FailureKey(key);
        var active = await ActiveCodesAsync(key, now);
        if (_failures.TryGetValue(failureKey, out int failed) && failed >= MaxFailedAttempts)
        {
            await ExpireAsync(active, now);
            return false;
        }

        var match = active.FirstOrDefault(otp => string.Equals(otp.otp, code.Trim(), StringComparison.Ordinal));
        if (match == null)
        {
            failed++;
            _failures.Set(failureKey, failed, Lifetime);
            if (failed >= MaxFailedAttempts)
                await ExpireAsync(active, now);
            return false;
        }

        if (consume)
        {
            match.is_verified = true;
            await _codes.UpdateAsync(match.id!, match);
            _failures.Remove(failureKey);
        }
        return true;
    }

    private async Task<List<OtpCode>> ActiveCodesAsync(string key, DateTime now)
    {
        var codes = await _codes.GetByKeyAsync(key);
        return codes
            .Where(code => code.is_verified != true && code.expires_at.HasValue && code.expires_at.Value > now)
            .ToList();
    }

    private async Task ExpireAsync(IEnumerable<OtpCode> codes, DateTime now)
    {
        foreach (var code in codes)
        {
            code.expires_at = now;
            await _codes.UpdateAsync(code.id!, code);
        }
    }

    private DateTime Now => _time.GetUtcNow().UtcDateTime;

    private static string FailureKey(string key) => "otp-failures:" + key;
}
