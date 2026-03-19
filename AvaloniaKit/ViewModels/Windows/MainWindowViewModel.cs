using AvaloniaKit.ViewModels.Messages;
using AvaloniaKit.ViewModels.UserControls.Chat;
using AvaloniaKit.ViewModels.UserControls.Contacts;
using AvaloniaKit.ViewModels.UserControls.Discover;
using AvaloniaKit.ViewModels.UserControls.Profile;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AvaloniaKit.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject,
    IRecipient<NavigateToServiceMessage>,
    IRecipient<NavigateBackToProfileMessage>,
    IRecipient<NavigateToFundTrackerMessage>,
    IRecipient<NavigateBackFromFundTrackerMessage>,
    IRecipient<NavigateToFundChartMessage>,
    IRecipient<NavigateBackFromFundChartMessage>,
    // ★ 新增：网易云
    IRecipient<NavigateToNeteaseMessage>,
    IRecipient<NavigateBackFromNeteaseMessage>,
    IRecipient<NavigateToNeteasePlayerMessage>,
    IRecipient<NavigateBackFromNeteasePlayerMessage>
{
    // ── 页面 ViewModel 实例 ──
    private readonly ChatViewModel            _chatVm           = new();
    private readonly ContactsViewModel        _contactsVm       = new();
    private readonly DiscoverViewModel        _discoverVm       = new();
    private readonly ProfileViewModel         _profileVm        = new();
    private readonly ServiceViewModel         _serviceVm        = new();
    private readonly FundTrackerViewModel     _fundTrackerVm    = new();
    private readonly FundChartViewModel       _fundChartVm      = new();
    private readonly NeteaseViewModel         _neteaseVm        = new();   // ★
    private readonly NeteasePlayerViewModel   _neteasePlayerVm  = new();   // ★

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
        _currentPage = _chatVm;
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public bool IsChatActive     => CurrentPage is ChatViewModel;
    public bool IsContactsActive => CurrentPage is ContactsViewModel;
    public bool IsDiscoverActive => CurrentPage is DiscoverViewModel;
    public bool IsProfileActive  => CurrentPage is ProfileViewModel;

    public string CurrentPageTitle => CurrentPage switch
    {
        ChatViewModel           => "微信",
        ContactsViewModel       => "通讯录",
        DiscoverViewModel       => "发现",
        ProfileViewModel        => "我",
        ServiceViewModel        => "服务",
        FundTrackerViewModel    => "基金自选跟踪",
        FundChartViewModel      => "净值走势",
        NeteaseViewModel        => "网易云音乐",
        NeteasePlayerViewModel  => "",   // 播放器自带顶栏
        _                       => ""
    };

    public bool ShowTitleBar => CurrentPage is not ProfileViewModel
                                           and not ServiceViewModel
                                           and not FundTrackerViewModel
                                           and not FundChartViewModel
                                           and not NeteaseViewModel        // ★
                                           and not NeteasePlayerViewModel; // ★

    public bool ShowTabBar => CurrentPage is not ServiceViewModel
                                        and not FundTrackerViewModel
                                        and not FundChartViewModel
                                        and not NeteaseViewModel           // ★
                                        and not NeteasePlayerViewModel;    // ★

    [RelayCommand] private void SwitchToChat()     => CurrentPage = _chatVm;
    [RelayCommand] private void SwitchToContacts() => CurrentPage = _contactsVm;
    [RelayCommand] private void SwitchToDiscover() => CurrentPage = _discoverVm;
    [RelayCommand] private void SwitchToProfile()  => CurrentPage = _profileVm;

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

    // ── ★ 网易云导航 ─────────────────────────────────────────────────────────
    public void Receive(NavigateToNeteaseMessage message)
    {
        _neteaseVm.OnNavigatedTo();
        CurrentPage = _neteaseVm;
    }

    public void Receive(NavigateBackFromNeteaseMessage message)
        => CurrentPage = _chatVm;

    public void Receive(NavigateToNeteasePlayerMessage message)
    {
        _neteasePlayerVm.OnNavigatedTo(
            message.SongId, message.SongName,
            message.Artist, message.Album, message.CoverUrl);
        CurrentPage = _neteasePlayerVm;
    }

    public void Receive(NavigateBackFromNeteasePlayerMessage message)
        => CurrentPage = _neteaseVm;
}
