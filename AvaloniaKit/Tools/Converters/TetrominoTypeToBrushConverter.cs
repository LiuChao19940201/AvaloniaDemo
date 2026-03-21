using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaKit.ViewModels.UserControls.Discover;

namespace AvaloniaKit.Converters;

/// <summary>
/// TetrominoType 枚举 → SolidColorBrush
/// 在 UserControl.Resources 中声明后通过 StaticResource 引用。
/// </summary>
public sealed class TetrominoTypeToBrushConverter : IValueConverter
{
    // 经典俄罗斯方块配色（与 Tetris Guideline 一致）
    private static readonly SolidColorBrush I     = Brush("#00BCD4"); // 青
    private static readonly SolidColorBrush O     = Brush("#FFD600"); // 黄
    private static readonly SolidColorBrush T     = Brush("#AB47BC"); // 紫
    private static readonly SolidColorBrush S     = Brush("#4CAF50"); // 绿
    private static readonly SolidColorBrush Z     = Brush("#F44336"); // 红
    private static readonly SolidColorBrush J     = Brush("#2196F3"); // 蓝
    private static readonly SolidColorBrush L     = Brush("#FF7043"); // 橙
    private static readonly SolidColorBrush Ghost = new(Color.FromArgb(55, 255, 255, 255));
    private static readonly SolidColorBrush Empty = new(Color.FromArgb(15, 200, 200, 220));

    private static SolidColorBrush Brush(string hex) => new(Color.Parse(hex));

    public object Convert(object? value, Type targetType,
                          object? parameter, CultureInfo culture)
        => value is TetrominoType t ? t switch
        {
            TetrominoType.I     => I,
            TetrominoType.O     => O,
            TetrominoType.T     => T,
            TetrominoType.S     => S,
            TetrominoType.Z     => Z,
            TetrominoType.J     => J,
            TetrominoType.L     => L,
            TetrominoType.Ghost => Ghost,
            _                   => Empty,
        } : Empty;

    public object ConvertBack(object? value, Type targetType,
                              object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
