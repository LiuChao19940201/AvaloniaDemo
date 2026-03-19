using Avalonia.Controls;
using System.Threading.Tasks;

namespace AvaloniaDemo.Views.Windows;

public partial class SplashWindow : Ursa.Controls.SplashWindow
{
    public SplashWindow()
    {
        InitializeComponent();

        // 窗体拖动（Avalonia 写法）
        PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        };
    }

    protected override async Task<Window?> CreateNextWindow()
    {
        // 确保方法体内有await关键字
        await Task.CompletedTask;

        if (this.DialogResult is true)
        {
            return new MainWindow();
        }
        return null;
    }
}