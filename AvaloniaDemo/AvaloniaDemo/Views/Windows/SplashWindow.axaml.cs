using Avalonia.Controls;
using AvaloniaDemo.ViewModels.UserControls;
using AvaloniaDemo.ViewModels.Windows;
using AvaloniaDemo.Views.Windows;
using System.Threading.Tasks;

namespace Ursa.Demo.Views;

public partial class SplashWindow : Controls.SplashWindow
{
    public SplashWindow()
    {
        InitializeComponent();
    }

    protected override async Task<Window?> CreateNextWindow()
    {
        // 确保方法体内有await关键字
        await Task.CompletedTask;

        if (this.DialogResult is true)
        {
            return new MainWindow()
            {
                DataContext = new HomeViewModel()
            };
        }
        return null;
    }
}