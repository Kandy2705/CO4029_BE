using CO4029_BE.API.Contracts.Mappers;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Application.Abstractions;
using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.Utils;

namespace CO4029_BE.Application.Services;

/// <summary>Self-service account flows of the mobile app: registration OTP, password reset, email change, token refresh.</summary>
public class AccountService
{
    public const string InvalidOtpMessage = "Mã OTP không hợp lệ hoặc đã hết hạn.";

    private readonly IUserRepository _users;
    private readonly OtpService _otp;
    private readonly IAuthGateway _auth;
    private readonly ICurrentUserResolver _currentUser;

    public AccountService(IUserRepository users, OtpService otp, IAuthGateway auth, ICurrentUserResolver currentUser)
    {
        _users = users;
        _otp = otp;
        _auth = auth;
        _currentUser = currentUser;
    }

    /// <summary>Sends a registration code; fails with 409 when the email already has an account.</summary>
    public async Task RequestRegisterOtpAsync(string? email)
    {
        var address = InputValidator.RequireEmail(email);
        if (await _users.GetByEmailAsync(address) != null)
            throw new InvalidOperationException("Email đã được sử dụng.");
        await _otp.IssueAsync(OtpPurpose.Register(address), address);
    }

    /// <summary>
    /// Sends a reset code when the email belongs to an active account. Always completes the same way so the
    /// endpoint cannot be used to find out which emails are registered.
    /// </summary>
    public async Task RequestPasswordResetAsync(string? email)
    {
        var address = InputValidator.RequireEmail(email);
        var user = await _users.GetByEmailAsync(address);
        if (user is not { is_active: true } || string.IsNullOrEmpty(user.email))
            return;
        try
        {
            await _otp.IssueAsync(OtpPurpose.PasswordReset(address), user.email);
        }
        catch (OtpCooldownException)
        {
            // A code was sent moments ago; staying silent keeps the response identical for every email.
        }
    }

    /// <summary>Checks a registration or reset code without consuming it.</summary>
    public Task<bool> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var address = InputValidator.RequireEmail(request.Email);
        var code = InputValidator.RequireOtp(request.OtpCode);
        var key = (request.Purpose ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "register" => OtpPurpose.Register(address),
            "reset" => OtpPurpose.PasswordReset(address),
            _ => throw new ArgumentException("purpose phải là register hoặc reset.")
        };
        return _otp.CheckAsync(key, code);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var address = InputValidator.RequireEmail(request.Email);
        var code = InputValidator.RequireOtp(request.OtpCode);
        var password = InputValidator.RequirePassword(request.NewPassword, "Mật khẩu mới");

        var user = await _users.GetByEmailAsync(address);
        if (user is not { is_active: true } || string.IsNullOrEmpty(user.id))
            throw new ArgumentException(InvalidOtpMessage);
        if (!await _otp.ConsumeAsync(OtpPurpose.PasswordReset(address), code))
            throw new ArgumentException(InvalidOtpMessage);

        await _auth.UpdatePasswordAsync(user.id, password);
    }

    /// <summary>Sends a code to the new address; the change only happens once that code is entered.</summary>
    public async Task RequestEmailChangeAsync(string accessToken, string? newEmail)
    {
        var user = await _currentUser.ResolveAsync(accessToken);
        var address = InputValidator.RequireEmail(newEmail, "Email mới");
        await EnsureEmailAvailableAsync(address, user.email);
        await _otp.IssueAsync(OtpPurpose.EmailChange(user.id!, address), address);
    }

    public async Task<UserReponse> ChangeEmailAsync(string accessToken, ChangeEmailRequest request)
    {
        var user = await _currentUser.ResolveAsync(accessToken);
        var address = InputValidator.RequireEmail(request.NewEmail, "Email mới");
        var code = InputValidator.RequireOtp(request.OtpCode);
        await EnsureEmailAvailableAsync(address, user.email);
        if (!await _otp.ConsumeAsync(OtpPurpose.EmailChange(user.id!, address), code))
            throw new ArgumentException(InvalidOtpMessage);

        await _auth.UpdateEmailAsync(user.id!, address);
        user.email = address;
        await _users.UpdateAsync(user.id!, user);
        return user.ToResponse();
    }

    /// <summary>Issues a new session from a refresh token, refusing accounts that were deactivated meanwhile.</summary>
    public async Task<LoginResponse> RefreshAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("refreshToken là bắt buộc.");
        var session = await _auth.RefreshSessionAsync(refreshToken.Trim());
        await _currentUser.ResolveAsync(session.AccessToken);
        return session;
    }

    private async Task EnsureEmailAvailableAsync(string address, string? currentEmail)
    {
        if (string.Equals(address, currentEmail, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Email mới trùng với email hiện tại.");
        if (await _users.GetByEmailAsync(address) != null)
            throw new InvalidOperationException("Email đã được sử dụng.");
    }
}
