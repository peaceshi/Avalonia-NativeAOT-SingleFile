using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Styling;

using CommunityToolkit.Mvvm.ComponentModel;

using Models;

namespace Avalonia_NativeAOT_SingleFile.ViewModels;

public partial class MainWindowViewModel
{
    [ObservableProperty] private ViewModelBase _currentPage = DefaultPage;
    [ObservableProperty] private bool _isNavigateBackEnabled;
    [ObservableProperty] private bool _isPaneOpen;
    [ObservableProperty] private bool _isWindowActive;
    [ObservableProperty] private double _materialOpacity;
    [ObservableProperty] private ObservableCollection<ViewModelBase> _pages = new([DefaultPage]);
    [ObservableProperty] private double _splitRectangleWidth = TitleBarHeight;
    [ObservableProperty] private bool _isDarkMode;

    partial void OnIsWindowActiveChanged(bool value) => MaterialOpacity =
        value ? Constants.MaterialOpacityActivated : Constants.MaterialOpacityDeactivated;

    partial void OnIsPaneOpenChanged(bool value) => SplitRectangleWidth = value ? OpenPaneLength : TitleBarHeight;
    partial void OnIsDarkModeChanged(bool value)
    {
        if (Application.Current is { } app)
        {
            app.RequestedThemeVariant = value ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }
}
