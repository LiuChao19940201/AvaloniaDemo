using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using System;

namespace AvaloniaDemo.Views
{
    public partial class MainView : UserControl
    {
        private WindowNotificationManager? _manager;

        public MainView()
        {
            InitializeComponent();
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