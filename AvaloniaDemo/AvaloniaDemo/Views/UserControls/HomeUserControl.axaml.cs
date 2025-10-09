using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using AvaloniaDemo.ViewModels.UserControls;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace AvaloniaDemo.Views.UserControls
{
    public partial class HomeUserControl : ReactiveUserControl<HomeViewModel>
    {
        private WindowNotificationManager? _manager;

        public HomeUserControl()
        {
            InitializeComponent();

            // 当视图激活时设置绑定
            this.WhenActivated(disposables =>
            {
                //双向绑定
                this.Bind(ViewModel, viewModel => viewModel.TimeStr, view => view.TimeTextBlock.Text)
                    .DisposeWith(disposables);

                //// 方式1：OneWayBind（推荐）
                //this.OneWayBind(ViewModel, vm => vm.TimeStr, view => view.TimeTextBlock.Text)
                //    .DisposeWith(disposables);

                //// 方式2：WhenAnyValue + BindTo（等效）
                //this.WhenAnyValue(x => x.ViewModel.TimeStr)
                //    .BindTo(this, x => x.TimeTextBlock.Text)
                //    .DisposeWith(disposables);
            });

            //注册消息监听
            MessageBus.Current.Listen<string>().Subscribe((msg) =>
            {
                Console.WriteLine($"接收消息：{msg}");
                _manager?.Show(new Notification("提示：", $"接收消息：{msg}", NotificationType.Error));
            });

        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            //var topLevel = TopLevel.GetTopLevel(this);
            _manager = new WindowNotificationManager(TopLevel.GetTopLevel(this)) { MaxItems = 3 };
        }

        private void InfoButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Content is string s)
            {
                //显示通知弹框
                _manager?.Show(new Notification("提示：", "This is message", NotificationType.Success));
            }
        }

    }
}