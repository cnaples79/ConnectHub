using Microsoft.Maui.Controls;
using ConnectHub.App.Views;

namespace ConnectHub.App;

public partial class AppShell : Shell
{
    private bool _routesRegistered;
    private readonly IPreferences _preferences;

    public AppShell()
    {
        try
        {
            InitializeComponent();
            _preferences = Preferences.Default;
            RegisterRoutes();
            
            // Delay the authentication check until the page is fully loaded
            Dispatcher.Dispatch(() => 
            {
                CheckAuthenticationState();
            });
            
            System.Diagnostics.Debug.WriteLine("AppShell initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AppShell initialization error: {ex}");
#if DEBUG
            throw;
#endif
        }
    }

    private void RegisterRoutes()
    {
        if (_routesRegistered) return;

        try
        {
            System.Diagnostics.Debug.WriteLine("Registering routes...");
            // Register routes for navigation
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("feed", typeof(FeedPage));
            Routing.RegisterRoute("chat", typeof(ChatPage));
            Routing.RegisterRoute("profile", typeof(ProfilePage));

            _routesRegistered = true;
            System.Diagnostics.Debug.WriteLine("Routes registered successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Route registration error: {ex}");
            throw;
        }
    }

    public void ShowAuthenticationTabs()
    {
        try
        {
            if (MainTabs != null)
                MainTabs.IsVisible = false;
            
            if (AuthenticationTabs != null)
            {
                AuthenticationTabs.IsVisible = true;
                Current?.GoToAsync("//login");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShowAuthenticationTabs error: {ex}");
        }
    }

    public void ShowMainTabs()
    {
        try
        {
            if (AuthenticationTabs != null)
                AuthenticationTabs.IsVisible = false;
            
            if (MainTabs != null)
            {
                MainTabs.IsVisible = true;
                Current?.GoToAsync("//feed");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShowMainTabs error: {ex}");
        }
    }

    private void CheckAuthenticationState()
    {
        try
        {
            var token = _preferences?.Get("auth_token", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                ShowAuthenticationTabs();
            }
            else
            {
                ShowMainTabs();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CheckAuthenticationState error: {ex}");
            ShowAuthenticationTabs(); // Default to authentication tabs on error
        }
    }

    protected override void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);
        System.Diagnostics.Debug.WriteLine($"Navigating to: {args.Target?.Location?.OriginalString ?? "unknown"}");
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);
        System.Diagnostics.Debug.WriteLine($"Navigated to: {args.Current?.Location?.OriginalString ?? "unknown"}");
    }
}
