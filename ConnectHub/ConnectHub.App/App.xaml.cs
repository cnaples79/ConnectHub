using Microsoft.Maui.Platform;
using System.Diagnostics;

namespace ConnectHub.App;

public partial class App : Application
{
    public App(AppShell appShell)
    {
        try
        {
            Debug.WriteLine("Starting App initialization...");
            
            // Initialize components first
            InitializeComponent();
            Debug.WriteLine("Components initialized successfully");
            
            // Set main page
            MainPage = appShell;
            Debug.WriteLine("MainPage set successfully");
            
            // Register for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Debug.WriteLine($"Unhandled exception: {e.ExceptionObject}");
            };
            
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Debug.WriteLine($"Unobserved Task exception: {e.Exception}");
                e.SetObserved();
            };
            
            Debug.WriteLine("App initialized successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"App initialization error: {ex}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"Inner exception: {ex.InnerException}");
                Debug.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
            }
            throw;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            Debug.WriteLine("Creating window...");
            Window window = base.CreateWindow(activationState);

            if (window != null)
            {
                window.Created += (s, e) =>
                {
                    Debug.WriteLine("Window created");
                };

                window.Activated += (s, e) =>
                {
                    Debug.WriteLine("Window activated");
                };

                window.Deactivated += (s, e) =>
                {
                    Debug.WriteLine("Window deactivated");
                };

                window.Stopped += (s, e) =>
                {
                    Debug.WriteLine("Window stopped");
                };
            }
            else
            {
                Debug.WriteLine("Window creation returned null");
            }

            Debug.WriteLine("Window creation completed");
            return window;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Window creation error: {ex}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
