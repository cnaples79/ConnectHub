using ConnectHub.App.ViewModels;
using System;
using System.Diagnostics;

namespace ConnectHub.App.Views;

public partial class FeedPage : ContentPage
{
    public FeedPage(FeedViewModel viewModel)
    {
        try
        {
            Debug.WriteLine("Initializing FeedPage...");
            InitializeComponent();
            BindingContext = viewModel;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing FeedPage: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    protected override async void OnAppearing()
    {
        try
        {
            Debug.WriteLine("FeedPage OnAppearing...");
            base.OnAppearing();
            
            if (BindingContext is FeedViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in FeedPage OnAppearing: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", "Unable to load feed. Please try again later.", "OK");
        }
    }
}
