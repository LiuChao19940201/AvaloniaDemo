using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.ViewModels.UserControls;
using AvaloniaDemo.Views.UserControls;
using AvaloniaDemo.Views.Windows;

namespace AvaloniaDemo
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new HomeViewModel()
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new HomeUserControl
                {
                    DataContext = new HomeViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}