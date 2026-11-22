using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.API.Contracts.Mappers;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Application.Abstractions;
using CO4029_BE.Utils;

namespace CO4029_BE.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly OtpService _otp;
    private readonly IAuthGateway _auth;
    private readonly ICurrentUserResolver _currentUser;

    public AuthService(IUserRepository userRepository
                     , OtpService otp
                     , IAuthGateway auth
                     , ICurrentUserResolver currentUser)
    {
        _userRepository = userRepository;
        _otp = otp;
        _auth = auth;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Signs in through the Auth REST API. The shared Supabase client is never signed in as the user, so
    /// later database calls keep running with the server key instead of the last user's JWT.
    /// </summary>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        var session = await _auth.SignInAsync(request.Email.Trim(), request.Password);
        if (session == null)
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        // Rejects accounts without an application profile and deactivated accounts.
        await _currentUser.ResolveAsync(session.AccessToken);
        return session;
    }

    public async Task<UserReponse> GetMeAsync(string accessToken)
    {
        var user = await _currentUser.ResolveAsync(accessToken);
        return user.ToResponse();
    }

    public async Task<bool> SendOtpCode(string email)
    {
        var address = InputValidator.RequireEmail(email);
        var user = await _userRepository.GetByEmailAsync(address);
        if (user == null)
            throw new KeyNotFoundException("Không tìm thấy user trong database.");
        if (!user.is_active)
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa.");
        await _otp.IssueAsync(OtpPurpose.PasswordChange(address), user.email!);
        return true;
    }

    public async Task<UserReponse> ChangePasswordAsync(
        string? email,
        string? otpCode,
        string? newPassword,
        string? oldPassword)
    {
        if (string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(otpCode)
            || string.IsNullOrWhiteSpace(newPassword)
            || string.IsNullOrWhiteSpace(oldPassword))
        {
            throw new ArgumentException("Thông tin đổi mật khẩu không đầy đủ.");
        }
        InputValidator.RequirePassword(newPassword, "Mật khẩu mới");

        var address = email.Trim();
        var user = await _userRepository.GetByEmailAsync(address);
        if (user == null || !user.is_active || string.IsNullOrEmpty(user.id))
            throw new UnauthorizedAccessException("Tài khoản không hợp lệ.");

        // Check the old password first so a typo does not burn the one-time code.
        if (await _auth.SignInAsync(address, oldPassword) == null)
            throw new UnauthorizedAccessException("Mật khẩu cũ không chính xác. Vui lòng thử lại.");
        if (!await _otp.ConsumeAsync(OtpPurpose.PasswordChange(address), otpCode))
            throw new ArgumentException(AccountService.InvalidOtpMessage);

        await _auth.UpdatePasswordAsync(user.id, newPassword);
        return user.ToResponse();
    }
}
