using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaKit.ViewModels.UserControls.Discover;

namespace AvaloniaKit.Converters;

/// <summary>
/// 将 TetrominoType 枚举转换为对应的 SolidColorBrush。
/// 在 XAML ResourceDictionary 或 UserControl.Resources 中声明后即可绑定使用。
/// </summary>
public class TetrominoTypeToBrushConverter : IValueConverter
{
    // 经典俄罗斯方块配色
    private static readonly SolidColorBrush BrushI     = new(Color.Parse("#00BCD4")); // 青色
    private static readonly SolidColorBrush BrushO     = new(Color.Parse("#FFD600")); // 黄色
    private static readonly SolidColorBrush BrushT     = new(Color.Parse("#AB47BC")); // 紫色
    private static readonly SolidColorBrush BrushS     = new(Color.Parse("#66BB6A")); // 绿色
    private static readonly SolidColorBrush BrushZ     = new(Color.Parse("#EF5350")); // 红色
    private static readonly SolidColorBrush BrushJ     = new(Color.Parse("#42A5F5")); // 蓝色
    private static readonly SolidColorBrush BrushL     = new(Color.Parse("#FF7043")); // 橙色
    private static readonly SolidColorBrush BrushGhost = new(Color.FromArgb(50, 255, 255, 255));
    private static readonly SolidColorBrush BrushEmpty = new(Color.FromArgb(18, 255, 255, 255));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TetrominoType type)
        {
            return type switch
            {
                TetrominoType.I     => BrushI,
                TetrominoType.O     => BrushO,
                TetrominoType.T     => BrushT,
                TetrominoType.S     => BrushS,
                TetrominoType.Z     => BrushZ,
                TetrominoType.J     => BrushJ,
                TetrominoType.L     => BrushL,
                TetrominoType.Ghost => BrushGhost,
                _                   => BrushEmpty,
            };
        }
        return BrushEmpty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
