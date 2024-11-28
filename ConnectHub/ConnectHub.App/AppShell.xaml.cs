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
                InitializeComponent();
                _preferences = preferences;
                _apiService = apiService;

                Debug.WriteLine("Initializing AppShell...");

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

                // Set initial state based on token
                var token = _preferences.Get<string>("auth_token", string.Empty);
                Debug.WriteLine($"Initial token: {token?.Substring(0, Math.Min(10, token?.Length ?? 0))}...");

                if (!string.IsNullOrEmpty(token))
                {
                    Debug.WriteLine("Token found, setting up authenticated state");
                    _apiService.Token = token;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            UpdateUIState(true);
                            Current.GoToAsync("//main/feed").ContinueWith(t =>
                            {
                                if (t.IsFaulted)
                                {
                                    Debug.WriteLine($"Navigation failed: {t.Exception}");
                                }
                            });
                            Debug.WriteLine("Navigated to feed page");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error during navigation: {ex}");
                        }
                    });
                }
                else
                {
                    Debug.WriteLine("No token found, setting up unauthenticated state");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            UpdateUIState(false);
                            Current.GoToAsync("//authentication/login").ContinueWith(t =>
                            {
                                if (t.IsFaulted)
                                {
                                    Debug.WriteLine($"Navigation failed: {t.Exception}");
                                }
                            });
                            Debug.WriteLine("Navigated to login page");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error during navigation: {ex}");
                        }
                    });
                }

                Debug.WriteLine("AppShell initialization completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing AppShell: {ex}");
                throw;
            }
        }

        private void RegisterRoutes()
        {
            try
            {
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
                Debug.WriteLine($"Error registering routes: {ex}");
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
                
                // Reset the HttpClient in ApiService
                _apiService.Token = null;

                // Update UI and navigate
                UpdateUIState(false);
                await Current.GoToAsync("//authentication/login");
                
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
                    try
                    {
                        if (isLoggedIn)
                        {
                            if (!ToolbarItems.Contains(_logoutButton))
                            {
                                ToolbarItems.Add(_logoutButton);
                                Debug.WriteLine("Added logout button to toolbar");
                            }
                            Current.CurrentItem = MainTabs;
                            Debug.WriteLine("Set current item to MainTabs");
                        }
                        else
                        {
                            if (ToolbarItems.Contains(_logoutButton))
                            {
                                ToolbarItems.Remove(_logoutButton);
                                Debug.WriteLine("Removed logout button from toolbar");
                            }
                            Current.CurrentItem = AuthenticationTabs;
                            Debug.WriteLine("Set current item to AuthenticationTabs");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in UI update: {ex}");
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
