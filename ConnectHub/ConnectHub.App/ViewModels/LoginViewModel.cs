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
        }

        [RelayCommand]
        private async Task NavigateToRegister()
        {
            await Shell.Current.GoToAsync("//authentication/register");
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
                var token = await _apiService.LoginAsync(Email, Password);
                
                if (!string.IsNullOrEmpty(token))
                {
                    Preferences.Default.Set("auth_token", token);
                    _apiService.Token = token;

                    if (Application.Current?.MainPage is AppShell appShell)
                    {
                        appShell.ShowLogoutButton();
                        await Shell.Current.GoToAsync("//main/feed");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Navigation failed", "OK");
                    }
                }
                else
                {
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
                Debug.WriteLine($"Login error: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", "An unexpected error occurred", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
