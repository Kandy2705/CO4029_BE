using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.API.Contracts.Mappers;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Application.Abstractions;
using CO4029_BE.Domain.Constants;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Utils;
using Supabase;

namespace CO4029_BE.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly Client _supabaseClient;
    private readonly OtpService _otp;
    private readonly IAuthGateway _auth;

    public UserService(IUserRepository userRepository, Client supabaseClient, OtpService otp, IAuthGateway auth)
    {
        _userRepository = userRepository;
        _supabaseClient = supabaseClient;
        _otp = otp;
        _auth = auth;
    }

    /// <summary>
    /// Creates a customer once the registration OTP proves the email belongs to the registrant. The Auth
    /// user is created already confirmed through the admin API, so no confirmation link is needed.
    /// </summary>
    public async Task<UserReponse> RegisterCustomerAsync(CreateUserRequest request)
    {
        var email = InputValidator.RequireEmail(request.Email);
        var password = InputValidator.RequirePassword(request.Password);
        var otpCode = InputValidator.RequireOtp(request.OtpCode);

        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null)
            throw new InvalidOperationException("Email đã được sử dụng.");
        if (!await _otp.ConsumeAsync(OtpPurpose.Register(email), otpCode))
            throw new ArgumentException(AccountService.InvalidOtpMessage);

        var metadata = new Dictionary<string, object>
        {
            ["name"] = request.Name ?? string.Empty,
            ["phone"] = request.Phone ?? string.Empty,
            ["birthday"] = request.Birthday?.ToString("yyyy-MM-dd") ?? string.Empty
        };

        var authUserId = await _auth.CreateConfirmedUserAsync(email, password, metadata);

        var user = new User
        {
            id = authUserId,
            email = email,
            name = request.Name,
            phone = request.Phone,
            birthday = request.Birthday,
            gender = request.Gender,
            role = UserRoles.Customer,
            is_active = true
        };

        try
        {
            var created = await _userRepository.CreateAsync(user);
            return created.ToResponse();
        }
        catch
        {
            // Do not leave an Auth account without a profile: it would block registering this email again.
            await _auth.DeleteUserAsync(authUserId);
            throw;
        }
    }

    public async Task<UserReponse> UpdateUserAsync(UpdateUserRequest request, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, _supabaseClient, _userRepository);

        user.name = request.Name ?? user.name;
        user.phone = request.Phone ?? user.phone;
        user.gender = request.Gender ?? user.gender;
        user.birthday = request.Birthday ??  user.birthday;
        await _userRepository.UpdateAsync(user.id!, user);
        return user.ToResponse();
    }

    public async Task<bool> DeleteUserAsync(string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, _supabaseClient, _userRepository);
        var gotrueUser = await _supabaseClient.Auth.GetUser(accessToken);
        if (gotrueUser == null)
            throw new UnauthorizedAccessException("Token Supabase không hợp lệ hoặc đã hết hạn.");
        
        await _userRepository.DeleteAsync(user.id!);
        await _auth.DeleteUserAsync(gotrueUser.Id!);
        return true;
    }

    public async Task<PagedResponse<UserReponse>> GetUsersForAdminAsync(
        string accessToken,
        int page,
        int pageSize,
        string? search,
        string? role,
        bool? isActive)
    {
        await AccessToken.GetAdminUser(accessToken, _supabaseClient, _userRepository);
        ValidatePagination(page, pageSize);
        if (search?.Length > 100)
            throw new ArgumentException("search không được dài quá 100 ký tự.");

        string? normalizedRole = null;
        if (!string.IsNullOrWhiteSpace(role))
        {
            if (!UserRoles.IsValid(role))
                throw new ArgumentException("Role phải là Customer, Employee hoặc Admin.");
            normalizedRole = UserRoles.Normalize(role);
        }

        var offset = (page - 1) * pageSize;
        var result = await _userRepository.GetPagedAsync(
            offset,
            pageSize,
            search,
            normalizedRole,
            isActive);

        return PagedResponse<UserReponse>.Create(
            result.Items.Select(user => user.ToResponse()),
            page,
            pageSize,
            result.TotalItems);
    }

    public async Task<UserReponse> GetUserForAdminAsync(string accessToken, string userId)
    {
        await AccessToken.GetAdminUser(accessToken, _supabaseClient, _userRepository);
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Không tìm thấy người dùng.");
        return user.ToResponse();
    }

    public async Task<UserReponse> UpdateUserForAdminAsync(
        string accessToken,
        string userId,
        AdminUpdateUserRequest request)
    {
        await AccessToken.GetAdminUser(accessToken, _supabaseClient, _userRepository);
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.name = request.Name ?? user.name;
        user.phone = request.Phone ?? user.phone;
        user.birthday = request.Birthday ?? user.birthday;
        user.gender = request.Gender ?? user.gender;

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (!UserRoles.IsValid(request.Role))
                throw new ArgumentException("Role phải là Customer, Employee hoặc Admin.");
            user.role = UserRoles.Normalize(request.Role);
        }

        await _userRepository.UpdateAsync(userId, user);
        return user.ToResponse();
    }

    public async Task<UserReponse> UpdateUserStatusForAdminAsync(
        string accessToken,
        string userId,
        UpdateUserStatusRequest request)
    {
        await AccessToken.GetAdminUser(accessToken, _supabaseClient, _userRepository);
        if (!request.IsActive.HasValue)
            throw new ArgumentException("isActive là bắt buộc.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.is_active = request.IsActive.Value;
        await _userRepository.UpdateAsync(userId, user);
        return user.ToResponse();
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("page phải lớn hơn hoặc bằng 1.");
        if (pageSize is < 1 or > 100)
            throw new ArgumentException("pageSize phải nằm trong khoảng 1 đến 100.");
    }
}
