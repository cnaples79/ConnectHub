using System.Collections.ObjectModel;
using ConnectHub.App.Services;
using ConnectHub.Shared.Models;
using CommunityToolkit.Mvvm.Input;

namespace ConnectHub.App.ViewModels
{
    public partial class ChatViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private string _message;
        private bool _isLoading;
        private int _currentUserId;

        public ObservableCollection<ChatMessage> Messages { get; } = new();
        
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ChatViewModel(IApiService apiService, int currentUserId)
        {
            _apiService = apiService;
            _currentUserId = currentUserId;
            Title = "Chat";
            LoadMessages();
        }

        [RelayCommand]
        private async Task LoadMessages()
        {
            if (IsLoading) return;
            
            IsLoading = true;
            try
            {
                var messages = await _apiService.GetChatHistoryAsync(_currentUserId);
                Messages.Clear();
                foreach (var message in messages)
                {
                    Messages.Add(message);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to load chat history", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(Message))
                return;

            try
            {
                await _apiService.SendMessageAsync(_currentUserId, Message);
                Message = string.Empty;
                await LoadMessages();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to send message", "OK");
            }
        }
    }
}
