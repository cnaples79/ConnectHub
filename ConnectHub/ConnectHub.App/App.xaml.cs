using Microsoft.Maui.Platform;
using System.Diagnostics;

namespace ConnectHub.App;

public partial class App : Application
{
    public App(AppShell appShell)
    {
        try
        {
            Debug.WriteLine("=== App Initialization Started ===");
            Debug.WriteLine($"Thread ID: {Environment.CurrentManagedThreadId}");
            Debug.WriteLine($"Is Main Thread: {MainThread.IsMainThread}");
            
            // Initialize components first
            Debug.WriteLine("Initializing Components...");
            InitializeComponent();
            Debug.WriteLine("Components initialized successfully");
            
            // Set main page
            Debug.WriteLine("Setting MainPage...");
            MainPage = appShell;
            Debug.WriteLine("MainPage set successfully");
            
            // Register for unhandled exceptions
            Debug.WriteLine("Registering exception handlers...");
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Debug.WriteLine($"[CRITICAL] Unhandled exception: {e.ExceptionObject}");
                if (e.ExceptionObject is Exception ex)
                {
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        Debug.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                    }
                }
            };
            
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Debug.WriteLine($"[CRITICAL] Unobserved Task exception: {e.Exception}");
                Debug.WriteLine($"Stack trace: {e.Exception.StackTrace}");
                if (e.Exception.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {e.Exception.InnerException.Message}");
                    Debug.WriteLine($"Inner exception stack trace: {e.Exception.InnerException.StackTrace}");
                }
                e.SetObserved();
            };
            
            Debug.WriteLine("=== App Initialization Completed Successfully ===");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CRITICAL] App initialization error: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                Debug.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
            }
            throw; // Re-throw to ensure the app crashes with the original exception
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            Debug.WriteLine("=== Creating Window ===");
            Debug.WriteLine($"Thread ID: {Environment.CurrentManagedThreadId}");
            Debug.WriteLine($"Is Main Thread: {MainThread.IsMainThread}");
            
            var window = base.CreateWindow(activationState);
            
            if (window != null)
            {
                // Subscribe to window events
                window.Created += (s, e) => Debug.WriteLine("Window Created Event");
                window.Activated += (s, e) => Debug.WriteLine("Window Activated Event");
                window.Deactivated += (s, e) => Debug.WriteLine("Window Deactivated Event");
                window.Stopped += (s, e) => Debug.WriteLine("Window Stopped Event");
                window.Resumed += (s, e) => Debug.WriteLine("Window Resumed Event");
                window.Destroying += (s, e) => Debug.WriteLine("Window Destroying Event");
                
                Debug.WriteLine("Window created successfully");
            }
            else
            {
                Debug.WriteLine("[ERROR] Window creation returned null");
            }
            
            return window;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CRITICAL] Window creation error: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                Debug.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
            }
            throw; // Re-throw to ensure the app crashes with the original exception
        }
    }
}
