using Android.Media;
using Avalonia.Threading;
using AvaloniaKit.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaKit.Android.Services;

// ══════════════════════════════════════════════════════════════════════════════
//  AndroidAudioService  （修复版）
//  修复点：
//  1. PrepareAsync 的 tcs.Task 不再用 .WaitAsync 阻塞，改为纯异步等待
//  2. ProgressChanged 事件直接在轮询线程触发（ViewModel 层用 Dispatcher.Post 处理）
//  3. Completion / Error 回调同样在 Android 主线程触发，无需额外派发
// ══════════════════════════════════════════════════════════════════════════════
public class AndroidAudioService : IAudioService, IDisposable
{
    private MediaPlayer? _player;
    private CancellationTokenSource? _pollCts;

    // ── 状态 ─────────────────────────────────────────────────────────────────
    public bool   IsPlaying  { get; private set; }
    public long   CurrentMs  => _player?.CurrentPosition ?? 0;
    public long   DurationMs { get; private set; }

    private double _volume = 1.0;
    public double Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0, 1);
            _player?.SetVolume((float)_volume, (float)_volume);
        }
    }

    // ── 事件 ─────────────────────────────────────────────────────────────────
    public event EventHandler<AudioProgressEventArgs>? ProgressChanged;
    public event EventHandler? PlaybackEnded;
    public event EventHandler<string>? PlaybackError;

    // ══════════════════════════════════════════════════════════════════════════
    //  PlayAsync
    // ══════════════════════════════════════════════════════════════════════════
    public async Task PlayAsync(string url)
    {
        Stop();

        try
        {
            _player = new MediaPlayer();
            _player.SetAudioAttributes(new AudioAttributes.Builder()
                !.SetUsage(AudioUsageKind.Media)
                !.SetContentType(AudioContentType.Music)
                !.Build()!);

            // 完成回调
            _player.Completion += (_, _) =>
            {
                IsPlaying = false;
                _pollCts?.Cancel();
                PlaybackEnded?.Invoke(this, EventArgs.Empty);
            };

            // 错误回调
            _player.Error += (_, e) =>
            {
                IsPlaying = false;
                _pollCts?.Cancel();
                PlaybackError?.Invoke(this, $"MediaPlayer error {e.What}:{e.Extra}");
            };

            // 设置数据源（网络流直接支持）
            _player.SetDataSource(url);

            // ★ 修复：使用纯异步 TaskCompletionSource，不用 .WaitAsync 避免死锁
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _player.Prepared += (_, _) => tcs.TrySetResult(true);

            // 同时注册错误，防止 Prepare 时出错导致 tcs 永远挂起
            EventHandler<MediaPlayer.ErrorEventArgs>? errorDuringPrepare = null;
            errorDuringPrepare = (_, e) =>
            {
                tcs.TrySetException(new Exception($"Prepare error {e.What}:{e.Extra}"));
                _player.Error -= errorDuringPrepare;
            };
            _player.Error += errorDuringPrepare;

            _player.PrepareAsync();

            // 15 秒超时
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var timeoutTask = Task.Delay(Timeout.Infinite, timeoutCts.Token);
            var completed   = await Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);

            if (completed != tcs.Task)
                throw new TimeoutException("Android MediaPlayer PrepareAsync 超时");

            await tcs.Task.ConfigureAwait(false); // 重新 await 以传播异常

            DurationMs = _player.Duration;
            _player.Start();
            IsPlaying = true;

            StartPolling();
        }
        catch (Exception ex)
        {
            PlaybackError?.Invoke(this, ex.Message);
        }
    }

    public void Pause()
    {
        if (_player == null || !IsPlaying) return;
        _player.Pause();
        IsPlaying = false;
        _pollCts?.Cancel();
    }

    public void Resume()
    {
        if (_player == null || IsPlaying) return;
        _player.Start();
        IsPlaying = true;
        StartPolling();
    }

    public void Stop()
    {
        _pollCts?.Cancel();
        _pollCts?.Dispose();
        _pollCts = null;

        if (_player != null)
        {
            try { _player.Stop(); } catch { }
            _player.Release();
            _player.Dispose();
            _player = null;
        }
        IsPlaying  = false;
        DurationMs = 0;
    }

    public void SeekTo(long ms)
    {
        _player?.SeekTo((int)ms, MediaPlayerSeekMode.Closest);
        ProgressChanged?.Invoke(this, new AudioProgressEventArgs
        {
            CurrentMs = ms, DurationMs = DurationMs
        });
    }

    // ── 进度轮询（在后台线程触发，ViewModel 用 Dispatcher.Post 处理）─────────
    private void StartPolling()
    {
        _pollCts?.Cancel();
        _pollCts = new CancellationTokenSource();
        _ = PollAsync(_pollCts.Token);
    }

    private async Task PollAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && IsPlaying)
        {
            await Task.Delay(500, ct).ConfigureAwait(false);
            if (ct.IsCancellationRequested || _player == null) break;

            ProgressChanged?.Invoke(this, new AudioProgressEventArgs
            {
                CurrentMs  = _player.CurrentPosition,
                DurationMs = DurationMs,
            });
        }
    }

    public void Dispose() => Stop();
}
