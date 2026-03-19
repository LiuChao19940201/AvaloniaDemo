using System.IO;
using System.Threading.Tasks;

namespace AvaloniaKit.Services;

public interface IImagePickerService
{
    Task<Stream?> PickImageAsync();
}