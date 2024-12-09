using ConnectHub.App.ViewModels;
using System;
using System.Diagnostics;

namespace ConnectHub.App.Views;

public partial class ChatPage : ContentPage
{
    public ChatPage(ChatViewModel viewModel)
    {
        try
        {
            Debug.WriteLine("Initializing ChatPage...");
            InitializeComponent();
            BindingContext = viewModel;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing ChatPage: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    protected override async void OnAppearing()
    {
        try
        {
            Debug.WriteLine("ChatPage OnAppearing...");
            base.OnAppearing();
            
            if (BindingContext is ChatViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ChatPage OnAppearing: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", "Unable to load chat. Please try again later.", "OK");
        }
    }
}
