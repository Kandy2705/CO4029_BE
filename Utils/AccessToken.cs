using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.Domain.Entities;
using Supabase;

namespace CO4029_BE.Utils;

public static class AccessToken
{
    public static Task<string> GetAccessToken(HttpContext context)
    {
        string? authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Thiếu Access token trong header hoặc sai định dạng (Bearer ...)");
        }
        return Task.FromResult(authorizationHeader.Substring("Bearer ".Length).Trim());
    }
    
    public static async Task<User> GetUser(string accessToken, Client _supabaseClient, IUserRepository _userRepository)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new UnauthorizedAccessException("Token không hợp lệ.");
        var gotrueUser = await _supabaseClient.Auth.GetUser(accessToken);
        if (gotrueUser == null)
            throw new UnauthorizedAccessException("Token Supabase không hợp lệ hoặc đã hết hạn.");
        var email = gotrueUser.Email;
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Token không chứa tài khoản hợp lệ.");
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            throw new UnauthorizedAccessException("Tài khoản không hợp lệ.");
        if (!user.is_active)
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa.");
        return user;
    }

    public static async Task<User> GetAdminUser(
        string accessToken,
        Client supabaseClient,
        IUserRepository userRepository)
    {
        var user = await GetUser(accessToken, supabaseClient, userRepository);
        AuthorizeHelper.EnsureAdmin(user);
        return user;
    }
}
