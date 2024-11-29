using Microsoft.Extensions.Logging;
using ConnectHub.App.Services;
using ConnectHub.App.ViewModels;
using ConnectHub.App.Views;
using ConnectHub.App.Converters;
using Microsoft.Extensions.Configuration;

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

        // Configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ApiBaseUrl", "https://localhost:5001/"}
            })
            .Build();
        builder.Services.AddSingleton<IConfiguration>(configuration);

        // Register Services
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
        builder.Services.AddSingleton<IGeocoding>(Geocoding.Default);
        builder.Services.AddSingleton<IFilePicker>(FilePicker.Default);
        builder.Services.AddSingleton<IApiService, ApiService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // Register AppShell
        builder.Services.AddSingleton<AppShell>();

        // Register converters
        builder.Services.AddSingleton<BooleanToObjectConverter>();

        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<FeedViewModel>();
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<NewPostViewModel>();
        builder.Services.AddTransient<CommentsViewModel>();
        
        // Register Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<FeedPage>();
        builder.Services.AddTransient<ChatPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<NewPostPage>();
        builder.Services.AddTransient<CommentsPage>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
