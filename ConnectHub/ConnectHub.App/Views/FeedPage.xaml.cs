using ConnectHub.App.ViewModels;

namespace ConnectHub.App.Views;

public partial class FeedPage : ContentPage
{
    public FeedPage(FeedViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
