using ConnectHub.App.ViewModels;
using System;
using System.Diagnostics;

namespace ConnectHub.App.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        try
        {
            Debug.WriteLine("Initializing ProfilePage...");
            InitializeComponent();
            BindingContext = viewModel;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing ProfilePage: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    protected override async void OnAppearing()
    {
        try
        {
            Debug.WriteLine("ProfilePage OnAppearing...");
            base.OnAppearing();
            
            if (BindingContext is ProfileViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ProfilePage OnAppearing: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", "Unable to load profile. Please try again later.", "OK");
        }
    }
}
