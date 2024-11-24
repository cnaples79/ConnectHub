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

        public ChatViewModel(IApiService apiService)
        {
            _apiService = apiService;
            Title = "Chat";
            LoadMessagesCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadMessages()
        {
            try
            {
                IsLoading = true;
                var messages = await _apiService.GetChatHistoryAsync();
                Messages.Clear();
                foreach (var message in messages)
                {
                    Messages.Add(message);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(Message))
                return;

            try
            {
                var message = await _apiService.SendMessageAsync(Message);
                Messages.Add(message);
                Message = string.Empty;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
