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

        /*

             //打包发布到Linux系统
             dotnet publish -c Release  -f:net10.0  -r linux-x64 --self-contained true  -o ./bin/Release/LinuxOutput

             //Linux系统中下载.net的运行时
            sudo apt-get update
            sudo apt-get install dotnet-runtime-10.0

            //运行在Linux系统中
            chmod +x ./AvaloniaDemo.Desktop
            ./AvaloniaDemo.Desktop

         */
    }
}