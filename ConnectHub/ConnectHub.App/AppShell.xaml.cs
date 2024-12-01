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

        public AppShell(IPreferences preferences, IApiService apiService)
        {
            try
            {
                Debug.WriteLine("=== AppShell Initialization Started ===");
                Debug.WriteLine($"Thread ID: {Environment.CurrentManagedThreadId}");
                Debug.WriteLine($"Is Main Thread: {MainThread.IsMainThread}");

                _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
                _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

                Debug.WriteLine("Initializing AppShell components...");
                InitializeComponent();
                Debug.WriteLine("AppShell components initialized");

                // Create logout button
                Debug.WriteLine("Creating logout button...");
                _logoutButton = new ToolbarItem
                {
                    Text = "Logout",
                    Command = new Command(async () => await HandleLogout()),
                    Order = ToolbarItemOrder.Primary,
                    Priority = 0
                };
                Debug.WriteLine("Logout button created");

                // Register routes
                Debug.WriteLine("Registering navigation routes...");
                RegisterRoutes();
                Debug.WriteLine("Navigation routes registered");

                // Set initial state based on token
                Debug.WriteLine("Checking authentication state...");
                var token = _preferences.Get<string>("auth_token", string.Empty);
                Debug.WriteLine($"Token status: {(string.IsNullOrEmpty(token) ? "not found" : "found")}");

                if (!string.IsNullOrEmpty(token))
                {
                    Debug.WriteLine("Token found, setting up authenticated state");
                    _apiService.Token = token;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            Debug.WriteLine("Updating UI state...");
                            UpdateUIState(true);
                            Debug.WriteLine("Navigating to feed...");
                            Current.GoToAsync("///feed");
                            Debug.WriteLine("Navigation completed");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ERROR] UI update failed: {ex.Message}");
                            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        }
                    });
                }
                else
                {
                    Debug.WriteLine("No token found, remaining in unauthenticated state");
                }

                Debug.WriteLine("=== AppShell Initialization Completed Successfully ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CRITICAL] AppShell initialization error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }
                throw; // Re-throw to ensure the app crashes with the original exception
            }
        }

        private void RegisterRoutes()
        {
            try
            {
                Debug.WriteLine("Registering routes...");
                
                // Register all routes
                Routing.RegisterRoute("login", typeof(LoginPage));
                Routing.RegisterRoute("register", typeof(RegisterPage));
                Routing.RegisterRoute("feed", typeof(FeedPage));
                Routing.RegisterRoute("chat", typeof(ChatPage));
                Routing.RegisterRoute("profile", typeof(ProfilePage));
                Routing.RegisterRoute("post/create", typeof(NewPostPage));
                Routing.RegisterRoute("post/comments", typeof(CommentsPage));
                
                Debug.WriteLine("Routes registered successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error registering routes: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task HandleLogout()
        {
            try
            {
                Debug.WriteLine("Handling logout...");
                
                // Clear the token
                _preferences.Remove("auth_token");
                _apiService.Token = null;

                // Update UI and navigate
                UpdateUIState(false);
                await Current.GoToAsync("///login");
                
                Debug.WriteLine("Logout completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during logout: {ex}");
                await DisplayAlert("Error", "Failed to logout", "OK");
            }
        }

        private void UpdateUIState(bool isLoggedIn)
        {
            try
            {
                Debug.WriteLine($"Updating UI state, isLoggedIn: {isLoggedIn}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (isLoggedIn)
                    {
                        if (!ToolbarItems.Contains(_logoutButton))
                        {
                            ToolbarItems.Add(_logoutButton);
                            Debug.WriteLine("Added logout button to toolbar");
                        }
                    }
                    else
                    {
                        if (ToolbarItems.Contains(_logoutButton))
                        {
                            ToolbarItems.Remove(_logoutButton);
                            Debug.WriteLine("Removed logout button from toolbar");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating UI state: {ex}");
            }
        }

        public void ShowLogoutButton()
        {
            Debug.WriteLine("ShowLogoutButton called");
            UpdateUIState(true);
        }

        public void HideLogoutButton()
        {
            Debug.WriteLine("HideLogoutButton called");
            UpdateUIState(false);
        }
    }
}
