using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using AvaloniaDemo.Services;
using System.IO;
using System.Threading.Tasks;

namespace AvaloniaDemo.Desktop.Services;

public class DesktopImagePickerService : IImagePickerService
{
    public async Task<Stream?> PickImageAsync()
    {
        var mainWindow = (Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (mainWindow is null) return null;

        var topLevel = TopLevel.GetTopLevel(mainWindow);
        if (topLevel is null) return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择头像图片",
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });

        if (files.Count == 0) return null;
        return await files[0].OpenReadAsync();
    }
}