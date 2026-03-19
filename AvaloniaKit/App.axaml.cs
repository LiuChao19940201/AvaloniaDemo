using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaKit.Services;
using AvaloniaKit.ViewModels.Windows;
using AvaloniaKit.Views.UserControls;
using AvaloniaKit.Views.Windows;
using System.Threading.Tasks;

namespace AvaloniaKit
{
    public partial class App : Application
    {
        /// <summary>主题持久化使用的 key</summary>
        public const string ThemeSettingKey = "theme";

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new SplashWindow()
                {
                    DataContext = new SplashViewModel()
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = new MainWindowViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();

            // ── 统一监听主题变化，不管哪个控件触发都能持久化 ──
            ActualThemeVariantChanged += OnActualThemeVariantChanged;

            // ── 启动时还原上次主题 ──
            Dispatcher.UIThread.InvokeAsync(RestoreThemeAsync, DispatcherPriority.Loaded);
        }

        /// <summary>
        /// 任何途径（ThemeToggleButton / 二维码按钮 / 代码）触发主题变化时自动存库。
        /// 运行在 UI 线程，直接 fire-and-forget 即可。
        /// </summary>
        private void OnActualThemeVariantChanged(object? sender, System.EventArgs e)
        {
            var theme = ActualThemeVariant;
            // Default 表示跟随系统，不做持久化（保留上次明确选择）
            if (theme == ThemeVariant.Default) return;

            _ = SaveThemeAsync(theme.ToString());
        }

        private static async Task SaveThemeAsync(string themeName)
        {
            try
            {
                var service = ServiceLocator.LocalDataService;
                if (service is null) return;
                await service.SaveSettingAsync(ThemeSettingKey, themeName);
            }
            catch { }
        }

        private static async Task RestoreThemeAsync()
        {
            try
            {
                var service = ServiceLocator.LocalDataService;
                if (service is null) return;

                var saved = await service.LoadSettingAsync(ThemeSettingKey);
                if (string.IsNullOrEmpty(saved)) return;

                var app = Current;
                if (app is null) return;

                app.RequestedThemeVariant = saved switch
                {
                    "Dark"  => ThemeVariant.Dark,
                    "Light" => ThemeVariant.Light,
                    _       => ThemeVariant.Default
                };
            }
            catch { }
        }
    }
}
