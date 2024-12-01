using System.Collections.ObjectModel;
using System.Windows.Input;
using ConnectHub.App.Services;
using ConnectHub.Shared.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace ConnectHub.App.ViewModels
{
    public partial class FeedViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly INavigationService _navigationService;
        private readonly int _pageSize = 10;
        private int _currentPage = 1;
        private bool _hasMoreItems = true;

        [ObservableProperty]
        private ObservableCollection<PostDto> _posts;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private bool _isLoading;

        public FeedViewModel(IApiService apiService, INavigationService navigationService)
        {
            Debug.WriteLine("Initializing FeedViewModel...");
            _apiService = apiService;
            _navigationService = navigationService;
            _posts = new ObservableCollection<PostDto>();
            
            // Load initial data
            MainThread.BeginInvokeOnMainThread(async () => await LoadInitialData());
        }

        [RelayCommand]
        private async Task NavigateToNewPost()
        {
            await _navigationService.NavigateToAsync("NewPost");
        }

        private async Task LoadInitialData()
        {
            try
            {
                Debug.WriteLine("Loading initial feed data...");
                IsLoading = true;
                var posts = await _apiService.GetFeedAsync(1, _pageSize);
                
                if (posts != null && posts.Any())
                {
                    foreach (var post in posts)
                    {
                        Posts.Add(post);
                    }
                    StatusMessage = string.Empty;
                    _currentPage = 1;
                    _hasMoreItems = posts.Count == _pageSize;
                }
                else
                {
                    StatusMessage = "No posts found. Follow some users to see their posts!";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading feed: {ex.Message}");
                StatusMessage = "Error loading feed. Please try again later.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadMoreItems()
        {
            if (IsBusy || !_hasMoreItems)
                return;

            try
            {
                Debug.WriteLine($"Loading more items, page {_currentPage + 1}...");
                IsBusy = true;
                
                var posts = await _apiService.GetFeedAsync(_currentPage + 1, _pageSize);
                
                if (posts != null && posts.Any())
                {
                    foreach (var post in posts)
                    {
                        Posts.Add(post);
                    }
                    _currentPage++;
                    _hasMoreItems = posts.Count == _pageSize;
                }
                else
                {
                    _hasMoreItems = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading more items: {ex.Message}");
                StatusMessage = "Unable to load more posts";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RefreshFeed()
        {
            if (IsBusy)
                return;

            try
            {
                Debug.WriteLine("Refreshing feed...");
                IsRefreshing = true;
                Posts.Clear();
                _currentPage = 1;
                _hasMoreItems = true;
                await LoadInitialData();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToChat()
        {
            try
            {
                Debug.WriteLine("Navigating to chat...");
                await Shell.Current.GoToAsync("chat");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to chat: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToProfile()
        {
            try
            {
                Debug.WriteLine("Navigating to profile...");
                await Shell.Current.GoToAsync("profile");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to profile: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task CreateNewPost()
        {
            try
            {
                Debug.WriteLine("Navigating to new post...");
                await Shell.Current.GoToAsync("post/create");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to new post: {ex.Message}");
            }
        }
    }
}
