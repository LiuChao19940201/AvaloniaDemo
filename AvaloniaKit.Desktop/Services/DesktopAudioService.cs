using AvaloniaKit.Services;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaKit.Desktop.Services;

// ══════════════════════════════════════════════════════════════════════════════
//  DesktopAudioService
//  使用 Windows Media Foundation（WMF）的 MCI / winmm 或
//  直接调用 Windows.Media.Playback（仅 Windows）。
//
//  跨系统备选：使用 System.Media.SoundPlayer（只支持 WAV），
//  因此这里改用更通用的 mciSendString（支持 MP3，Windows 内置）。
//
//  Linux / macOS Desktop 如需支持，可在此文件底部加
//  #if / RuntimeInformation 分支，调用 afplay(macOS) 或 mpg123(Linux)。
// ══════════════════════════════════════════════════════════════════════════════
public class DesktopAudioService : IAudioService, IDisposable
{
    // ── WinMM MCI P/Invoke ────────────────────────────────────────────────────
    [DllImport("winmm.dll", CharSet = CharSet.Auto)]
    private static extern int mciSendString(
        string command, System.Text.StringBuilder? returnString,
        int returnLength, IntPtr hwndCallback);

    private const string ALIAS = "netease_audio";

    // ── 状态 ─────────────────────────────────────────────────────────────────
    public bool   IsPlaying  { get; private set; }
    public long   CurrentMs  { get; private set; }
    public long   DurationMs { get; private set; }

    private double _volume = 1.0;
    public double Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0, 1);
            // MCI 音量 0~1000
            int vol = (int)(_volume * 1000);
            mciSendString($"setaudio {ALIAS} volume to {vol}", null, 0, IntPtr.Zero);
        }
    }

    // ── 事件 ─────────────────────────────────────────────────────────────────
    public event EventHandler<AudioProgressEventArgs>? ProgressChanged;
    public event EventHandler? PlaybackEnded;
    public event EventHandler<string>? PlaybackError;

    // ── 内部 ─────────────────────────────────────────────────────────────────
    private CancellationTokenSource? _cts;
    private bool _opened = false;

    // ══════════════════════════════════════════════════════════════════════════
    //  PlayAsync
    // ══════════════════════════════════════════════════════════════════════════
    public async Task PlayAsync(string url)
    {
        Stop(); // 停止上一首

        try
        {
            // MCI 只能播放本地文件；先下载到临时文件
            string tmpFile = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"netease_{Guid.NewGuid():N}.mp3");

            StatusText("下载中…");
            using var http = new System.Net.Http.HttpClient();
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            http.DefaultRequestHeaders.TryAddWithoutValidation(
                "Referer", "https://music.163.com/");

            var bytes = await http.GetByteArrayAsync(url);
            await System.IO.File.WriteAllBytesAsync(tmpFile, bytes);

            // 打开并播放
            string openCmd = $"open \"{tmpFile}\" type mpegvideo alias {ALIAS}";
            int ret = mciSendString(openCmd, null, 0, IntPtr.Zero);
            if (ret != 0)
            {
                PlaybackError?.Invoke(this, $"MCI open 失败: {ret}");
                return;
            }
            _opened = true;

            // 获取时长
            var sb = new System.Text.StringBuilder(128);
            mciSendString($"status {ALIAS} length", sb, 128, IntPtr.Zero);
            if (long.TryParse(sb.ToString().Trim(), out long dur))
                DurationMs = dur;

            // 播放
            mciSendString($"play {ALIAS}", null, 0, IntPtr.Zero);
            IsPlaying = true;

            // 启动进度轮询
            StartPolling(tmpFile);
        }
        catch (Exception ex)
        {
            PlaybackError?.Invoke(this, ex.Message);
        }
    }

    public void Pause()
    {
        if (!_opened || !IsPlaying) return;
        mciSendString($"pause {ALIAS}", null, 0, IntPtr.Zero);
        IsPlaying = false;
        _cts?.Cancel();
    }

    public void Resume()
    {
        if (!_opened || IsPlaying) return;
        mciSendString($"resume {ALIAS}", null, 0, IntPtr.Zero);
        IsPlaying = true;
        StartPolling(null);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        if (_opened)
        {
            mciSendString($"stop {ALIAS}",  null, 0, IntPtr.Zero);
            mciSendString($"close {ALIAS}", null, 0, IntPtr.Zero);
            _opened = false;
        }
        IsPlaying  = false;
        CurrentMs  = 0;
        DurationMs = 0;
    }

    public void SeekTo(long ms)
    {
        if (!_opened) return;
        mciSendString($"seek {ALIAS} to {ms}", null, 0, IntPtr.Zero);
        if (IsPlaying)
            mciSendString($"play {ALIAS}", null, 0, IntPtr.Zero);
        CurrentMs = ms;
        ProgressChanged?.Invoke(this, new AudioProgressEventArgs
        {
            CurrentMs = CurrentMs, DurationMs = DurationMs
        });
    }

    // ── 进度轮询 ──────────────────────────────────────────────────────────────
    private void StartPolling(string? tmpFile)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        _ = PollAsync(ct, tmpFile);
    }

    private async Task PollAsync(CancellationToken ct, string? tmpFile)
    {
        var sb = new System.Text.StringBuilder(128);
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(500, ct).ConfigureAwait(false);
            if (ct.IsCancellationRequested) break;

            // 当前位置
            sb.Clear();
            mciSendString($"status {ALIAS} position", sb, 128, IntPtr.Zero);
            if (long.TryParse(sb.ToString().Trim(), out long pos))
                CurrentMs = pos;

            ProgressChanged?.Invoke(this, new AudioProgressEventArgs
            {
                CurrentMs = CurrentMs, DurationMs = DurationMs
            });

            // 检测播放结束
            sb.Clear();
            mciSendString($"status {ALIAS} mode", sb, 128, IntPtr.Zero);
            if (sb.ToString().Trim() == "stopped")
            {
                IsPlaying = false;
                PlaybackEnded?.Invoke(this, EventArgs.Empty);
                // 清理临时文件
                if (tmpFile != null)
                {
                    try { System.IO.File.Delete(tmpFile); } catch { }
                }
                break;
            }
        }
    }

    private void StatusText(string _) { /* 可选：接入日志 */ }

    public void Dispose() => Stop();
}
