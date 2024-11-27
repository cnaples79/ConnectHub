using Microsoft.Maui.Controls;
using ConnectHub.App.Views;
using System.Diagnostics;

namespace ConnectHub.App
{
    public partial class AppShell : Shell
    {
        private readonly IPreferences _preferences;

        public AppShell()
        {
            try
            {
                Debug.WriteLine("Initializing AppShell...");
                InitializeComponent();
                _preferences = Preferences.Default;
                
                Debug.WriteLine("Registering routes...");
                RegisterRoutes();
                
                MainThread.BeginInvokeOnMainThread(() => 
                {
                    Debug.WriteLine("Checking initial authentication state...");
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
                Debug.WriteLine("Starting route registration...");
                
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
                Debug.WriteLine("ShowAuthenticationTabs called");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Debug.WriteLine("Setting tab visibility - Auth:true, Main:false");
                    MainTabs.IsVisible = false;
                    AuthenticationTabs.IsVisible = true;
                    
                    Debug.WriteLine("Attempting navigation to login...");
                    Current.GoToAsync("//login").ContinueWith(t =>
                    {
                        if (t.Exception != null)
                            Debug.WriteLine($"Navigation to login failed: {t.Exception}");
                        else
                            Debug.WriteLine("Navigation to login completed");
                    });
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
                Debug.WriteLine("ShowMainTabs called");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Debug.WriteLine("Setting tab visibility - Auth:false, Main:true");
                        AuthenticationTabs.IsVisible = false;
                        MainTabs.IsVisible = true;

                        Debug.WriteLine($"MainTabs.Items count: {MainTabs.Items.Count}");
                        var firstItem = MainTabs.Items.FirstOrDefault();
                        Debug.WriteLine($"First tab type: {firstItem?.GetType().Name}");

                        if (firstItem is ShellSection feedSection)
                        {
                            Debug.WriteLine($"FeedSection.Items count: {feedSection.Items.Count}");
                            var firstContent = feedSection.Items.FirstOrDefault();
                            Debug.WriteLine($"First content type: {firstContent?.GetType().Name}");

                            if (firstContent is ShellContent feedContent)
                            {
                                Debug.WriteLine("Creating feed content...");
                                var content = feedContent.ContentTemplate.CreateContent();
                                Debug.WriteLine($"Feed content created: {content?.GetType().Name}");
                            }
                            else
                            {
                                Debug.WriteLine("First content is not ShellContent");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("First item is not ShellSection");
                        }
                        
                        Debug.WriteLine("Main tabs shown successfully");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error showing main tabs: {ex}");
                        throw;
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
                Debug.WriteLine($"Token exists: {!string.IsNullOrEmpty(token)}");
                
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
            Debug.WriteLine($"OnNavigating - Source: {args.Current?.Location?.OriginalString ?? "null"}, Target: {args.Target?.Location?.OriginalString ?? "null"}");
        }

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);
            Debug.WriteLine($"OnNavigated - Current: {args.Current?.Location?.OriginalString ?? "null"}, Previous: {args.Previous?.Location?.OriginalString ?? "null"}");
        }
    }
}
