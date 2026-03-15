using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaDemo.ViewModels.UserControls;

public partial class DiscoverViewModel : ObservableObject
{
    [RelayCommand] private void OpenMoments()  { }
    [RelayCommand] private void OpenChannels() { }
    [RelayCommand] private void OpenLive()     { }
    [RelayCommand] private void OpenScan()     { }
    [RelayCommand] private void OpenListen()   { }
    [RelayCommand] private void OpenRead()     { }
    [RelayCommand] private void OpenSearch()   { }
    [RelayCommand] private void OpenNearby()   { }
    [RelayCommand] private void OpenGames()    { }
    [RelayCommand] private void OpenMiniApp()  { }
}
