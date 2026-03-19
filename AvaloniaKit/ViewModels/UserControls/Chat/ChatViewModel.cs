using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using AvaloniaDemo.ViewModels.Messages;

namespace AvaloniaDemo.ViewModels.UserControls.Chat;

public partial class ChatViewModel : ObservableObject
{
    public ObservableCollection<ChatItemViewModel> ChatList { get; } = new()
    {
        new() { Name = "基金自选跟踪", Preview = "点击查看自选基金实时净值", Time = "昨天",  Unread = 0, IsFundTracker = true  },
        new() { Name = "文件传输助手", Preview = "欢迎使用微信",             Time = "昨天",  Unread = 0  },
        new() { Name = "张三",         Preview = "好的，明天见",             Time = "10:32", Unread = 2  },
        new() { Name = "产品讨论群",   Preview = "李四：下周评审",           Time = "09:15", Unread = 5  },
        new() { Name = "王五",         Preview = "发了一张图片",             Time = "昨天",  Unread = 0  },
        new() { Name = "家庭群",       Preview = "妈妈：吃饭了",             Time = "周一",  Unread = 1  },
    };

    [RelayCommand]
    private void OpenChat(ChatItemViewModel item)
    {
        if (item.IsFundTracker)
            WeakReferenceMessenger.Default.Send(new NavigateToFundTrackerMessage());

        // 其他普通聊天项的逻辑留空，后续扩展
    }
}

public partial class ChatItemViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnread))]
    private string _name = "";

    [ObservableProperty] private string _preview = "";
    [ObservableProperty] private string _time    = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnread))]
    private int _unread = 0;

    /// <summary>true 表示这一行是「基金自选跟踪」功能入口，而非普通聊天</summary>
    public bool IsFundTracker { get; init; } = false;

    public string AvatarLetter => Name.Length > 0 ? Name[..1] : "?";
    public bool   HasUnread    => Unread > 0;
}
