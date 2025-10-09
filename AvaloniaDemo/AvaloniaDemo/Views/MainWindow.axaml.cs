using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AvaloniaDemo.ViewModels;

namespace AvaloniaDemo.Views
{
    public partial class MainWindow : ReactiveWindow<MainViewModel>
    {
        public MainWindow()
        {
            //InitializeComponent();
            AvaloniaXamlLoader.Load(this);
        }
    }
}