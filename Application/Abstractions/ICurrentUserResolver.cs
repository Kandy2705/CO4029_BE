using CO4029_BE.Domain.Entities;

namespace CO4029_BE.Application.Abstractions;

/// <summary>Resolves the active application user that owns a Supabase access token.</summary>
public interface ICurrentUserResolver
{
    /// <exception cref="UnauthorizedAccessException">Token invalid/expired or account inactive.</exception>
    Task<User> ResolveAsync(string accessToken);
}
