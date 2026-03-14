using System;
using Ursa.Controls;

namespace AvaloniaDemo.Views.Windows
{
    public partial class MainWindow : UrsaWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // 只在桌面端设置窗口大小
            if (!OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS())
            {
                this.Width = 350;
                this.Height = 700;
            }
        }
    }
}