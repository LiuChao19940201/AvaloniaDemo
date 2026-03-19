using Avalonia.Threading;
using Irihi.Avalonia.Shared.Contracts;
using ReactiveUI;
using System;

namespace AvaloniaKit.ViewModels.Windows
{
    public partial class SplashViewModel : ReactiveObject, IDialogContext
    {
        private double _progress;

        public double Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private Random _r = new();

        public SplashViewModel()
        {
            DispatcherTimer.Run(OnUpdate, TimeSpan.FromMilliseconds(20), DispatcherPriority.Default);
        }

        private bool OnUpdate()
        {
            Progress += 10 * _r.NextDouble();
            if (Progress <= 100)
            {
                return true;
            }
            else
            {
                RequestClose?.Invoke(this, true);
                return false;
            }
        }

        public void Close()
        {
            RequestClose?.Invoke(this, false);
        }

        public event EventHandler<object?>? RequestClose;
    }
}