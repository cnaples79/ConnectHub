using ConnectHub.App.ViewModels;

namespace ConnectHub.App.Views;

public partial class ChatPage : ContentPage
{
    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
