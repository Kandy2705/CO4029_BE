using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.API.Contracts.Mappers;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Utils;
using Supabase;

namespace CO4029_BE.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly Client _supabaseClient;
    private IConfiguration configuration;

    public UserService(IUserRepository userRepository, Client supabaseClient, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _supabaseClient = supabaseClient;
        this.configuration = configuration;
    }

    public async Task<UserReponse> RegisterCustomerAsync(CreateUserRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new InvalidOperationException("Email đã được sử dụng.");

        var metadata = new Dictionary<string, object?>
        {
            ["name"] = request.Name,
            ["phone"] = request.Phone,
            ["birthday"] = request.Birthday?.ToString("yyyy-MM-dd")
        };

        var signUpResult = await _supabaseClient.Auth.SignUp(
            request.Email,
            request.Password,
            new Supabase.Gotrue.SignUpOptions
            {
                Data = metadata
            }
        );

        var authUser = signUpResult?.User;
        if (authUser == null)
            throw new InvalidOperationException("Không thể tạo tài khoản Supabase Auth.");

        var user = new User
        {
            id = authUser.Id,
            email = request.Email,
            name = request.Name,
            phone = request.Phone,
            birthday = request.Birthday,
            gender = request.Gender,
            role = "Customer"
        };

        var created = await _userRepository.CreateAsync(user);
        return created.ToResponse();
    }

    public async Task<UserReponse> UpdateUserAsync(UpdateUserRequest request, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, _supabaseClient, _userRepository);

        user.name = request.Name ?? user.name;
        user.phone = request.Phone ?? user.phone;
        user.gender = request.Gender ?? user.gender;
        user.birthday = request.Birthday ??  user.birthday;
        await _userRepository.UpdateAsync(user.id, user);
        return user.ToResponse();
    }

    public async Task<bool> DeleteUserAsync(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new UnauthorizedAccessException("Token không hợp lệ.");
        var gotrueUser = await _supabaseClient.Auth.GetUser(accessToken);
        if (gotrueUser == null)
            throw new UnauthorizedAccessException("Token Supabase không hợp lệ hoặc đã hết hạn.");
        var email = gotrueUser.Email;
        if (string.IsNullOrEmpty(email))
            throw new Exception("Không lấy được email từ Supabase user.");
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            throw new KeyNotFoundException("Không tìm thấy user trong database.");
        
        await _userRepository.DeleteAsync(user.id);
        await _supabaseClient.AdminAuth(configuration["SupabaseAdminKey"])
            .DeleteUser(gotrueUser.Id);
        return true;
    }
}