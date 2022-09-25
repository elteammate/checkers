using System;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Color = Checkers.Logic.Color;

namespace Checkers.Views;

public partial class BoardTile : UserControl
{
    private readonly Rectangle _rectangle;

    private static readonly Brush WhiteBrush = new SolidColorBrush(Colors.White);
    private static readonly Brush BlackBrush = new SolidColorBrush(Colors.Aqua);

    public Color Color
    {
        set => _rectangle.Fill = value switch
        {
            Color.White => WhiteBrush,
            Color.Black => BlackBrush,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    public BoardTile()
    {
        AvaloniaXamlLoader.Load(this);
        _rectangle = this.FindControl<Rectangle>("Tile")!;
    }
}
