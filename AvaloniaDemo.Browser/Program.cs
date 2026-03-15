// ── Program.cs ───────────────────────────────────────────────────────────────
// 路径: AvaloniaDemo.Browser/Program.cs
//
// 字体策略（Browser / WASM）：
//   浏览器沙箱环境没有系统字体，必须将字体文件嵌入到程序集中才能显示中文。
//   方案：
//     1. 将中文字体（如阿里巴巴普惠体）放入主项目 Assets/Fonts/ 目录
//     2. 在主项目 .csproj 中确保 <AvaloniaResource Include="Assets\**" /> 已存在
//     3. 在下方 FontFallbacks 中指定字体的 avares:// URI
//
//   本项目所有图标均已改为内联 SVG Path，不依赖任何图标字体，
//   只需保证中文文字能正常渲染即可。
// ─────────────────────────────────────────────────────────────────────────────
using Avalonia;
using Avalonia.Browser;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using AvaloniaDemo;
using System.Runtime.Versioning;
using System.Threading.Tasks;

[assembly: SupportedOSPlatform("browser")]

internal sealed partial class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
        .WithInterFont()
        .With(new FontManagerOptions
        {
            // Browser 没有系统字体，必须指定嵌入字体作为默认字体。
            // Inter 已通过 WithInterFont() 注册，此处作为英文/UI 默认字体。
            DefaultFamilyName = "avares://Avalonia.Fonts.Inter/Assets#Inter",

            FontFallbacks =
            [
                // ── 中文 Fallback ──────────────────────────────────────────
                // 请将字体文件放入主项目 Assets/Fonts/ 目录，并确保 .csproj 中
                // 有 <AvaloniaResource Include="Assets\**" />
                //
                // 推荐：阿里巴巴普惠体 3.0（免费商用）
                //   下载：https://www.alibabafonts.com/#/font
                //   文件：AlibabaPuHuiTi-3-55-Regular.ttf
                //
                new FontFallback
                {
                    FontFamily = new FontFamily(
                        "avares://AvaloniaDemo/Assets/Fonts/AlibabaPuHuiTi-3-55-Regular.ttf#Alibaba PuHuiTi 3.0")
                }
                // ── 如需更多字重，继续添加 ────────────────────────────────
                // new FontFallback
                // {
                //     FontFamily = new FontFamily(
                //         "avares://AvaloniaDemo/Assets/Fonts/AlibabaPuHuiTi-3-75-SemiBold.ttf#Alibaba PuHuiTi 3.0")
                // }
            ]
        })
        .UseReactiveUI()
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
