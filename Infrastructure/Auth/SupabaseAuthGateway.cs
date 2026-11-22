using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Application.Abstractions;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;

namespace CO4029_BE.Infrastructure.Auth;

/// <summary>
/// Talks to Supabase Auth without signing the shared <see cref="Supabase.Client"/> in as an end user.
/// Password checks and token refresh use the Auth REST endpoints; account changes use the admin API,
/// which needs the service-role key (Supabase__ServiceRoleKey).
/// </summary>
public class SupabaseAuthGateway : IAuthGateway
{
    private readonly HttpClient _http;
    private readonly Supabase.Client _client;
    private readonly string _url;
    private readonly string _apiKey;
    private readonly string? _adminKey;

    public SupabaseAuthGateway(HttpClient http, Supabase.Client client, IConfiguration config)
    {
        _http = http;
        _client = client;
        _url = (First(config["Supabase:Url"], config["supabaseUrl"])
                ?? throw new InvalidOperationException("Supabase URL chưa được cấu hình.")).TrimEnd('/');
        _adminKey = First(config["Supabase:ServiceRoleKey"], config["SupabaseAdminKey"]);
        _apiKey = First(config["Supabase:PublishableKey"], config["supabaseApiKey"], _adminKey)
                  ?? throw new InvalidOperationException("Supabase API key chưa được cấu hình.");
    }

    public async Task<LoginResponse?> SignInAsync(string email, string password)
    {
        using var response = await PostTokenAsync("password", new { email, password });
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            return null;
        response.EnsureSuccessStatusCode();
        return await ReadSessionAsync(response);
    }

    public async Task<LoginResponse> RefreshSessionAsync(string refreshToken)
    {
        using var response = await PostTokenAsync("refresh_token", new { refresh_token = refreshToken });
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn.");
        response.EnsureSuccessStatusCode();
        return await ReadSessionAsync(response);
    }

    public async Task<string> CreateConfirmedUserAsync(
        string email, string password, IDictionary<string, object> metadata)
    {
        Supabase.Gotrue.User? created;
        try
        {
            created = await Admin().CreateUser(email, password, new AdminUserAttributes
            {
                EmailConfirm = true,
                UserMetadata = new Dictionary<string, object>(metadata)
            });
        }
        catch (GotrueException ex) when (ex.StatusCode == 422)
        {
            throw new InvalidOperationException("Email đã được sử dụng.", ex);
        }
        return created?.Id ?? throw new InvalidOperationException("Không thể tạo tài khoản Supabase Auth.");
    }

    public Task DeleteUserAsync(string authUserId) => Admin().DeleteUser(authUserId);

    private static async Task<LoginResponse> ReadSessionAsync(HttpResponseMessage response)
    {

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>()
                   ?? throw new InvalidOperationException("Supabase Auth không trả về phiên đăng nhập.");
        if (string.IsNullOrEmpty(body.AccessToken) || string.IsNullOrEmpty(body.RefreshToken))
            throw new InvalidOperationException("Supabase Auth không trả về phiên đăng nhập.");

        DateTime? expiresAt = body.ExpiresAt is > 0
            ? DateTimeOffset.FromUnixTimeSeconds(body.ExpiresAt.Value).UtcDateTime
            : body.ExpiresIn is > 0 ? DateTime.UtcNow.AddSeconds(body.ExpiresIn.Value) : null;
        return new LoginResponse
        {
            AccessToken = body.AccessToken,
            RefreshToken = body.RefreshToken,
            ExpiresAt = expiresAt
        };
    }

    public async Task UpdatePasswordAsync(string authUserId, string newPassword)
    {
        var result = await Admin().UpdateUserById(authUserId, new AdminUserAttributes { Password = newPassword });
        if (result == null)
            throw new InvalidOperationException("Không thể cập nhật mật khẩu.");
    }

    public async Task UpdateEmailAsync(string authUserId, string newEmail)
    {
        var result = await Admin().UpdateUserById(authUserId, new AdminUserAttributes
        {
            Email = newEmail,
            EmailConfirm = true
        });
        if (result == null)
            throw new InvalidOperationException("Không thể cập nhật email.");
    }

    private IGotrueAdminClient<User> Admin()
    {
        if (string.IsNullOrWhiteSpace(_adminKey))
            throw new InvalidOperationException("Supabase Admin Auth chưa được cấu hình.");
        return _client.AdminAuth(_adminKey);
    }

    private Task<HttpResponseMessage> PostTokenAsync(string grantType, object payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_url}/auth/v1/token?grant_type={grantType}")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("apikey", _apiKey);
        return _http.SendAsync(request);
    }

    private static string? First(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")] public string? AccessToken { get; set; }
        [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; }
        [JsonPropertyName("expires_in")] public long? ExpiresIn { get; set; }
        [JsonPropertyName("expires_at")] public long? ExpiresAt { get; set; }
    }
}
