using Avalonia.Threading;
using AvaloniaKit.ViewModels.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;

namespace AvaloniaKit.ViewModels.UserControls.Discover.Games;

public partial class SnakeViewModel : ObservableObject
{
    public const int Rows = 20;
    public const int Cols = 20;

    [ObservableProperty] private int _score;
    [ObservableProperty] private int _length;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private bool _isGameOver;

    public ObservableCollection<SnakeCell> Cells { get; } = new();

    private readonly SnakeCell[,] _grid = new SnakeCell[Rows, Cols];
    private readonly LinkedList<(int r, int c)> _snake = new();

    private (int r, int c) _food;
    private (int r, int c) _dir = (0, 1);

    private Timer? _timer;
    private readonly Random _rng = new();

    public SnakeViewModel()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
            {
                var cell = new SnakeCell { Row = r, Col = c };
                Cells.Add(cell);
                _grid[r, c] = cell;
            }
    }

    // ============================
    // Commands
    // ============================

    [RelayCommand]
    private void Start()
    {
        Reset();

        SpawnFood();   // ✅ 先生成食物
        Render();      // ✅ 再渲染（避免被覆盖）

        IsRunning = true;
        IsPaused = false;

        StartTimer();
    }

    [RelayCommand]
    private void TogglePause()
    {
        if (!IsRunning || IsGameOver) return;

        IsPaused = !IsPaused;

        if (IsPaused)
            _timer?.Stop();
        else
            _timer?.Start();
    }

    [RelayCommand]
    private void Up()
    {
        if (_dir != (1, 0))
            _dir = (-1, 0);
    }

    [RelayCommand]
    private void Down()
    {
        if (_dir != (-1, 0))
            _dir = (1, 0);
    }

    [RelayCommand]
    private void Left()
    {
        if (_dir != (0, 1))
            _dir = (0, -1);
    }

    [RelayCommand]
    private void Right()
    {
        if (_dir != (0, -1))
            _dir = (0, 1);
    }

    [RelayCommand]
    private void GoBack()
    {
        _timer?.Stop();
        IsRunning = false;
        IsPaused = false;
        IsGameOver = false;

        WeakReferenceMessenger.Default.Send(new NavigateBackFromGameBoxesMessage());
    }

    // ============================
    // 核心循环
    // ============================

    private void StartTimer()
    {
        _timer?.Stop();
        _timer = new Timer(120);
        _timer.Elapsed += (_, _) =>
            Dispatcher.UIThread.Post(Tick);

        _timer.Start();
    }

    private void Tick()
    {
        if (!IsRunning || IsPaused || IsGameOver)
            return;

        var head = _snake.First!.Value;
        var next = (head.r + _dir.r, head.c + _dir.c);

        // 撞墙 or 撞自己
        if (!InBounds(next) || IsSnake(next))
        {
            _timer?.Stop();
            IsGameOver = true;
            IsRunning = false;

            Render();
            return;
        }

        _snake.AddFirst(next);

        if (next == _food)
        {
            Score += 10;
            SpawnFood();
        }
        else
        {
            _snake.RemoveLast();
        }

        Length = _snake.Count;

        Render();
    }

    // ============================
    // 渲染（关键修复点）
    // ============================

    private void Render()
    {
        // 1. 清空
        foreach (var c in Cells)
            c.Color = "#222222";

        // 2. 食物（先画）
        _grid[_food.r, _food.c].Color = "#FF5252";

        // 3. 蛇（后画，避免被覆盖）
        foreach (var s in _snake)
        {
            if (s.Equals(_snake.First!.Value))
                _grid[s.r, s.c].Color = "#4CAF50"; // 蛇头
            else
                _grid[s.r, s.c].Color = "#66BB6A"; // 蛇身
        }
    }

    private void SpawnFood()
    {
        if (_snake.Count >= Rows * Cols - 1)
            return;

        while (true)
        {
            var r = _rng.Next(Rows);
            var c = _rng.Next(Cols);

            if (!IsSnake((r, c)))
            {
                _food = (r, c);
                return;
            }
        }
    }

    private void Reset()
    {
        _timer?.Stop();

        _snake.Clear();

        // ✅ 初始蛇长度 = 3（更自然）
        _snake.AddFirst((10, 10));
        _snake.AddLast((10, 9));
        _snake.AddLast((10, 8));

        Score = 0;
        Length = 3;
        IsGameOver = false;

        _dir = (0, 1);

        foreach (var cell in Cells)
            cell.Color = "#222222";
    }

    // ============================
    // 工具
    // ============================

    private bool InBounds((int r, int c) p)
        => p.r >= 0 && p.r < Rows && p.c >= 0 && p.c < Cols;

    private bool IsSnake((int r, int c) p)
        => _snake.Contains(p);
}

// ============================
// Cell
// ============================

public partial class SnakeCell : ObservableObject
{
    public int Row { get; init; }
    public int Col { get; init; }

    [ObservableProperty]
    private string _color = "#222222";
}