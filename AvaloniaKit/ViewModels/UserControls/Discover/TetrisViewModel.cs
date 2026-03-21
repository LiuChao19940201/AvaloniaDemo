using AvaloniaKit.ViewModels.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AvaloniaKit.ViewModels.UserControls.Discover;

public partial class TetrisViewModel : ObservableObject
{
    public TetrisViewModel()
    {
        
    }

    [RelayCommand]
    private void GoBack()
    {
        WeakReferenceMessenger.Default.Send(new NavigateBackFromTetrisMessage());
    }
}