using Avalonia;
using Avalonia.Browser;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using AvaloniaKit;
using AvaloniaKit.Browser.Services;
using AvaloniaKit.Services;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;

[assembly: SupportedOSPlatform("browser")]

internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        // 先导入 storage.js 模块，再注册服务
        await JSHost.ImportAsync("storage", "/storage.js");

        ServiceLocator.LocalDataService = new BrowserLocalDataService();
        ServiceLocator.ImagePickerService = new BrowserImagePickerService();

        await BuildAvaloniaApp()
            .WithInterFont()
            .With(new FontManagerOptions
            {
                FontFallbacks =
                [
                    new FontFallback
                    {
                        FontFamily = new FontFamily("avares://AvaloniaKit/Assets/Fonts/AlibabaPuHuiTi-3-55-Regular.ttf#Alibaba PuHuiTi 3.0")
                    }
                ]
            })
            .UseReactiveUI()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}