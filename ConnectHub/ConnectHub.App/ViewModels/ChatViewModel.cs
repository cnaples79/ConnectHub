using System.Collections.ObjectModel;
using ConnectHub.App.Services;
using ConnectHub.Shared.Models;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

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
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (IsBusy) return;
                await LoadMessages();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ChatViewModel InitializeAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(Message))
                return;

            try
            {
                IsLoading = true;
                Debug.WriteLine($"Sending message: {Message}");
                
                await _apiService.SendMessageAsync(Message);
                Message = string.Empty;
                
                // Reload messages to show the new one
                await LoadMessages();
                Debug.WriteLine("Message sent successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending message: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to send message. Please try again.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadMessages()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                IsLoading = true;
                Debug.WriteLine("Loading chat messages...");

                var messages = await _apiService.GetChatHistoryAsync();
                Messages.Clear();
                foreach (var message in messages)
                {
                    Messages.Add(message);
                }
                Debug.WriteLine($"Loaded {messages.Count} messages");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading messages: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to load messages. Please try again.", "OK");
            }
            finally
            {
                IsBusy = false;
                IsLoading = false;
            }
        }
    }
}
