using CO4029_BE.API.Contracts.Responses;

namespace CO4029_BE.Application.Abstractions;

/// <summary>
/// Operations on Supabase Auth that must not touch the session of the shared <see cref="Supabase.Client"/>
/// singleton: they go through the Auth REST API or the admin (service-role) API instead.
/// </summary>
public interface IAuthGateway
{
    /// <summary>Password sign-in; returns null when Supabase Auth rejects the credentials.</summary>
    Task<LoginResponse?> SignInAsync(string email, string password);

    /// <summary>Creates an Auth user whose email is already confirmed (ownership proven by OTP); returns its id.</summary>
    Task<string> CreateConfirmedUserAsync(string email, string password, IDictionary<string, object> metadata);

    /// <summary>Sets a new password for the Auth user (admin API).</summary>
    Task UpdatePasswordAsync(string authUserId, string newPassword);

    /// <summary>Changes the Auth user's email and marks it confirmed (admin API).</summary>
    Task UpdateEmailAsync(string authUserId, string newEmail);

    /// <summary>Deletes the Auth user (admin API).</summary>
    Task DeleteUserAsync(string authUserId);

    /// <summary>Exchanges a refresh token for a new session.</summary>
    Task<LoginResponse> RefreshSessionAsync(string refreshToken);
}
