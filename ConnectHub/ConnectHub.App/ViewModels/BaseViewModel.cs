using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace ConnectHub.App.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string title;

    public bool IsNotBusy => !IsBusy;
}
