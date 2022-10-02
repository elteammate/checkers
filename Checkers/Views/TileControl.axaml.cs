using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Checkers.Logic;

namespace Checkers.Views;

public partial class TileControl : UserControl
{
    private const string HoveredClass = "Hovered";

    public TileControl()
    {
        AvaloniaXamlLoader.Load(this);
        var overlay = this.FindControl<Rectangle>("TileOverlay")!;

        PointerEntered += (_, _) => overlay.Classes.Add(HoveredClass);
        PointerExited += (_, _) => overlay.Classes.Remove(HoveredClass);
        PointerPressed += (_, _) => Board!.OnTilePressed(this);
    }

    public Position? Position { get; set; }
    public BoardTile? Tile { get; set; }
    public PieceSprite? PieceSprite { get; set; }
    public BoardView? Board { get; set; }
}
