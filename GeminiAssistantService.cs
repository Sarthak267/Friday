using System.Net;
using System.Text;
using System.Text.Json;

namespace Friday;

/// <summary>Small Gemini REST client kept separate from the UI so it can later be replaced by a backend proxy.</summary>
public sealed class GeminiAssistantService
{
    private const string Model = "gemini-3.6-flash";
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(45) };
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly List<ConversationMessage> _conversation = [];

    public async Task<string> GetResponseAsync(string apiKey, string userMessage)
    {
        _conversation.Add(new ConversationMessage("user", userMessage));
        var payload = new
        {
            system_instruction = new
            {
                parts = new[]
                {
                    new { text = "You are Friday, a precise, calm, and practical personal assistant. Help with everyday questions, planning, and software development. Be concise unless the user requests detail. Never claim you performed an external action or accessed device data unless it was explicitly supplied." }
                }
            },
            contents = _conversation.Select(message => new { role = message.Role, parts = new[] { new { text = message.Text } } }),
            generationConfig = new { temperature = 0.7, maxOutputTokens = 1024 }
        };

        var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={Uri.EscapeDataString(apiKey)}";
        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await Client.PostAsync(requestUri, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            _conversation.RemoveAt(_conversation.Count - 1);
            throw new GeminiRequestException("Gemini rejected the API key. Open settings and check it, then try again.");
        }
        if ((int)response.StatusCode == 429)
        {
            _conversation.RemoveAt(_conversation.Count - 1);
            throw new GeminiRequestException("Gemini's free-tier limit has been reached. Please try again later.");
        }
        if (!response.IsSuccessStatusCode)
        {
            _conversation.RemoveAt(_conversation.Count - 1);
            throw new GeminiRequestException("Gemini could not complete that request. Please try again shortly.");
        }

        var result = JsonSerializer.Deserialize<GeminiResponse>(responseBody, JsonOptions);
        var text = result?.Candidates?.FirstOrDefault()?.Content?.Parts?
            .Select(part => part.Text)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        if (string.IsNullOrWhiteSpace(text))
            return "I did not receive a usable response. Please try asking that another way.";

        var reply = text.Trim();
        _conversation.Add(new ConversationMessage("model", reply));
        return reply;
    }

    private sealed class GeminiResponse
    {
        public List<Candidate>? Candidates { get; set; }
    }

    private sealed class Candidate
    {
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        public string? Text { get; set; }
    }

    private sealed record ConversationMessage(string Role, string Text);
}

public sealed class GeminiRequestException(string message) : Exception(message);
