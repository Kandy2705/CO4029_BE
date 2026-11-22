using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Application.Abstractions;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Infrastructure.Repository;

namespace CO4029_BE.Tests.Fakes;

public class RecordingMailer : IOtpMailer
{
    public List<(string To, string Otp)> Sent { get; } = new();
    public bool Fail { get; set; }

    public void SendOTPMail(string recipientMail, string otp)
    {
        if (Fail)
            throw new IOException("smtp.gmail.com:587 authentication failed");
        Sent.Add((recipientMail, otp));
    }

    public string LastOtpTo(string email) => Sent.Last(s => s.To == email).Otp;
}

/// <summary>Stands in for Supabase Auth: email → (auth id, password); tokens are "token-{authId}".</summary>
public class FakeAuthGateway : IAuthGateway
{
    public Dictionary<string, (string Id, string Password)> Accounts { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> RefreshTokens { get; } = new(); // refresh token -> auth id
    public List<string> Deleted { get; } = new();

    public string AddAccount(string email, string password, string? id = null)
    {
        id ??= Guid.NewGuid().ToString();
        Accounts[email] = (id, password);
        return id;
    }

    public Task<LoginResponse?> SignInAsync(string email, string password)
    {
        if (Accounts.TryGetValue(email, out var account) && account.Password == password)
            return Task.FromResult<LoginResponse?>(Session(account.Id));
        return Task.FromResult<LoginResponse?>(null);
    }

    public Task<string> CreateConfirmedUserAsync(string email, string password, IDictionary<string, object> metadata)
    {
        if (Accounts.ContainsKey(email))
            throw new InvalidOperationException("Email đã được sử dụng.");
        return Task.FromResult(AddAccount(email, password));
    }

    public Task UpdatePasswordAsync(string authUserId, string newPassword)
    {
        var email = Accounts.First(a => a.Value.Id == authUserId).Key;
        Accounts[email] = (authUserId, newPassword);
        return Task.CompletedTask;
    }

    public Task UpdateEmailAsync(string authUserId, string newEmail)
    {
        var (email, account) = Accounts.First(a => a.Value.Id == authUserId);
        Accounts.Remove(email);
        Accounts[newEmail] = account;
        return Task.CompletedTask;
    }

    public Task DeleteUserAsync(string authUserId)
    {
        Deleted.Add(authUserId);
        var email = Accounts.FirstOrDefault(a => a.Value.Id == authUserId).Key;
        if (email != null)
            Accounts.Remove(email);
        return Task.CompletedTask;
    }

    public Task<LoginResponse> RefreshSessionAsync(string refreshToken)
    {
        if (!RefreshTokens.Remove(refreshToken, out var id))
            throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn.");
        return Task.FromResult(Session(id));
    }

    private LoginResponse Session(string id)
    {
        var refresh = "refresh-" + Guid.NewGuid();
        RefreshTokens[refresh] = id;
        return new LoginResponse { AccessToken = "token-" + id, RefreshToken = refresh, ExpiresAt = DateTime.UtcNow.AddHours(1) };
    }
}

/// <summary>Accepts tokens of the form "token-{userId}" for active users, like AccessToken.GetUser.</summary>
public class FakeCurrentUserResolver : ICurrentUserResolver
{
    private readonly IUserRepository _users;

    public FakeCurrentUserResolver(IUserRepository users) => _users = users;

    public async Task<User> ResolveAsync(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken) || !accessToken.StartsWith("token-"))
            throw new UnauthorizedAccessException("Token Supabase không hợp lệ hoặc đã hết hạn.");
        var user = await _users.GetByIdAsync(accessToken["token-".Length..]);
        if (user == null)
            throw new UnauthorizedAccessException("Tài khoản không hợp lệ.");
        if (!user.is_active)
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa.");
        return user;
    }
}
