using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaDemo.ViewModels.UserControls;

public partial class ProfileViewModel : ObservableObject
{
    [ObservableProperty] private string _nickname = "Hello World";
    [ObservableProperty] private string _wechatId = "LC862739233";
    [ObservableProperty] private int _friendCount = 2;

    [RelayCommand]
    private void OpenService()
    {
    }

    [RelayCommand]
    private void OpenFavorites()
    {
    }

    [RelayCommand]
    private void OpenMoments()
    {
    }

    [RelayCommand]
    private void OpenChannels()
    {
    }

    [RelayCommand]
    private void OpenOrders()
    {
    }

    [RelayCommand]
    private void OpenEmoji()
    {
    }

    [RelayCommand]
    private void OpenSettings()
    {
    }

    [RelayCommand]
    private void OpenQrCode()
    {
    }

    [RelayCommand]
    private void AddStatus()
    {
    }

    [RelayCommand]
    private void OpenFriends()
    {
    }
}