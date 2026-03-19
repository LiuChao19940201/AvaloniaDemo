using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using AvaloniaKit.Services;
using AvaloniaKit.ViewModels.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AvaloniaKit.ViewModels.UserControls.Profile;

public partial class ProfileViewModel : ObservableObject
{
    [ObservableProperty] private string _nickname = "Hello World";
    [ObservableProperty] private string _wechatId = "LC862739233";
    [ObservableProperty] private int _friendCount = 2;
    [ObservableProperty] private Bitmap? _avatarBitmap;
    [ObservableProperty] private bool _hasAvatar;

    /// <summary>头像缩略图宽度（70dp显示 × 3倍屏 ≈ 200px 足够清晰）</summary>
    private const int AvatarDecodeWidth = 200;

    public ProfileViewModel()
    {
        _ = LoadAvatarOnStartupAsync();
    }

    private async Task LoadAvatarOnStartupAsync()
    {
        try
        {
            var service = ServiceLocator.LocalDataService;
            if (service is null) return;

            var bytes = await service.LoadAvatarAsync();
            if (bytes is null || bytes.Length == 0) return;

            using var ms = new MemoryStream(bytes);
            // 已保存的是缩略图 PNG，直接解码即可
            AvatarBitmap = new Bitmap(ms);
            HasAvatar = true;
        }
        catch
        {
            // 数据损坏或格式不兼容时静默忽略
        }
    }

    [RelayCommand]
    private async Task PickAvatar()
    {
        var pickerService = ServiceLocator.ImagePickerService;
        if (pickerService is null) return;

        var stream = await pickerService.PickImageAsync();
        if (stream is null) return;

        try
        {
            using (stream)
            {
                // 只解码为缩略图，而非原始分辨率
                // 4000×3000 原图 → 48MB 像素 → WASM 直接 OOM 卡死
                // DecodeToWidth(200) → ~200×150 → 120KB 像素 → 安全
                AvatarBitmap = Bitmap.DecodeToWidth(stream, AvatarDecodeWidth,
                    BitmapInterpolationMode.MediumQuality);
                HasAvatar = true;
            }

            // 将缩略图编码为 PNG 再持久化（~10-30KB，远小于原始 5MB）
            var dataService = ServiceLocator.LocalDataService;
            if (dataService is not null)
            {
                using var saveStream = new MemoryStream();
                AvatarBitmap.Save(saveStream); // 编码为 PNG
                await dataService.SaveAvatarAsync(saveStream.ToArray());
            }
        }
        catch
        {
            // 图片格式不支持等异常
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

    // 切换主题 + 持久化
    [RelayCommand]
    private async Task OpenQrCode()
    {
        var app = Application.Current;
        if (app is null) return;

        app.RequestedThemeVariant = app.ActualThemeVariant == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
    }

    [RelayCommand]
    private void AddStatus() { }

    [RelayCommand]
    private void OpenFriends() { }
}