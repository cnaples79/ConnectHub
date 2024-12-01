using Microsoft.Extensions.Logging;
using ConnectHub.App.Services;
using ConnectHub.App.ViewModels;
using ConnectHub.App.Views;
using ConnectHub.App.Converters;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace ConnectHub.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        try
        {
            Debug.WriteLine("Starting MauiProgram initialization...");
            
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            Debug.WriteLine("Configuring essentials...");
            builder.ConfigureEssentials(essentials =>
            {
                essentials.UseVersionTracking();
            });

            // Configuration
            Debug.WriteLine("Setting up configuration...");
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"ApiBaseUrl", "http://localhost:5000/"}
                })
                .Build();
            builder.Services.AddSingleton<IConfiguration>(configuration);

            // Register Services
            Debug.WriteLine("Registering services...");
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
            Debug.WriteLine("Registering AppShell...");
            builder.Services.AddSingleton<AppShell>();

            // Register converters
            Debug.WriteLine("Registering converters...");
            builder.Services.AddSingleton<BooleanToObjectConverter>();

            // Register ViewModels
            Debug.WriteLine("Registering ViewModels...");
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<FeedViewModel>();
            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<NewPostViewModel>();
            builder.Services.AddTransient<CommentsViewModel>();
            
            // Register Views
            Debug.WriteLine("Registering Views...");
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<FeedPage>();
            builder.Services.AddTransient<ChatPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<NewPostPage>();
            builder.Services.AddTransient<CommentsPage>();

            // Add logging
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Trace);

            Debug.WriteLine("Building MauiApp...");
            var app = builder.Build();
            Debug.WriteLine("MauiApp built successfully");
            
            return app;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in CreateMauiApp: {ex}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"Inner exception: {ex.InnerException}");
                Debug.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
            }
            throw;
        }
    }
}
