using Microsoft.Maui.Platform;

namespace ConnectHub.App;

public partial class App : Application
{
    public App()
    {
        try
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"App initialization error: {ex}");
            MainPage = new ContentPage
            {
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label
                        {
                            Text = "An error occurred while starting the app. Please try again.",
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center
                        }
                    }
                }
            };
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);

        if (window != null)
        {
            window.Created += (s, e) =>
            {
                // Window created
            };

            window.Activated += (s, e) =>
            {
                // Window activated
            };

            window.Deactivated += (s, e) =>
            {
                // Window deactivated
            };

            window.Stopped += (s, e) =>
            {
                // Window being destroyed
            };

            if (DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst)
            {
                Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
                {
                    // Window size will be handled by Info.plist
                });
            }
        }

        return window ?? throw new InvalidOperationException("Failed to create window");
    }
}
