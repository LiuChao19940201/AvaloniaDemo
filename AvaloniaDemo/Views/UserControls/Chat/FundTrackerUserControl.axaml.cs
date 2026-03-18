using Avalonia.Controls;
using AvaloniaDemo.ViewModels.UserControls.Chat;

namespace AvaloniaDemo.Views.UserControls.Chat;

public partial class FundTrackerUserControl : UserControl
{
    public FundTrackerUserControl()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is FundTrackerViewModel vm)
            vm.OnNavigatedTo();
    }
}
