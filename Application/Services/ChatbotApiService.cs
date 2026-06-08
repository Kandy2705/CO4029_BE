using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AgenticAR.Application.Services;

public record ChatbotApiRequest(string message, string history_id, string? user_id);
public record ChatbotApiResponse(string type, JsonElement? content);

public class ChatbotApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ChatbotApiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(config["ChatbotUrl"]!);
        _apiKey = config["ChatbotApiKey"] ?? string.Empty;
    }

    public async Task<ChatbotApiResponse> AskAsync(string message, string historyId, string? userId = null)
    {
        var payload = new ChatbotApiRequest(message, historyId, userId);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/chat?format=tmp")
        {
            Content = JsonContent.Create(payload)
        };
        httpRequest.Headers.Add("X-API-Key", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ChatbotApiResponse>())!;
    }
}