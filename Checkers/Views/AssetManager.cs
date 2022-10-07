using System;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Checkers.Logic.AI;

namespace Checkers.Views;

/// <summary>
/// A helper class for loading images and other assets from the resources.
/// </summary>
public static class AssetManager
{
    private static readonly Uri UriToAssets = new("avares://Checkers/Assets/");

    private static readonly Lazy<IAssetLoader> AssetLoader =
        new(() => AvaloniaLocator.Current.GetService<IAssetLoader>()!);

    public static readonly Lazy<Bitmap> WhitePiece = LazyLoadBitmap("white-piece.png");
    public static readonly Lazy<Bitmap> BlackPiece = LazyLoadBitmap("black-piece.png");
    public static readonly Lazy<Bitmap> WhiteKing = LazyLoadBitmap("white-king.png");
    public static readonly Lazy<Bitmap> BlackKing = LazyLoadBitmap("black-king.png");

    public static readonly Lazy<Bitmap> BoardBg = LazyLoadBitmap("wood-texture.jpg");

    public static readonly Lazy<NeuralNetwork[]> NeuralNetworks = new(() =>
    {
        const int count = 30;
        var neuralNetworks = new NeuralNetwork[count];

        for (var id = 0; id < count; id++)
            neuralNetworks[id] = NeuralNetwork.Load($"Assets/networks/{id}.json");

        return neuralNetworks;
    });

    private static Stream Get(string name)
    {
        var uri = new Uri(UriToAssets, name);
        return AssetLoader.Value.Open(uri);
    }

    private static Lazy<Bitmap> LazyLoadBitmap(string name) =>
        new(() => new Bitmap(Get(name)));
}
