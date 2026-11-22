using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.Application.Services;
using CO4029_BE.Tests.Fakes;

namespace CO4029_BE.Tests;

public class AccountServiceTests
{
    private readonly TestBed bed = new();

    [Fact]
    public async Task Register_otp_is_sent_to_a_new_email()
    {
        await bed.Account.RequestRegisterOtpAsync("new@hcmut.edu.vn");
        Assert.Equal("new@hcmut.edu.vn", Assert.Single(bed.Mailer.Sent).To);
        Assert.Equal("register:new@hcmut.edu.vn", Assert.Single(bed.Otps.Items.Values).email);
    }

    [Fact]
    public async Task Register_otp_is_refused_for_a_registered_email()
    {
        bed.AddUser("taken@hcmut.edu.vn");
        await Assert.ThrowsAsync<InvalidOperationException>(() => bed.Account.RequestRegisterOtpAsync("taken@hcmut.edu.vn"));
        Assert.Empty(bed.Mailer.Sent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("a@b")]
    [InlineData("Name <a@b.vn>")]
    public async Task Malformed_email_is_rejected(string email)
        => await Assert.ThrowsAsync<ArgumentException>(() => bed.Account.RequestRegisterOtpAsync(email));

    [Fact]
    public async Task Forgot_password_is_silent_for_unknown_and_inactive_emails()
    {
        bed.AddUser("off@hcmut.edu.vn", active: false);
        await bed.Account.RequestPasswordResetAsync("nobody@hcmut.edu.vn");
        await bed.Account.RequestPasswordResetAsync("off@hcmut.edu.vn");
        Assert.Empty(bed.Mailer.Sent);
    }

    [Fact]
    public async Task Forgot_password_is_silent_during_cooldown()
    {
        bed.AddUser("a@hcmut.edu.vn");
        await bed.Account.RequestPasswordResetAsync("a@hcmut.edu.vn");
        await bed.Account.RequestPasswordResetAsync("a@hcmut.edu.vn");
        Assert.Single(bed.Mailer.Sent);
    }

    [Fact]
    public async Task Reset_password_with_valid_code_changes_the_password()
    {
        bed.AddUser("a@hcmut.edu.vn", "oldpass1");
        await bed.Account.RequestPasswordResetAsync("a@hcmut.edu.vn");
        var otp = bed.Mailer.LastOtpTo("a@hcmut.edu.vn");

        Assert.True(await bed.Account.VerifyOtpAsync(new VerifyOtpRequest
            { Email = "a@hcmut.edu.vn", OtpCode = otp, Purpose = "reset" }));
        await bed.Account.ResetPasswordAsync(new ResetPasswordRequest
            { Email = "a@hcmut.edu.vn", OtpCode = otp, NewPassword = "newpass1" });

        Assert.NotNull(await bed.Auth.SignInAsync("a@hcmut.edu.vn", "newpass1"));
        Assert.Null(await bed.Auth.SignInAsync("a@hcmut.edu.vn", "oldpass1"));
    }

    [Fact]
    public async Task Reset_password_rejects_a_wrong_code_and_short_password()
    {
        bed.AddUser("a@hcmut.edu.vn", "oldpass1");
        await bed.Account.RequestPasswordResetAsync("a@hcmut.edu.vn");
        var otp = bed.Mailer.LastOtpTo("a@hcmut.edu.vn");
        var wrong = otp == "0000" ? "1111" : "0000";

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => bed.Account.ResetPasswordAsync(
            new ResetPasswordRequest { Email = "a@hcmut.edu.vn", OtpCode = wrong, NewPassword = "newpass1" }));
        Assert.Equal(AccountService.InvalidOtpMessage, ex.Message);
        await Assert.ThrowsAsync<ArgumentException>(() => bed.Account.ResetPasswordAsync(
            new ResetPasswordRequest { Email = "a@hcmut.edu.vn", OtpCode = otp, NewPassword = "123" }));
        Assert.NotNull(await bed.Auth.SignInAsync("a@hcmut.edu.vn", "oldpass1"));
    }

    [Fact]
    public async Task Registration_code_cannot_reset_a_password()
    {
        bed.AddUser("a@hcmut.edu.vn", "oldpass1");
        await bed.Otp.IssueAsync(OtpPurpose.Register("a@hcmut.edu.vn"), "a@hcmut.edu.vn");
        var otp = bed.Mailer.LastOtpTo("a@hcmut.edu.vn");

        await Assert.ThrowsAsync<ArgumentException>(() => bed.Account.ResetPasswordAsync(
            new ResetPasswordRequest { Email = "a@hcmut.edu.vn", OtpCode = otp, NewPassword = "newpass1" }));
    }

    [Fact]
    public async Task Verify_otp_rejects_unknown_purpose()
        => await Assert.ThrowsAsync<ArgumentException>(() => bed.Account.VerifyOtpAsync(
            new VerifyOtpRequest { Email = "a@hcmut.edu.vn", OtpCode = "1234", Purpose = "email" }));

    [Fact]
    public async Task Email_change_sends_code_to_new_address_and_updates_both_stores()
    {
        var user = bed.AddUser("old@hcmut.edu.vn", "secret123");
        var token = TestBed.TokenFor(user);

        await bed.Account.RequestEmailChangeAsync(token, "new@hcmut.edu.vn");
        var otp = bed.Mailer.LastOtpTo("new@hcmut.edu.vn");
        var result = await bed.Account.ChangeEmailAsync(token,
            new ChangeEmailRequest { NewEmail = "new@hcmut.edu.vn", OtpCode = otp });

        Assert.Equal("new@hcmut.edu.vn", result.Email);
        Assert.Equal("new@hcmut.edu.vn", bed.Users.Items[user.id!].email);
        Assert.NotNull(await bed.Auth.SignInAsync("new@hcmut.edu.vn", "secret123"));
    }

    [Fact]
    public async Task Email_change_refuses_taken_or_same_address_and_wrong_code()
    {
        var user = bed.AddUser("old@hcmut.edu.vn");
        bed.AddUser("other@hcmut.edu.vn");
        var token = TestBed.TokenFor(user);

        await Assert.ThrowsAsync<InvalidOperationException>(() => bed.Account.RequestEmailChangeAsync(token, "other@hcmut.edu.vn"));
        await Assert.ThrowsAsync<ArgumentException>(() => bed.Account.RequestEmailChangeAsync(token, "OLD@hcmut.edu.vn"));
        await Assert.ThrowsAsync<ArgumentException>(() => bed.Account.ChangeEmailAsync(token,
            new ChangeEmailRequest { NewEmail = "new@hcmut.edu.vn", OtpCode = "1234" }));
        Assert.Equal("old@hcmut.edu.vn", bed.Users.Items[user.id!].email);
    }

    [Fact]
    public async Task Email_change_code_of_one_user_does_not_work_for_another()
    {
        var alice = bed.AddUser("alice@hcmut.edu.vn");
        var bob = bed.AddUser("bob@hcmut.edu.vn");
        await bed.Account.RequestEmailChangeAsync(TestBed.TokenFor(alice), "new@hcmut.edu.vn");
        var otp = bed.Mailer.LastOtpTo("new@hcmut.edu.vn");

        await Assert.ThrowsAsync<ArgumentException>(() => bed.Account.ChangeEmailAsync(TestBed.TokenFor(bob),
            new ChangeEmailRequest { NewEmail = "new@hcmut.edu.vn", OtpCode = otp }));
    }

    [Fact]
    public async Task Email_change_requires_a_valid_token()
        => await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            bed.Account.RequestEmailChangeAsync("garbage", "new@hcmut.edu.vn"));

    [Fact]
    public async Task Refresh_returns_a_new_session_and_rotates_the_refresh_token()
    {
        bed.AddUser("a@hcmut.edu.vn", "secret123");
        var login = await bed.AuthService.LoginAsync(new LoginRequest { Email = "a@hcmut.edu.vn", Password = "secret123" });

        var refreshed = await bed.Account.RefreshAsync(login.RefreshToken);

        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => bed.Account.RefreshAsync(login.RefreshToken));
    }

    [Fact]
    public async Task Refresh_is_refused_once_the_account_is_deactivated()
    {
        var user = bed.AddUser("a@hcmut.edu.vn", "secret123");
        var login = await bed.AuthService.LoginAsync(new LoginRequest { Email = "a@hcmut.edu.vn", Password = "secret123" });
        user.is_active = false;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => bed.Account.RefreshAsync(login.RefreshToken));
        await Assert.ThrowsAsync<ArgumentException>(() => bed.Account.RefreshAsync(" "));
    }
}
