using System.Linq;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia_NativeAOT_SingleFile.ViewModels;

public partial class MainWindowViewModel
{
    [RelayCommand]
    private void TogglePane() => IsPaneOpen = !IsPaneOpen;

    [RelayCommand]
    private void NavigateToHome()
    {
        Pages.Add(new HomePageViewModel());
        CurrentPage = Pages.Last();
    }

    [RelayCommand]
    private void NavigateBack()
    {
        if (Pages.Count <= 1) return;
        Pages.RemoveAt(Pages.Count - 1);
        CurrentPage = Pages.Last();
    }

    // [RelayCommand]
    // private static void NavigateToInfo()
    // {
    //     var aboutWindowViewModel = new AboutWindowViewModel();
    //     var aboutWindow = new AboutWindow();
    //     aboutWindowViewModel.RequestClose += () => { aboutWindow.Close(); };
    //     if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
    //         {
    //             MainWindow: not null
    //         } desktop)
    //         aboutWindow.ShowDialog(desktop.MainWindow);
    // }

    [RelayCommand]
    public void WindowActivated() => IsWindowActive = true;

    [RelayCommand]
    public void WindowDeactivated() => IsWindowActive = false;
}
