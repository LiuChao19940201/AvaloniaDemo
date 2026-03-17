using AvaloniaDemo.ViewModels.UserControls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaDemo.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    // ── 四个页面 ViewModel 实例（单例，切换不重建，状态保留） ──
    private readonly ChatViewModel _chatVm = new();
    private readonly ContactsViewModel _contactsVm = new();
    private readonly DiscoverViewModel _discoverVm = new();
    private readonly ProfileViewModel _profileVm = new();

    // ── ContentControl 绑定目标：赋值即切换页面 ──
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChatActive))]
    [NotifyPropertyChangedFor(nameof(IsContactsActive))]
    [NotifyPropertyChangedFor(nameof(IsDiscoverActive))]
    [NotifyPropertyChangedFor(nameof(IsProfileActive))]
    [NotifyPropertyChangedFor(nameof(CurrentPageTitle))]
    [NotifyPropertyChangedFor(nameof(ShowTitleBar))]
    private ObservableObject _currentPage;

    public MainWindowViewModel()
    {
        _currentPage = _chatVm; // 默认打开「发现」
    }

    // ── Tab 高亮状态（供 AXAML Converter 绑定） ──
    public bool IsChatActive => CurrentPage is ChatViewModel;

    public bool IsContactsActive => CurrentPage is ContactsViewModel;
    public bool IsDiscoverActive => CurrentPage is DiscoverViewModel;
    public bool IsProfileActive => CurrentPage is ProfileViewModel;

    // ── 顶部公共标题文字 ──
    public string CurrentPageTitle => CurrentPage switch
    {
        ChatViewModel => "微信",
        ContactsViewModel => "通讯录",
        DiscoverViewModel => "发现",
        ProfileViewModel => "我",
        _ => ""
    };

    // ── 「我」页面自带 Header，不显示公共标题栏 ──
    public bool ShowTitleBar => CurrentPage is not ProfileViewModel;

    // ── Tab 切换命令（由底部 TabBar Button 绑定） ──
    [RelayCommand] private void SwitchToChat() => CurrentPage = _chatVm;

    [RelayCommand] private void SwitchToContacts() => CurrentPage = _contactsVm;

    [RelayCommand] private void SwitchToDiscover() => CurrentPage = _discoverVm;

    [RelayCommand] private void SwitchToProfile() => CurrentPage = _profileVm;
}