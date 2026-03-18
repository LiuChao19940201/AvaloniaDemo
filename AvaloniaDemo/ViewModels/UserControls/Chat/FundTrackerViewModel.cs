using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaDemo.ViewModels.Messages;
using System.Collections.Generic;

namespace AvaloniaDemo.ViewModels.UserControls.Chat;

// ══════════════════════════════════════════════════════════════════════════════
//  FundTrackerViewModel
//  功能：自选列表（本地持久化） + 搜索基金 + 添加 + 删除 + 实时净值刷新
//  平台：Desktop 用文件持久化；Web/无文件系统时自动退化为内存持久化
// ══════════════════════════════════════════════════════════════════════════════
public partial class FundTrackerViewModel : ObservableObject
{
    // ── HTTP ─────────────────────────────────────────────────────────────────
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };

    static FundTrackerViewModel()
    {
        _http.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        _http.DefaultRequestHeaders.TryAddWithoutValidation(
            "Referer", "https://fund.eastmoney.com/");
    }

    // ── 持久化路径（Desktop）；Web 平台 GetSaveFilePath() 返回 null ──────────
    private static string? GetSaveFilePath()
    {
        try
        {
            string dir = AppContext.BaseDirectory;
            if (string.IsNullOrEmpty(dir)) return null;
            return Path.Combine(dir, "fund_watchlist.json");
        }
        catch { return null; }
    }

    // ── 自选代码列表（内存，同步到磁盘） ────────────────────────────────────
    private readonly ObservableCollection<string> _watchCodes = new();

    // ── 状态属性 ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool   _isLoading   = false;
    [ObservableProperty] private bool   _isOffline   = false;
    [ObservableProperty] private string _statusText  = "";

    // ── 搜索面板 ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool   _showSearch  = false;   // 面板是否展开
    [ObservableProperty] private string _searchText  = "";
    [ObservableProperty] private bool   _isSearching = false;
    [ObservableProperty] private string _searchStatus = "";

    // ── 自选基金列表（显示在表格中） ─────────────────────────────────────────
    public ObservableCollection<FundItemViewModel> Funds { get; } = new();

    // ── 搜索结果列表 ──────────────────────────────────────────────────────────
    public ObservableCollection<SearchResultItem> SearchResults { get; } = new();

    private CancellationTokenSource? _refreshCts;

    // ── 构造：加载持久化列表 ──────────────────────────────────────────────────
    public FundTrackerViewModel()
    {
        LoadWatchlist();
    }

    // ── 页面进入 ──────────────────────────────────────────────────────────────
    public void OnNavigatedTo()
    {
        if (Funds.Count > 0 && !IsOffline) return;
        _ = DoRefreshAsync();
    }

    // ── 导航：返回 ────────────────────────────────────────────────────────────
    [RelayCommand]
    private void GoBack()
        => WeakReferenceMessenger.Default.Send(new NavigateBackFromFundTrackerMessage());

    // ══════════════════════════════════════════════════════════════════════════
    //  自选列表刷新
    // ══════════════════════════════════════════════════════════════════════════
    [RelayCommand]
    private void Refresh() => _ = DoRefreshAsync();

    private async Task DoRefreshAsync()
    {
        _refreshCts?.Cancel();
        _refreshCts?.Dispose();
        _refreshCts = new CancellationTokenSource();
        var ct = _refreshCts.Token;

        IsLoading  = true;
        IsOffline  = false;
        StatusText = "加载中…";
        Funds.Clear();

        if (_watchCodes.Count == 0)
        {
            IsLoading  = false;
            StatusText = "自选列表为空，点击右上角「+」添加基金";
            return;
        }

        int ok = 0;
        try
        {
            foreach (var code in _watchCodes.ToList())
            {
                ct.ThrowIfCancellationRequested();
                var item = await FetchFundAsync(code, ct);
                Funds.Add(item);
                if (!item.IsMock) ok++;
            }
        }
        catch (OperationCanceledException) { return; }
        finally { IsLoading = false; }

        if (ok == 0)
        {
            IsOffline  = true;
            StatusText = "网络不可用，显示本地数据";
        }
        else
        {
            StatusText = $"更新于 {DateTime.Now:HH:mm:ss}";
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  搜索功能
    // ══════════════════════════════════════════════════════════════════════════

    // 展开/收起搜索面板
    [RelayCommand]
    private void ToggleSearch()
    {
        ShowSearch = !ShowSearch;
        if (!ShowSearch)
        {
            SearchText    = "";
            SearchStatus  = "";
            SearchResults.Clear();
        }
    }

    // 搜索基金（名称或代码）
    [RelayCommand]
    private async Task SearchFund()
    {
        string keyword = SearchText.Trim();
        if (string.IsNullOrEmpty(keyword)) return;

        IsSearching  = true;
        SearchStatus = "搜索中…";
        SearchResults.Clear();

        try
        {
            try
            {
                // 先尝试全量搜索 JS
                string url = "https://fund.eastmoney.com/js/fundcode_search.js";
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                string raw = await _http.GetStringAsync(url, cts.Token);

                var match = Regex.Match(raw, @"var r = (\[.+\])");
                if (match.Success)
                {
                    using var doc = JsonDocument.Parse(match.Groups[1].Value);
                    int count = 0;
                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        string code   = item[0].GetString() ?? "";
                        string pinyin = item[1].GetString() ?? "";
                        string name   = item[2].GetString() ?? "";

                        if (code.Contains(keyword)
                            || name.Contains(keyword)
                            || pinyin.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            SearchResults.Add(new SearchResultItem { Code = code, Name = name });
                            if (++count >= 8) break;
                        }
                    }
                    SearchStatus = count > 0
                        ? $"找到 {count} 条，选中后点「添加」"
                        : "未找到相关基金";
                    return;   // finally 会在 return 前执行，IsSearching 会被重置
                }
            }
            catch (OperationCanceledException) { }
            catch { }

            // 降级：直接按6位代码查询
            await SearchByCodeAsync(keyword);
        }
        finally
        {
            IsSearching = false;
        }
    }


    private async Task SearchByCodeAsync(string code)
    {
        if (!Regex.IsMatch(code, @"^\d{6}$"))
        {
            SearchStatus = "未找到（可直接输入6位基金代码重试）";
            return;
        }
        try
        {
            long   ts  = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string url = $"https://fundgz.1234567.com.cn/js/{code}.js?rt={ts}";
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            string raw   = await _http.GetStringAsync(url, cts.Token);
            var    m     = Regex.Match(raw, @"jsonpgz\((.+)\)");
            if (m.Success)
            {
                using var doc = JsonDocument.Parse(m.Groups[1].Value);
                string name = doc.RootElement.TryGet("name") ?? code;
                SearchResults.Add(new SearchResultItem { Code = code, Name = name });
                SearchStatus = "找到 1 条，选中后点「添加」";
            }
            else
            {
                SearchStatus = "未找到该基金代码";
            }
        }
        catch
        {
            SearchStatus = "搜索失败，请检查网络";
        }
    }

    // 把搜索结果中选中项加入自选
    [RelayCommand]
    private async Task AddFund(SearchResultItem? item)
    {
        if (item is null) return;
        if (_watchCodes.Contains(item.Code))
        {
            SearchStatus = $"{item.Code} 已在自选列表中";
            return;
        }

        _watchCodes.Add(item.Code);
        SaveWatchlist();

        // 立即拉取这一只的净值并插入列表
        StatusText = $"正在加载 {item.Name}…";
        var fund = await FetchFundAsync(item.Code, CancellationToken.None);
        Funds.Add(fund);
        StatusText = $"已添加 {item.Name}，更新于 {DateTime.Now:HH:mm:ss}";
        SearchStatus = $"{item.Code} 已添加到自选";
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  点击基金行 → 跳转曲线页
    // ══════════════════════════════════════════════════════════════════════════
    [RelayCommand]
    private void OpenChart(FundItemViewModel? item)
    {
        if (item is null) return;
        WeakReferenceMessenger.Default.Send(
            new NavigateToFundChartMessage(item.Code, item.Name));
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  删除自选
    // ══════════════════════════════════════════════════════════════════════════
    [RelayCommand]
    private void RemoveFund(FundItemViewModel? item)
    {
        if (item is null) return;
        _watchCodes.Remove(item.Code);
        Funds.Remove(item);
        SaveWatchlist();
        StatusText = $"已从自选移除 {item.Name}";
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  持久化（Desktop：JSON文件；Web：仅内存）
    // ══════════════════════════════════════════════════════════════════════════
    private void SaveWatchlist()
    {
        string? path = GetSaveFilePath();
        if (path is null) return;
        try
        {
            File.WriteAllText(path,
                JsonSerializer.Serialize(_watchCodes.ToList()));
        }
        catch { }
    }

    private void LoadWatchlist()
    {
        string? path = GetSaveFilePath();
        if (path is null || !File.Exists(path)) return;
        try
        {
            var list = JsonSerializer.Deserialize<List<string>>(
                           File.ReadAllText(path));
            if (list is null) return;
            foreach (var code in list)
                if (!_watchCodes.Contains(code))
                    _watchCodes.Add(code);
        }
        catch { }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Net fetch（天天基金 → 东方财富 → mock）
    // ══════════════════════════════════════════════════════════════════════════
    private async Task<FundItemViewModel> FetchFundAsync(string code, CancellationToken ct)
    {
        // -- 数据源1：天天基金 JSONP ------------------------------------------
        try
        {
            long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string url = $"https://fundgz.1234567.com.cn/js/{code}.js?rt={ts}";
            using var reqCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            reqCts.CancelAfter(TimeSpan.FromSeconds(10));

            string raw = await _http.GetStringAsync(url, reqCts.Token);
            var m = Regex.Match(raw, @"jsonpgz\((.+)\)");
            if (m.Success)
            {
                using var doc = JsonDocument.Parse(m.Groups[1].Value);
                var root = doc.RootElement;
                return new FundItemViewModel
                {
                    Code      = code,
                    Name      = root.TryGet("name")  ?? code,
                    LastNav   = root.TryGet("dwjz")  ?? "--",
                    EstNav    = root.TryGet("gsz")   ?? "--",
                    ChangeRaw = root.TryGet("gszzl") ?? "0",
                    UpdatedAt = (root.TryGet("gztime") ?? "--").Length >= 5
                                    ? root.TryGet("gztime")![..5]
                                    : root.TryGet("gztime") ?? "--",
                    Source    = "天天基金",
                    IsMock    = false,
                };
            }
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
        catch (OperationCanceledException) { throw; }
        catch { }

        // -- 数据源2：东方财富 API --------------------------------------------
        try
        {
            string url = $"https://push2.eastmoney.com/api/qt/slist/get" +
                         $"?fltt=2&fields=f2,f3,f12,f14&secid=0.{code}";
            using var reqCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            reqCts.CancelAfter(TimeSpan.FromSeconds(10));

            string raw = await _http.GetStringAsync(url, reqCts.Token);
            using var doc = JsonDocument.Parse(raw);

            if (doc.RootElement.TryGetProperty("data", out var data) &&
                data.TryGetProperty("diff", out var diff) &&
                diff.ValueKind == JsonValueKind.Array &&
                diff.GetArrayLength() > 0)
            {
                var first = diff[0];
                string nav = first.TryGet("f2") ?? "--";
                return new FundItemViewModel
                {
                    Code      = code,
                    Name      = first.TryGet("f14") ?? code,
                    LastNav   = nav,
                    EstNav    = nav,
                    ChangeRaw = first.TryGet("f3") ?? "0",
                    UpdatedAt = DateTime.Now.ToString("HH:mm"),
                    Source    = "东方财富",
                    IsMock    = false,
                };
            }
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
        catch (OperationCanceledException) { throw; }
        catch { }

        return FundItemViewModel.Mock(code);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  SearchResultItem
// ══════════════════════════════════════════════════════════════════════════════
public partial class SearchResultItem : ObservableObject
{
    [ObservableProperty] private string _code = "";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private bool   _isSelected = false;

    public string Display => $"{Code}  {Name}";
}

// ══════════════════════════════════════════════════════════════════════════════
//  FundItemViewModel
// ══════════════════════════════════════════════════════════════════════════════
public partial class FundItemViewModel : ObservableObject
{
    [ObservableProperty] private string _code      = "";
    [ObservableProperty] private string _name      = "";
    [ObservableProperty] private string _lastNav   = "--";
    [ObservableProperty] private string _estNav    = "--";
    [ObservableProperty] private string _changeRaw = "0";
    [ObservableProperty] private string _updatedAt = "--";
    [ObservableProperty] private string _source    = "--";
    [ObservableProperty] private bool   _isMock    = false;

    public bool   IsUp       => double.TryParse(ChangeRaw, out double v) && v >= 0;
    public string ChangeText
    {
        get
        {
            if (!double.TryParse(ChangeRaw, out double v)) return "--";
            return (v >= 0 ? "+" : "") + v.ToString("F2") + "%";
        }
    }

    // A股惯例：涨红跌绿
    public string ChipBackground => IsUp ? "#1AE05C5C" : "#1A18B06A";
    public string ChipForeground => IsUp ? "#C0392B"   : "#18B06A";

    private static readonly (string code, string name, double nav, double chg)[] _mocks =
    {
        ("000001", "华夏成长混合",    1.8423,  0.56),
        ("110022", "易方达消费行业",  3.2100, -0.32),
        ("161725", "招商中证白酒",    1.1560,  1.20),
        ("000961", "天弘沪深300ETF",  1.3210,  0.08),
        ("270042", "广发纳斯达克100", 2.6780, -0.75),
    };

    internal static FundItemViewModel Mock(string code)
    {
        foreach (var (c, n, nav, chg) in _mocks)
            if (c == code)
                return new FundItemViewModel
                {
                    Code      = c,
                    Name      = n,
                    LastNav   = nav.ToString("F4"),
                    EstNav    = "--",
                    ChangeRaw = chg.ToString("F2"),
                    UpdatedAt = "离线",
                    Source    = "本地缓存",
                    IsMock    = true,
                };
        return new FundItemViewModel { Code = code, Name = code, IsMock = true };
    }
}

// ── JsonElement 扩展 ─────────────────────────────────────────────────────────
internal static class JsonElementExtensions
{
    internal static string? TryGet(this JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null
            ? v.GetString() ?? v.GetRawText()
            : null;
}
