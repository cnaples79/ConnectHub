using Microsoft.Extensions.Logging;
using ConnectHub.App.Services;
using ConnectHub.App.ViewModels;
using ConnectHub.App.Views;

namespace ConnectHub.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.ConfigureEssentials(essentials =>
        {
            essentials.UseVersionTracking();
        });

        // Register Services
        builder.Services.AddSingleton<IApiService, ApiService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);

        // Register Views and ViewModels
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginViewModel>();
        
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<RegisterViewModel>();
        
        builder.Services.AddTransient<FeedPage>();
        builder.Services.AddTransient<FeedViewModel>();
        
        builder.Services.AddTransient<ChatPage>();
        builder.Services.AddTransient<ChatViewModel>();
        
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ProfileViewModel>();

#if DEBUG
        builder.Logging.AddDebug().SetMinimumLevel(LogLevel.Trace);
#endif

        return builder.Build();
    }
}
