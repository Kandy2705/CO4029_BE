using CO4029_BE.Application.Abstractions;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.Utils;

namespace CO4029_BE.Infrastructure.Auth;

public class SupabaseCurrentUserResolver : ICurrentUserResolver
{
    private readonly Supabase.Client _client;
    private readonly IUserRepository _users;

    public SupabaseCurrentUserResolver(Supabase.Client client, IUserRepository users)
    {
        _client = client;
        _users = users;
    }

    public Task<User> ResolveAsync(string accessToken) => AccessToken.GetUser(accessToken, _client, _users);
}
