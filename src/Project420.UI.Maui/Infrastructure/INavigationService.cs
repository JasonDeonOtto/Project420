namespace Project420.UI.Maui.Infrastructure;

/// <summary>
/// Service for handling navigation throughout the application.
/// Abstracts Shell navigation for testability and cleaner ViewModels.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to the specified route.
    /// </summary>
    /// <param name="route">The route to navigate to (e.g., "///products" or "productDetail").</param>
    /// <param name="animate">Whether to animate the navigation.</param>
    Task NavigateToAsync(string route, bool animate = true);

    /// <summary>
    /// Navigates to the specified route with parameters.
    /// </summary>
    /// <param name="route">The route to navigate to.</param>
    /// <param name="parameters">Dictionary of parameters to pass to the destination page.</param>
    /// <param name="animate">Whether to animate the navigation.</param>
    Task NavigateToAsync(string route, IDictionary<string, object> parameters, bool animate = true);

    /// <summary>
    /// Navigates back to the previous page.
    /// </summary>
    /// <param name="animate">Whether to animate the navigation.</param>
    Task GoBackAsync(bool animate = true);

    /// <summary>
    /// Pops to the root page.
    /// </summary>
    Task PopToRootAsync(bool animate = true);
}
