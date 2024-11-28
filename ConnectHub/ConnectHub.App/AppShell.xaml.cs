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
        private readonly IApiService _apiService;
        private readonly ToolbarItem _logoutButton;
        
        public ICommand LogoutCommand { get; }

        public AppShell(IPreferences preferences, IApiService apiService)
        {
            InitializeComponent();
            _preferences = preferences;
            _apiService = apiService;
            BindingContext = this;

            // Create logout button
            _logoutButton = new ToolbarItem
            {
                Text = "Logout",
                Command = new Command(async () => await HandleLogout()),
                Order = ToolbarItemOrder.Primary,
                Priority = 0
            };

            // Register routes
            RegisterRoutes();

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

            Debug.WriteLine("AppShell initialized");
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
            
            Debug.WriteLine("Routes registered");
        }

        private async Task HandleLogout()
        {
            try
            {
                Debug.WriteLine("Handling logout...");
                
                // Clear the token
                _preferences.Remove("auth_token");
                
                // Reset the HttpClient in ApiService
                _apiService.Token = null;

                // Update UI and navigate
                UpdateUIState(false);
                await Current.GoToAsync("//authentication/login");
                
                Debug.WriteLine("Logout completed successfully");
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
                try
                {
                    Debug.WriteLine($"Updating UI state, isLoggedIn: {isLoggedIn}");
                    
                    if (isLoggedIn)
                    {
                        Current.CurrentItem = MainTabs;
                        if (!ToolbarItems.Contains(_logoutButton))
                        {
                            ToolbarItems.Add(_logoutButton);
                            Debug.WriteLine("Added logout button to toolbar");
                        }
                    }
                    else
                    {
                        Current.CurrentItem = AuthenticationTabs;
                        if (ToolbarItems.Contains(_logoutButton))
                        {
                            ToolbarItems.Remove(_logoutButton);
                            Debug.WriteLine("Removed logout button from toolbar");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating UI state: {ex}");
                }
            });
        }

        public async Task ShowMainTabs()
        {
            Debug.WriteLine("Showing main tabs...");
            UpdateUIState(true);
            await Current.GoToAsync("//main/feed");
        }
    }
}
