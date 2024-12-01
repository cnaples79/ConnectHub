using ConnectHub.App.Services;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace ConnectHub.App.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly INavigationService _navigationService;

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public LoginViewModel(IApiService apiService, INavigationService navigationService)
        {
            Debug.WriteLine("=== LoginViewModel Constructor ===");
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            Title = "Login";
            Debug.WriteLine("LoginViewModel initialized successfully");
        }

        [RelayCommand]
        private async Task NavigateToRegister()
        {
            try
            {
                Debug.WriteLine("Attempting to navigate to register page...");
                await Shell.Current.GoToAsync("///register");
                Debug.WriteLine("Navigation to register page successful");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Navigation to register failed: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", "Unable to navigate to registration page. Please try again.", "OK");
            }
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            Debug.WriteLine("=== Login Attempt Started ===");
            Debug.WriteLine($"Email: {Email}");
            Debug.WriteLine($"Password length: {(Password?.Length ?? 0)}");

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                Debug.WriteLine("[WARNING] Login attempted with empty credentials");
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter email and password", "OK");
                return;
            }

            try
            {
                IsLoading = true;
                Debug.WriteLine("Calling API for login...");
                var token = await _apiService.LoginAsync(Email, Password);
                Debug.WriteLine($"Login API response received. Token empty: {string.IsNullOrEmpty(token)}");
                
                if (!string.IsNullOrEmpty(token))
                {
                    Debug.WriteLine("Login successful, storing token...");
                    Preferences.Default.Set("auth_token", token);
                    _apiService.Token = token;

                    Debug.WriteLine("Attempting post-login navigation...");
                    if (Application.Current?.MainPage is AppShell appShell)
                    {
                        Debug.WriteLine("Updating AppShell UI...");
                        appShell.ShowLogoutButton();
                        Debug.WriteLine("Navigating to feed...");
                        await Shell.Current.GoToAsync("//main/feed");
                        Debug.WriteLine("Navigation to feed successful");
                    }
                    else
                    {
                        Debug.WriteLine("[ERROR] MainPage is not AppShell");
                        await Application.Current.MainPage.DisplayAlert("Error", "Navigation failed", "OK");
                    }
                }
                else
                {
                    Debug.WriteLine("[ERROR] Login returned empty token");
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid login credentials", "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ERROR] Login HTTP error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", "Unable to connect to the server. Please check your internet connection.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Unexpected login error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }
                await Application.Current.MainPage.DisplayAlert("Error", "An unexpected error occurred during login. Please try again.", "OK");
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine("=== Login Attempt Completed ===");
            }
        }
    }
}
