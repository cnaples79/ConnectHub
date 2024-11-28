using Microsoft.Maui.Controls;
using ConnectHub.App.Views;
using System.Diagnostics;
using System;
using System.Windows.Input;
using System.Threading.Tasks;
using ConnectHub.App.Services;

namespace ConnectHub.App
{
    public partial class AppShell : Shell
    {
        private readonly IPreferences _preferences;
        private ToolbarItem _logoutButton;
        
        public ICommand LogoutCommand { get; }

        public AppShell(IPreferences preferences)
        {
            InitializeComponent();
            _preferences = preferences;
            BindingContext = this;

            // Register routes
            RegisterRoutes();

            LogoutCommand = new Command(async () => await HandleLogout());

            // Store reference to logout button
            _logoutButton = new ToolbarItem
            {
                Text = "Logout",
                Command = LogoutCommand
            };

            // Set initial state
            var token = _preferences.Get<string>("auth_token", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Current.GoToAsync("//main/feed");
                    UpdateUIState(true);
                });
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Current.GoToAsync("//authentication/login");
                    UpdateUIState(false);
                });
            }
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("feed", typeof(FeedPage));
            Routing.RegisterRoute("chat", typeof(ChatPage));
            Routing.RegisterRoute("profile", typeof(ProfilePage));
            Routing.RegisterRoute("post/create", typeof(NewPostPage));
            Routing.RegisterRoute("post/comments", typeof(CommentsPage));
        }

        private async Task HandleLogout()
        {
            try
            {
                // Clear the token
                _preferences.Remove("auth_token");
                
                // Reset the HttpClient in ApiService
                var apiService = Handler.MauiContext.Services.GetService<IApiService>();
                if (apiService != null)
                {
                    apiService.Token = null;
                }

                // Update UI and navigate
                UpdateUIState(false);
                await Current.GoToAsync("//authentication/login");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logout error: {ex}");
                await Current.DisplayAlert("Error", "Failed to logout", "OK");
            }
        }

        private void UpdateUIState(bool isLoggedIn)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (isLoggedIn)
                {
                    Current.CurrentItem = MainTabs;
                    if (!ToolbarItems.Contains(_logoutButton))
                        ToolbarItems.Add(_logoutButton);
                }
                else
                {
                    Current.CurrentItem = AuthenticationTabs;
                    if (ToolbarItems.Contains(_logoutButton))
                        ToolbarItems.Remove(_logoutButton);
                }
            });
        }

        public async Task ShowMainTabs()
        {
            UpdateUIState(true);
            await Current.GoToAsync("//main/feed");
        }
    }
}
