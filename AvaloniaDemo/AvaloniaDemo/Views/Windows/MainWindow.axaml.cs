using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AvaloniaDemo.ViewModels.UserControls;

namespace AvaloniaDemo.Views.Windows
{
    public partial class MainWindow : ReactiveWindow<HomeViewModel>
    {
        public MainWindow()
        {
            //InitializeComponent();
            AvaloniaXamlLoader.Load(this);
        }
    }
}