using AvaloniaKit.ViewModels.Windows;
using System;

namespace AvaloniaKit.Views.Windows;

public partial class MainWindow : Ursa.Controls.UrsaWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        // 窗体拖动（Avalonia 写法）
        PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        };

        // 只在桌面端设置窗口大小
        if (!OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS())
        {
            this.Width = 350;
            this.Height = 700;
        }
    }
}