using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.API.Contracts.Mappers;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Utils;
using Supabase;

namespace CO4029_BE.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly Client _supabaseClient;
    private readonly IOtpCodeRepository _otpCodeRepository;
    private readonly EmailService _emailService;

    public AuthService(IUserRepository userRepository
                     , IOtpCodeRepository otpCodeRepository
                     , Client supabaseClient
                     , EmailService emailService)
    {
        _userRepository = userRepository;
        _supabaseClient = supabaseClient;
        _otpCodeRepository = otpCodeRepository;
        _emailService = emailService;
    }
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var session = await _supabaseClient.Auth.SignIn(
            email: request.Email,
            password: request.Password
        );
        if (session == null || session.User == null) 
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng. Xin vui lòng đăng nhập lại !!!");
        var authUser = session.User;
        
        var userEmail = authUser.Email;
        var user = await _userRepository.GetByEmailAsync(userEmail);

        if (user == null)
            throw new KeyNotFoundException("Không tìm thấy user");

        return session.ToResponse();
    }
    public async Task<UserReponse> GetMeAsync(string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, _supabaseClient, _userRepository);
        user = await _userRepository.GetByEmailAsync(user.email);
        return user.ToResponse();
    }

    public async Task<bool> SendOtpCode(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            throw new KeyNotFoundException("Không tìm thấy user trong database.");
        var otps = await _otpCodeRepository.GetAllAsync();
        foreach (var otp in otps)
        {
            if (otp.email == user.email && otp.is_verified == false)
                throw new InvalidOperationException("User đã được gửi otp code");
        }
        string OTP = OTPGenerator.GenerateOTP();
        _emailService.SendOTPMail(user.email, OTP);
        await _otpCodeRepository.CreateAsync(new OtpCode
        {
            otp = OTP,
            email = user.email,
            expires_at = DateTime.UtcNow.AddMinutes(5),
            created_at = DateTime.UtcNow
        });
        return true;
    }

    public async Task<UserReponse> ChangePasswordAsync(string email, string otpCode, string newPassword, string oldPassword)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        bool isOtpValid = await VerifyOTPCode(user,otpCode);
        if (!isOtpValid)
        {
            throw new Exception("OTP không hợp lệ hoặc đã hết hạn.");
        }
        var session = await _supabaseClient.Auth.SignInWithPassword(email, oldPassword);
        if (session == null)
            throw new Exception("Mật khẩu cũ không chính xác. Vui lòng thử lại.");
        var updatedResult = await _supabaseClient.Auth.Update(new Supabase.Gotrue.UserAttributes
        {
            Password = newPassword
        });
        if (updatedResult == null)
            throw new Exception("Không thể thay đổi mật khẩu cho người dùng.");
        return user.ToResponse();
    }

    private async Task<bool> VerifyOTPCode(User user, string otpCode)
    {
        var otps =  await _otpCodeRepository.GetAllAsync();
        foreach (var otp in otps)
        {
            if (otp.email == user.email && otp.is_verified == false &&
                String.Equals(otp.otp, otpCode) &&
                otp.expires_at.HasValue && otp.expires_at.Value > DateTime.UtcNow)
            {
                otp.is_verified = true;
                await _otpCodeRepository.UpdateAsync(otp.id, otp);
                return true;
            }
        }

        return false;
    }
}