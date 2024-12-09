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
                return;

            var messageToSend = Message;
            Message = string.Empty; // Clear input immediately for better UX

            try
            {
                IsSending = true;
                Debug.WriteLine($"Sending message: {messageToSend} to receiver: {SelectedReceiverId}");
                
                await _apiService.SendMessageAsync(messageToSend, SelectedReceiverId);
                await LoadMessagesAsync();
                
                Debug.WriteLine("Message sent successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending message: {ex.Message}");
                Message = messageToSend; // Restore the message if sending failed
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to send message. Please try again.", "OK");
                });
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
                
                Debug.WriteLine("Loading chat messages...");

                var userId = _preferences.Get<int>("user_id", 0);
                if (userId == 0)
                {
                    ErrorMessage = "Please log in to view messages.";
                    Debug.WriteLine("User ID not found");
                    return;
                }

                var messages = await _apiService.GetChatHistoryAsync();
                
                if (messages != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Messages.Clear();
                        foreach (var message in messages.OrderByDescending(m => m.CreatedAt))
                        {
                            Messages.Add(message);
                        }
                    });
                    
                    Debug.WriteLine($"Loaded {messages.Count} messages");
                }
                else
                {
                    Debug.WriteLine("No messages returned from API");
                    ErrorMessage = "Failed to load messages. Please try again.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading messages: {ex.Message}");
                ErrorMessage = "Failed to load messages. Please try again.";
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
                });
            }
            finally
            {
                IsBusy = false;
                IsLoading = false;
            }
        }
    }
}
