namespace AvaloniaDemo.Services;

public static class ServiceLocator
{
    public static IStatusBarService? StatusBarService { get; set; }
    public static IDeviceService? DeviceService { get; set; }
}