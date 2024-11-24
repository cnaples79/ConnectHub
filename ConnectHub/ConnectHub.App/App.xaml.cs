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
            System.Diagnostics.Debug.WriteLine($"Initialization Error: {ex}");
#if DEBUG
            throw;
#endif
        }
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        
        if (window != null)
        {
            // Set window size for desktop platforms
            window.Width = 800;
            window.Height = 600;
            window.MinimumWidth = 400;
            window.MinimumHeight = 400;
        }
        
        return window;
    }
}
