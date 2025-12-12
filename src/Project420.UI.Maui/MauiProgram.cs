using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Project420.UI.Maui.Infrastructure;
using Project420.UI.Maui.Infrastructure.Services;

namespace Project420.UI.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register Infrastructure Services
		builder.Services.AddSingleton<INavigationService, NavigationService>();
		builder.Services.AddSingleton<IDialogService, DialogService>();
		builder.Services.AddSingleton<ISecureStorageService, SecureStorageService>();

		// Register HttpClient for API calls
		builder.Services.AddHttpClient("Project420API", client =>
		{
			// Configure base address when API is ready
			// client.BaseAddress = new Uri("https://api.project420.com");
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		// Register ViewModels here as they are created
		// Example: builder.Services.AddTransient<MainViewModel>();

		// Register Pages here as they are created
		// Example: builder.Services.AddTransient<MainPage>();

		return builder.Build();
	}
}
