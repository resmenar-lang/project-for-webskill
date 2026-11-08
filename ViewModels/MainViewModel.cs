using CommunityToolkit.Mvvm.ComponentModel;

namespace MyAvaloniaApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Avalonia!";
}
