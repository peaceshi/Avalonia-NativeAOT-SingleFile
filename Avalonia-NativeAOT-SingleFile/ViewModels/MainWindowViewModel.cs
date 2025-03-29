using Avalonia_NativeAOT_SingleFile.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MessengerGenerator;

namespace Avalonia_NativeAOT_SingleFile.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly ViewModelBase DefaultPage = new WelcomePageViewModel();

    [ObservableProperty] private string _receivedText = string.Empty;

    public MainWindowViewModel() =>
        _pages.CollectionChanged += (_, _) =>
            IsNavigateBackEnabled = Pages.Count > 1;

    public static string Greeting => "Welcome to Avalonia!";
    public static double TitleBarHeight => 48;
    public static double MinWindowHeight => 240;
    public static double MinWindowWidth => 320;
    public static double OpenPaneLength => 300;

    [MessageHandler]
    public void HandleDialogResult(DialogResultMessage message) => ReceivedText = message.Result;

    [RelayCommand]
    private void ShowDialog() =>
        WeakReferenceMessenger.Default.Send(new OpenDialogMessage());
}