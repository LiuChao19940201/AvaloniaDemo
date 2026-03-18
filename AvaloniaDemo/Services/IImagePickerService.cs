using System.IO;
using System.Threading.Tasks;

namespace AvaloniaDemo.Services;

public interface IImagePickerService
{
    Task<Stream?> PickImageAsync();
}