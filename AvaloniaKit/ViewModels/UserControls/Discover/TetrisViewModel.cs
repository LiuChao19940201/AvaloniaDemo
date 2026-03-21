using AvaloniaKit.ViewModels.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Timers;

namespace AvaloniaKit.ViewModels.UserControls.Discover;

// ═══════════════════════════════════════════════════════════════════════════════
// 数据模型
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>方块类型（决定颜色）</summary>
public enum TetrominoType
{
    Empty = 0,
    I = 1, O = 2, T = 3, S = 4, Z = 5, J = 6, L = 7,
    Ghost = 8   // 幽灵（落点预览）
}

/// <summary>游戏板单个格子（供 ItemsControl 绑定）</summary>
public partial class CellViewModel : ObservableObject
{
    [ObservableProperty] private TetrominoType _type = TetrominoType.Empty;
    public int Row { get; init; }
    public int Col { get; init; }
}

/// <summary>所有方块的 4 种旋转形态（每个 int[2] = [row, col] 相对于锚点偏移）</summary>
public static class TetrominoShapes
{
    // index 0 = Empty 占位，1..7 对应 TetrominoType
    public static readonly int[][][][] All =
    {
        Array.Empty<int[][]>(), // 0: Empty

        // 1: I
        new[]
        {
            new[]{new[]{0,0},new[]{0,1},new[]{0,2},new[]{0,3}},
            new[]{new[]{0,2},new[]{1,2},new[]{2,2},new[]{3,2}},
            new[]{new[]{2,0},new[]{2,1},new[]{2,2},new[]{2,3}},
            new[]{new[]{0,1},new[]{1,1},new[]{2,1},new[]{3,1}},
        },
        // 2: O
        new[]
        {
            new[]{new[]{0,0},new[]{0,1},new[]{1,0},new[]{1,1}},
            new[]{new[]{0,0},new[]{0,1},new[]{1,0},new[]{1,1}},
            new[]{new[]{0,0},new[]{0,1},new[]{1,0},new[]{1,1}},
            new[]{new[]{0,0},new[]{0,1},new[]{1,0},new[]{1,1}},
        },
        // 3: T
        new[]
        {
            new[]{new[]{0,0},new[]{0,1},new[]{0,2},new[]{1,1}},
            new[]{new[]{0,1},new[]{1,0},new[]{1,1},new[]{2,1}},
            new[]{new[]{1,1},new[]{2,0},new[]{2,1},new[]{2,2}},
            new[]{new[]{0,1},new[]{1,1},new[]{1,2},new[]{2,1}},
        },
        // 4: S
        new[]
        {
            new[]{new[]{0,1},new[]{0,2},new[]{1,0},new[]{1,1}},
            new[]{new[]{0,0},new[]{1,0},new[]{1,1},new[]{2,1}},
            new[]{new[]{1,1},new[]{1,2},new[]{2,0},new[]{2,1}},
            new[]{new[]{0,1},new[]{1,1},new[]{1,2},new[]{2,2}},
        },
        // 5: Z
        new[]
        {
            new[]{new[]{0,0},new[]{0,1},new[]{1,1},new[]{1,2}},
            new[]{new[]{0,2},new[]{1,1},new[]{1,2},new[]{2,1}},
            new[]{new[]{1,0},new[]{1,1},new[]{2,1},new[]{2,2}},
            new[]{new[]{0,1},new[]{1,0},new[]{1,1},new[]{2,0}},
        },
        // 6: J
        new[]
        {
            new[]{new[]{0,0},new[]{1,0},new[]{1,1},new[]{1,2}},
            new[]{new[]{0,1},new[]{0,2},new[]{1,1},new[]{2,1}},
            new[]{new[]{1,0},new[]{1,1},new[]{1,2},new[]{2,2}},
            new[]{new[]{0,1},new[]{1,1},new[]{2,0},new[]{2,1}},
        },
        // 7: L
        new[]
        {
            new[]{new[]{0,2},new[]{1,0},new[]{1,1},new[]{1,2}},
            new[]{new[]{0,1},new[]{1,1},new[]{2,1},new[]{2,2}},
            new[]{new[]{1,0},new[]{1,1},new[]{1,2},new[]{2,0}},
            new[]{new[]{0,0},new[]{0,1},new[]{1,1},new[]{2,1}},
        },
    };
}

// ═══════════════════════════════════════════════════════════════════════════════
// TetrisViewModel
// ═══════════════════════════════════════════════════════════════════════════════

public partial class TetrisViewModel : ObservableObject
{
    // ── 常量 ──────────────────────────────────────────────────────────────────
    private const int Rows   = 20;
    private const int Cols   = 10;
    private const int InitMs = 800;
    private const int MinMs  = 80;

    // ── 事件（View 订阅后触发震动 & 音效）────────────────────────────────────
    public event EventHandler? LinesClearedEvent;
    public event EventHandler? PieceLockedEvent;
    public event EventHandler? GameOverEvent;

    // ── 可观察属性 ────────────────────────────────────────────────────────────
    [ObservableProperty] private int  _score;
    [ObservableProperty] private int  _level  = 1;
    [ObservableProperty] private int  _lines;
    [ObservableProperty] private bool _isGameOver;
    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private TetrominoType _nextType;

    // ── 格子集合（绑定给 ItemsControl）────────────────────────────────────────
    public ObservableCollection<CellViewModel> Cells        { get; } = new();
    public ObservableCollection<CellViewModel> PreviewCells { get; } = new();

    // ── 内部状态 ──────────────────────────────────────────────────────────────
    private readonly TetrominoType[,] _board = new TetrominoType[Rows, Cols];
    private TetrominoType _curType;
    private int _curRot, _curRow, _curCol;
    private TetrominoType _nextQueued;
    private readonly Random _rng = new();
    private Timer? _timer;

    // ═════════════════════════════════════════════════════════════════════════
    public TetrisViewModel()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                Cells.Add(new CellViewModel { Row = r, Col = c });

        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                PreviewCells.Add(new CellViewModel { Row = r, Col = c });

        _nextQueued = RandomType();
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Commands
    // ═════════════════════════════════════════════════════════════════════════

    [RelayCommand]
    private void GoBack()
    {
        _timer?.Stop();
        WeakReferenceMessenger.Default.Send(new NavigateBackFromTetrisMessage());
    }

    [RelayCommand]
    private void Start()
    {
        ResetBoard();
        Score = 0; Level = 1; Lines = 0;
        IsGameOver = false; IsPaused = false; IsRunning = true;
        _nextQueued = RandomType();
        SpawnPiece();
        StartTimer();
    }

    [RelayCommand]
    private void Restart() => Start();

    [RelayCommand]
    private void TogglePause()
    {
        if (!IsRunning || IsGameOver) return;
        IsPaused = !IsPaused;
        if (IsPaused) _timer?.Stop();
        else          _timer?.Start();
    }

    /// <summary>左移（触摸按钮 & 键盘共用）</summary>
    [RelayCommand]
    public void MoveLeft()
    {
        if (!CanAct()) return;
        if (TryMove(_curRow, _curCol - 1, _curRot)) { _curCol--; Render(); }
    }

    /// <summary>右移</summary>
    [RelayCommand]
    public void MoveRight()
    {
        if (!CanAct()) return;
        if (TryMove(_curRow, _curCol + 1, _curRot)) { _curCol++; Render(); }
    }

    /// <summary>软降（加速）</summary>
    [RelayCommand]
    public void SoftDrop()
    {
        if (!CanAct()) return;
        if (TryMove(_curRow + 1, _curCol, _curRot))
        {
            _curRow++;
            Score += 1;
            Render();
        }
        else LockPiece();
    }

    /// <summary>旋转（含 Wall-kick）</summary>
    [RelayCommand]
    public void Rotate()
    {
        if (!CanAct()) return;
        int nextRot = (_curRot + 1) % 4;
        foreach (int k in new[] { 0, -1, 1, -2, 2 })
        {
            if (TryMove(_curRow, _curCol + k, nextRot))
            {
                _curCol += k; _curRot = nextRot;
                Render(); return;
            }
        }
    }

    /// <summary>硬降（立即落底）</summary>
    [RelayCommand]
    public void HardDrop()
    {
        if (!CanAct()) return;
        int drop = 0;
        while (TryMove(_curRow + 1 + drop, _curCol, _curRot)) drop++;
        Score   += drop * 2;
        _curRow += drop;
        LockPiece();
    }

    // ═════════════════════════════════════════════════════════════════════════
    // 内部逻辑
    // ═════════════════════════════════════════════════════════════════════════

    private bool CanAct() => IsRunning && !IsPaused && !IsGameOver;

    private void StartTimer()
    {
        _timer?.Stop(); _timer?.Dispose();
        _timer = new Timer(GetInterval()) { AutoReset = true };
        _timer.Elapsed += (_, _) =>
            Avalonia.Threading.Dispatcher.UIThread.Post(Tick);
        _timer.Start();
    }

    private void UpdateTimerInterval()
    {
        if (_timer != null) _timer.Interval = GetInterval();
    }

    private double GetInterval()
        => Math.Max(InitMs * Math.Pow(0.85, Level - 1), MinMs);

    private void Tick()
    {
        if (!CanAct()) return;
        if (TryMove(_curRow + 1, _curCol, _curRot)) { _curRow++; Render(); }
        else LockPiece();
    }

    private void LockPiece()
    {
        foreach (var cell in GetShape(_curType, _curRot))
        {
            int r = _curRow + cell[0], c = _curCol + cell[1];
            if (InBounds(r, c)) _board[r, c] = _curType;
        }

        PieceLockedEvent?.Invoke(this, EventArgs.Empty);

        int cleared = ClearLines();
        if (cleared > 0)
        {
            AddScore(cleared);
            LinesClearedEvent?.Invoke(this, EventArgs.Empty);
        }

        SpawnPiece();
    }

    private int ClearLines()
    {
        int count = 0;
        for (int r = Rows - 1; r >= 0; r--)
        {
            bool full = true;
            for (int c = 0; c < Cols && full; c++)
                if (_board[r, c] == TetrominoType.Empty) full = false;

            if (!full) continue;

            for (int rr = r; rr > 0; rr--)
                for (int c = 0; c < Cols; c++)
                    _board[rr, c] = _board[rr - 1, c];
            for (int c = 0; c < Cols; c++)
                _board[0, c] = TetrominoType.Empty;

            count++; r++;
        }
        return count;
    }

    private void AddScore(int cleared)
    {
        Score += new[] { 0, 100, 300, 500, 800 }[Math.Min(cleared, 4)] * Level;
        Lines += cleared;
        int newLevel = Lines / 10 + 1;
        if (newLevel != Level) { Level = newLevel; UpdateTimerInterval(); }
    }

    private void SpawnPiece()
    {
        _curType = _nextQueued;
        _nextQueued = RandomType();
        NextType    = _nextQueued;
        _curRot = 0; _curRow = 0; _curCol = Cols / 2 - 2;

        if (!TryMove(_curRow, _curCol, _curRot))
        {
            IsGameOver = true; IsRunning = false;
            _timer?.Stop();
            GameOverEvent?.Invoke(this, EventArgs.Empty);
            Render(); return;
        }

        RefreshPreview();
        Render();
    }

    // ── 渲染 ──────────────────────────────────────────────────────────────────

    private void Render()
    {
        // 1. board
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                CellAt(r, c).Type = _board[r, c];

        if (IsGameOver) return;

        // 2. 幽灵
        int ghostRow = _curRow;
        while (TryMove(ghostRow + 1, _curCol, _curRot)) ghostRow++;
        if (ghostRow != _curRow)
            foreach (var cell in GetShape(_curType, _curRot))
            {
                int gr = ghostRow + cell[0], gc = _curCol + cell[1];
                if (InBounds(gr, gc) && _board[gr, gc] == TetrominoType.Empty)
                    CellAt(gr, gc).Type = TetrominoType.Ghost;
            }

        // 3. 当前方块
        foreach (var cell in GetShape(_curType, _curRot))
        {
            int dr = _curRow + cell[0], dc = _curCol + cell[1];
            if (InBounds(dr, dc)) CellAt(dr, dc).Type = _curType;
        }
    }

    private void RefreshPreview()
    {
        foreach (var c in PreviewCells) c.Type = TetrominoType.Empty;
        var cells = GetShape(_nextQueued, 0);
        int minR = int.MaxValue, minC = int.MaxValue,
            maxR = int.MinValue, maxC = int.MinValue;
        foreach (var cell in cells)
        {
            if (cell[0] < minR) minR = cell[0]; if (cell[0] > maxR) maxR = cell[0];
            if (cell[1] < minC) minC = cell[1]; if (cell[1] > maxC) maxC = cell[1];
        }
        int offR = (4 - (maxR - minR + 1)) / 2 - minR;
        int offC = (4 - (maxC - minC + 1)) / 2 - minC;
        foreach (var cell in cells)
        {
            int r = cell[0] + offR, c = cell[1] + offC;
            if (r >= 0 && r < 4 && c >= 0 && c < 4)
                PreviewCells[r * 4 + c].Type = _nextQueued;
        }
    }

    // ── 辅助 ──────────────────────────────────────────────────────────────────

    private CellViewModel CellAt(int row, int col) => Cells[row * Cols + col];

    private static int[][] GetShape(TetrominoType type, int rot)
        => type == TetrominoType.Empty
            ? Array.Empty<int[]>()
            : TetrominoShapes.All[(int)type][rot];

    private bool TryMove(int row, int col, int rot)
    {
        foreach (var cell in GetShape(_curType, rot))
        {
            int r = row + cell[0], c = col + cell[1];
            if (!InBounds(r, c) || _board[r, c] != TetrominoType.Empty) return false;
        }
        return true;
    }

    private static bool InBounds(int r, int c)
        => r >= 0 && r < Rows && c >= 0 && c < Cols;

    private void ResetBoard()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                _board[r, c] = TetrominoType.Empty;
        foreach (var cell in Cells) cell.Type = TetrominoType.Empty;
    }

    private TetrominoType RandomType() => (TetrominoType)(_rng.Next(7) + 1);
}
