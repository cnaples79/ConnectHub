using System.Collections.ObjectModel;
using System.Windows.Input;
using ConnectHub.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ConnectHub.App.ViewModels
{
    public partial class FeedViewModel : ObservableObject
    {
        private readonly IApiService _apiService;
        private readonly INavigationService _navigationService;
        private int _currentPage = 0;
        private const int PageSize = 20;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private ObservableCollection<Post> _posts;

        public FeedViewModel(IApiService apiService, INavigationService navigationService)
        {
            _apiService = apiService;
            _navigationService = navigationService;
            Posts = new ObservableCollection<Post>();
            LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsRefreshing = true;
                _currentPage = 0;
                var posts = await _apiService.GetFeedAsync(_currentPage * PageSize, PageSize);
                Posts.Clear();
                foreach (var post in posts)
                {
                    Posts.Add(post);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to load feed", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await LoadInitialData();
        }

        [RelayCommand]
        private async Task LoadMore()
        {
            try
            {
                _currentPage++;
                var posts = await _apiService.GetFeedAsync(_currentPage * PageSize, PageSize);
                foreach (var post in posts)
                {
                    Posts.Add(post);
                }
            }
            catch (Exception ex)
            {
                // Silently fail on load more
                _currentPage--;
            }
        }

        [RelayCommand]
        private async Task LikePost(Post post)
        {
            try
            {
                await _apiService.LikePostAsync(post.Id);
                post.LikeCount++;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to like post", "OK");
            }
        }

        [RelayCommand]
        private async Task ShowComments(Post post)
        {
            await _navigationService.NavigateToAsync("CommentsPage", new Dictionary<string, object>
            {
                { "PostId", post.Id }
            });
        }

        [RelayCommand]
        private async Task CreatePost()
        {
            await _navigationService.NavigateToAsync("CreatePostPage");
        }

        [RelayCommand]
        private async Task OpenProfile()
        {
            await _navigationService.NavigateToAsync("ProfilePage");
        }

        [RelayCommand]
        private async Task OpenChat()
        {
            await _navigationService.NavigateToAsync("ChatListPage");
        }

        [RelayCommand]
        private async Task SharePost(Post post)
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = post.Content,
                Title = "Share Post"
            });
        }

        [RelayCommand]
        private async Task ShowPostOptions(Post post)
        {
            var action = await Shell.Current.DisplayActionSheet(
                "Post Options",
                "Cancel",
                null,
                "Report Post",
                "Copy Link",
                "Share"
            );

            switch (action)
            {
                case "Report Post":
                    await _apiService.ReportPostAsync(post.Id);
                    await Shell.Current.DisplayAlert("Success", "Post reported", "OK");
                    break;
                case "Copy Link":
                    await Clipboard.SetTextAsync($"https://connecthub.app/posts/{post.Id}");
                    break;
                case "Share":
                    await SharePost(post);
                    break;
            }
        }
    }
}
