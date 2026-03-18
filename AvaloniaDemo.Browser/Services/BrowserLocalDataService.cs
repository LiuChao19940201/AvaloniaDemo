using AvaloniaDemo.Services;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace AvaloniaDemo.Browser.Services;

[SupportedOSPlatform("browser")]
public class BrowserLocalDataService : ILocalDataService
{
    public Task SaveAvatarAsync(byte[] imageData)
    {
        BrowserStorage.SetItem("avatar", Convert.ToBase64String(imageData));
        return Task.CompletedTask;
    }

    public Task<byte[]?> LoadAvatarAsync()
    {
        var base64 = BrowserStorage.GetItem("avatar");
        if (base64 is null) return Task.FromResult<byte[]?>(null);
        return Task.FromResult<byte[]?>(Convert.FromBase64String(base64));
    }

    // ✅ 新增：通用设置（localStorage 本身就是 key-value）
    public Task SaveSettingAsync(string key, string value)
    {
        BrowserStorage.SetItem(key, value);
        return Task.CompletedTask;
    }

    public Task<string?> LoadSettingAsync(string key)
    {
        return Task.FromResult(BrowserStorage.GetItem(key));
    }
}