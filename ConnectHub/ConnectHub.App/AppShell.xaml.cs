using ConnectHub.App.Views;

namespace ConnectHub.App;

public partial class AppShell : Shell
{
    private bool _routesRegistered;

    public AppShell()
    {
        try
        {
            InitializeComponent();
            RegisterRoutes();
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
            // Register routes for navigation
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("feed", typeof(FeedPage));
            Routing.RegisterRoute("chat", typeof(ChatPage));
            Routing.RegisterRoute("profile", typeof(ProfilePage));

            _routesRegistered = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Route registration error: {ex}");
            throw;
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
