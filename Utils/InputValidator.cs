using System.Net.Mail;

namespace CO4029_BE.Utils;

public static class InputValidator
{
    /// <summary>Supabase Auth's default minimum password length.</summary>
    public const int MinPasswordLength = 6;

    public static string RequireEmail(string? email, string field = "Email")
    {
        var value = email?.Trim();
        if (string.IsNullOrEmpty(value) || value.Length > 254
            || !MailAddress.TryCreate(value, out var parsed) || parsed.Address != value
            || !value.Contains('.', StringComparison.Ordinal))
        {
            throw new ArgumentException($"{field} không hợp lệ.");
        }
        return value;
    }

    public static string RequirePassword(string? password, string field = "Mật khẩu")
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
            throw new ArgumentException($"{field} phải có ít nhất {MinPasswordLength} ký tự.");
        if (password.Length > 72)
            throw new ArgumentException($"{field} không được dài quá 72 ký tự.");
        return password;
    }

    public static string RequireOtp(string? otp)
    {
        var value = otp?.Trim();
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Mã OTP là bắt buộc.");
        return value;
    }
}
