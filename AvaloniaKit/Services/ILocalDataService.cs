using System.Threading.Tasks;

namespace AvaloniaKit.Services;

public interface ILocalDataService
{
    Task SaveAvatarAsync(byte[] imageData);
    Task<byte[]?> LoadAvatarAsync();

    // ✅ 新增：通用 key-value 设置存储
    Task SaveSettingAsync(string key, string value);
    Task<string?> LoadSettingAsync(string key);
}