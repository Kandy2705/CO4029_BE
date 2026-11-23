using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.Application.Services;
using CO4029_BE.Domain.Constants;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Tests.Fakes;

namespace CO4029_BE.Tests;

public class AuthServiceTests
{
    private readonly TestBed bed = new();

    [Fact]
    public async Task Login_returns_tokens_for_valid_credentials()
    {
        bed.AddUser("a@hcmut.edu.vn", "secret123");
        var session = await bed.AuthService.LoginAsync(new LoginRequest { Email = " a@hcmut.edu.vn ", Password = "secret123" });
        Assert.StartsWith("token-", session.AccessToken);
        Assert.False(string.IsNullOrEmpty(session.RefreshToken));
    }

    [Fact]
    public async Task Login_rejects_wrong_password_inactive_account_and_missing_profile()
    {
        bed.AddUser("a@hcmut.edu.vn", "secret123");
        bed.AddUser("off@hcmut.edu.vn", "secret123", active: false);
        bed.Auth.AddAccount("noprofile@hcmut.edu.vn", "secret123");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => bed.AuthService.LoginAsync(
            new LoginRequest { Email = "a@hcmut.edu.vn", Password = "wrong" }));
        var inactive = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => bed.AuthService.LoginAsync(
            new LoginRequest { Email = "off@hcmut.edu.vn", Password = "secret123" }));
        Assert.Contains("vô hiệu hóa", inactive.Message);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => bed.AuthService.LoginAsync(
            new LoginRequest { Email = "noprofile@hcmut.edu.vn", Password = "secret123" }));
    }

    [Fact]
    public async Task Change_password_with_otp_and_old_password()
    {
        bed.AddUser("a@hcmut.edu.vn", "oldpass1");
        await bed.AuthService.SendOtpCode("a@hcmut.edu.vn");
        var otp = bed.Mailer.LastOtpTo("a@hcmut.edu.vn");

        await bed.AuthService.ChangePasswordAsync("a@hcmut.edu.vn", otp, "newpass1", "oldpass1");

        Assert.NotNull(await bed.Auth.SignInAsync("a@hcmut.edu.vn", "newpass1"));
    }

    [Fact]
    public async Task Wrong_old_password_does_not_burn_the_code()
    {
        bed.AddUser("a@hcmut.edu.vn", "oldpass1");
        await bed.AuthService.SendOtpCode("a@hcmut.edu.vn");
        var otp = bed.Mailer.LastOtpTo("a@hcmut.edu.vn");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            bed.AuthService.ChangePasswordAsync("a@hcmut.edu.vn", otp, "newpass1", "typo"));
        await bed.AuthService.ChangePasswordAsync("a@hcmut.edu.vn", otp, "newpass1", "oldpass1");
    }

    [Fact]
    public async Task Change_password_rejects_invalid_code()
    {
        bed.AddUser("a@hcmut.edu.vn", "oldpass1");
        await Assert.ThrowsAsync<ArgumentException>(() =>
            bed.AuthService.ChangePasswordAsync("a@hcmut.edu.vn", "1234", "newpass1", "oldpass1"));
        Assert.NotNull(await bed.Auth.SignInAsync("a@hcmut.edu.vn", "oldpass1"));
    }

    [Fact]
    public async Task Send_otp_requires_an_existing_active_account()
    {
        bed.AddUser("off@hcmut.edu.vn", active: false);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => bed.AuthService.SendOtpCode("none@hcmut.edu.vn"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => bed.AuthService.SendOtpCode("off@hcmut.edu.vn"));
    }
}

public class RegistrationTests
{
    private readonly TestBed bed = new();

    private UserService Users(InMemoryUserRepository? repo = null)
        => new(repo ?? bed.Users, new Supabase.Client("http://localhost", "test-key"), bed.Otp, bed.Auth);

    private static CreateUserRequest Request(string otp) => new()
    {
        Email = "new@hcmut.edu.vn",
        Password = "secret123",
        Name = "Sinh viên",
        Phone = "0900000000",
        Gender = "Nữ",
        OtpCode = otp
    };

    [Fact]
    public async Task Register_with_valid_code_creates_auth_account_and_customer_profile()
    {
        await bed.Account.RequestRegisterOtpAsync("new@hcmut.edu.vn");
        var otp = bed.Mailer.LastOtpTo("new@hcmut.edu.vn");

        var created = await Users().RegisterCustomerAsync(Request(otp));

        Assert.Equal(UserRoles.Customer, created.Role);
        Assert.True(created.IsActive);
        Assert.Equal(bed.Auth.Accounts["new@hcmut.edu.vn"].Id, created.Id);
        Assert.NotNull(await bed.Auth.SignInAsync("new@hcmut.edu.vn", "secret123"));
    }

    [Fact]
    public async Task Register_without_valid_code_creates_nothing()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Users().RegisterCustomerAsync(Request("")));
        await Assert.ThrowsAsync<ArgumentException>(() => Users().RegisterCustomerAsync(Request("1234")));
        Assert.Empty(bed.Auth.Accounts);
        Assert.Empty(bed.Users.Items);
    }

    [Fact]
    public async Task Register_rolls_back_the_auth_account_when_the_profile_cannot_be_saved()
    {
        await bed.Account.RequestRegisterOtpAsync("new@hcmut.edu.vn");
        var otp = bed.Mailer.LastOtpTo("new@hcmut.edu.vn");

        await Assert.ThrowsAsync<IOException>(() => Users(new FailingUserRepository()).RegisterCustomerAsync(Request(otp)));

        Assert.Single(bed.Auth.Deleted);
        Assert.Empty(bed.Auth.Accounts);
    }

    private sealed class FailingUserRepository : InMemoryUserRepository
    {
        public override Task<User> CreateAsync(User entity) => throw new IOException("db down");
    }
}

public class UserEntityMappingTests
{
    [Fact]
    public void Profile_id_is_sent_on_insert_so_it_matches_the_auth_user()
    {
        // With shouldInsert=false PostgREST left the id out and the column default generated a different uuid.
        var key = typeof(CO4029_BE.Domain.Entities.User).GetProperty("id")!
            .GetCustomAttributes(typeof(Supabase.Postgrest.Attributes.PrimaryKeyAttribute), false)
            .Cast<Supabase.Postgrest.Attributes.PrimaryKeyAttribute>().Single();
        Assert.True(key.ShouldInsert);
    }
}
