using CO4029_BE.Utils;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CO4029_BE.Tests;

/// <summary>
/// The Gmail account that sends OTP codes comes from App Settings. When it is missing the request must fail as a
/// server error (500, generic message), not as MailKit's "Value cannot be null (Parameter 'address')" bad request.
/// </summary>
public class MailConfigTests
{
    private static EmailService Service(params (string Key, string? Value)[] settings)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(settings.Select(s => new KeyValuePair<string, string?>(s.Key, s.Value)))
            .Build();
        return new EmailService(config);
    }

    [Fact]
    public void Missing_sender_is_reported_as_not_configured()
    {
        var service = Service((EmailService.AppPasswordKey, "app-password"));

        Assert.False(service.IsConfigured);
        var error = Assert.Throws<MailNotConfiguredException>(() => service.SendOTPMail("someone@hcmut.edu.vn", "1234"));
        Assert.IsAssignableFrom<IOException>(error);
        Assert.Contains(EmailService.SenderKey, error.Message);
    }

    [Fact]
    public void Missing_app_password_is_reported_as_not_configured()
    {
        var service = Service((EmailService.SenderKey, "sender@gmail.com"), (EmailService.AppPasswordKey, " "));

        Assert.False(service.IsConfigured);
        Assert.Throws<MailNotConfiguredException>(() => service.SendOTPMail("someone@hcmut.edu.vn", "1234"));
    }

    [Fact]
    public void Both_settings_present_counts_as_configured()
    {
        Assert.True(Service((EmailService.SenderKey, "sender@gmail.com"), (EmailService.AppPasswordKey, "app-password")).IsConfigured);
    }

    [Fact]
    public void Not_configured_is_not_an_argument_error()
    {
        // ArgumentException would be mapped to 400 with the raw message shown in the app.
        Assert.False(typeof(ArgumentException).IsAssignableFrom(typeof(MailNotConfiguredException)));
    }
}
