using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaKit.ViewModels.UserControls.Discover;

#if ANDROID
using Android.OS;
#endif

namespace AvaloniaKit.Views.UserControls.Discover;

public partial class TetrisUserControl : UserControl
{
    private TetrisViewModel? Vm => DataContext as TetrisViewModel;

    public TetrisUserControl()
    {
        InitializeComponent();
        Focusable = true;
        KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.None);

        DataContextChanged += OnDataContextChanged;

        // ── 关键修复 ──────────────────────────────────────────────────────
        // 使用 Tunnel（Preview）阶段拦截键盘事件，优先于子控件的 Bubble 阶段。
        // handledEventsToo: true 保证即使子控件已标记 Handled 也能收到。
        // 这样 Space / Up 不会被 Button 的默认点击行为截走。
        AddHandler(
            KeyDownEvent,
            OnKeyDownTunnel,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }

    // ─── ViewModel 事件订阅 ──────────────────────────────────────────────

    private TetrisViewModel? _prevVm;

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (_prevVm != null)
        {
            _prevVm.LinesClearedEvent -= OnLinesCleared;
            _prevVm.PieceLockedEvent  -= OnPieceLocked;
            _prevVm.GameOverEvent     -= OnGameOver;
        }
        _prevVm = Vm;
        if (Vm == null) return;
        Vm.LinesClearedEvent += OnLinesCleared;
        Vm.PieceLockedEvent  += OnPieceLocked;
        Vm.GameOverEvent     += OnGameOver;
    }

    private void OnLinesCleared(object? sender, System.EventArgs e)
    {
        Vibrate(VibeMode.ClearLines);
        PlaySound(SoundId.ClearLines);
    }

    private void OnPieceLocked(object? sender, System.EventArgs e)
    {
        Vibrate(VibeMode.Lock);
        PlaySound(SoundId.Lock);
    }

    private void OnGameOver(object? sender, System.EventArgs e)
    {
        Vibrate(VibeMode.GameOver);
        PlaySound(SoundId.GameOver);
    }

    // ─── 焦点管理 ────────────────────────────────────────────────────────

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Focus();
    }

    /// <summary>
    /// 点击触摸按钮后，按钮会抢焦点。
    /// 在 PointerReleased 阶段将焦点拉回 UserControl。
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        Focus();
    }

    // ─── Tunnel 阶段键盘拦截（最先收到，阻止子控件处理）────────────────

    private void OnKeyDownTunnel(object? sender, KeyEventArgs e)
    {
        if (Vm == null) return;

        // 游戏操作键一律在此处消费，阻止继续冒泡到按钮
        switch (e.Key)
        {
            case Key.Left:
            case Key.A:
                Vm.MoveLeftCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Right:
            case Key.D:
                Vm.MoveRightCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Down:
            case Key.S:
                Vm.SoftDropCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Up:
            case Key.W:
                // ↑ / W = 旋转（注意：Up 键如果不在 Tunnel 阶段截取，
                // 会触发焦点移动到上方按钮，然后 Space 就变成那个按钮的点击）
                Vm.RotateCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Space:
                // Space 在 Bubble 阶段会触发当前聚焦按钮的 Click。
                // 在 Tunnel 阶段标记 Handled = true，阻断按钮的 Click。
                Vm.HardDropCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.P:
            case Key.Escape:
                Vm.TogglePauseCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Enter:
                if (!Vm.IsRunning)
                    (Vm.IsGameOver ? Vm.RestartCommand : Vm.StartCommand).Execute(null);
                e.Handled = true;
                break;
        }
    }

    // ─── 震动（Android）─────────────────────────────────────────────────

    private enum VibeMode { Lock, ClearLines, GameOver }

    private static void Vibrate(VibeMode mode)
    {
#if ANDROID
        try
        {
            var vibrator = (Android.OS.Vibrator?)
                Android.App.Application.Context
                    .GetSystemService(Android.Content.Context.VibratorService);
            if (vibrator?.HasVibrator != true) return;

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var effect = mode switch
                {
                    VibeMode.Lock =>
                        Android.OS.VibrationEffect.CreateOneShot(
                            25, Android.OS.VibrationEffect.DefaultAmplitude),
                    VibeMode.ClearLines =>
                        Android.OS.VibrationEffect.CreateWaveform(
                            new long[] { 0, 50, 30, 80 },
                            new int[]  { 0, 160,  0, 255 }, -1),
                    VibeMode.GameOver =>
                        Android.OS.VibrationEffect.CreateWaveform(
                            new long[] { 0, 100, 60, 100, 60, 300 },
                            new int[]  { 0, 200,  0, 200,  0, 255 }, -1),
                    _ => Android.OS.VibrationEffect.CreateOneShot(
                            25, Android.OS.VibrationEffect.DefaultAmplitude)
                };
                vibrator.Vibrate(effect);
            }
            else
            {
#pragma warning disable CA1422
                long[] pattern = mode switch
                {
                    VibeMode.ClearLines => new long[] { 0, 50, 30, 80 },
                    VibeMode.GameOver   => new long[] { 0, 100, 60, 300 },
                    _                   => new long[] { 0, 25 }
                };
                vibrator.Vibrate(pattern, -1);
#pragma warning restore CA1422
            }
        }
        catch { /* 权限未授予时静默忽略 */ }
#endif
    }

    // ─── 音效（预留接口）────────────────────────────────────────────────

    private enum SoundId { Lock, ClearLines, GameOver }

    private static void PlaySound(SoundId id)
    {
        // 替换为实际音频库调用，例如：
        // var file = id switch
        // {
        //     SoundId.Lock       => "avares://AvaloniaKit/Assets/sounds/lock.wav",
        //     SoundId.ClearLines => "avares://AvaloniaKit/Assets/sounds/clear.wav",
        //     SoundId.GameOver   => "avares://AvaloniaKit/Assets/sounds/gameover.wav",
        //     _                  => null
        // };
        // if (file != null) AudioService.Play(file);
        _ = id;
    }
}
