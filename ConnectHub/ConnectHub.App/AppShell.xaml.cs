using Microsoft.Maui.Controls;
using ConnectHub.App.Views;
using Microsoft.Maui.Dispatching;
using System.Diagnostics;
using System;

namespace ConnectHub.App
{
    public partial class AppShell : Shell
    {
        private readonly IPreferences _preferences;

        public AppShell()
        {
            try
            {
                InitializeComponent();
                _preferences = Preferences.Default;
                RegisterRoutes();
                
                // Delay the authentication check until the page is fully loaded
                Dispatcher.Dispatch(() => 
                {
                    CheckAuthenticationState();
                });
                
                Debug.WriteLine("AppShell initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AppShell initialization error: {ex}");
#if DEBUG
                throw;
#endif
            }
        }

        private void RegisterRoutes()
        {
            try
            {
                Debug.WriteLine("Registering routes...");
                
                Routing.RegisterRoute("login", typeof(LoginPage));
                Routing.RegisterRoute("register", typeof(RegisterPage));
                Routing.RegisterRoute("feed", typeof(FeedPage));
                Routing.RegisterRoute("chat", typeof(ChatPage));
                Routing.RegisterRoute("profile", typeof(ProfilePage));

                Debug.WriteLine("Routes registered successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Route registration error: {ex}");
#if DEBUG
                throw;
#endif
            }
        }

        public void ShowAuthenticationTabs()
        {
            try
            {
                Debug.WriteLine("Showing authentication tabs");
                MainTabs.IsVisible = false;
                AuthenticationTabs.IsVisible = true;
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try 
                    {
                        await Current.GoToAsync("///login");
                        Debug.WriteLine("Navigated to login page");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Navigation to login failed: {ex}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowAuthenticationTabs error: {ex}");
            }
        }

        public void ShowMainTabs()
        {
            try
            {
                Debug.WriteLine("Showing main tabs");
                AuthenticationTabs.IsVisible = false;
                MainTabs.IsVisible = true;
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try 
                    {
                        await Current.GoToAsync("///feed");
                        Debug.WriteLine("Navigated to feed page");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Navigation to feed failed: {ex}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowMainTabs error: {ex}");
            }
        }

        private void CheckAuthenticationState()
        {
            try
            {
                Debug.WriteLine("Checking authentication state");
                var token = _preferences?.Get("auth_token", string.Empty);
                if (string.IsNullOrEmpty(token))
                {
                    Debug.WriteLine("No token found, showing authentication tabs");
                    ShowAuthenticationTabs();
                }
                else
                {
                    Debug.WriteLine("Token found, showing main tabs");
                    ShowMainTabs();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CheckAuthenticationState error: {ex}");
                ShowAuthenticationTabs(); // Default to authentication tabs on error
            }
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);
            Debug.WriteLine($"Navigating to: {args.Target?.Location?.OriginalString ?? "unknown"}");
        }

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);
            Debug.WriteLine($"Navigated to: {args.Current?.Location?.OriginalString ?? "unknown"}");
        }
    }
}
