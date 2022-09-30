using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Checkers.Logic;
using Checkers.ViewModels;

namespace Checkers.Views;

public partial class BoardView : UserControl
{
    private readonly BoardViewModel _viewModel;
    private readonly double _cellSize;
    private Dictionary<Position, PieceSprite> _pieceSprites = new();
    private readonly Grid _boardGrid;
    private readonly Canvas _boardCanvas;
    private readonly Grid _boardOverlayGrid;
    private readonly Image _boardBackground;

    public BoardView()
    {
        _viewModel = new BoardViewModel();
        DataContext = _viewModel;

        AvaloniaXamlLoader.Load(this);

        _boardGrid = this.FindControl<Grid>("BoardGrid")!;
        _boardCanvas = this.FindControl<Canvas>("BoardCanvas")!;
        _boardOverlayGrid = this.FindControl<Grid>("BoardOverlayGrid")!;
        _boardBackground = this.FindControl<Image>("BoardBackground")!;

        _cellSize = _boardGrid.Width / Game.BoardWidth;

        InitializeBoard();
        InitializePieces();
    }

    private void InitializeBoard()
    {
        var gridColumns = new StringBuilder()
            .Insert(0, "auto,", Game.BoardWidth)
            .Remove(Game.BoardWidth * 5 - 1, 1)
            .ToString();

        var gridRows = new StringBuilder()
            .Insert(0, "auto,", Game.BoardHeight)
            .Remove(Game.BoardHeight * 5 - 1, 1)
            .ToString();

        _boardGrid.ColumnDefinitions = ColumnDefinitions.Parse(gridColumns);
        _boardGrid.RowDefinitions = RowDefinitions.Parse(gridRows);
        _boardOverlayGrid.ColumnDefinitions = ColumnDefinitions.Parse(gridColumns);
        _boardOverlayGrid.RowDefinitions = RowDefinitions.Parse(gridRows);

        for (var row = 0; row < Game.BoardHeight; row++)
        {
            for (var column = 0; column < Game.BoardWidth; column++)
            {
                var color = (row + column) % 2 == 0 ? Color.White : Color.Black;

                var tile = new BoardTile
                {
                    Width = _cellSize,
                    Height = _cellSize,
                    Color = color,
                };

                tile.SetValue(Grid.ColumnProperty, column);
                tile.SetValue(Grid.RowProperty, row);

                if (color == Color.Black)
                {
                    TileControl tileControl = new()
                    {
                        Width = _cellSize,
                        Height = _cellSize,
                        Tile = tile,
                    };

                    tileControl.SetValue(Grid.ColumnProperty, column);
                    tileControl.SetValue(Grid.RowProperty, row);
                    _boardOverlayGrid.Children.Add(tileControl);
                }

                _boardGrid.Children.Add(tile);
            }
        }

        _boardBackground.Source = AssetManager.BoardBg.Value;
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
