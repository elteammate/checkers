using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;

namespace Checkers.Views;

public partial class TileControl : UserControl
{
    private const string HoveredClass = "Hovered";
    public BoardTile? Tile { get; set; }
    public PieceSprite? PieceSprite { get; set; }

    public TileControl()
    {
        AvaloniaXamlLoader.Load(this);
        var overlay = this.FindControl<Rectangle>("TileOverlay")!;

        PointerEntered += (_, _) =>
        {
            overlay.Classes.Add(HoveredClass);
            Console.WriteLine("aba");
        };
        PointerExited += (_, _) => overlay.Classes.Remove(HoveredClass);
    }
}
