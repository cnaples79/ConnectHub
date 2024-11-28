using ConnectHub.App.ViewModels;

namespace ConnectHub.App.Views;

public partial class NewPostPage : ContentPage
{
    public NewPostPage(NewPostViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
