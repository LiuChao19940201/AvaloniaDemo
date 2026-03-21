using AvaloniaKit.Messages;
using AvaloniaKit.ViewModels.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AvaloniaKit.ViewModels.UserControls.Discover;

public partial class DiscoverViewModel : ObservableObject
{
    [RelayCommand]
    private void OpenMoments()
    {
    }

    [RelayCommand]
    private void OpenChannels()
    {
    }

    [RelayCommand]
    private void OpenLive()
    {
    }

    [RelayCommand]
    private void OpenScan()
    {
    }

    [RelayCommand]
    private void OpenListen()
    {
    }

    [RelayCommand]
    private void OpenRead()
    {
    }

    [RelayCommand]
    private void OpenSearch()
    {
    }

    [RelayCommand]
    private void OpenNearby()
    {
    }

    [RelayCommand]
    private void OpenGames()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToGameBoxesMessages());
    }

    [RelayCommand]
    private void OpenMiniApp()
    {
    }
}