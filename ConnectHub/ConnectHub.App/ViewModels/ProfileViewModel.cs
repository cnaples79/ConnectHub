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
        private readonly IPreferences _preferences;

        [ObservableProperty]
        private User? _user;

        [ObservableProperty]
        private ObservableCollection<Post> _userPosts = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public int FollowersCount => User?.Followers?.Count ?? 0;
        public int FollowingCount => User?.Following?.Count ?? 0;

        public ProfileViewModel(IApiService apiService, INavigationService navigationService, IPreferences preferences)
        {
            Debug.WriteLine("Initializing ProfileViewModel...");
            _apiService = apiService;
            _navigationService = navigationService;
            _preferences = preferences;
            Title = "Profile";
            InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var userId = _preferences.Get<int>("user_id", 0);
                if (userId == 0)
                {
                    throw new Exception("User ID not found");
                }

                Debug.WriteLine($"Loading profile for user {userId}...");
                User = await _apiService.GetUserProfileAsync(userId);

                Debug.WriteLine("Loading user posts...");
                var postDtos = await _apiService.GetUserPostsAsync(userId, 1, 10);
                UserPosts.Clear();
                foreach (var postDto in postDtos)
                {
                    var post = new Post
                    {
                        Id = int.Parse(postDto.Id),
                        Content = postDto.Content,
                        CreatedAt = postDto.CreatedAt,
                        UserId = userId,
                        LikesCount = postDto.LikesCount
                    };
                    UserPosts.Add(post);
                }
                Debug.WriteLine($"Loaded {UserPosts.Count} posts");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading profile: {ex.Message}");
                ErrorMessage = "Unable to load profile. Please try again later.";
                await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
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
                await _apiService.LogoutAsync(_preferences.Get<int>("user_id", 0));
                await _navigationService.NavigateToAsync("///login");
            }
        }
    }
}
