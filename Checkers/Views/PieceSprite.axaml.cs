using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Checkers.Logic;

namespace Checkers.Views;

public partial class PieceSprite : UserControl
{
    private readonly Image _sprite;

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

    public PieceSprite()
    {
        AvaloniaXamlLoader.Load(this);
        _sprite = this.FindControl<Image>("Sprite")!;
    }
}
