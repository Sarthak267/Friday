#if ANDROID
using Android.Content;
using Android.OS;
using Android.Service.Voice;
using Android.Speech;
using Android.Speech.Tts;
using AndroidTextToSpeech = Android.Speech.Tts.TextToSpeech;
using AndroidQueueMode = Android.Speech.Tts.QueueMode;
using AndroidOperationResult = Android.Speech.Tts.OperationResult;

namespace Friday;

/// <summary>
/// Android keeps the selected VoiceInteractionService alive. This service recognizes a phrase beginning
/// with “Friday”, forwards the remainder to Gemini, and speaks the reply without opening the app UI.
/// </summary>
[Android.App.Service(Name = "com.companyname.friday.FridayVoiceInteractionService", Permission = "android.permission.BIND_VOICE_INTERACTION", Exported = true)]
[Android.App.IntentFilter(["android.service.voice.VoiceInteractionService"])]
[Android.App.MetaData("android.voice_interaction", Resource = "@xml/friday_voice_interaction")]
public sealed class FridayVoiceInteractionService : VoiceInteractionService
{
    private SpeechRecognizer? _recognizer;
    private GeminiAssistantService? _assistant;
    private AndroidTextToSpeech? _speaker;
    private bool _isListening;

    public override void OnReady()
    {
        base.OnReady();
        StartWakeListening();
    }

    public override Android.App.StartCommandResult OnStartCommand(Intent? intent, Android.App.StartCommandFlags flags, int startId)
    {
        StartWakeListening();
        return Android.App.StartCommandResult.Sticky;
    }

    public override void OnShutdown()
    {
        StopWakeListening();
        _speaker?.Shutdown();
        _speaker = null;
        base.OnShutdown();
    }

    public override void OnDestroy()
    {
        StopWakeListening();
        _speaker?.Shutdown();
        _speaker = null;
        base.OnDestroy();
    }

    private void StartWakeListening()
    {
        if (!HandsFreeAssistant.IsEnabled || _isListening || !SpeechRecognizer.IsRecognitionAvailable(this))
            return;

        var recognizer = _recognizer ??= SpeechRecognizer.CreateSpeechRecognizer(this);
        if (recognizer is null)
            return;

        recognizer.SetRecognitionListener(new WakePhraseListener(this));
        var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
        intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
        intent.PutExtra(RecognizerIntent.ExtraPartialResults, false);
        intent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
        _isListening = true;
        recognizer.StartListening(intent);
    }

    private void StopWakeListening()
    {
        _isListening = false;
        _recognizer?.Cancel();
        _recognizer?.Destroy();
        _recognizer = null;
    }

    private void HandleTranscript(string transcript)
    {
        _isListening = false;
        var command = transcript.Trim();
        if (!command.StartsWith("friday", StringComparison.OrdinalIgnoreCase))
        {
            ResumeListeningSoon();
            return;
        }

        command = command["friday".Length..].Trim(' ', ',', '.', '?', '!');
        if (string.IsNullOrWhiteSpace(command))
        {
            Speak("Yes?");
            ResumeListeningSoon();
            return;
        }

        _ = AnswerAsync(command);
    }

    private async Task AnswerAsync(string command)
    {
        try
        {
            var key = await SecureStorage.Default.GetAsync("gemini_api_key");
            if (string.IsNullOrWhiteSpace(key))
            {
                Speak("Please connect Gemini in Friday's settings first.");
                return;
            }

            _assistant ??= new GeminiAssistantService();
            var answer = await _assistant.GetResponseAsync(key, command);
            Speak(answer);
        }
        catch (Exception)
        {
            Speak("I could not complete that request. Please try again.");
        }
        finally
        {
            ResumeListeningSoon();
        }
    }

    private void Speak(string text)
    {
        _speaker ??= new AndroidTextToSpeech(this, new SpeakerReadyListener());
        _speaker.SetLanguage(Java.Util.Locale.Uk);
        _speaker.SetSpeechRate(0.91f);
        _speaker.SetPitch(1.03f);
        _speaker.Speak(text, AndroidQueueMode.Flush, null, "friday-hands-free-response");
    }

    private async void ResumeListeningSoon()
    {
        await Task.Delay(TimeSpan.FromSeconds(3));
        StartWakeListening();
    }

    private sealed class WakePhraseListener(FridayVoiceInteractionService service) : Java.Lang.Object, IRecognitionListener
    {
        public void OnResults(Bundle? results)
        {
            var transcript = results?.GetStringArrayList(SpeechRecognizer.ResultsRecognition)?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(transcript))
                service.ResumeListeningSoon();
            else
                service.HandleTranscript(transcript);
        }

        public void OnError(SpeechRecognizerError error) => service.ResumeListeningSoon();
        public void OnBeginningOfSpeech() { }
        public void OnBufferReceived(byte[]? buffer) { }
        public void OnEndOfSpeech() { }
        public void OnEvent(int eventType, Bundle? @params) { }
        public void OnPartialResults(Bundle? partialResults) { }
        public void OnReadyForSpeech(Bundle? @params) { }
        public void OnRmsChanged(float rmsdB) { }
    }

    private sealed class SpeakerReadyListener : Java.Lang.Object, AndroidTextToSpeech.IOnInitListener
    {
        public void OnInit(AndroidOperationResult status) { }
    }
}

[Android.App.Service(Name = "com.companyname.friday.FridayVoiceInteractionSessionService", Permission = "android.permission.BIND_VOICE_INTERACTION", Exported = true)]
public sealed class FridayVoiceInteractionSessionService : VoiceInteractionSessionService
{
    public override VoiceInteractionSession OnNewSession(Bundle? args) => new FridayVoiceInteractionSession(this);
}

internal sealed class FridayVoiceInteractionSession(Context context) : VoiceInteractionSession(context)
{
}

/// <summary>
/// Declared so Android can recognise Friday as a complete voice-assistant provider.
/// Wake-phrase recognition itself is handled by FridayVoiceInteractionService.
/// </summary>
[Android.App.Service(Name = "com.companyname.friday.FridayRecognitionService", Permission = "android.permission.BIND_SPEECH_RECOGNITION_SERVICE", Exported = true)]
[Android.App.IntentFilter(["android.speech.RecognitionService"])]
[Android.App.MetaData("android.speech", Resource = "@xml/friday_recognition_service")]
public sealed class FridayRecognitionService : RecognitionService
{
    protected override void OnStartListening(Intent? recognizerIntent, RecognitionService.Callback? listener) =>
        listener?.Error(SpeechRecognizerError.Client);

    protected override void OnStopListening(RecognitionService.Callback? listener) =>
        listener?.Error(SpeechRecognizerError.Client);

    protected override void OnCancel(RecognitionService.Callback? listener) { }
}
#endif
