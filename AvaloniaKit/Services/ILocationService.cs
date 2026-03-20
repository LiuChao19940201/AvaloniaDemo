using System.Threading.Tasks;

namespace AvaloniaKit.Services;

/// <summary>
/// 地理位置服务接口。
/// Desktop/Browser 用 IP 定位，Android/iOS 用 GPS。
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// 获取当前位置的城市名（中文）。
    /// 失败时返回 null，调用方应回退到默认城市。
    /// </summary>
    Task<string?> GetCityNameAsync();
}

// ══════════════════════════════════════════════════════════════════════
//  Desktop 实现：纯 IP 定位，无需权限申请
//  ipapi.co 免费接口，每月 3 万次
// ══════════════════════════════════════════════════════════════════════
// 文件路径：AvaloniaKit.Desktop/Services/DesktopLocationService.cs

/*
using AvaloniaKit.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvaloniaKit.Desktop.Services;

public class DesktopLocationService : ILocationService
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(8) };

    public async Task<string?> GetCityNameAsync()
    {
        try
        {
            string raw = await _http.GetStringAsync("https://ipapi.co/json/");
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            if (root.TryGetProperty("city", out var city))
                return city.GetString();
        }
        catch { }
        return null;
    }
}
*/

// ══════════════════════════════════════════════════════════════════════
//  Android 实现：使用 Xamarin.Essentials Geolocation + 逆地理编码
//  文件路径：AvaloniaKit.Android/Services/AndroidLocationService.cs
// ══════════════════════════════════════════════════════════════════════

/*
using AvaloniaKit.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Threading.Tasks;

namespace AvaloniaKit.Android.Services;

public class AndroidLocationService : ILocationService
{
    public async Task<string?> GetCityNameAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted) return null;

            var location = await Geolocation.GetLastKnownLocationAsync()
                           ?? await Geolocation.GetLocationAsync(new GeolocationRequest
                           {
                               DesiredAccuracy = GeolocationAccuracy.Low,
                               Timeout = TimeSpan.FromSeconds(10)
                           });

            if (location is null) return null;

            var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
            var pm = placemarks?.FirstOrDefault();
            // 优先返回区/县，其次城市
            return pm?.SubAdministrativeArea ?? pm?.AdminArea;
        }
        catch { return null; }
    }
}
*/

// ══════════════════════════════════════════════════════════════════════
//  Browser 实现：用浏览器 Geolocation API（需用户授权）
//              失败则回退 IP 定位
//  文件路径：AvaloniaKit.Browser/Services/BrowserLocationService.cs
// ══════════════════════════════════════════════════════════════════════

/*
using AvaloniaKit.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvaloniaKit.Browser.Services;

public class BrowserLocationService : ILocationService
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(8) };

    public async Task<string?> GetCityNameAsync()
    {
        // 浏览器端同样使用 IP 定位（JS Geolocation 需要 HTTPS 且用户交互触发）
        try
        {
            string raw = await _http.GetStringAsync("https://ipapi.co/json/");
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("city", out var city))
                return city.GetString();
        }
        catch { }
        return null;
    }
}
*/
