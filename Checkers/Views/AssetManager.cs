using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Checkers.Views;

public static class AssetManager
{
    private static readonly Uri UriToAssets = new("avares://Checkers/Assets/");

    private static readonly Lazy<IAssetLoader> Asset =
        new(() => AvaloniaLocator.Current.GetService<IAssetLoader>()!);

    private static Stream Get(string name)
    {
        var uri = new Uri(UriToAssets, name);
        return Asset.Value.Open(uri);
    }

    public static readonly Lazy<Bitmap> WhitePiece = new(() => new Bitmap(Get("white-piece.png")));
    public static readonly Lazy<Bitmap> BlackPiece = new(() => new Bitmap(Get("black-piece.png")));
    public static readonly Lazy<Bitmap> WhiteKing = new(() => new Bitmap(Get("white-king.png")));
    public static readonly Lazy<Bitmap> BlackKing = new(() => new Bitmap(Get("black-king.png")));

    public static readonly Lazy<Bitmap> BoardBg = new(() => new Bitmap(Get("wood-texture.jpg")));
}
