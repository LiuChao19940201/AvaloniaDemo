using Android.App;
using Android.Content.PM;
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
    }

    /*
        
         # 清理项目
         dotnet clean TestAva.Android.csproj -c Release

         # 构建并打包 Android 项目（以 net8.0-android 为例）
         dotnet build AvaloniaDemo.Android.csproj -c Release -f:net8.0-android -r android-arm64 -p:AndroidPackageFormat=apk --self-contained false
    
     */
}
