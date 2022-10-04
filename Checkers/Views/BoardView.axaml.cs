using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Checkers.Logic;

namespace Checkers.Views;

public partial class BoardView : UserControl
{
    private readonly Image _boardBackground;
    private readonly Canvas _boardCanvas;
    private readonly Grid _boardGrid;
    private readonly Grid _boardOverlayGrid;
    private readonly double _cellSize;
    private readonly Game _game;
    private readonly Dictionary<Position, PieceSprite> _pieceSprites = new();

    private TileControl? _selectedTile;

    public BoardView()
    {
        _game = GameFactory.Create();

        AvaloniaXamlLoader.Load(this);

        _boardGrid = this.FindControl<Grid>("BoardGrid")!;
        _boardCanvas = this.FindControl<Canvas>("BoardCanvas")!;
        _boardOverlayGrid = this.FindControl<Grid>("BoardOverlayGrid")!;
        _boardBackground = this.FindControl<Image>("BoardBackground")!;

        _cellSize = _boardGrid.Width / Game.BoardWidth;

        _game.MoveMade += (_, move) => MovePieceSprites(move);

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
        for (var column = 0; column < Game.BoardWidth; column++)
        {
            var color = (row + column) % 2 == 0 ? Color.White : Color.Black;

            var tile = new BoardTile
            {
                Width = _cellSize,
                Height = _cellSize,
                Color = color
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
                    Position = new Position(7 - row, column),
                    Board = this
                };

                tileControl.SetValue(Grid.ColumnProperty, column);
                tileControl.SetValue(Grid.RowProperty, row);
                _boardOverlayGrid.Children.Add(tileControl);
            }

            _boardGrid.Children.Add(tile);
        }

        _boardBackground.Source = AssetManager.BoardBg.Value;
    }

    private void InitializePieces()
    {
        foreach (var pieceOnBoard in _game.PieceMapping.Value)
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

    private void MovePieceSprites(Move move)
    {
        if (_pieceSprites.TryGetValue(move.From, out var pieceSprite))
        {
            _pieceSprites.Remove(move.From);
            _pieceSprites.Add(move.To, pieceSprite);

            var x = move.To.Column * _cellSize;
            var y = move.To.Row * _cellSize;

            pieceSprite.SetValue(Canvas.LeftProperty, x);
            pieceSprite.SetValue(Canvas.BottomProperty, y);
        }

        if (move.Jumped != null &&
            _pieceSprites.TryGetValue(move.Jumped!, out var jumpedPieceSprite))
        {
            _pieceSprites.Remove(move.Jumped!);
            _boardCanvas.Children.Remove(jumpedPieceSprite);
        }

        if (move.Color != _game.CurrentPlayer) SelectTile(null);
    }

    private void SelectTile(TileControl? tile)
    {
        if (_selectedTile != null) _selectedTile.IsSelected = false;
        _selectedTile = tile;
        if (_selectedTile != null) _selectedTile.IsSelected = true;
    }

    public void OnTilePressed(TileControl tile)
    {
        var pos = tile.Position!;

        if (_game.CurrentPlayer == _game.Board[pos.Index].GetColor() &&
            _game.MoveFinder.GetMoves().FirstOrDefault(move => move.From == pos) != null)
        {
            SelectTile(tile);
        }
        else if (_selectedTile != null)
        {
            var move = _game.MoveFinder.GetMoves().FirstOrDefault(
                move => move.From == _selectedTile.Position! && move.To == pos);

            if (move != null) _game.MakeMove(move);
        }
    }
}
