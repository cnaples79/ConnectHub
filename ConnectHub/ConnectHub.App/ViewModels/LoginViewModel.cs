using ConnectHub.App.Services;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
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
        public async Task LoginAsync()
        {
            try
            {
                Debug.WriteLine("Starting login process...");
                if (IsLoading)
                    return;

                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Error", "Please fill in all fields", "OK");
                    return;
                }

                IsLoading = true;
                Debug.WriteLine("Calling API for login...");
                
                var token = await _apiService.LoginAsync(Email, Password);
                
                if (!string.IsNullOrEmpty(token))
                {
                    Debug.WriteLine("Login successful, storing token...");
                    Preferences.Set("auth_token", token);
                    _apiService.Token = token; // Set token in API service
                    
                    // Ensure we're on the main thread for UI updates
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                            Debug.WriteLine("Attempting to navigate to feed page...");
                            await _navigationService.NavigateToAsync("//main/feed");
                            Debug.WriteLine("Navigation successful");
                        }
                        catch (Exception navEx)
                        {
                            Debug.WriteLine($"Navigation failed: {navEx.Message}");
                            Debug.WriteLine($"Stack trace: {navEx.StackTrace}");
                            await Shell.Current.DisplayAlert("Navigation Error", 
                                "Unable to navigate after login. Please try again.", "OK");
                            throw;
                        }
                    });
                }
                else
                {
                    Debug.WriteLine("Login failed - invalid response");
                    await Shell.Current.DisplayAlert("Error", "Invalid email or password", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", "An error occurred during login", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
