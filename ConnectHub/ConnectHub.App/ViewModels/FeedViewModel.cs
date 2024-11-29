using System.Collections.ObjectModel;
using System.Windows.Input;
using ConnectHub.App.Services;
using ConnectHub.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace ConnectHub.App.ViewModels
{
    public partial class FeedViewModel : ObservableObject
    {
        private readonly IApiService _apiService;
        private readonly ILogger<FeedViewModel> _logger;
        private readonly IConnectivity _connectivity;
        private readonly INavigationService _navigationService;
        private readonly int _pageSize = 10;
        private int _currentPage = 1;
        private bool _hasMoreItems = true;
        private DateTime _lastRefreshTime;

        [ObservableProperty]
        private ObservableCollection<Post> _posts;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private bool _isLoading;

        public FeedViewModel(IApiService apiService, ILogger<FeedViewModel> logger, IConnectivity connectivity, INavigationService navigationService)
        {
            _apiService = apiService;
            _logger = logger;
            _connectivity = connectivity;
            _navigationService = navigationService;
            _posts = new ObservableCollection<Post>();
            _lastRefreshTime = DateTime.MinValue;
        }

        [RelayCommand]
        private async Task LoadMoreItems()
        {
            if (IsLoading || !_hasMoreItems)
                return;

            try
            {
                IsLoading = true;
                StatusMessage = "Loading more posts...";

                if (!_connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
                {
                    StatusMessage = "No internet connection. Please check your network settings.";
                    return;
                }

                var skip = (_currentPage - 1) * _pageSize;
                var newPosts = await _apiService.GetFeedAsync(skip, _pageSize);
                if (newPosts == null || !newPosts.Any())
                {
                    _hasMoreItems = false;
                    StatusMessage = "No more posts to load";
                    return;
                }

                foreach (var post in newPosts)
                {
                    Posts.Add(post);
                }

                _currentPage++;
                StatusMessage = string.Empty;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error loading posts");
                StatusMessage = "Failed to load posts. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading posts");
                StatusMessage = "An unexpected error occurred. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshFeed()
        {
            if (IsLoading || (DateTime.Now - _lastRefreshTime).TotalSeconds < 30)
            {
                StatusMessage = "Please wait a moment before refreshing again.";
                IsRefreshing = false;
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Refreshing feed...";

                if (!_connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
                {
                    StatusMessage = "No internet connection. Please check your network settings.";
                    return;
                }

                _currentPage = 1;
                _hasMoreItems = true;
                var refreshedPosts = await _apiService.GetFeedAsync(0, _pageSize);

                Posts.Clear();
                if (refreshedPosts != null)
                {
                    foreach (var post in refreshedPosts)
                    {
                        Posts.Add(post);
                    }
                }

                _currentPage++;
                _lastRefreshTime = DateTime.Now;
                StatusMessage = string.Empty;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error refreshing feed");
                StatusMessage = "Failed to refresh feed. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error refreshing feed");
                StatusMessage = "An unexpected error occurred. Please try again.";
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task LikePost(Post post)
        {
            try
            {
                if (!_connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
                {
                    StatusMessage = "No internet connection. Please check your network settings.";
                    return;
                }

                var success = await _apiService.LikePostAsync(post.Id);
                if (success)
                {
                    post.LikesCount++;
                    post.IsLikedByCurrentUser = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking post");
                StatusMessage = "Failed to like post. Please try again.";
            }
        }

        [RelayCommand]
        private async Task UnlikePost(Post post)
        {
            try
            {
                if (!_connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
                {
                    StatusMessage = "No internet connection. Please check your network settings.";
                    return;
                }

                var success = await _apiService.UnlikePostAsync(post.Id);
                if (success)
                {
                    post.LikesCount--;
                    post.IsLikedByCurrentUser = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking post");
                StatusMessage = "Failed to unlike post. Please try again.";
            }
        }

        [RelayCommand]
        private async Task NavigateToComments(int postId)
        {
            try
            {
                await Shell.Current.GoToAsync($"comments?postId={postId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to comments for post {PostId}", postId);
                StatusMessage = "Failed to open comments. Please try again.";
            }
        }

        [RelayCommand]
        private async Task NavigateToNewPost()
        {
            try
            {
                await Shell.Current.GoToAsync("newpost");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to new post page");
                StatusMessage = "Failed to open new post page. Please try again.";
            }
        }

        [RelayCommand]
        private async Task NavigateToProfile()
        {
            await _navigationService.NavigateToAsync("profile");
        }

        [RelayCommand]
        private async Task NavigateToChat()
        {
            await _navigationService.NavigateToAsync("chat");
        }

        public async Task Initialize()
        {
            if (Posts.Count == 0)
            {
                await RefreshFeed();
            }
        }
    }
}
