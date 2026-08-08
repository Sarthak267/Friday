namespace Friday;

public partial class MainPage : ContentPage
{
    private const string ApiKeyName = "gemini_api_key";
    private readonly IDispatcherTimer _clock;
    private readonly GeminiAssistantService _assistant = new();
    private readonly VoiceCommandService _voiceCommands = new();

    public MainPage()
    {
        InitializeComponent();
        UpdateClock();

        _clock = Dispatcher.CreateTimer();
        _clock.Interval = TimeSpan.FromSeconds(1);
        _clock.Tick += (_, _) => UpdateClock();
        _clock.Start();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var hasKey = !string.IsNullOrWhiteSpace(await SecureStorage.Default.GetAsync(ApiKeyName));
        if (!hasKey)
        {
            AssistantResponse.Text = "Connect Gemini in settings to bring Friday online.";
            ActivityLabel.Text = "AI provider setup required";
        }
    }

    private void UpdateClock() => TimeLabel.Text = DateTime.Now.ToString("HH:mm");

    private async void OnListenClicked(object? sender, EventArgs e)
    {
        ListenButton.IsEnabled = false;
        ListenButton.Text = "◉  LISTENING";
        AssistantResponse.Text = "Listening…";
        ActivityLabel.Text = "Speak your command after the prompt";

        try
        {
            var result = await _voiceCommands.ListenOnceAsync();
            if (!result.Success || string.IsNullOrWhiteSpace(result.Transcript))
            {
                AssistantResponse.Text = result.ErrorMessage ?? "I didn't catch that. Please try again.";
                ActivityLabel.Text = "Voice command was not captured";
                return;
            }

            CommandEntry.Text = result.Transcript;
            ActivityLabel.Text = $"Voice command: {result.Transcript}";
            await SendToFridayAsync(result.Transcript);
        }
        finally
        {
            ListenButton.IsEnabled = true;
            ListenButton.Text = "◉  LISTEN";
        }
    }

    private async void OnBriefClicked(object? sender, EventArgs e) => await SendToFridayAsync("Give me a concise daily briefing based on the information you have. Be direct and helpful.");

    private async void OnSystemsClicked(object? sender, EventArgs e) => await SendToFridayAsync("Give me a concise system-status acknowledgement. Do not claim you can access hardware you cannot access.");

    private async void OnCommandSubmitted(object? sender, EventArgs e)
    {
        var command = CommandEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(command))
            return;

        CommandEntry.Text = string.Empty;
        await SendToFridayAsync(command);
    }

    private async void OnSettingsClicked(object? sender, EventArgs e)
    {
        var currentKey = await SecureStorage.Default.GetAsync(ApiKeyName);
        var title = string.IsNullOrEmpty(currentKey) ? "Connect Gemini" : "Replace Gemini key";
        var prompt = "Paste your Gemini API key. It is stored only in this device's secure keychain.";
        var key = await DisplayPromptAsync(title, prompt, "Save securely", "Cancel", "Gemini API key", -1, Keyboard.Text, string.Empty);

        if (string.IsNullOrWhiteSpace(key))
            return;

        await SecureStorage.Default.SetAsync(ApiKeyName, key.Trim());
        AssistantResponse.Text = "Gemini connected. Friday is ready.";
        ActivityLabel.Text = "API key saved securely on this device";
    }

    private async Task SendToFridayAsync(string prompt)
    {
        var apiKey = await SecureStorage.Default.GetAsync(ApiKeyName);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            AssistantResponse.Text = "Please open settings and connect your Gemini API key first.";
            ActivityLabel.Text = "Command waiting for AI provider setup";
            return;
        }

        try
        {
            AssistantResponse.Text = "Thinking…";
            ActivityLabel.Text = "Friday is processing your request";
            var response = await _assistant.GetResponseAsync(apiKey, prompt);
            AssistantResponse.Text = response;
            ActivityLabel.Text = "Response received from Gemini";
            SemanticScreenReader.Announce(response);
        }
        catch (GeminiRequestException ex)
        {
            AssistantResponse.Text = ex.Message;
            ActivityLabel.Text = "Gemini request could not be completed";
        }
        catch (Exception)
        {
            AssistantResponse.Text = "I could not reach Gemini. Please check your internet connection and try again.";
            ActivityLabel.Text = "Network request failed";
        }
    }
}
