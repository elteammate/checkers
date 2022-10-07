using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Checkers.Logic;

namespace Checkers.Views;

/// <summary>
///     This control represents the piece on the board.
///     It does not handle any logic.
///     This sprite should be placed on a canvas.
/// </summary>
public partial class PieceSprite : UserControl
{
    private const string CapturedClass = "Captured";
    private readonly Image _sprite;

    public double TileSize = 100;

    public PieceSprite()
    {
        AvaloniaXamlLoader.Load(this);
        _sprite = this.FindControl<Image>("Sprite")!;
    }

    /// <summary>
    ///     A piece that this sprite represents.
    ///     Updates the sprite when the piece changes.
    /// </summary>
    public Piece Piece
    {
        set
        {
            _sprite.Source = value switch
            {
                Piece.Black => AssetManager.BlackPiece.Value,
                Piece.White => AssetManager.WhitePiece.Value,
                Piece.BlackKing => AssetManager.BlackKing.Value,
                Piece.WhiteKing => AssetManager.WhiteKing.Value,
                Piece.Empty => _sprite.Source,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };

            if (value == Piece.Empty)
                _sprite.Classes.Add(CapturedClass);
        }
    }

    /// <summary>
    ///     The position of the piece on the board.
    ///     When set, the sprite is moved to the correct position.
    /// </summary>
    public Position Position
    {
        set
        {
            SetValue(Canvas.LeftProperty, value.Column * TileSize);
            SetValue(Canvas.BottomProperty, value.Row * TileSize);
        }
    }
}
