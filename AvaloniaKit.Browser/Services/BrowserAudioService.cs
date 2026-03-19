using Avalonia.Threading;
using AvaloniaKit.Services;
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace AvaloniaKit.Browser.Services;

// ══════════════════════════════════════════════════════════════════════════════
//  BrowserAudioService
//  通过 JSImport / JSExport 调用 audio.js 提供的 HTML5 Audio 能力。
//  依赖：Program.cs 中 await JSHost.ImportAsync("audio", "/audio.js")
// ══════════════════════════════════════════════════════════════════════════════
[SupportedOSPlatform("browser")]
public class BrowserAudioService : IAudioService, IDisposable
{
    // ── JSImport 声明（对应 audio.js 中的 export function）───────────────────
    [JSImport("audioPlay",          "audio")] private static partial void   JsPlay(string url);
    [JSImport("audioPause",         "audio")] private static partial void   JsPause();
    [JSImport("audioResume",        "audio")] private static partial void   JsResume();
    [JSImport("audioStop",          "audio")] private static partial void   JsStop();
    [JSImport("audioSeek",          "audio")] private static partial void   JsSeek(double ms);
    [JSImport("audioSetVolume",     "audio")] private static partial void   JsSetVolume(double v);
    [JSImport("audioGetCurrentMs",  "audio")] private static partial double JsGetCurrentMs();
    [JSImport("audioGetDurationMs", "audio")] private static partial double JsGetDurationMs();
    [JSImport("audioIsPlaying",     "audio")] private static partial bool   JsIsPlaying();

    // audioSetCallbacks(onProgress, onEnded, onError, onCanPlay)
    [JSImport("audioSetCallbacks", "audio")]
    private static partial void JsSetCallbacks(
        [JSMarshalAs<JSType.Function<JSType.Number, JSType.Number>>] Action<double, double> onProgress,
        [JSMarshalAs<JSType.Function>]                                Action                onEnded,
        [JSMarshalAs<JSType.Function<JSType.String>>]                 Action<string>        onError,
        [JSMarshalAs<JSType.Function>]                                Action                onCanPlay);

    // ── 状态 ─────────────────────────────────────────────────────────────────
    public bool   IsPlaying  { get; private set; }
    public long   CurrentMs  => (long)JsGetCurrentMs();
    public long   DurationMs => (long)JsGetDurationMs();

    private double _volume = 1.0;
    public double Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0, 1);
            JsSetVolume(_volume);
        }
    }

    // ── 事件 ─────────────────────────────────────────────────────────────────
    public event EventHandler<AudioProgressEventArgs>? ProgressChanged;
    public event EventHandler?                         PlaybackEnded;
    public event EventHandler<string>?                 PlaybackError;

    // ── PlayAsync 用的 TaskCompletionSource ──────────────────────────────────
    private TaskCompletionSource<bool>? _playCts;

    public BrowserAudioService()
    {
        // 注册 JS → C# 回调
        JsSetCallbacks(
            onProgress: (currentMs, durationMs) =>
            {
                // JS ontimeupdate 在 JS 主线程触发，直接 Post 到 Avalonia UI 线程
                Dispatcher.UIThread.Post(() =>
                {
                    IsPlaying = JsIsPlaying();
                    ProgressChanged?.Invoke(this, new AudioProgressEventArgs
                    {
                        CurrentMs  = (long)currentMs,
                        DurationMs = (long)durationMs,
                    });
                });
            },
            onEnded: () =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    IsPlaying = false;
                    PlaybackEnded?.Invoke(this, EventArgs.Empty);
                });
            },
            onError: msg =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    IsPlaying = false;
                    _playCts?.TrySetException(new Exception(msg));
                    _playCts = null;
                    PlaybackError?.Invoke(this, msg);
                });
            },
            onCanPlay: () =>
            {
                // canplaythrough：缓冲完毕，通知 PlayAsync 可以返回
                Dispatcher.UIThread.Post(() =>
                {
                    IsPlaying = true;
                    _playCts?.TrySetResult(true);
                    _playCts = null;
                });
            });
    }

    // ══════════════════════════════════════════════════════════════════════════
    public async Task PlayAsync(string url)
    {
        JsStop();
        IsPlaying = false;

        _playCts = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        JsPlay(url);   // 触发 audio.load() + audio.play()

        // 等待 canplaythrough 或 error，最多 20 秒
        var timeout = Task.Delay(TimeSpan.FromSeconds(20));
        var result  = await Task.WhenAny(_playCts.Task, timeout);

        if (result == timeout)
        {
            _playCts?.TrySetCanceled();
            _playCts = null;
            // 超时不抛异常：Browser Audio 可能 autoplay 被阻止，仍保持 src 已设置状态
            // 让用户点击播放按钮手动触发 resume
        }
        else
        {
            // 传播异常（如果有 error 回调触发的话）
            await _playCts!.Task.ConfigureAwait(false);
        }
    }

    public void Pause()
    {
        JsPause();
        IsPlaying = false;
    }

    public void Resume()
    {
        JsResume();
        IsPlaying = JsIsPlaying();
    }

    public void Stop()
    {
        JsStop();
        IsPlaying = false;
        _playCts?.TrySetCanceled();
        _playCts = null;
    }

    public void SeekTo(long ms)
    {
        JsSeek(ms);
        ProgressChanged?.Invoke(this, new AudioProgressEventArgs
        {
            CurrentMs  = ms,
            DurationMs = DurationMs,
        });
    }

    public void Dispose() => Stop();
}
