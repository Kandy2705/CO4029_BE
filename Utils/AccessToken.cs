using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.Domain.Entities;
using Supabase;

namespace CO4029_BE.Utils;

public static class AccessToken
{
    public static async Task<String> GetAccessToken(HttpContext context)
    {
        string authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Thiếu Access token trong header hoặc sai định dạng (Bearer ...)");
        }
        return authorizationHeader.Substring("Bearer ".Length).Trim();
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
            throw new Exception("Không lấy được email từ Supabase user.");
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            throw new Exception("Không tìm thấy user trong database.");
        return user;
    }
}