using Avalonia.Controls;
using AvaloniaKit.ViewModels.UserControls.Chat;

namespace AvaloniaKit.Views.UserControls.Chat;

public partial class NeteaseUserControl : UserControl
{
    public NeteaseUserControl()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is NeteaseViewModel vm)
            vm.OnNavigatedTo();
    }
}
