namespace Friday;

public sealed record VoiceCommandResult(bool Success, string? Transcript = null, string? ErrorMessage = null);

/// <summary>Captures one spoken command using the operating system's speech recognizer.</summary>
public sealed class VoiceCommandService
{
#if ANDROID
    public async Task<VoiceCommandResult> ListenOnceAsync(CancellationToken cancellationToken = default)
    {
        var permission = await Permissions.RequestAsync<Permissions.Microphone>();
        if (permission != PermissionStatus.Granted)
            return new(false, ErrorMessage: "Microphone permission is required for voice commands.");

        var context = Android.App.Application.Context;
        if (!Android.Speech.SpeechRecognizer.IsRecognitionAvailable(context))
            return new(false, ErrorMessage: "Speech recognition is unavailable on this phone.");

        var completion = new TaskCompletionSource<VoiceCommandResult>();
        var recognizer = Android.Speech.SpeechRecognizer.CreateSpeechRecognizer(context);
        var listener = new AndroidRecognitionListener(completion, recognizer);
        recognizer.SetRecognitionListener(listener);

        using var cancellation = cancellationToken.Register(() => listener.Complete(new(false, ErrorMessage: "Voice command cancelled.")));
        var intent = new Android.Content.Intent(Android.Speech.RecognizerIntent.ActionRecognizeSpeech);
        intent.PutExtra(Android.Speech.RecognizerIntent.ExtraLanguageModel, Android.Speech.RecognizerIntent.LanguageModelFreeForm);
        intent.PutExtra(Android.Speech.RecognizerIntent.ExtraPrompt, "How may I assist?");
        intent.PutExtra(Android.Speech.RecognizerIntent.ExtraPartialResults, false);
        recognizer.StartListening(intent);

        return await completion.Task;
    }

    private sealed class AndroidRecognitionListener : Java.Lang.Object, Android.Speech.IRecognitionListener
    {
        private readonly TaskCompletionSource<VoiceCommandResult> _completion;
        private readonly Android.Speech.SpeechRecognizer _recognizer;
        private int _completed;

        public AndroidRecognitionListener(TaskCompletionSource<VoiceCommandResult> completion, Android.Speech.SpeechRecognizer recognizer)
        {
            _completion = completion;
            _recognizer = recognizer;
        }

        public void Complete(VoiceCommandResult result)
        {
            if (Interlocked.Exchange(ref _completed, 1) != 0)
                return;

            _recognizer.StopListening();
            _recognizer.Destroy();
            _completion.TrySetResult(result);
        }

        public void OnResults(Android.OS.Bundle? results)
        {
            var phrases = results?.GetStringArrayList(Android.Speech.SpeechRecognizer.ResultsRecognition);
            var transcript = phrases?.FirstOrDefault();
            Complete(string.IsNullOrWhiteSpace(transcript)
                ? new(false, ErrorMessage: "I didn't catch that. Please try again.")
                : new(true, transcript));
        }

        public void OnError(Android.Speech.SpeechRecognizerError error) => Complete(new(false, ErrorMessage: error switch
        {
            Android.Speech.SpeechRecognizerError.InsufficientPermissions => "Microphone permission is required for voice commands.",
            Android.Speech.SpeechRecognizerError.Network or Android.Speech.SpeechRecognizerError.NetworkTimeout => "Voice recognition needs an internet connection.",
            Android.Speech.SpeechRecognizerError.NoMatch or Android.Speech.SpeechRecognizerError.SpeechTimeout => "I didn't catch that. Please try again.",
            _ => "Voice recognition is temporarily unavailable. Please try again."
        }));

        public void OnBeginningOfSpeech() { }
        public void OnBufferReceived(byte[]? buffer) { }
        public void OnEndOfSpeech() { }
        public void OnEvent(int eventType, Android.OS.Bundle? @params) { }
        public void OnPartialResults(Android.OS.Bundle? partialResults) { }
        public void OnReadyForSpeech(Android.OS.Bundle? @params) { }
        public void OnRmsChanged(float rmsdB) { }
    }
#else
    public Task<VoiceCommandResult> ListenOnceAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new VoiceCommandResult(false, ErrorMessage: "Voice commands are currently available on Android."));
#endif
}
