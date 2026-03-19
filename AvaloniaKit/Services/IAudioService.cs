using System;
using System.Threading.Tasks;

namespace AvaloniaKit.Services;

// ══════════════════════════════════════════════════════════════════════════════
//  IAudioService  — 跨平台音频播放接口
//  Desktop  → WindowsAudioService  (WMF / MediaFoundation)
//  Android  → AndroidAudioService  (Android MediaPlayer)
//  Browser  → BrowserAudioService  (HTML5 Audio via JS Interop)
// ══════════════════════════════════════════════════════════════════════════════
public interface IAudioService
{
    // ── 状态 ─────────────────────────────────────────────────────────────────
    bool   IsPlaying    { get; }
    long   CurrentMs    { get; }
    long   DurationMs   { get; }
    double Volume       { get; set; }   // 0.0 ~ 1.0

    // ── 事件 ─────────────────────────────────────────────────────────────────
    /// <summary>进度更新（约 500ms 触发一次）</summary>
    event EventHandler<AudioProgressEventArgs>? ProgressChanged;

    /// <summary>播放结束</summary>
    event EventHandler? PlaybackEnded;

    /// <summary>播放出错</summary>
    event EventHandler<string>? PlaybackError;

    // ── 操作 ─────────────────────────────────────────────────────────────────
    /// <summary>加载并播放 URL（http/https MP3）</summary>
    Task PlayAsync(string url);

    /// <summary>暂停</summary>
    void Pause();

    /// <summary>恢复</summary>
    void Resume();

    /// <summary>停止并释放资源</summary>
    void Stop();

    /// <summary>跳转到指定毫秒</summary>
    void SeekTo(long ms);
}

public class AudioProgressEventArgs : EventArgs
{
    public long CurrentMs  { get; init; }
    public long DurationMs { get; init; }
}
