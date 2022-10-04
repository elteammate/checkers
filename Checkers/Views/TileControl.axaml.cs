using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Checkers.Logic;

namespace Checkers.Views;

public partial class TileControl : UserControl
{
    private const string HoveredClass = "Hovered";
    private const string SelectedClass = "Selected";

    private readonly Rectangle _overlay;

    public TileControl()
    {
        AvaloniaXamlLoader.Load(this);
        _overlay = this.FindControl<Rectangle>("TileOverlay")!;

        PointerEntered += (_, _) => _overlay.Classes.Add(HoveredClass);
        PointerExited += (_, _) => _overlay.Classes.Remove(HoveredClass);
        PointerPressed += (_, _) => Board!.OnTilePressed(this);
    }

    public Position? Position { get; set; }
    public BoardTile? Tile { get; set; }
    public PieceSprite? PieceSprite { get; set; }
    public BoardView? Board { get; set; }


    public bool IsSelected
    {
        set
        {
            if (value)
                _overlay.Classes.Add(SelectedClass);
            else
                _overlay.Classes.Remove(SelectedClass);
        }
    }
}
