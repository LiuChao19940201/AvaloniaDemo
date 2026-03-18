using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using AvaloniaDemo.Services;
using AvaloniaDemo.ViewModels.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;

namespace AvaloniaDemo.ViewModels.UserControls.Profile;

public partial class ProfileViewModel : ObservableObject
{
    [ObservableProperty] private string _nickname = "Hello World";
    [ObservableProperty] private string _wechatId = "LC862739233";
    [ObservableProperty] private int _friendCount = 2;
    [ObservableProperty] private Bitmap? _avatarBitmap;
    [ObservableProperty] private bool _hasAvatar;

    [RelayCommand]
    private async Task PickAvatar()
    {
        var service = ServiceLocator.ImagePickerService;
        if (service is null) return;

        var stream = await service.PickImageAsync();
        if (stream is null) return;

        using (stream)
        {
            AvatarBitmap = new Bitmap(stream);
            HasAvatar = true;
        }
    }

    [RelayCommand]
    private void OpenService()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToServiceMessage());
    }

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

        app.RequestedThemeVariant = app.ActualThemeVariant == ThemeVariant.Dark
            ? ThemeVariant.Light : ThemeVariant.Dark;
    }

    [RelayCommand]
    private void AddStatus() { }

    [RelayCommand]
    private void OpenFriends() { }
}