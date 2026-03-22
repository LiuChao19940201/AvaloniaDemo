using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.Hardware.Camera2;
using Android.Locations;
using Android.Media;
using Android.Net.Wifi;
using Android.Nfc;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AvaloniaKit.Services;
using System;
using System.Linq;

namespace AvaloniaKit.Android.Services;

public class AndroidDeviceService : IDeviceService
{
    private readonly Activity _activity;
    private const int RequestCodeCamera = 1001;

    public AndroidDeviceService(Activity activity)
    {
        _activity = activity;
    }

    public void OpenCamera()
    {
        // Android 6.0+ CAMERA 是危险权限，需要运行时申请
        if (ContextCompat.CheckSelfPermission(_activity, global::Android.Manifest.Permission.Camera)
            != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(
                _activity,
                [global::Android.Manifest.Permission.Camera],
                RequestCodeCamera);
            return;
        }

        var intent = new Intent(MediaStore.ActionImageCapture);
        // 不再依赖 ResolveActivity（Android 11+ 包可见性限制），直接尝试启动
        try
        {
            _activity.StartActivity(intent);
        }
        catch (ActivityNotFoundException)
        {
            // 设备没有相机应用
        }
    }

    public void Vibrate()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            // Android 12+ 推荐使用 VibratorManager
            if (_activity.GetSystemService(Context.VibratorManagerService) is VibratorManager vibratorManager)
            {
                var vibrator = vibratorManager.DefaultVibrator;
                vibrator.Vibrate(VibrationEffect.CreateOneShot(300, VibrationEffect.DefaultAmplitude));
            }
        }
        else if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            if (_activity.GetSystemService(Context.VibratorService) is Vibrator vibrator)
            {
                vibrator.Vibrate(VibrationEffect.CreateOneShot(300, VibrationEffect.DefaultAmplitude));
            }
        }
        else
        {
#pragma warning disable CA1422
            if (_activity.GetSystemService(Context.VibratorService) is Vibrator vibrator)
            {
                vibrator.Vibrate(300);
            }
#pragma warning restore CA1422
        }
    }

    public void OpenAlbum()
    {
        var intent = new Intent(Intent.ActionPick);
        intent.SetType("image/*");
        _activity.StartActivity(intent);
    }

    public void PlaySound()
    {
        // ToneGenerator 在某些设备/模拟器上可能抛出 Java.Lang.RuntimeException("Init failed")
        // 因此这里捕获异常并提供降级处理，避免应用崩溃。
        try
        {
            using var toneGenerator = new ToneGenerator(Stream.Music, 100);
            // 使用短时长避免长时间占用资源
            toneGenerator.StartTone(Tone.PropBeep, 150);
        }
        catch (Java.Lang.RuntimeException)
        {
            // 初始化失败：尝试降级为系统提示音（尽量不抛出）
            try
            {
                var uri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                var ringtone = RingtoneManager.GetRingtone(_activity, uri);
                ringtone?.Play();
            }
            catch
            {
                // 最终降级：什么都不做，避免崩溃
            }
        }
        catch
        {
            // 兜底，确保不向外抛出异常
        }
    }

    public string GetBluetoothStatus()
    {
        BluetoothAdapter? adapter;

        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            var bluetoothManager = _activity.GetSystemService(Context.BluetoothService) as BluetoothManager;
            adapter = bluetoothManager?.Adapter;
        }
        else
        {
#pragma warning disable CA1422
            adapter = BluetoothAdapter.DefaultAdapter;
#pragma warning restore CA1422
        }

        if (adapter is null)
            return "设备不支持蓝牙";
        return adapter.IsEnabled ? "已开启" : "已关闭";
    }

    public string GetGpsLocation()
    {
        if (_activity.GetSystemService(Context.LocationService) is not LocationManager locationManager)
            return "无法获取定位服务";

        var isGpsEnabled = locationManager.IsProviderEnabled(LocationManager.GpsProvider);
        if (!isGpsEnabled)
            return "GPS 未开启";

        var lastKnown = locationManager.GetLastKnownLocation(LocationManager.GpsProvider)
                        ?? locationManager.GetLastKnownLocation(LocationManager.NetworkProvider);

        return lastKnown is not null
            ? $"纬度: {lastKnown.Latitude:F6}, 经度: {lastKnown.Longitude:F6}"
            : "GPS 已开启，暂无缓存位置";
    }

    public string GetNfcStatus()
    {
        var nfcAdapter = NfcAdapter.GetDefaultAdapter(_activity);
        if (nfcAdapter is null)
            return "设备不支持 NFC";
        return nfcAdapter.IsEnabled ? "已开启" : "已关闭";
    }

    public string GetWifiStatus()
    {
        if (_activity.GetSystemService(Context.WifiService) is not WifiManager wifiManager)
            return "无法获取 WiFi 服务";
        return wifiManager.IsWifiEnabled ? "已开启" : "已关闭";
    }

    public void ToggleFlashlight(bool on)
    {
        if (_activity.GetSystemService(Context.CameraService) is not CameraManager cameraManager)
            return;

        var cameraId = cameraManager.GetCameraIdList().FirstOrDefault();
        if (cameraId is not null)
        {
            cameraManager.SetTorchMode(cameraId, on);
        }
    }

    public void SetBrightness(float level)
    {
        var window = _activity.Window;
        if (window is null) return;

        var layoutParams = window.Attributes;
        if (layoutParams is null) return;

        layoutParams.ScreenBrightness = Math.Clamp(level, 0f, 1f);
        window.Attributes = layoutParams;
    }

    public string GetSensorInfo()
    {
        if (_activity.GetSystemService(Context.SensorService) is not SensorManager sensorManager)
            return "无法获取传感器服务";

        var sensors = sensorManager.GetSensorList(SensorType.All);
        if (sensors is null || sensors.Count == 0)
            return "未检测到传感器";

        var sensorNames = sensors.Take(5).Select(s => s?.Name ?? "未知");
        var suffix = sensors.Count > 5 ? $" ... 共 {sensors.Count} 个" : "";
        return string.Join(", ", sensorNames) + suffix;
    }

    public void SendNotification(string title, string message)
    {
        const string channelId = "avalonia_demo_test";

        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            if (_activity.GetSystemService(Context.NotificationService) is NotificationManager notificationManager)
            {
                var channel = new NotificationChannel(channelId, "测试通知", NotificationImportance.High);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        var notification = new NotificationCompat.Builder(_activity, channelId)!
            .SetContentTitle(title)!
            .SetContentText(message)!
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)!
            .SetAutoCancel(true)!
            .Build();

        NotificationManagerCompat.From(_activity)!.Notify(1001, notification);
    }
}