using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaDemo.ViewModels.UserControls;

public partial class ProfileViewModel : ObservableObject
{
    [ObservableProperty] private string _nickname = "Hello World";
    [ObservableProperty] private string _wechatId = "LC862739233";
    [ObservableProperty] private int _friendCount = 2;

    [RelayCommand]
    private void OpenService() { }

    [RelayCommand]
    private void OpenFavorites() { }

    [RelayCommand]
    private void OpenMoments() { }

    [RelayCommand]
    private void OpenChannels() { }

    [RelayCommand]
    private void OpenOrders() { }

    [RelayCommand]
    private void OpenEmoji() { }

    [RelayCommand]
    private void OpenSettings() { }

    [RelayCommand]
    private void OpenQrCode()
    {
        var app = Application.Current;
        if (app is null) return;

        // 当前是 Dark → 切到 Light，否则切到 Dark
        app.RequestedThemeVariant =
            app.ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
    }

    [RelayCommand]
    private void AddStatus() { }

    [RelayCommand]
    private void OpenFriends() { }
}