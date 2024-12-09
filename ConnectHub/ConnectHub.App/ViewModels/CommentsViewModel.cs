using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectHub.App.Services;
using ConnectHub.Shared.Models;

namespace ConnectHub.App.ViewModels;

[QueryProperty("PostId", "postId")]
public partial class CommentsViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    
    [ObservableProperty]
    private int postId;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddComment))]
    private string newComment;
    
    [ObservableProperty]
    private ObservableCollection<Comment> comments;

    public bool CanAddComment => !string.IsNullOrWhiteSpace(NewComment);

    public CommentsViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Comments = new ObservableCollection<Comment>();
        Title = "Comments";
    }

    [RelayCommand]
    public async Task LoadComments()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var loadedComments = await _apiService.GetCommentsAsync(PostId);
            Comments.Clear();
            foreach (var commentDto in loadedComments)
            {
                var comment = new Comment
                {
                    Id = int.Parse(commentDto.Id),
                    Content = commentDto.Content,
                    CreatedAt = commentDto.CreatedAt,
                    UserId = int.Parse(commentDto.User.Id),
                    PostId = PostId
                };
                Comments.Add(comment);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading comments: {ex}");
            await Shell.Current.DisplayAlert("Error", "Failed to load comments", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddComment))]
    private async Task AddComment()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await _apiService.AddCommentAsync(PostId, NewComment);
            NewComment = string.Empty;
            await LoadComments();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error adding comment: {ex}");
            await Shell.Current.DisplayAlert("Error", "Failed to add comment", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnPostIdChanged(int value)
    {
        MainThread.BeginInvokeOnMainThread(async () => await LoadComments());
    }
}
