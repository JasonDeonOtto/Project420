using CommunityToolkit.Mvvm.ComponentModel;

namespace Project420.UI.Maui.Infrastructure;

/// <summary>
/// Base class for all ViewModels in the application.
/// Provides common functionality and patterns for MVVM architecture.
/// </summary>
/// <remarks>
/// Uses CommunityToolkit.Mvvm for property change notifications and commanding.
/// All ViewModels should inherit from this base class for consistency.
/// </remarks>
public abstract partial class ViewModelBase : ObservableObject
{
    /// <summary>
    /// Indicates whether the ViewModel is currently performing an async operation.
    /// Use this to disable UI elements during operations.
    /// </summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Title displayed in the page header or navigation bar.
    /// </summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>
    /// Error message to display to the user.
    /// Bind this to a Label with IsVisible converter.
    /// </summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Success message to display to the user.
    /// Bind this to a Label with IsVisible converter.
    /// </summary>
    [ObservableProperty]
    private string _successMessage = string.Empty;

    /// <summary>
    /// Initializes the ViewModel.
    /// Called when the page appears.
    /// Override this method to load data or perform initialization logic.
    /// </summary>
    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the page is navigated away from.
    /// Override to clean up resources or save state.
    /// </summary>
    public virtual Task OnDisappearingAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears all messages (error and success).
    /// </summary>
    protected void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    /// <summary>
    /// Sets an error message and clears success message.
    /// </summary>
    protected void SetError(string message)
    {
        ErrorMessage = message;
        SuccessMessage = string.Empty;
    }

    /// <summary>
    /// Sets a success message and clears error message.
    /// </summary>
    protected void SetSuccess(string message)
    {
        SuccessMessage = message;
        ErrorMessage = string.Empty;
    }
}
