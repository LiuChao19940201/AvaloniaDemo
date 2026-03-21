using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaKit.ViewModels.UserControls.Discover;

#if ANDROID
using Android.App;
using Android.OS;
using Android.Media;
#endif

namespace AvaloniaKit.Views.UserControls.Discover;

public partial class TetrisUserControl : UserControl
{
    private TetrisViewModel? Vm => DataContext as TetrisViewModel;

    public TetrisUserControl()
    {
        InitializeComponent();

        // 订阅 ViewModel 事件（震动 + 音效）
        DataContextChanged += OnDataContextChanged;

        // 接收键盘焦点
        Focusable = true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ViewModel 事件订阅
    // ─────────────────────────────────────────────────────────────────────────

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

        if (Vm != null)
        {
            Vm.LinesClearedEvent += OnLinesCleared;
            Vm.PieceLockedEvent  += OnPieceLocked;
            Vm.GameOverEvent     += OnGameOver;
        }
    }

    // ── 消除行：震动 + 音效 ──────────────────────────────────────────────────
    private void OnLinesCleared(object? sender, System.EventArgs e)
    {
        PlayVibration(VibrationKind.ClearLines);
        PlaySound(SoundKind.ClearLines);
    }

    // ── 方块锁定：轻震动 ─────────────────────────────────────────────────────
    private void OnPieceLocked(object? sender, System.EventArgs e)
    {
        PlayVibration(VibrationKind.PieceLock);
        PlaySound(SoundKind.PieceLock);
    }

    // ── 游戏结束：长震动 ─────────────────────────────────────────────────────
    private void OnGameOver(object? sender, System.EventArgs e)
    {
        PlayVibration(VibrationKind.GameOver);
        PlaySound(SoundKind.GameOver);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 键盘输入（桌面端）
    // ─────────────────────────────────────────────────────────────────────────

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (Vm == null) return;

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
                Vm.RotateCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Space:
                Vm.HardDropCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Escape:
            case Key.P:
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

    // ─────────────────────────────────────────────────────────────────────────
    // 震动（Android 平台）
    // ─────────────────────────────────────────────────────────────────────────

    private enum VibrationKind { PieceLock, ClearLines, GameOver }
    private enum SoundKind     { PieceLock, ClearLines, GameOver }

    private static void PlayVibration(VibrationKind kind)
    {
#if ANDROID
        try
        {
            var vibrator = (Vibrator?)Application.Context
                .GetSystemService(Android.Content.Context.VibratorService);
            if (vibrator == null || !vibrator.HasVibrator) return;

            if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                VibrationEffect effect = kind switch
                {
                    VibrationKind.PieceLock  => VibrationEffect.CreateOneShot(30,  VibrationEffect.DefaultAmplitude),
                    VibrationKind.ClearLines => VibrationEffect.CreateWaveform(
                        new long[]  { 0, 60, 40, 60 },
                        new int[]   { 0, 180, 0, 255 }, -1),
                    VibrationKind.GameOver   => VibrationEffect.CreateWaveform(
                        new long[]  { 0, 120, 80, 120, 80, 300 },
                        new int[]   { 0, 200, 0, 200, 0, 255  }, -1),
                    _                        => VibrationEffect.CreateOneShot(30, VibrationEffect.DefaultAmplitude)
                };
                vibrator.Vibrate(effect);
            }
            else
            {
#pragma warning disable CA1422
                long[] pattern = kind switch
                {
                    VibrationKind.ClearLines => new long[] { 0, 60, 40, 60 },
                    VibrationKind.GameOver   => new long[] { 0, 120, 80, 300 },
                    _                        => new long[] { 0, 30 }
                };
                vibrator.Vibrate(pattern, -1);
#pragma warning restore CA1422
            }
        }
        catch { /* 权限未授予时静默忽略 */ }
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 音效（跨平台，使用 Avalonia + NAudio / SkiaSharp.Extended 等）
    // 这里提供一个可替换的桩实现；实际音效播放需视项目音频库而定。
    // 推荐使用 Plugin.Maui.Audio 或 SimpleAudio 等库。
    // ─────────────────────────────────────────────────────────────────────────

    private static void PlaySound(SoundKind kind)
    {
        // 示例：替换为实际音频播放逻辑
        // var file = kind switch
        // {
        //     SoundKind.PieceLock  => "sounds/lock.wav",
        //     SoundKind.ClearLines => "sounds/clear.wav",
        //     SoundKind.GameOver   => "sounds/gameover.wav",
        //     _                    => null
        // };
        // if (file != null) AudioPlayer.Play(file);

        _ = kind; // 消除未使用警告
    }
}
