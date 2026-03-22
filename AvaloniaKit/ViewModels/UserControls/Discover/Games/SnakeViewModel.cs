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

    // ===== Commands =====

    [RelayCommand]
    private void Start()
    {
        Reset();
        IsRunning = true;
        SpawnFood();
        StartTimer();
    }

    [RelayCommand]
    private void TogglePause()
    {
        if (!IsRunning) return;
        IsPaused = !IsPaused;
    }

    [RelayCommand] private void Up() => _dir = (-1, 0);
    [RelayCommand] private void Down() => _dir = (1, 0);
    [RelayCommand] private void Left() => _dir = (0, -1);
    [RelayCommand] private void Right() => _dir = (0, 1);

    [RelayCommand]
    private void GoBack()
    {
        _timer?.Stop();
        IsRunning = false;
        WeakReferenceMessenger.Default.Send(new NavigateBackFromGameBoxesMessage());
    }

    // ===== 核心 =====

    private void StartTimer()
    {
        _timer?.Stop();
        _timer = new Timer(120);
        _timer.Elapsed += (_, _) => Dispatcher.UIThread.Post(Tick);
        _timer.Start();
    }

    private void Tick()
    {
        if (!IsRunning || IsPaused || IsGameOver) return;

        var head = _snake.First!.Value;
        var next = (head.r + _dir.r, head.c + _dir.c);

        if (!InBounds(next) || IsSnake(next))
        {
            IsGameOver = true;
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

    private void Render()
    {
        foreach (var c in Cells)
            c.Color = "#111";

        foreach (var s in _snake)
            _grid[s.r, s.c].Color = "#4CAF50";

        _grid[_food.r, _food.c].Color = "#FF5252";
    }

    private void SpawnFood()
    {
        while (true)
        {
            var r = _rng.Next(Rows);
            var c = _rng.Next(Cols);
            if (!IsSnake((r, c)))
            {
                _food = (r, c);
                break;
            }
        }
    }

    private void Reset()
    {
        _snake.Clear();
        _snake.AddFirst((10, 10));
        Score = 0;
        Length = 1;
        IsGameOver = false;
        _dir = (0, 1);
    }

    private bool InBounds((int r, int c) p)
        => p.r >= 0 && p.r < Rows && p.c >= 0 && p.c < Cols;

    private bool IsSnake((int r, int c) p)
        => _snake.Contains(p);
}

public partial class SnakeCell : ObservableObject
{
    public int Row { get; init; }
    public int Col { get; init; }

    [ObservableProperty]
    private string _color = "#111";
}