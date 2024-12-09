using ConnectHub.App.Services;
using ConnectHub.Shared.Models;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ConnectHub.App.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly INavigationService _navigationService;
        private int _currentUserId;

        [ObservableProperty]
        private User? _user;

        [ObservableProperty]
        private ObservableCollection<Post> _userPosts = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public ProfileViewModel(IApiService apiService, INavigationService navigationService, int currentUserId)
        {
            Debug.WriteLine("Initializing ProfileViewModel...");
            _apiService = apiService;
            _navigationService = navigationService;
            _currentUserId = currentUserId;
            Title = "Profile";
            InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                Debug.WriteLine("Loading user profile...");
                User = await _apiService.GetUserProfileAsync(_currentUserId);

                Debug.WriteLine("Loading user posts...");
                var postDtos = await _apiService.GetUserPostsAsync(User.Id, 1, 10);
                UserPosts.Clear();
                foreach (var postDto in postDtos)
                {
                    var post = new Post
                    {
                        Id = int.Parse(postDto.Id),
                        Content = postDto.Content,
                        CreatedAt = postDto.CreatedAt,
                        UserId = User.Id,
                        LikesCount = postDto.LikesCount
                    };
                    UserPosts.Add(post);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading profile: {ex.Message}");
                ErrorMessage = "Unable to load profile. Please try again later.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshProfile()
        {
            await InitializeAsync();
        }

        [RelayCommand]
        private async Task UpdateProfileAsync()
        {
            if (User == null)
                return;

            try
            {
                IsLoading = true;
                await _apiService.UpdateProfileAsync(User.Username);
                await Application.Current.MainPage.DisplayAlert("Success", "Profile updated successfully", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to update profile", "OK");
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
                await _apiService.LogoutAsync(_currentUserId);
                await _navigationService.NavigateToAsync("///login");
            }
        }
    }
}
