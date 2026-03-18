using Android.App;
using Android.Content;
using AvaloniaDemo.Services;
using System.IO;
using System.Threading.Tasks;

namespace AvaloniaDemo.Android.Services;

public class AndroidImagePickerService : IImagePickerService
{
    public const int RequestCode = 2001;

    private static TaskCompletionSource<Stream?>? _tcs;
    private readonly Activity _activity;

    public AndroidImagePickerService(Activity activity)
    {
        _activity = activity;
    }

    public Task<Stream?> PickImageAsync()
    {
        _tcs = new TaskCompletionSource<Stream?>();

        var intent = new Intent(Intent.ActionPick);
        intent.SetType("image/*");
        _activity.StartActivityForResult(intent, RequestCode);

        return _tcs.Task;
    }

    /// <summary>
    /// 由 MainActivity.OnActivityResult 调用，完成任务
    /// </summary>
    public static void HandleActivityResult(
        int requestCode, Result resultCode, Intent? data, ContentResolver contentResolver)
    {
        if (requestCode != RequestCode || _tcs is null) return;

        if (resultCode == Result.Ok && data?.Data is global::Android.Net.Uri uri)
        {
            try
            {
                var stream = contentResolver.OpenInputStream(uri);
                _tcs.TrySetResult(stream);
            }
            catch
            {
                _tcs.TrySetResult(null);
            }
        }
        else
        {
            _tcs.TrySetResult(null);
        }

        _tcs = null;
    }
}