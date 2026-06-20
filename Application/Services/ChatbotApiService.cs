using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace CO4029_BE.Application.Services;

public record ChatbotApiRequest(string message, string history_id, string? user_id);
public record ChatbotApiResponse(string type, JsonElement? content);

public class ChatbotApiService
{
    private readonly HttpClient? _httpClient;
    private readonly string _apiKey;
    private readonly bool _isConfigured;

    public ChatbotApiService(HttpClient httpClient, IConfiguration config)
    {
        var chatbotUrl = config["ChatbotUrl"];
        var apiKey = config["ChatbotApiKey"];
        _isConfigured = !string.IsNullOrEmpty(chatbotUrl) && !string.IsNullOrEmpty(apiKey);

        if (_isConfigured)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(chatbotUrl!);
            _apiKey = apiKey!;
        }
        else
        {
            _httpClient = null;
            _apiKey = string.Empty;
        }
    }

    public async Task<ChatbotApiResponse> AskAsync(string message, string historyId, string? userId = null)
    {
        if (!_isConfigured)
        {
            var fallback = JsonSerializer.SerializeToElement(new
            {
                role = "assistant",
                content = "Chatbot chưa được cấu hình. Vui lòng liên hệ quản trị viên để thiết lập."
            });
            return new ChatbotApiResponse("text", fallback);
        }

        var payload = new ChatbotApiRequest(message, historyId, userId);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/chat?format=tmp")
        {
            Content = JsonContent.Create(payload)
        };
        httpRequest.Headers.Add("X-API-Key", _apiKey);

        var response = await _httpClient!.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ChatbotApiResponse>())!;
    }
}