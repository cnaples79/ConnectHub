using System.Collections.ObjectModel;
using ConnectHub.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectHub.App.Services;

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
        Title = "Feed";
        _apiService = apiService;
        _navigationService = navigationService;
        Posts = new ObservableCollection<Post>();
        LoadInitialDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadInitialData()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
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
            System.Diagnostics.Debug.WriteLine($"Feed loading error: {ex}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
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
            await Shell.Current.DisplayAlert("Error", "Failed to like post", "OK");
            System.Diagnostics.Debug.WriteLine($"Like error: {ex}");
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
