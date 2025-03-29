using System;
using Avalonia;

namespace Avalonia_NativeAOT_SingleFile;

public static class Program
{
    /// <summary>
    ///     Application entry point. Initializes and configures the Avalonia application with platform-specific settings
    ///     before starting the desktop lifetime manager.
    ///     Don't use any Avalonia, third-party APIs or any
    ///     SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    ///     yet and stuff might break.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application</param>
    /// <remarks>
    ///     Configures Win32-specific options:
    ///     <para>- Uses ANGLE rendering via EGL for hardware-accelerated graphics</para>
    ///     <para>- Enables per-monitor DPI awareness for proper high-DPI display scaling</para>
    ///     The <see cref="STAThreadAttribute" /> indicates this thread uses Single-Threaded Apartment model
    ///     required for COM components and some Windows UI operations.
    /// </remarks>
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp()
            .With(new Win32PlatformOptions
            {
                RenderingMode = [Win32RenderingMode.AngleEgl],
                DpiAwareness = Win32DpiAwareness.PerMonitorDpiAware
            })
            .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            // .WithInterFont() // We use Windows 11 Segoe Fluent Icons.
            .LogToTrace();
}