using System;
using System.Diagnostics;
using Avalonia_NativeAOT_SingleFile.Messages;
using Avalonia_NativeAOT_SingleFile.ViewModels;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;

namespace Avalonia_NativeAOT_SingleFile.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        Activated += (_, _) => ViewModel?.WindowActivated();
        Deactivated += (_, _) => ViewModel?.WindowDeactivated();
        WeakReferenceMessenger.Default.Register<RequestCloseMessage>(this, OnRequestClose);
    }

    private AboutWindowViewModel? ViewModel => DataContext as AboutWindowViewModel;

    private void OnRequestClose(object recipient, RequestCloseMessage message)
    {
        try
        {
            Close(message.Result);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to close dialog: {ex}");
        }
    }

    public void Dispose() => WeakReferenceMessenger.Default.Unregister<RequestCloseMessage>(this);
}