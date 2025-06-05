using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia_NativeAOT_SingleFile.Messages;
using Avalonia_NativeAOT_SingleFile.ViewModels;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;

namespace Avalonia_NativeAOT_SingleFile.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Activated += (_, _) => ViewModel?.WindowActivated();
        Deactivated += (_, _) => ViewModel?.WindowDeactivated();

        WeakReferenceMessenger.Default.Register<OpenDialogMessage>
            (this, OnOpenDialogMessageReceived);
    }


    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    // 符合 void 签名的方法
    private void OnOpenDialogMessageReceived(object recipient, OpenDialogMessage message) =>
        // 丢弃任务但捕获所有异常
        _ = HandleOpenDialogAsync();

    // Proper async method with Task return type
    private async Task HandleOpenDialogAsync()
    {
        try
        {
            var aboutWindow = new AboutWindow
            {
                DataContext = new AboutWindowViewModel()
            };

            var result = await aboutWindow.ShowDialog<string>(this).ConfigureAwait(false);

            WeakReferenceMessenger.Default.Send(new DialogResultMessage(result));
        }
        catch (Exception ex)
        {
            // Proper error handling
            Debug.WriteLine($"Dialog error: {ex}");
        }
    }
}
