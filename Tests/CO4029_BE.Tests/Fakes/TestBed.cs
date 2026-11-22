using CO4029_BE.Application.Services;
using CO4029_BE.Domain.Constants;
using CO4029_BE.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Time.Testing;

namespace CO4029_BE.Tests.Fakes;

/// <summary>Wires the services under test to in-memory fakes.</summary>
public class TestBed
{
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 22, 8, 0, 0, TimeSpan.Zero));
    public InMemoryOtpRepository Otps { get; } = new();
    public InMemoryUserRepository Users { get; } = new();
    public RecordingMailer Mailer { get; } = new();
    public FakeAuthGateway Auth { get; } = new();
    public MemoryCache Cache { get; } = new(new MemoryCacheOptions());

    public OtpService Otp => new(Otps, Mailer, Cache, Time);
    public FakeCurrentUserResolver CurrentUser => new(Users);
    public AccountService Account => new(Users, Otp, Auth, CurrentUser);
    public AuthService AuthService => new(Users, Otp, Auth, CurrentUser);

    /// <summary>Adds an Auth account and its profile row; returns the profile.</summary>
    public User AddUser(string email, string password = "secret123", bool active = true, string role = UserRoles.Customer)
    {
        var id = Auth.AddAccount(email, password);
        var user = new User { id = id, email = email, name = "Test", role = role, is_active = active };
        Users.Items[id] = user;
        return user;
    }

    public static string TokenFor(User user) => "token-" + user.id;
}
