using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Checkers.Logic;

namespace Checkers.Views;

/// <summary>
///     This control is used to highlight squares and take user input.
///     Is is placed on top of the board and is semi-transparent.
/// </summary>
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
        PointerPressed += (_, _) => Board.OnTilePressed(this);
    }

    /// <summary>
    ///     The position of the tile on the board. Should not be null.
    /// </summary>
    public Position Position { get; init; }

    /// <summary>
    ///     A reference to the board. Used to call the OnTilePressed method.
    /// </summary>
    public BoardView Board { get; init; } = null!;

    /// <summary>
    ///     Marks if the tile is selected, updates the tile on change.
    /// </summary>
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
