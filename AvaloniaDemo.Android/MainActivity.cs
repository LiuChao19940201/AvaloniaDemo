using Android.App;
using Android.Content.PM;
using Android.Graphics;
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
            // Android 系统字体链自带中文 (Noto Sans CJK) + Emoji 支持
            // 不设置 DefaultFamilyName，不覆盖系统默认字体
            return base.CustomizeAppBuilder(builder)
                .WithInterFont()
                .UseReactiveUI();
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Window != null)
            {
                // 设置内容扩展到状态栏
                WindowCompat.SetDecorFitsSystemWindows(Window, false);

                // 设置状态栏背景为透明，并根据 Android 版本设置状态栏图标颜色
                if (OperatingSystem.IsAndroidVersionAtLeast(35))
                {
                    // Android 35+ 新写法
                    Window.DecorView.SetBackgroundColor(Color.Transparent);

                    var controller = WindowCompat.GetInsetsController(Window, Window.DecorView);
                    controller?.AppearanceLightStatusBars = true; // 状态栏图标深色
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
            }
        }
    }

    /*
        
        #先切到 Android 项目目录
        cd AvaloniaDemo.Android

         # 清理项目
         dotnet clean AvaloniaDemo.Android.csproj -c Release

         # 构建并打包 Android 项目（以 net10.0-android 为例）
         dotnet build AvaloniaDemo.Android.csproj -c Release -f:net10.0-android -r android-arm64 -p:AndroidPackageFormat=apk --self-contained false
    
     */
}
