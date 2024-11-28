using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectHub.App.Services;

namespace ConnectHub.App.ViewModels;

public partial class NewPostViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPost))]
    private string content;

    public bool CanPost => !string.IsNullOrWhiteSpace(Content);

    public NewPostViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Title = "New Post";
    }

    [RelayCommand(CanExecute = nameof(CanPost))]
    private async Task CreatePost()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await _apiService.CreatePostAsync(Content);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating post: {ex}");
            await Shell.Current.DisplayAlert("Error", "Failed to create post", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
