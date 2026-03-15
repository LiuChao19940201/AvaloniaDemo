using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AvaloniaDemo.ViewModels.UserControls;

public partial class ChatItemViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnread))]
    private string _name = "";

    [ObservableProperty] private string _preview = "";
    [ObservableProperty] private string _time = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnread))]
    private int _unread = 0;

    public string AvatarLetter => Name.Length > 0 ? Name[..1] : "?";

    // ── 供 AXAML IsVisible 绑定：Unread > 0 时显示红点 ──
    public bool HasUnread => Unread > 0;
}

public partial class ChatViewModel : ObservableObject
{
    public ObservableCollection<ChatItemViewModel> ChatList { get; } = new()
    {
        new() { Name = "文件传输助手", Preview = "欢迎使用微信",    Time = "昨天",  Unread = 0 },
        new() { Name = "张三",         Preview = "好的，明天见",    Time = "10:32", Unread = 2 },
        new() { Name = "产品讨论群",   Preview = "李四：下周评审",  Time = "09:15", Unread = 5 },
        new() { Name = "王五",         Preview = "发了一张图片",    Time = "昨天",  Unread = 0 },
        new() { Name = "家庭群",       Preview = "妈妈：吃饭了",    Time = "周一",  Unread = 1 },
    };

    [RelayCommand]
    private void OpenChat(ChatItemViewModel item)
    { }
}