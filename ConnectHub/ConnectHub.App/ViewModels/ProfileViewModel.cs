using ConnectHub.App.Services;
using ConnectHub.Shared.Models;
using CommunityToolkit.Mvvm.Input;

namespace ConnectHub.App.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly INavigationService _navigationService;
        private User _user;
        private bool _isLoading;

        public User User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ProfileViewModel(IApiService apiService, INavigationService navigationService)
        {
            _apiService = apiService;
            _navigationService = navigationService;
            Title = "Profile";
            LoadProfileCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadProfile()
        {
            try
            {
                IsLoading = true;
                User = await _apiService.GetUserProfileAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task UpdateProfile()
        {
            if (User == null)
                return;

            try
            {
                IsLoading = true;
                await _apiService.UpdateProfileAsync(User);
                await Application.Current.MainPage.DisplayAlert("Success", "Profile updated successfully", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Logout()
        {
            var logout = await Application.Current.MainPage.DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (logout)
            {
                await _apiService.LogoutAsync();
                await _navigationService.NavigateToAsync("///login");
            }
        }
    }
}
