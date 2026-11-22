using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.Domain.Entities;
using Supabase;
using Supabase.Gotrue.Exceptions;

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
        Supabase.Gotrue.User? gotrueUser;
        try
        {
            gotrueUser = await _supabaseClient.Auth.GetUser(accessToken);
        }
        catch (GotrueException ex) when (IsRejectedToken(ex))
        {
            // Supabase rejects malformed, expired or revoked JWTs by throwing instead of returning null.
            throw new UnauthorizedAccessException("Token Supabase không hợp lệ hoặc đã hết hạn.", ex);
        }
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

    // Status codes Supabase Auth uses when the token itself is the problem. Network failures,
    // rate limiting (429) and Supabase server errors (5xx) are not token problems, so they
    // still surface as 500 instead of being reported to the client as an invalid login.
    private static bool IsRejectedToken(GotrueException ex) =>
        ex.StatusCode is 400 or 401 or 403 or 404 or 422;

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
