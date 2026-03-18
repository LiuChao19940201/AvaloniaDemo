using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;
using Avalonia;
using Avalonia.Android;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using AvaloniaDemo.Android.Services;
using AvaloniaDemo.Services;
using System;
using Color = Android.Graphics.Color;

namespace AvaloniaDemo.Android
{
    [Activity(
        Label = "AvaloniaDemo.Android",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity<App>
    {
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            return base.CustomizeAppBuilder(builder)
                .WithInterFont()
                .With(new FontManagerOptions
                {
                    FontFallbacks =
                    [
                        new FontFallback
                        {
                            FontFamily = new FontFamily("avares://AvaloniaDemo/Assets/Fonts/AlibabaPuHuiTi-3-55-Regular.ttf#Alibaba PuHuiTi 3.0")
                        }
                    ]
                })
                .UseReactiveUI();
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Window != null)
            {
                WindowCompat.SetDecorFitsSystemWindows(Window, false);

                if (OperatingSystem.IsAndroidVersionAtLeast(35))
                {
                    Window.DecorView.SetBackgroundColor(Color.Transparent);
                    var controller = WindowCompat.GetInsetsController(Window, Window.DecorView);
                    controller?.AppearanceLightStatusBars = true;
                }
                else
                {
#pragma warning disable CA1422
                    Window.SetStatusBarColor(Color.Transparent);
#pragma warning restore CA1422
                    var controller = WindowCompat.GetInsetsController(Window, Window.DecorView);
                    controller?.AppearanceLightStatusBars = true;
                }

                ServiceLocator.StatusBarService = new AndroidStatusBarService(this);
                ServiceLocator.DeviceService = new AndroidDeviceService(this);
                ServiceLocator.ImagePickerService = new AndroidImagePickerService(this); // ← 新增
            }
        }

        // ← 新增：接收相册选图回调
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AndroidImagePickerService.HandleActivityResult(requestCode, resultCode, data, ContentResolver!);
        }
    }

    /*

         # 先切到 Android 项目目录
         cd AvaloniaDemo.Android

         # 清理项目
         dotnet clean AvaloniaDemo.Android.csproj -c Release

         # 构建并打包 Android 项目（以 net10.0-android 为例）
         dotnet build AvaloniaDemo.Android.csproj -c Release -f:net10.0-android -r android-arm64 -p:AndroidPackageFormat=apk --self-contained false

     */
}