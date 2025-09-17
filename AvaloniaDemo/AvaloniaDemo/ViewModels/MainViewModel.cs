using ReactiveUI;
using System.Reactive;
using System.Timers;
using System;

namespace AvaloniaDemo.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        public string Greeting => "Welcome to Avalonia!";

        public ReactiveCommand<Unit, Unit> TestCommand { get; }


        private Timer _timer;


        private DateTime _time;

        public DateTime Time
        {
            get { return _time; }
            set { this.RaiseAndSetIfChanged(ref _time, value); }
        }


        public MainViewModel()
        {
            TestCommand = ReactiveCommand.Create(() =>
            {
                // 发送消息到消息总线
                MessageBus.Current.SendMessage<string>("Hello from MainViewModel!");
            });

            //注册消息监听
            MessageBus.Current.Listen<string>().Subscribe((msg) =>
            {
                Console.WriteLine($"接收消息：{msg}");
            });

            Time = DateTime.Now;
            _timer = new Timer(1000);
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            Time = DateTime.Now;
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Elapsed -= TimerOnElapsed;
            _timer.Dispose();
        }
    }
}
