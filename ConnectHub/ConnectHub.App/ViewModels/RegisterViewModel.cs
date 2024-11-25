using ConnectHub.App.Services;
using CommunityToolkit.Mvvm.Input;

namespace ConnectHub.App.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly INavigationService _navigationService;

        private string _username;
        private string _email;
        private string _password;
        private string _confirmPassword;
        private bool _isLoading;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public RegisterViewModel(IApiService apiService, INavigationService navigationService)
        {
            _apiService = apiService;
            _navigationService = navigationService;
            Title = "Create Account";
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || 
                string.IsNullOrWhiteSpace(Email) || 
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all fields", "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Passwords do not match", "OK");
                return;
            }

            try
            {
                IsLoading = true;
                var success = await _apiService.RegisterAsync(Username, Email, Password, ConfirmPassword);
                
                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Account created successfully! Please login.", "OK");
                    await _navigationService.NavigateToAsync("//login");
                }
            }
            catch (HttpRequestException ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToLoginAsync()
        {
            await _navigationService.NavigateToAsync("//login");
        }
    }
}
