using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CO4029_BE.Application.Abstractions;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CO4029_BE.Tests;

/// <summary>Black-box HTTP tests of the new endpoints: real routing, binding and error mapping; fake Supabase.</summary>
public class ApiTests : IClassFixture<ApiTests.Factory>
{
    public sealed class Factory : WebApplicationFactory<Program>
    {
        public TestBed Bed { get; } = new();
        public InMemoryBuildingRepository Buildings { get; } = new();
        public InMemoryFloorRepository Floors { get; } = new();
        public InMemoryRoomRepository Rooms { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("Supabase:Url", "http://localhost:54321");
            builder.UseSetting("Supabase:PublishableKey", "test-key");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IUserRepository>();
                services.RemoveAll<IOtpCodeRepository>();
                services.RemoveAll<IBuildingRepository>();
                services.RemoveAll<IFloorRepository>();
                services.RemoveAll<IRoomRepository>();
                services.RemoveAll<IAuthGateway>();
                services.RemoveAll<ICurrentUserResolver>();
                services.RemoveAll<IOtpMailer>();
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<IUserRepository>(Bed.Users);
                services.AddSingleton<IOtpCodeRepository>(Bed.Otps);
                services.AddSingleton<IBuildingRepository>(Buildings);
                services.AddSingleton<IFloorRepository>(Floors);
                services.AddSingleton<IRoomRepository>(Rooms);
                services.AddSingleton<IAuthGateway>(Bed.Auth);
                services.AddSingleton<IOtpMailer>(Bed.Mailer);
                services.AddSingleton<TimeProvider>(Bed.Time);
                services.AddScoped<ICurrentUserResolver, FakeCurrentUserResolver>();
            });
        }
    }

    private readonly Factory factory;
    private readonly HttpClient client;
    private TestBed Bed => factory.Bed;

    public ApiTests(Factory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    private static async Task<JsonElement> Body(HttpResponseMessage response)
        => await response.Content.ReadFromJsonAsync<JsonElement>();

    private static string Unique(string name) => $"{name}-{Guid.NewGuid():N}@hcmut.edu.vn";

    [Fact]
    public async Task Full_registration_flow_over_http()
    {
        var email = Unique("reg");
        var otpResponse = await client.PostAsJsonAsync("/api/v1/users/register/request-otp", new { email });
        Assert.Equal(HttpStatusCode.OK, otpResponse.StatusCode);
        var otp = Bed.Mailer.LastOtpTo(email);

        var verify = await Body(await client.PostAsJsonAsync("/api/v1/users/verify-otp",
            new { email, otpCode = otp, purpose = "register" }));
        Assert.True(verify.GetProperty("data").GetBoolean());

        var create = await client.PostAsJsonAsync("/api/v1/users/create-customer",
            new { email, password = "secret123", name = "SV", otpCode = otp });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        Assert.Equal("Customer", (await Body(create)).GetProperty("data").GetProperty("role").GetString());

        var login = await client.PostAsJsonAsync("/api/v1/users/login", new { email, password = "secret123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Create_customer_without_code_is_400()
    {
        var response = await client.PostAsJsonAsync("/api/v1/users/create-customer",
            new { email = Unique("nocode"), password = "secret123" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("BAD_REQUEST", (await Body(response)).GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Register_otp_for_taken_email_is_409_and_resend_too_soon_is_429()
    {
        var taken = Unique("taken");
        Bed.AddUser(taken);
        var conflict = await client.PostAsJsonAsync("/api/v1/users/register/request-otp", new { email = taken });
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);

        var email = Unique("cool");
        await client.PostAsJsonAsync("/api/v1/users/register/request-otp", new { email });
        var again = await client.PostAsJsonAsync("/api/v1/users/register/request-otp", new { email });
        Assert.Equal((HttpStatusCode)429, again.StatusCode);
        Assert.True(again.Headers.Contains("Retry-After"));
    }

    [Fact]
    public async Task Forgot_password_answers_the_same_for_known_and_unknown_emails()
    {
        var known = Unique("known");
        Bed.AddUser(known, "oldpass1");
        var a = await client.PostAsJsonAsync("/api/v1/users/forgot-password", new { email = known });
        var b = await client.PostAsJsonAsync("/api/v1/users/forgot-password", new { email = Unique("ghost") });

        Assert.Equal(HttpStatusCode.OK, a.StatusCode);
        Assert.Equal(HttpStatusCode.OK, b.StatusCode);
        Assert.Equal((await Body(a)).GetProperty("message").GetString(), (await Body(b)).GetProperty("message").GetString());

        var otp = Bed.Mailer.LastOtpTo(known);
        var bad = await client.PostAsJsonAsync("/api/v1/users/reset-password",
            new { email = known, otpCode = otp == "0000" ? "1111" : "0000", newPassword = "newpass1" });
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        var ok = await client.PostAsJsonAsync("/api/v1/users/reset-password",
            new { email = known, otpCode = otp, newPassword = "newpass1" });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.NotNull(await Bed.Auth.SignInAsync(known, "newpass1"));
    }

    [Fact]
    public async Task Email_change_over_http_requires_bearer_token()
    {
        var anonymous = await client.PostAsJsonAsync("/api/v1/users/request-email-change", new { newEmail = Unique("x") });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var user = Bed.AddUser(Unique("old"));
        var newEmail = Unique("new");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users/request-email-change")
        {
            Content = JsonContent.Create(new { newEmail })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestBed.TokenFor(user));
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(request)).StatusCode);

        using var change = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users/change-email")
        {
            Content = JsonContent.Create(new { newEmail, otpCode = Bed.Mailer.LastOtpTo(newEmail) })
        };
        change.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestBed.TokenFor(user));
        var changed = await client.SendAsync(change);
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        Assert.Equal(newEmail, (await Body(changed)).GetProperty("data").GetProperty("email").GetString());
    }

    [Fact]
    public async Task Refresh_token_endpoint()
    {
        var email = Unique("refresh");
        Bed.AddUser(email, "secret123");
        var login = await Body(await client.PostAsJsonAsync("/api/v1/users/login", new { email, password = "secret123" }));
        var refresh = login.GetProperty("data").GetProperty("refreshToken").GetString();

        var ok = await client.PostAsJsonAsync("/api/v1/users/refresh-token", new { refreshToken = refresh });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        var reused = await client.PostAsJsonAsync("/api/v1/users/refresh-token", new { refreshToken = refresh });
        Assert.Equal(HttpStatusCode.Unauthorized, reused.StatusCode);
    }

    [Fact]
    public async Task Campus_map_is_401_without_token_and_200_for_a_customer()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/campus/map")).StatusCode);

        await factory.Buildings.CreateAsync(new Building { ID = "b9-" + Guid.NewGuid(), Name = "B9" });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/campus/map");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestBed.TokenFor(Bed.AddUser(Unique("map"))));
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains((await Body(response)).GetProperty("data").GetProperty("buildings").EnumerateArray(),
            b => b.GetProperty("name").GetString() == "B9");
    }

    [Fact]
    public async Task Unexpected_errors_do_not_leak_details()
    {
        Bed.Mailer.Fail = true;
        try
        {
            var response = await client.PostAsJsonAsync("/api/v1/users/register/request-otp", new { email = Unique("smtp") });
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            var message = (await Body(response)).GetProperty("message").GetString();
            Assert.DoesNotContain("smtp", message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Bed.Mailer.Fail = false;
        }
    }
}
