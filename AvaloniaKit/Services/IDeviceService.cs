namespace AvaloniaKit.Services;

public interface IDeviceService
{
    void OpenCamera();
    void Vibrate();
    void OpenAlbum();
    void PlaySound();
    string GetBluetoothStatus();
    string GetGpsLocation();
    string GetNfcStatus();
    string GetWifiStatus();
    void ToggleFlashlight(bool on);
    void SetBrightness(float level);
    string GetSensorInfo();
    void SendNotification(string title, string message);
}