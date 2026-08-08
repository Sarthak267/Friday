namespace Friday;

public sealed record HandsFreeSetupResult(bool IsActive, string Message);

/// <summary>Controls the user's opt-in Android Assistant mode. It never enables background listening by default.</summary>
public static class HandsFreeAssistant
{
    private const string IsEnabledPreference = "hands_free_mode_enabled";
    public static bool IsEnabled => Preferences.Default.Get(IsEnabledPreference, false);

#if ANDROID
    private const string AssistantRole = "android.app.role.ASSISTANT";

    public static Task<HandsFreeSetupResult> EnableAsync()
    {
        Preferences.Default.Set(IsEnabledPreference, true);
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        if (activity is null)
            return Task.FromResult(new HandsFreeSetupResult(false, "Open Friday again to approve Android Assistant mode."));

        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
        {
            var roleManager = activity.GetSystemService(Android.Content.Context.RoleService) as Android.App.Roles.RoleManager;
            if (roleManager?.IsRoleHeld(AssistantRole) == true)
            {
                activity.StartService(new Android.Content.Intent(activity, typeof(FridayVoiceInteractionService)));
                return Task.FromResult(new HandsFreeSetupResult(true, "Hands-free mode is enabled. Say “Friday” followed by your question."));
            }

            if (roleManager?.IsRoleAvailable(AssistantRole) == true)
            {
                try
                {
                    activity.StartActivityForResult(roleManager.CreateRequestRoleIntent(AssistantRole), 4917);
                    return Task.FromResult(new HandsFreeSetupResult(false, "Approve Friday as your default assistant in the Android dialog, then return here."));
                }
                catch (Android.Content.ActivityNotFoundException)
                {
                    // Some Android variants do not expose the role picker. Fall through to Settings.
                }
            }
        }

        var settingsIntent = new Android.Content.Intent(Android.Provider.Settings.ActionVoiceInputSettings);
        activity.StartActivity(settingsIntent);
        return Task.FromResult(new HandsFreeSetupResult(false, "Select Friday as the default digital assistant in Android settings, then return to Friday."));
    }

    public static void Disable()
    {
        Preferences.Default.Set(IsEnabledPreference, false);
        var context = Android.App.Application.Context;
        context.StopService(new Android.Content.Intent(context, typeof(FridayVoiceInteractionService)));
    }
#else
    public static Task<HandsFreeSetupResult> EnableAsync() =>
        Task.FromResult(new HandsFreeSetupResult(false, "Hands-free assistant mode is currently available on Android."));

    public static void Disable() => Preferences.Default.Set(IsEnabledPreference, false);
#endif
}
