using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace AvaloniaDemo.ViewModels.UserControls
{
    public partial class HomeViewModel : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ReactiveCommand<Unit, Unit> TestCommand { get; }

        // 响应式命令
        public ReactiveCommand<Unit, Unit> AddTaskCommand { get; }

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

        private string? _newTaskTitle;

        public string? NewTaskTitle
        {
            get => _newTaskTitle;
            set => this.RaiseAndSetIfChanged(ref _newTaskTitle, value);
        }

        private ObservableCollection<string>? strColl;

        public ObservableCollection<string>? StrColl
        {
            get => strColl;
            set => this.RaiseAndSetIfChanged(ref strColl, value);
        }

        public HomeViewModel()
        {
            StrColl =
            [
                "任务一",
                "任务二",
                "任务三",
                "任务四",
                "任务五"
            ];

            TestCommand = ReactiveCommand.Create(() =>
            {
                // 发送消息到消息总线
                MessageBus.Current.SendMessage<string>("Hello from MainViewModel!");
            });

            // 命令定义与启用条件
            // AddTaskCommand：标题不为空且不超过5字符时可用
            var canAddTask = this.WhenAnyValue(
                x => x.NewTaskTitle,
                title => !string.IsNullOrWhiteSpace(title) && title.Length <= 5
            );

            AddTaskCommand = ReactiveCommand.Create(AddTask, canAddTask);

            //注册消息监听
            MessageBus.Current.Listen<string>().Subscribe((msg) =>
            {
                Console.WriteLine($"接收消息：{msg}");
            });

            // 订阅命令执行结果（可选）
            TestCommand.Execute().Subscribe().DisposeWith(_disposables);

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

        private void AddTask()
        {
            NewTaskTitle = string.Empty; // 清空输入
        }

        // 实现IDisposable接口，清理所有订阅和资源
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}