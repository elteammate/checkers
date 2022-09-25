using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Checkers.Logic;
using Checkers.ViewModels;
using Color = Avalonia.Media.Color;

namespace Checkers.Views;

public partial class BoardView : UserControl
{
    private readonly BoardViewModel _viewModel;
    private readonly double _cellSize;
    private Dictionary<Position, PieceSprite> _pieceSprites = new();
    private readonly Grid _boardGrid;
    private readonly Canvas _boardCanvas;

    private static readonly Brush
        WhiteTileBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));

    private static readonly Brush
        BlackTileBrush = new SolidColorBrush(Color.FromRgb(81, 191, 250));

    public BoardView()
    {
        _viewModel = new BoardViewModel();
        DataContext = _viewModel;

        AvaloniaXamlLoader.Load(this);

        _boardGrid = this.FindControl<Grid>("BoardGrid")!;
        _boardCanvas = this.FindControl<Canvas>("BoardCanvas")!;

        _cellSize = _boardGrid.Width / Game.BoardWidth;

        InitializeBoard();
        InitializePieces();
    }

    private void InitializeBoard()
    {
        _boardGrid.ColumnDefinitions = ColumnDefinitions.Parse(
            new StringBuilder()
                .Insert(0, "auto,", Game.BoardWidth)
                .Remove(5 * Game.BoardWidth - 1, 1)
                .ToString()
        );

        _boardGrid.RowDefinitions = RowDefinitions.Parse(
            new StringBuilder()
                .Insert(0, "auto,", Game.BoardHeight)
                .Remove(5 * Game.BoardWidth - 1, 1)
                .ToString()
        );

        for (var row = 0; row < Game.BoardHeight; row++)
        {
            for (var column = 0; column < Game.BoardWidth; column++)
            {
                var cell = new Rectangle
                {
                    Width = _cellSize,
                    Height = _cellSize,
                    Fill = (row + column) % 2 == 1
                        ? BlackTileBrush
                        : WhiteTileBrush,
                };

                cell.SetValue(Grid.ColumnProperty, column);
                cell.SetValue(Grid.RowProperty, row);
                cell.Opacity = 0.5;

                _boardGrid.Children.Add(cell);
            }
        }

        var backgroundImage = new Image
        {
            Source = AssetManager.BoardBg.Value,
            Width = _boardCanvas.Width,
            Height = _boardCanvas.Height,
            ZIndex = -1,
        };
        _boardCanvas.Children.Add(backgroundImage);
    }

    private void InitializePieces()
    {
        _viewModel.Pieces.CollectionChanged += UpdatePieces;
        foreach (var pieceOnBoard in _viewModel.Pieces)
        {
            var pieceSprite = new PieceSprite();
            var position = pieceOnBoard.Key;
            var pieceType = pieceOnBoard.Value;

            var x = position.Column * _cellSize;
            var y = position.Row * _cellSize;

            pieceSprite.Piece = pieceType;
            pieceSprite.SetValue(Canvas.LeftProperty, x);
            pieceSprite.SetValue(Canvas.BottomProperty, y);
            _pieceSprites.Add(position, pieceSprite);

            _boardCanvas.Children.Add(pieceSprite);
        }
    }

    private void UpdatePieces(object sender, NotifyCollectionChangedEventArgs e)
    { }
}
