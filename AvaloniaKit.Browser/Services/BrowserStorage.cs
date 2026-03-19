using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace AvaloniaKit.Browser.Services;

[SupportedOSPlatform("browser")]
internal static partial class BrowserStorage
{
    [JSImport("getItem", "storage")]
    internal static partial string? GetItem(string key);

    [JSImport("setItem", "storage")]
    internal static partial void SetItem(string key, string value);

    // 返回 base64 字符串，高效编组，不再返回 JSObject
    [JSImport("pickImageFile", "storage")]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    internal static partial Task<string?> PickImageFileAsync();
}