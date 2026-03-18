using Avalonia;
using Avalonia.Browser;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using AvaloniaDemo;
using AvaloniaDemo.Browser.Services;
using AvaloniaDemo.Services;
using System.Runtime.Versioning;
using System.Threading.Tasks;

[assembly: SupportedOSPlatform("browser")]

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
        ServiceLocator.ImagePickerService = new BrowserImagePickerService(); // ← 新增
        return BuildAvaloniaApp()
            .WithInterFont()
            .With(new FontManagerOptions
            {
                FontFallbacks =
                [
                    new FontFallback
                    {
                        FontFamily = new FontFamily("avares://AvaloniaDemo/Assets/Fonts/AlibabaPuHuiTi-3-55-Regular.ttf#Alibaba PuHuiTi 3.0")
                    }
                ]
            })
            .UseReactiveUI()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}