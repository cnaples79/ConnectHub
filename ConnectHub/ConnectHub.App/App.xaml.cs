using Microsoft.Maui.Platform;
using System.Diagnostics;

namespace ConnectHub.App;

public partial class App : Application
{
    public App(AppShell appShell)
    {
        try
        {
            InitializeComponent();
            MainPage = appShell;
            Debug.WriteLine("App initialized successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"App initialization error: {ex}");
            throw;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            Window window = base.CreateWindow(activationState);

            if (window != null)
            {
                window.Created += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("Window created");
                };

                window.Activated += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("Window activated");
                };

                window.Deactivated += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("Window deactivated");
                };

                window.Stopped += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("Window stopped");
                };

                if (DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst)
                {
                    Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
                    {
                        System.Diagnostics.Debug.WriteLine("Configuring MacCatalyst window");
                    });
                }
            }

            return window ?? throw new InvalidOperationException("Failed to create window");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Window creation error: {ex}");
            throw;
        }
    }
}
