using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace Project420.UI.Maui.Infrastructure.Services;

/// <summary>
/// Implementation of IDialogService using MAUI's DisplayAlert and CommunityToolkit.Maui.
/// </summary>
public class DialogService : IDialogService
{
    /// <summary>
    /// Displays an alert dialog.
    /// </summary>
    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }
    }

    /// <summary>
    /// Displays a confirmation dialog.
    /// </summary>
    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }
        return false;
    }

    /// <summary>
    /// Displays a prompt dialog.
    /// </summary>
    public async Task<string?> ShowPromptAsync(
        string title,
        string message,
        string accept = "OK",
        string cancel = "Cancel",
        string placeholder = "",
        string initialValue = "",
        int maxLength = -1,
        Keyboard? keyboard = null)
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayPromptAsync(
                title,
                message,
                accept,
                cancel,
                placeholder,
                maxLength,
                keyboard ?? Keyboard.Default,
                initialValue);
        }
        return null;
    }

    /// <summary>
    /// Displays an action sheet.
    /// </summary>
    public async Task<string> ShowActionSheetAsync(string title, string cancel, string? destruction = null, params string[] buttons)
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons);
        }
        return cancel;
    }

    /// <summary>
    /// Shows a loading indicator.
    /// Note: This is a placeholder. For production, use a custom loading overlay or CommunityToolkit.Maui's Popup.
    /// </summary>
    public void ShowLoading(string message = "Loading...")
    {
        // TODO: Implement custom loading overlay
        // For now, this is a no-op. Consider using:
        // - Custom ContentView overlay
        // - CommunityToolkit.Maui Popup
        // - ActivityIndicator in a global layout
    }

    /// <summary>
    /// Hides the loading indicator.
    /// </summary>
    public void HideLoading()
    {
        // TODO: Implement custom loading overlay dismissal
    }

    /// <summary>
    /// Displays a toast message using CommunityToolkit.Maui.
    /// </summary>
    public async Task ShowToastAsync(string message, int duration = 3000)
    {
        var toast = Toast.Make(message, ToastDuration.Short);
        await toast.Show();
    }
}
