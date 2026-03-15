using Avalonia.Controls;
using AvaloniaDemo.ViewModels.Windows;
using System;

namespace AvaloniaDemo.Views.Windows;

public partial class MainWindow : Ursa.Controls.UrsaWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        // 只在桌面端设置窗口大小
        if (!OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS())
        {
            this.Width = 350;
            this.Height = 700;
        }
    }
}
