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

        [ObservableProperty]
        private string _message = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public ObservableCollection<ChatMessage> Messages { get; } = new();

        public ChatViewModel(IApiService apiService)
        {
            Debug.WriteLine("Initializing ChatViewModel...");
            _apiService = apiService;
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
            }
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(Message))
                return;

            var messageToSend = Message;
            Message = string.Empty; // Clear input immediately for better UX

            try
            {
                IsLoading = true;
                Debug.WriteLine($"Sending message: {messageToSend}");
                
                await _apiService.SendMessageAsync(messageToSend);
                
                // Reload messages to show the new one
                await LoadMessagesAsync();
                Debug.WriteLine("Message sent successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending message: {ex.Message}");
                Message = messageToSend; // Restore the message if sending failed
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
            await LoadMessagesAsync();
        }

        private async Task LoadMessagesAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                IsLoading = true;
                Debug.WriteLine("Loading chat messages...");

                var messages = await _apiService.GetChatHistoryAsync();
                
                if (messages != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
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
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to load messages. Please try again.", "OK");
                }
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
