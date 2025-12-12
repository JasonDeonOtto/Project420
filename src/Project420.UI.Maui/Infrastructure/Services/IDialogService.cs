namespace Project420.UI.Maui.Infrastructure.Services;

/// <summary>
/// Service for displaying dialogs, alerts, and prompts to the user.
/// Abstracts platform-specific dialog implementations for testability.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Displays an alert dialog with a title, message, and OK button.
    /// </summary>
    /// <param name="title">The title of the alert.</param>
    /// <param name="message">The message to display.</param>
    /// <param name="cancel">The text for the OK button (default: "OK").</param>
    Task ShowAlertAsync(string title, string message, string cancel = "OK");

    /// <summary>
    /// Displays a confirmation dialog with Yes/No options.
    /// </summary>
    /// <param name="title">The title of the confirmation.</param>
    /// <param name="message">The message to display.</param>
    /// <param name="accept">The text for the accept button (default: "Yes").</param>
    /// <param name="cancel">The text for the cancel button (default: "No").</param>
    /// <returns>True if the user accepted, false otherwise.</returns>
    Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No");

    /// <summary>
    /// Displays a prompt dialog asking the user for text input.
    /// </summary>
    /// <param name="title">The title of the prompt.</param>
    /// <param name="message">The message to display.</param>
    /// <param name="accept">The text for the accept button (default: "OK").</param>
    /// <param name="cancel">The text for the cancel button (default: "Cancel").</param>
    /// <param name="placeholder">Placeholder text for the input field.</param>
    /// <param name="initialValue">Initial value for the input field.</param>
    /// <param name="maxLength">Maximum length of the input (default: -1 for no limit).</param>
    /// <param name="keyboard">Keyboard type to use (default: Default).</param>
    /// <returns>The text entered by the user, or null if cancelled.</returns>
    Task<string?> ShowPromptAsync(
        string title,
        string message,
        string accept = "OK",
        string cancel = "Cancel",
        string placeholder = "",
        string initialValue = "",
        int maxLength = -1,
        Keyboard? keyboard = null);

    /// <summary>
    /// Displays an action sheet with multiple options.
    /// </summary>
    /// <param name="title">The title of the action sheet.</param>
    /// <param name="cancel">The text for the cancel button.</param>
    /// <param name="destruction">The text for the destruction button (optional).</param>
    /// <param name="buttons">The array of button texts to display.</param>
    /// <returns>The text of the button that was pressed.</returns>
    Task<string> ShowActionSheetAsync(string title, string cancel, string? destruction = null, params string[] buttons);

    /// <summary>
    /// Displays a loading/busy indicator.
    /// </summary>
    /// <param name="message">The message to display (optional).</param>
    void ShowLoading(string message = "Loading...");

    /// <summary>
    /// Hides the loading/busy indicator.
    /// </summary>
    void HideLoading();

    /// <summary>
    /// Displays a toast message (brief notification).
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="duration">Duration in milliseconds (default: 3000).</param>
    Task ShowToastAsync(string message, int duration = 3000);
}
