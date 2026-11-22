using CO4029_BE.Application.Services;
using CO4029_BE.Tests.Fakes;
using CO4029_BE.Utils;

namespace CO4029_BE.Tests;

public class OtpGeneratorTests
{
    [Fact]
    public void Generates_four_digits_by_default()
    {
        var otp = OTPGenerator.GenerateOTP();
        Assert.Equal(4, otp.Length);
        Assert.All(otp, c => Assert.InRange(c, '0', '9'));
    }

    [Fact]
    public void Every_digit_including_nine_can_appear()
    {
        var seen = new HashSet<char>();
        for (int i = 0; i < 2000 && seen.Count < 10; i++)
            foreach (var c in OTPGenerator.GenerateOTP(8))
                seen.Add(c);
        Assert.Equal(10, seen.Count);
    }

    [Fact]
    public void Rejects_non_positive_length()
        => Assert.Throws<ArgumentOutOfRangeException>(() => OTPGenerator.GenerateOTP(0));
}

public class OtpServiceTests
{
    private readonly TestBed bed = new();
    private const string Key = "reset:a@hcmut.edu.vn";
    private const string Mail = "a@hcmut.edu.vn";

    [Fact]
    public async Task Issue_emails_the_code_and_stores_it_for_five_minutes()
    {
        await bed.Otp.IssueAsync(Key, Mail);

        var (to, otp) = Assert.Single(bed.Mailer.Sent);
        Assert.Equal(Mail, to);
        var stored = Assert.Single(bed.Otps.Items.Values);
        Assert.Equal(otp, stored.otp);
        Assert.Equal(Key, stored.email);
        Assert.Equal(bed.Time.GetUtcNow().UtcDateTime.AddMinutes(5), stored.expires_at);
    }

    [Fact]
    public async Task Second_request_within_cooldown_is_refused_with_wait_time()
    {
        await bed.Otp.IssueAsync(Key, Mail);
        bed.Time.Advance(TimeSpan.FromSeconds(20));

        var ex = await Assert.ThrowsAsync<OtpCooldownException>(() => bed.Otp.IssueAsync(Key, Mail));
        Assert.Equal(40, ex.RetryAfterSeconds);
        Assert.Single(bed.Mailer.Sent);
    }

    [Fact]
    public async Task New_code_after_cooldown_replaces_the_old_one()
    {
        await bed.Otp.IssueAsync(Key, Mail);
        var first = bed.Mailer.LastOtpTo(Mail);
        bed.Time.Advance(TimeSpan.FromSeconds(61));
        await bed.Otp.IssueAsync(Key, Mail);
        var second = bed.Mailer.LastOtpTo(Mail);

        if (first != second)
            Assert.False(await bed.Otp.ConsumeAsync(Key, first));
        Assert.True(await bed.Otp.ConsumeAsync(Key, second));
    }

    [Fact]
    public async Task Code_can_be_consumed_only_once()
    {
        await bed.Otp.IssueAsync(Key, Mail);
        var otp = bed.Mailer.LastOtpTo(Mail);

        Assert.True(await bed.Otp.ConsumeAsync(Key, otp));
        Assert.False(await bed.Otp.ConsumeAsync(Key, otp));
    }

    [Fact]
    public async Task Check_does_not_consume()
    {
        await bed.Otp.IssueAsync(Key, Mail);
        var otp = bed.Mailer.LastOtpTo(Mail);

        Assert.True(await bed.Otp.CheckAsync(Key, otp));
        Assert.True(await bed.Otp.ConsumeAsync(Key, otp));
    }

    [Fact]
    public async Task Expired_code_is_rejected()
    {
        await bed.Otp.IssueAsync(Key, Mail);
        var otp = bed.Mailer.LastOtpTo(Mail);
        bed.Time.Advance(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(1));

        Assert.False(await bed.Otp.ConsumeAsync(Key, otp));
    }

    [Fact]
    public async Task Code_is_bound_to_its_key()
    {
        await bed.Otp.IssueAsync("register:" + Mail, Mail);
        var otp = bed.Mailer.LastOtpTo(Mail);

        Assert.False(await bed.Otp.ConsumeAsync(Key, otp));
        Assert.True(await bed.Otp.ConsumeAsync("register:" + Mail, otp));
    }

    [Fact]
    public async Task Five_wrong_guesses_invalidate_the_code()
    {
        await bed.Otp.IssueAsync(Key, Mail);
        var otp = bed.Mailer.LastOtpTo(Mail);
        var wrong = otp == "0000" ? "1111" : "0000";

        for (int i = 0; i < OtpService.MaxFailedAttempts; i++)
            Assert.False(await bed.Otp.ConsumeAsync(Key, wrong));

        Assert.False(await bed.Otp.ConsumeAsync(Key, otp));
    }

    [Fact]
    public async Task Failed_mail_stores_no_code()
    {
        bed.Mailer.Fail = true;
        await Assert.ThrowsAsync<IOException>(() => bed.Otp.IssueAsync(Key, Mail));
        Assert.Empty(bed.Otps.Items);
    }

    [Fact]
    public void Purpose_keys_are_normalised_and_distinct()
    {
        Assert.Equal("a@b.vn", OtpPurpose.PasswordChange(" A@B.vn "));
        Assert.Equal("reset:a@b.vn", OtpPurpose.PasswordReset("A@b.vn"));
        Assert.Equal("register:a@b.vn", OtpPurpose.Register("a@B.VN"));
        Assert.Equal("email-change:u1:a@b.vn", OtpPurpose.EmailChange("u1", "A@b.vn"));
    }
}
