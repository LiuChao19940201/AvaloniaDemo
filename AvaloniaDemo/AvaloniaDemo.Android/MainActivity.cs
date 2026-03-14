using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.View;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;

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
                //.WithFont_Roboto()
                .UseReactiveUI();
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // 设置状态栏透明
            if (Window != null)
            {
                Window.SetStatusBarColor(Color.Transparent);

                // 让内容延伸到状态栏下面
                WindowCompat.SetDecorFitsSystemWindows(Window, false);
            }
        }
    }

    /*
        
        #先切到 Android 项目目录
        cd AvaloniaDemo/AvaloniaDemo.Android

         # 清理项目
         dotnet clean AvaloniaDemo.Android.csproj -c Release

         # 构建并打包 Android 项目（以 net10.0-android 为例）
         dotnet build AvaloniaDemo.Android.csproj -c Release -f:net10.0-android -r android-arm64 -p:AndroidPackageFormat=apk --self-contained false
    
     */
}
