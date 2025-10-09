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

            //ע����Ϣ����
            MessageBus.Current.Listen<string>().Subscribe((msg) =>
            {
                Console.WriteLine($"������Ϣ��{msg}");
                _manager?.Show(new Notification("��ʾ��", $"������Ϣ��{msg}", NotificationType.Error));
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
                //��ʾ֪ͨ����
                _manager?.Show(new Notification("��ʾ��", "This is message", NotificationType.Success));
            }
        }

    }
}