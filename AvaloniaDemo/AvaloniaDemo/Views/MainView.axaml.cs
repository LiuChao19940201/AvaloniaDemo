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

            // ����ͼ����ʱ���ð�
            this.WhenActivated(disposables =>
            {
                //˫���
                this.Bind(ViewModel, viewModel => viewModel.TimeStr, view => view.TimeTextBlock.Text)
                    .DisposeWith(disposables);

                //// ��ʽ1��OneWayBind���Ƽ���
                //this.OneWayBind(ViewModel, vm => vm.TimeStr, view => view.TimeTextBlock.Text)
                //    .DisposeWith(disposables);

                //// ��ʽ2��WhenAnyValue + BindTo����Ч��
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