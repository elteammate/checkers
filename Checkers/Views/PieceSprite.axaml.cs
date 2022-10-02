using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Checkers.Logic;

namespace Checkers.Views;

public partial class PieceSprite : UserControl
{
    private readonly Image _sprite;

    public PieceSprite()
    {
        AvaloniaXamlLoader.Load(this);
        _sprite = this.FindControl<Image>("Sprite")!;
    }

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
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
    }
}
