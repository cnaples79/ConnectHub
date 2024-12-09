using System.Collections.ObjectModel;
using ConnectHub.App.Services;
using ConnectHub.Shared.Models;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace ConnectHub.App.ViewModels
{
    public partial class ChatViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IPreferences _preferences;

        [ObservableProperty]
        private string _message = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isSending;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private int _selectedReceiverId;

        public ObservableCollection<ChatMessage> Messages { get; } = new();

        public ChatViewModel(IApiService apiService, IPreferences preferences)
        {
            Debug.WriteLine("Initializing ChatViewModel...");
            _apiService = apiService;
            _preferences = preferences;
            Title = "Chat";
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (IsBusy) return;
                await LoadMessagesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ChatViewModel InitializeAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ErrorMessage = "Unable to load chat. Please try again.";
            }
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(Message) || IsSending || SelectedReceiverId == 0)
            {
                if (SelectedReceiverId == 0)
                {
                    ErrorMessage = "Please select a receiver";
                    Debug.WriteLine("Send message failed: No receiver selected");
                }
                return;
            }

            var token = _preferences.Get<string>("token", null);
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Please log in to send messages";
                Debug.WriteLine("Send message failed: User not logged in");
                return;
            }

            var messageToSend = Message;
            Message = string.Empty; // Clear input immediately for better UX

            try
            {
                IsSending = true;
                ErrorMessage = string.Empty;
                Debug.WriteLine($"Sending message: {messageToSend} to receiver: {SelectedReceiverId}");
                
                await _apiService.SendMessageAsync(messageToSend, SelectedReceiverId);
                await LoadMessagesAsync(); // Reload messages after sending
                
                Debug.WriteLine("Message sent successfully");
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine("Unauthorized: User not logged in");
                ErrorMessage = "Please log in to send messages";
                Message = messageToSend; // Restore the message
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending message: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                Message = messageToSend; // Restore the message
                ErrorMessage = "Failed to send message. Please try again.";
            }
            finally
            {
                IsSending = false;
            }
        }

        [RelayCommand]
        private async Task LoadMessages()
        {
            await LoadMessagesAsync();
        }

        private async Task LoadMessagesAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                IsLoading = true;
                ErrorMessage = string.Empty;

                var token = _preferences.Get<string>("token", null);
                if (string.IsNullOrEmpty(token))
                {
                    ErrorMessage = "Please log in to view messages";
                    return;
                }

                var messages = await _apiService.GetChatHistoryAsync();
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Messages.Clear();
                    foreach (var message in messages.OrderByDescending(m => m.CreatedAt))
                    {
                        Messages.Add(message);
                    }
                });
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "Please log in to view messages";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading messages: {ex.Message}");
                ErrorMessage = "Unable to load messages. Please try again.";
            }
            finally
            {
                IsBusy = false;
                IsLoading = false;
            }
        }
    }
}
