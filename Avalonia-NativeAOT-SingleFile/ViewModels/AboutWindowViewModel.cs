using System;
using Avalonia_NativeAOT_SingleFile.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Avalonia_NativeAOT_SingleFile.ViewModels;

public partial class AboutWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _inputText = string.Empty;

    [ObservableProperty] private bool _isWindowActive;
    [ObservableProperty] private double _materialOpacity;
    public static string Title => "About";

    [RelayCommand]
    public void WindowActivated() => IsWindowActive = true;

    partial void OnIsWindowActiveChanged(bool value) => MaterialOpacity = value ? 0.35 : 1;

    [RelayCommand]
    public void WindowDeactivated() => IsWindowActive = false;

    public event Action? RequestClose;

    [RelayCommand]
    private void Close() => RequestClose?.Invoke();

    [RelayCommand]
    private void Confirm() =>
        WeakReferenceMessenger.Default.Send(new RequestCloseMessage(InputText));
}
