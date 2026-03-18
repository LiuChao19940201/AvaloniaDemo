using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaDemo.ViewModels.Messages;

namespace AvaloniaDemo.ViewModels.UserControls.Chat;

// ══════════════════════════════════════════════════════════════════════════════
//  FundChartViewModel
//  · 区间：当月 / 近三个月（对应 WinForm 的 rbMonth / rb3Month）
//  · 数据：东方财富历史净值 API（lsjz），与 ChartForm.LoadData 完全一致
//  · 折线：把净值点序列暴露给 View，由 Canvas 手绘，不依赖第三方图表库
// ══════════════════════════════════════════════════════════════════════════════
public partial class FundChartViewModel : ObservableObject
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };

    static FundChartViewModel()
    {
        _http.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        _http.DefaultRequestHeaders.TryAddWithoutValidation(
            "Referer", "https://fundf10.eastmoney.com/");
    }

    // ── 基金信息（由导航消息传入）────────────────────────────────────────────
    [ObservableProperty] private string _fundCode = "";
    [ObservableProperty] private string _fundName = "";
    [ObservableProperty] private string _pageTitle = "";

    // ── UI 状态 ───────────────────────────────────────────────────────────────
    [ObservableProperty] private bool   _isLoading   = false;
    [ObservableProperty] private bool   _hasError    = false;
    [ObservableProperty] private string _errorText   = "";
    [ObservableProperty] private string _statusText  = "";

    // ── 区间选择（0 = 当月，1 = 近三个月）────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMonthSelected))]
    [NotifyPropertyChangedFor(nameof(Is3MonthSelected))]
    private int _selectedRange = 0;

    public bool IsMonthSelected  => SelectedRange == 0;
    public bool Is3MonthSelected => SelectedRange == 1;

    // ── 折线数据点（暴露给 View 的 Canvas 绘图）──────────────────────────────
    public ObservableCollection<NavPoint> NavPoints { get; } = new();

    // ── 涨跌颜色（供 View 绑定）──────────────────────────────────────────────
    [ObservableProperty] private string _lineColor   = "#4080FF";
    [ObservableProperty] private string _changeText  = "--";
    [ObservableProperty] private bool   _isUp        = true;

    // Y 轴范围（供 Canvas 绘图归一化用）
    [ObservableProperty] private double _yMin = 0;
    [ObservableProperty] private double _yMax = 1;

    private CancellationTokenSource? _cts;

    // ── 由 MainWindowViewModel.Receive 调用，传入基金信息 ─────────────────────
    public void OnNavigatedTo(string code, string name)
    {
        FundCode  = code;
        FundName  = name;
        PageTitle = $"{name}（{code}）";
        SelectedRange = 0;
        _ = LoadDataAsync(0);
    }

    // ── 导航：返回基金列表 ────────────────────────────────────────────────────
    [RelayCommand]
    private void GoBack()
        => WeakReferenceMessenger.Default.Send(new NavigateBackFromFundChartMessage());

    // ── 区间切换 ──────────────────────────────────────────────────────────────
    [RelayCommand]
    private void SelectMonth()
    {
        if (SelectedRange == 0) return;
        SelectedRange = 0;
        _ = LoadDataAsync(0);
    }

    [RelayCommand]
    private void Select3Month()
    {
        if (SelectedRange == 1) return;
        SelectedRange = 1;
        _ = LoadDataAsync(1);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  历史净值加载（对应 ChartForm.LoadData）
    // ══════════════════════════════════════════════════════════════════════════
    private async Task LoadDataAsync(int range)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        IsLoading  = true;
        HasError   = false;
        NavPoints.Clear();
        StatusText = "数据加载中…";

        try
        {
            DateTime endDate   = DateTime.Today;
            DateTime startDate = range == 0
                ? new DateTime(endDate.Year, endDate.Month, 1)   // 当月1日
                : endDate.AddMonths(-3);                          // 近三个月

            string url = $"https://api.fund.eastmoney.com/f10/lsjz" +
                         $"?fundCode={FundCode}" +
                         $"&pageIndex=1&pageSize=200" +
                         $"&startDate={startDate:yyyy-MM-dd}" +
                         $"&endDate={endDate:yyyy-MM-dd}";

            using var reqCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            reqCts.CancelAfter(TimeSpan.FromSeconds(12));

            string raw = await _http.GetStringAsync(url, reqCts.Token);

            using var doc  = JsonDocument.Parse(raw);
            var root       = doc.RootElement;

            if (!root.TryGetProperty("Data", out var data) ||
                !data.TryGetProperty("LSJZList", out var list) ||
                list.ValueKind != JsonValueKind.Array ||
                list.GetArrayLength() == 0)
            {
                SetError("暂无历史净值数据");
                return;
            }

            // 解析并排序（API 返回倒序，需正序）
            var points = new List<NavPoint>();
            foreach (var item in list.EnumerateArray())
            {
                string dateStr = item.TryGetStr("FSRQ") ?? "";
                string navStr  = item.TryGetStr("DWJZ") ?? "";
                if (DateTime.TryParse(dateStr, out DateTime dt) &&
                    double.TryParse(navStr, out double nav))
                {
                    points.Add(new NavPoint { Date = dt, Nav = nav });
                }
            }
            points.Sort((a, b) => a.Date.CompareTo(b.Date));

            if (points.Count == 0)
            {
                SetError("暂无可用数据");
                return;
            }

            // 计算 Y 轴范围（与 ChartForm 完全一致）
            double minNav = double.MaxValue, maxNav = double.MinValue;
            foreach (var p in points)
            {
                if (p.Nav < minNav) minNav = p.Nav;
                if (p.Nav > maxNav) maxNav = p.Nav;
            }
            double padding = (maxNav - minNav) * 0.1;
            if (padding < 0.001) padding = 0.001;
            YMin = Math.Round(minNav - padding, 4);
            YMax = Math.Round(maxNav + padding, 4);

            // 涨跌判断（区间首尾对比）
            bool up    = points[^1].Nav >= points[0].Nav;
            IsUp       = up;
            LineColor  = up ? "#C0392B" : "#18B06A";   // 涨红跌绿（A股惯例）
            double chg = points.Count >= 2
                ? (points[^1].Nav - points[0].Nav) / points[0].Nav * 100
                : 0;
            ChangeText = $"{(up ? "+" : "")}{chg:F2}%  {points[0].Date:MM/dd} → {points[^1].Date:MM/dd}";

            foreach (var p in points)
                NavPoints.Add(p);

            StatusText = $"共 {points.Count} 个交易日  最新净值 {points[^1].Nav:F4}";
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            SetError("请求超时，请检查网络");
        }
        catch (OperationCanceledException) { /* 被新请求取消，静默退出 */ }
        catch (Exception ex)
        {
            SetError($"加载失败：{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void SetError(string msg)
    {
        HasError  = true;
        ErrorText = msg;
    }
}

// ── 净值数据点 ───────────────────────────────────────────────────────────────
public class NavPoint
{
    public DateTime Date { get; set; }
    public double   Nav  { get; set; }
}

// ── JsonElement 扩展（复用）──────────────────────────────────────────────────
internal static class ChartJsonExtensions
{
    internal static string? TryGetStr(this JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null
            ? v.GetString()
            : null;
}
