using Android.App;
using AvaloniaDemo.Services;

namespace AvaloniaDemo.Android.Services
{
    public class AndroidStatusBarService : IStatusBarService
    {
        private readonly Activity _activity;

        public AndroidStatusBarService(Activity activity)
        {
            _activity = activity;
        }

        public int GetStatusBarHeight()
        {
            var resourceId = _activity.Resources?.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId is > 0)
            {
                // 获取像素值并转换为 dp
                var heightPx = _activity.Resources!.GetDimensionPixelSize(resourceId.Value);
                var density = _activity.Resources.DisplayMetrics?.Density ?? 1f;
                return (int)(heightPx / density);
            }
            return 40; // 默认值
        }
    }
}
