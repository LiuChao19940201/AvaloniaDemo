using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AvaloniaDemo.ViewModels.UserControls.Chat;

namespace AvaloniaDemo.ViewModels.UserControls.Chat
{
    // ── NavChartView 放在 ViewModel 命名空间，供 AXAML 中 xmlns:vm 引用 ────────
    // ── 自绘折线图控件（纯 Avalonia，Desktop/Web 全兼容）────────────────────────
    public class NavChartView : Control
    {
        // ── AvaloniaProperty 定义 ────────────────────────────────────────────
        public static readonly StyledProperty<ObservableCollection<NavPoint>?> PointsProperty =
            AvaloniaProperty.Register<NavChartView, ObservableCollection<NavPoint>?>(nameof(Points));

        public static readonly StyledProperty<double> YMinProperty =
            AvaloniaProperty.Register<NavChartView, double>(nameof(YMin), 0);

        public static readonly StyledProperty<double> YMaxProperty =
            AvaloniaProperty.Register<NavChartView, double>(nameof(YMax), 1);

        public static readonly StyledProperty<string> LineColorProperty =
            AvaloniaProperty.Register<NavChartView, string>(nameof(LineColor), "#4080FF");

        public ObservableCollection<NavPoint>? Points
        {
            get => GetValue(PointsProperty);
            set => SetValue(PointsProperty, value);
        }
        public double YMin
        {
            get => GetValue(YMinProperty);
            set => SetValue(YMinProperty, value);
        }
        public double YMax
        {
            get => GetValue(YMaxProperty);
            set => SetValue(YMaxProperty, value);
        }
        public string LineColor
        {
            get => GetValue(LineColorProperty);
            set => SetValue(LineColorProperty, value);
        }

        // 属性变化时重绘
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == PointsProperty)
            {
                // 监听集合变化
                if (change.OldValue is INotifyCollectionChanged oldCol)
                    oldCol.CollectionChanged -= OnCollectionChanged;
                if (change.NewValue is INotifyCollectionChanged newCol)
                    newCol.CollectionChanged += OnCollectionChanged;
                InvalidateVisual();
            }
            else if (change.Property == YMinProperty ||
                     change.Property == YMaxProperty ||
                     change.Property == LineColorProperty)
            {
                InvalidateVisual();
            }
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => InvalidateVisual();

        // ── 核心绘制（对应 ChartForm 里 Chart 的配置逻辑）────────────────────
        public override void Render(DrawingContext ctx)
        {
            base.Render(ctx);

            double w = Bounds.Width;
            double h = Bounds.Height;
            if (w <= 0 || h <= 0) return;

            var points = Points;
            if (points == null || points.Count < 2) return;

            double yMin   = YMin;
            double yMax   = YMax;
            double yRange = yMax - yMin;
            if (yRange <= 0) yRange = 1;

            // ── 颜色解析 ─────────────────────────────────────────────────────
            Color lineCol = TryParseColor(LineColor) ?? Color.Parse("#4080FF");
            Color fillCol = Color.FromArgb(30, lineCol.R, lineCol.G, lineCol.B);
            Color gridCol = Color.Parse("#E6E6F0");
            Color labelCol = Color.Parse("#999999");

            var linePen    = new Pen(new SolidColorBrush(lineCol), 2.0);
            var gridPen    = new Pen(new SolidColorBrush(gridCol), 0.6);
            var markerBrush = new SolidColorBrush(lineCol);

            int count = points.Count;

            // ── 计算像素坐标 ─────────────────────────────────────────────────
            double PX(int i) => i * (w - 1) / (count - 1);
            double PY(double nav) => h - (nav - yMin) / yRange * h;

            // ── 横向网格线（5 条）+ Y 轴标签 ─────────────────────────────────
            var labelTypeFace = new Typeface("微软雅黑");
            for (int g = 0; g <= 4; g++)
            {
                double ratio  = g / 4.0;
                double navVal = yMin + ratio * yRange;
                double y      = h - ratio * h;

                ctx.DrawLine(gridPen, new Point(0, y), new Point(w, y));

                // Y 轴标签
                var ft = new FormattedText(
                    navVal.ToString("F4"),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    labelTypeFace, 10,
                    new SolidColorBrush(labelCol));
                ctx.DrawText(ft, new Point(2, y - ft.Height / 2));
            }

            // ── X 轴日期标签（最多显示 6 个）────────────────────────────────
            int labelStep = Math.Max(1, count / 6);
            for (int i = 0; i < count; i += labelStep)
            {
                double x  = PX(i);
                string lb = points[i].Date.ToString("MM/dd");
                var ft = new FormattedText(
                    lb,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    labelTypeFace, 10,
                    new SolidColorBrush(labelCol));
                ctx.DrawText(ft, new Point(x - ft.Width / 2, h + 2));
            }

            // ── 填充区域（折线下方半透明）────────────────────────────────────
            var fillGeom = new StreamGeometry();
            using (var sgc = fillGeom.Open())
            {
                sgc.BeginFigure(new Point(PX(0), h), true);
                sgc.LineTo(new Point(PX(0), PY(points[0].Nav)));
                for (int i = 1; i < count; i++)
                    sgc.LineTo(new Point(PX(i), PY(points[i].Nav)));
                sgc.LineTo(new Point(PX(count - 1), h));
                sgc.EndFigure(true);
            }
            ctx.DrawGeometry(new SolidColorBrush(fillCol), null, fillGeom);

            // ── 折线 ─────────────────────────────────────────────────────────
            var lineGeom = new StreamGeometry();
            using (var sgc = lineGeom.Open())
            {
                sgc.BeginFigure(new Point(PX(0), PY(points[0].Nav)), false);
                for (int i = 1; i < count; i++)
                    sgc.LineTo(new Point(PX(i), PY(points[i].Nav)));
            }
            ctx.DrawGeometry(null, linePen, lineGeom);

            // ── 数据点圆圈（数据点 ≤ 60 个时才画，太密了不画）────────────────
            if (count <= 60)
            {
                foreach (var (i, pt) in Indexed(points))
                {
                    double cx = PX(i);
                    double cy = PY(pt.Nav);
                    ctx.DrawEllipse(markerBrush, null, new Point(cx, cy), 3.5, 3.5);
                    ctx.DrawEllipse(new SolidColorBrush(Colors.White), null, new Point(cx, cy), 2.0, 2.0);
                }
            }
        }

        // ── 工具方法 ─────────────────────────────────────────────────────────
        private static Color? TryParseColor(string? hex)
        {
            try { return Color.Parse(hex ?? ""); }
            catch { return null; }
        }

        private static IEnumerable<(int index, T item)> Indexed<T>(IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
                yield return (i, list[i]);
        }
    }
}

// ══════════════════════════════════════════════════════════════════════════════
namespace AvaloniaDemo.Views.UserControls.Chat
{
    public partial class FundChartUserControl : Avalonia.Controls.UserControl
    {
        public FundChartUserControl()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            // 数据由 MainWindowViewModel.Receive 在切换 CurrentPage 前注入，
            // 这里不需要额外触发加载。
        }
    }
}
