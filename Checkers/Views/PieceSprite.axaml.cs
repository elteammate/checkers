using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Checkers.Logic;

namespace Checkers.Views;

public partial class PieceSprite : UserControl
{
    private readonly Uri _uriToSprites = new("avares://Checkers/Assets/");
    private readonly Image _sprite;

    private readonly Bitmap _blackPieceSprite;
    private readonly Bitmap _whitePieceSprite;
    private readonly Bitmap _blackKingSprite;
    private readonly Bitmap _whiteKingSprite;

    public Piece Piece
    {
        set
        {
            _sprite.Source = value switch
            {
                Piece.Black => _blackPieceSprite,
                Piece.White => _whitePieceSprite,
                Piece.BlackKing => _blackKingSprite,
                Piece.WhiteKing => _whiteKingSprite,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
    }

    public PieceSprite()
    {
        AvaloniaXamlLoader.Load(this);

        _sprite = this.FindControl<Image>("Sprite")!;
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;

        _blackPieceSprite = new Bitmap(assets.Open(new Uri(_uriToSprites, "black-piece.png")));
        _whitePieceSprite = new Bitmap(assets.Open(new Uri(_uriToSprites, "white-piece.png")));
        _blackKingSprite = new Bitmap(assets.Open(new Uri(_uriToSprites, "black-king.png")));
        _whiteKingSprite = new Bitmap(assets.Open(new Uri(_uriToSprites, "white-king.png")));
    }
}
