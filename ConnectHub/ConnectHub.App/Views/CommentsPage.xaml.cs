using ConnectHub.App.ViewModels;

namespace ConnectHub.App.Views;

public partial class CommentsPage : ContentPage
{
    public CommentsPage(CommentsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
