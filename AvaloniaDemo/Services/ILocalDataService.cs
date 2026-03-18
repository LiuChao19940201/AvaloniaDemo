using System.Threading.Tasks;

namespace AvaloniaDemo.Services;

public interface ILocalDataService
{
    Task SaveAvatarAsync(byte[] imageData);
    Task<byte[]?> LoadAvatarAsync();
}