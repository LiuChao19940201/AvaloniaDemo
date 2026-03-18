using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using AvaloniaDemo.Services;
using System.IO;
using System.Threading.Tasks;

namespace AvaloniaDemo.Browser.Services;

public class BrowserImagePickerService : IImagePickerService
{
    public async Task<Stream?> PickImageAsync()
    {
        var singleView = Application.Current?.ApplicationLifetime
            as ISingleViewApplicationLifetime;

        var mainView = singleView?.MainView;
        if (mainView is null) return null;

        var topLevel = TopLevel.GetTopLevel(mainView);
        if (topLevel is null) return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择头像图片",
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });

        if (files.Count == 0) return null;

        // ✅ 关键修复：浏览器流底层是 JS Interop 异步驱动
        // 必须在这里先将数据完整异步读入 MemoryStream
        // 再返回纯内存流，避免 ViewModel 的 new Bitmap(stream) 同步读时死锁
        await using var browserStream = await files[0].OpenReadAsync();
        var memoryStream = new MemoryStream();
        await browserStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream; // 返回的是纯内存流，new Bitmap() 同步读安全
    }
}