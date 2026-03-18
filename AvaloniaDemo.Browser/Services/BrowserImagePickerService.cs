using AvaloniaDemo.Services;
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace AvaloniaDemo.Browser.Services;

[SupportedOSPlatform("browser")]
public class BrowserImagePickerService : IImagePickerService
{
    public async Task<Stream?> PickImageAsync()
    {
        try
        {
            var base64 = await BrowserStorage.PickImageFileAsync();

            if (string.IsNullOrEmpty(base64)) return null;

            // base64 → byte[] → MemoryStream，全在 .NET 侧完成，无额外互操作开销
            var bytes = Convert.FromBase64String(base64);
            return new MemoryStream(bytes);
        }
        catch
        {
            return null;
        }
    }
}