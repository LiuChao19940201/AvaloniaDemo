using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AvaloniaDemo.ViewModels.Windows;

namespace AvaloniaDemo.Views.Windows
{
    public partial class MainWindow : ReactiveWindow<MainViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}