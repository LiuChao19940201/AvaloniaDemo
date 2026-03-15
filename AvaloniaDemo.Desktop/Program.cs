using Avalonia;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using System;

namespace AvaloniaDemo.Desktop
{
    internal sealed class Program
    {
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .With(new FontManagerOptions
                {
                    DefaultFamilyName = "avares://Avalonia.Fonts.Inter/Assets#Inter",
                    FontFallbacks =
                    [
                        new FontFallback { FontFamily = new FontFamily("Segoe UI Emoji") },
                        new FontFallback { FontFamily = new FontFamily("Microsoft YaHei") }
                    ]
                })
                .LogToTrace()
                .UseReactiveUI();
    }
}