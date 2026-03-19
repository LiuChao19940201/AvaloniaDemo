using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using AvaloniaDemo.Services;
using AvaloniaDemo.ViewModels.Windows;
using AvaloniaDemo.Views.UserControls;
using AvaloniaDemo.Views.Windows;
using System;
using System.Threading.Tasks;

namespace AvaloniaDemo
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

            // ✅ 启动后异步还原上次保存的主题
            _ = RestoreThemeAsync();
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
                    "Dark" => ThemeVariant.Dark,
                    "Light" => ThemeVariant.Light,
                    _ => ThemeVariant.Default
                };
            }
            catch
            {
                // 首次启动无数据或数据异常时使用默认主题
            }
        }
    }
}