using System.Collections.ObjectModel;
using ConnectHub.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectHub.App.Services;
using System.Diagnostics;

namespace ConnectHub.App.ViewModels;

public partial class FeedViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;
    private int _currentPage = 0;
    private const int PageSize = 20;

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    private ObservableCollection<Post> _posts;
    public ObservableCollection<Post> Posts
    {
        get => _posts;
        set => SetProperty(ref _posts, value);
    }

    public FeedViewModel(IApiService apiService, INavigationService navigationService)
    {
        Debug.WriteLine("Initializing FeedViewModel");
        Title = "Feed";
        _apiService = apiService;
        _navigationService = navigationService;
        Posts = new ObservableCollection<Post>();
        
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                Debug.WriteLine("Loading initial data from constructor");
                await LoadInitialData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading initial data: {ex}");
            }
        });
    }

    [RelayCommand]
    private async Task LoadInitialData()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            Debug.WriteLine("Loading initial feed data...");

            var token = Preferences.Get("auth_token", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("No auth token found. Redirecting to login.");
                await Shell.Current.GoToAsync("//login");
                return;
            }

            var posts = await _apiService.GetFeedAsync(0, 10);
            if (posts != null && posts.Any())
            {
                Posts.Clear();
                foreach (var post in posts)
                {
                    Posts.Add(post);
                }
                Debug.WriteLine($"Successfully loaded {posts.Count} posts");
            }
            else
            {
                Debug.WriteLine("No posts returned from API");
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"HTTP error loading feed: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "Failed to load feed. Please check your internet connection.", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading feed: {ex}");
            await Shell.Current.DisplayAlert("Error", "An unexpected error occurred while loading the feed.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        Debug.WriteLine("Manual refresh requested");
        await LoadInitialData();
    }

    [RelayCommand]
    private async Task Like(Post post)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await _apiService.LikePostAsync(post.Id);
            await LoadInitialData();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Like error: {ex}");
            await Shell.Current.DisplayAlert("Error", "Failed to like post", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Comment(Post post)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await _navigationService.NavigateToAsync($"post/comments?postId={post.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "Failed to open comments", "OK");
            System.Diagnostics.Debug.WriteLine($"Comment navigation error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NewPost()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await _navigationService.NavigateToAsync("post/create");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "Failed to open new post page", "OK");
            System.Diagnostics.Debug.WriteLine($"New post navigation error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenProfile()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await Shell.Current.GoToAsync("//profile");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenChat()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await Shell.Current.GoToAsync("//chat");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
