using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using AvaloniaDemo.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace AvaloniaDemo.Views
{
    public partial class MainView : ReactiveUserControl<MainViewModel>
    {
        private WindowNotificationManager? _manager;

        public MainView()
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

        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            var topLevel = TopLevel.GetTopLevel(this);
            _manager = new WindowNotificationManager(topLevel) { MaxItems = 3 };
        }

        private void InfoButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Content is string s)
            {
                _manager?.Show(Enum.TryParse<NotificationType>(s, out NotificationType t)
                    ? new Notification(t.ToString(), "This is message", t)
                    : new Notification(s, "This is message"));
            }
        }

    }
}