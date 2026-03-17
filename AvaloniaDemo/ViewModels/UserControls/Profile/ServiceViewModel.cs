using AvaloniaDemo.ViewModels.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AvaloniaDemo.ViewModels.UserControls.Profile;

public partial class ServiceViewModel : ObservableObject
{
    [RelayCommand]
    private void GoBack()
    {
        WeakReferenceMessenger.Default.Send(new NavigateBackToProfileMessage());
    }

    [RelayCommand] private void OpenCamera() { /* TODO: 打开相机 */ }
    [RelayCommand] private void Vibrate() { /* TODO: 震动测试 */ }
    [RelayCommand] private void OpenAlbum() { /* TODO: 打开相册 */ }
    [RelayCommand] private void PlaySound() { /* TODO: 音效测试 */ }
    [RelayCommand] private void TestBluetooth() { /* TODO: 蓝牙测试 */ }
    [RelayCommand] private void TestGps() { /* TODO: GPS 定位 */ }
    [RelayCommand] private void TestNfc() { /* TODO: NFC 测试 */ }
    [RelayCommand] private void TestWifi() { /* TODO: WiFi 测试 */ }
    [RelayCommand] private void TestFlashlight() { /* TODO: 闪光灯 */ }
    [RelayCommand] private void TestBrightness() { /* TODO: 屏幕亮度 */ }
    [RelayCommand] private void TestSensor() { /* TODO: 传感器 */ }
    [RelayCommand] private void TestNotification() { /* TODO: 通知推送 */ }
}