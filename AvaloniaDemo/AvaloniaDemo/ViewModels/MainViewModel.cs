using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace AvaloniaDemo.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ReactiveCommand<Unit, Unit> TestCommand { get; }

        private DateTime time;

        public DateTime Time
        {
            get { return time; }
            set { this.RaiseAndSetIfChanged(ref time, value); }
        }

        private string? timeStr;

        public string? TimeStr
        {
            get { return timeStr; }
            set { this.RaiseAndSetIfChanged(ref timeStr, value); }
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

            // 订阅命令执行结果（可选）
            TestCommand
                .Execute()
                .Subscribe()
                .DisposeWith(_disposables);

            // 使用响应式定时器替代传统Timer
            // Observable.Interval创建一个定期发射值的可观察序列
            Observable.Interval(TimeSpan.FromSeconds(1))
                      //.Take(5)  // 只执行5次
                      //.Delay(TimeSpan.FromSeconds(2))  // 延迟2秒启动
                      .ObserveOn(RxApp.MainThreadScheduler)  // 自动切换到 UI 线程
                      .Subscribe(_ =>
                      {
                          Time = DateTime.Now; // 更新时间属性
                          TimeStr = Time.ToString();
                      })
                      .DisposeWith(_disposables); // 自动管理订阅生命周期

            // 初始时间设置
            Time = DateTime.Now;
            TimeStr = Time.ToString();

        }

        // 实现IDisposable接口，清理所有订阅和资源
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
