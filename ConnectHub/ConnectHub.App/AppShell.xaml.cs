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
        
        public ICommand LogoutCommand { get; }

        public AppShell(IPreferences preferences)
        {
            InitializeComponent();
            _preferences = preferences;

            // Register routes
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("feed", typeof(FeedPage));
            Routing.RegisterRoute("chat", typeof(ChatPage));
            Routing.RegisterRoute("profile", typeof(ProfilePage));

            LogoutCommand = new Command(async () => await HandleLogout());

            // Check if user is logged in
            var token = _preferences.Get("auth_token", string.Empty);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AuthenticationTabs.IsVisible = string.IsNullOrEmpty(token);
                MainTabs.IsVisible = !string.IsNullOrEmpty(token);
            });
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

                // Update UI
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AuthenticationTabs.IsVisible = true;
                    MainTabs.IsVisible = false;
                });

                await Shell.Current.GoToAsync("//login");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logout error: {ex}");
                await Shell.Current.DisplayAlert("Error", "Failed to logout", "OK");
            }
        }

        public void ShowMainTabs()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AuthenticationTabs.IsVisible = false;
                MainTabs.IsVisible = true;
            });
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
