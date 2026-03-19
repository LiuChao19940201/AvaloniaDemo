using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using AvaloniaKit.ViewModels.UserControls.Chat;

namespace AvaloniaKit.ViewModels.UserControls.Chat
{
    // ── 分类标签选中状态 Converter ────────────────────────────────────────────
    // 绑定方式：SelectedCategoryIndex → 与 DataContext 的 Category 在列表中的下标比较
    // 由于 AXAML 里直接用 IndexOf 比较较复杂，改用 VM 的 SelectedCategoryIndex
    // 与 DiscoverCategory 对象传 CommandParameter 的方式来驱动 Command，
    // 选中样式则直接用 SelectedCategoryIndex == item 在集合中的 index 的比较。
    //
    // 实际上最简单的方案是：让 DiscoverCategory 自己带一个 IsSelected 属性，
    // 由 ViewModel 在切换时更新，AXAML 直接绑定 IsSelected。
    // 这样不需要 Converter，完全避免 AXAML 里的复杂计算。
    // （已在 ViewModel 里实现 SelectedCategoryIndex，AXAML Classes.active 绑定由
    //   DiscoverCategory.IsSelected 驱动）
}

namespace AvaloniaKit.Views.UserControls.Chat
{
    public partial class FundTrackerUserControl : UserControl
    {
        public FundTrackerUserControl()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is FundTrackerViewModel vm)
                vm.OnNavigatedTo();
        }
    }
}
