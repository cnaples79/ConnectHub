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
            System.Diagnostics.Debug.WriteLine("App initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"App initialization error: {ex}");
            MainPage = new ContentPage
            {
                Content = new VerticalStackLayout
                {
                    Spacing = 10,
                    Padding = new Thickness(20),
                    Children =
                    {
                        new Label
                        {
                            Text = "An error occurred while starting the app:",
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center
                        },
                        new Label
                        {
                            Text = ex.Message,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            TextColor = Colors.Red
                        },
                        new Label
                        {
                            Text = ex.StackTrace ?? "",
                            FontSize = 12,
                            HorizontalOptions = LayoutOptions.Fill,
                            LineBreakMode = LineBreakMode.WordWrap
                        }
                    }
                }
            };
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
