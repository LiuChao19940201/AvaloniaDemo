using AvaloniaDemo.ViewModels.Messages;
using AvaloniaDemo.ViewModels.UserControls.Chat;
using AvaloniaDemo.ViewModels.UserControls.Contacts;
using AvaloniaDemo.ViewModels.UserControls.Discover;
using AvaloniaDemo.ViewModels.UserControls.Profile;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AvaloniaDemo.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject,
    IRecipient<NavigateToServiceMessage>,
    IRecipient<NavigateBackToProfileMessage>,
    IRecipient<NavigateToFundTrackerMessage>,
    IRecipient<NavigateBackFromFundTrackerMessage>,
    IRecipient<NavigateToFundChartMessage>,
    IRecipient<NavigateBackFromFundChartMessage>
{
    // ── 页面 ViewModel 实例（单例，切换不重建，状态保留） ──
    private readonly ChatViewModel         _chatVm         = new();
    private readonly ContactsViewModel     _contactsVm     = new();
    private readonly DiscoverViewModel     _discoverVm     = new();
    private readonly ProfileViewModel      _profileVm      = new();
    private readonly ServiceViewModel      _serviceVm      = new();
    private readonly FundTrackerViewModel  _fundTrackerVm  = new();
    private readonly FundChartViewModel    _fundChartVm    = new();

    // ── ContentControl 绑定目标：赋值即切换页面 ──
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChatActive))]
    [NotifyPropertyChangedFor(nameof(IsContactsActive))]
    [NotifyPropertyChangedFor(nameof(IsDiscoverActive))]
    [NotifyPropertyChangedFor(nameof(IsProfileActive))]
    [NotifyPropertyChangedFor(nameof(CurrentPageTitle))]
    [NotifyPropertyChangedFor(nameof(ShowTitleBar))]
    [NotifyPropertyChangedFor(nameof(ShowTabBar))]
    private ObservableObject _currentPage;

    public MainWindowViewModel()
    {
        _currentPage = _chatVm; // 默认打开「微信」
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    // ── Tab 高亮状态（供 AXAML Converter 绑定） ──
    public bool IsChatActive     => CurrentPage is ChatViewModel;
    public bool IsContactsActive => CurrentPage is ContactsViewModel;
    public bool IsDiscoverActive => CurrentPage is DiscoverViewModel;
    public bool IsProfileActive  => CurrentPage is ProfileViewModel;

    // ── 顶部公共标题文字 ──
    public string CurrentPageTitle => CurrentPage switch
    {
        ChatViewModel        => "微信",
        ContactsViewModel    => "通讯录",
        DiscoverViewModel    => "发现",
        ProfileViewModel     => "我",
        ServiceViewModel     => "服务",
        FundTrackerViewModel => "基金自选跟踪",
        FundChartViewModel   => "净值走势",
        _                    => ""
    };

    // ── 自带 Header 的页面不显示公共标题栏 ──
    public bool ShowTitleBar => CurrentPage is not ProfileViewModel
                                           and not ServiceViewModel
                                           and not FundTrackerViewModel
                                           and not FundChartViewModel;

    // ── 子页全屏，隐藏底部 TabBar ──
    public bool ShowTabBar => CurrentPage is not ServiceViewModel
                                        and not FundTrackerViewModel
                                        and not FundChartViewModel;

    // ── Tab 切换命令（由底部 TabBar Button 绑定） ──
    [RelayCommand] private void SwitchToChat()     => CurrentPage = _chatVm;
    [RelayCommand] private void SwitchToContacts() => CurrentPage = _contactsVm;
    [RelayCommand] private void SwitchToDiscover() => CurrentPage = _discoverVm;
    [RelayCommand] private void SwitchToProfile()  => CurrentPage = _profileVm;

    // ── 接收子页面导航消息 ──
    public void Receive(NavigateToServiceMessage message)
    {
        _serviceVm.OnNavigatedTo();
        CurrentPage = _serviceVm;
    }

    public void Receive(NavigateBackToProfileMessage message)
        => CurrentPage = _profileVm;

    public void Receive(NavigateToFundTrackerMessage message)
    {
        _fundTrackerVm.OnNavigatedTo();
        CurrentPage = _fundTrackerVm;
    }

    public void Receive(NavigateBackFromFundTrackerMessage message)
        => CurrentPage = _chatVm;

    public void Receive(NavigateToFundChartMessage message)
    {
        _fundChartVm.OnNavigatedTo(message.Code, message.Name);
        CurrentPage = _fundChartVm;
    }

    public void Receive(NavigateBackFromFundChartMessage message)
        => CurrentPage = _fundTrackerVm;
}
