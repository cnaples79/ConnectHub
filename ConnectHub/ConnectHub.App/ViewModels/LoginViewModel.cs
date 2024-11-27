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
            _apiService = apiService;
            _navigationService = navigationService;
            Title = "Login";
            Debug.WriteLine("LoginViewModel initialized");
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter email and password", "OK");
                return;
            }

            try
            {
                IsLoading = true;
                Debug.WriteLine($"Attempting login with email: {Email}...");
                var token = await _apiService.LoginAsync(Email, Password);
                
                if (!string.IsNullOrEmpty(token))
                {
                    Debug.WriteLine($"Login successful, token received: {token.Substring(0, 10)}...");
                    Preferences.Default.Set("auth_token", token);
                    Debug.WriteLine("Token stored in preferences");

                    if (Application.Current?.MainPage is AppShell appShell)
                    {
                        Debug.WriteLine("Found AppShell, attempting to show main tabs...");
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            try
                            {
                                Debug.WriteLine("Calling ShowMainTabs...");
                                appShell.ShowMainTabs();
                                Debug.WriteLine("ShowMainTabs called successfully");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error in ShowMainTabs: {ex}");
                                throw;
                            }
                        });
                    }
                    else
                    {
                        Debug.WriteLine($"MainPage is not AppShell, it is: {Application.Current?.MainPage?.GetType().Name}");
                        await Application.Current.MainPage.DisplayAlert("Error", "Navigation failed", "OK");
                    }
                }
                else
                {
                    Debug.WriteLine("Login failed - empty token received");
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid login credentials", "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Login HTTP error: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", "Login failed. Please check your credentials and try again.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected login error: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", "An unexpected error occurred", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToRegisterAsync()
        {
            Debug.WriteLine("Navigating to register page...");
            await Shell.Current.GoToAsync("//register");
        }
    }
}
