using ConnectHub.App.Services;
using CommunityToolkit.Mvvm.Input;

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
                    // Store the token in preferences with consistent key
                    Preferences.Default.Set("auth_token", token);

                    // Get the AppShell instance and show main tabs
                    if (Application.Current?.MainPage is AppShell appShell)
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            appShell.ShowMainTabs();
                            await Shell.Current.GoToAsync("//feed");
                        });
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid login credentials", "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Login failed. Please check your credentials and try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "An unexpected error occurred", "OK");
                System.Diagnostics.Debug.WriteLine($"Unexpected login error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToRegisterAsync()
        {
            await _navigationService.NavigateToAsync("//register");
        }
    }
}
