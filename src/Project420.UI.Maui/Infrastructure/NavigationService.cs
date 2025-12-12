namespace Project420.UI.Maui.Infrastructure;

/// <summary>
/// Implementation of INavigationService using Shell navigation.
/// </summary>
public class NavigationService : INavigationService
{
    /// <summary>
    /// Navigates to the specified route.
    /// </summary>
    public async Task NavigateToAsync(string route, bool animate = true)
    {
        await Shell.Current.GoToAsync(route, animate);
    }

    /// <summary>
    /// Navigates to the specified route with parameters.
    /// </summary>
    public async Task NavigateToAsync(string route, IDictionary<string, object> parameters, bool animate = true)
    {
        await Shell.Current.GoToAsync(route, animate, parameters);
    }

    /// <summary>
    /// Navigates back to the previous page.
    /// </summary>
    public async Task GoBackAsync(bool animate = true)
    {
        await Shell.Current.GoToAsync("..", animate);
    }

    /// <summary>
    /// Pops to the root page.
    /// </summary>
    public async Task PopToRootAsync(bool animate = true)
    {
        await Shell.Current.Navigation.PopToRootAsync(animate);
    }
}
