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
}