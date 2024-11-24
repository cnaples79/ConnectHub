namespace ConnectHub.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("feed", typeof(FeedPage));
        Routing.RegisterRoute("chat", typeof(ChatPage));
        Routing.RegisterRoute("profile", typeof(ProfilePage));
    }
}
