using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Checkers.Logic;

namespace Checkers.Views;

public partial class BoardView : UserControl
{
    private readonly Image _boardBackground;
    private readonly Canvas _boardCanvas;
    private readonly Grid _boardGrid;
    private readonly Grid _boardOverlayGrid;
    private readonly Panel _logPanel;
    private readonly double _tileSize;

    private Game _game;

    /// <summary>
    /// A game being played on the board.
    /// </summary>
    private Game Game
    {
        get => _game;
        set
        {
            _game = value;
            _game.MoveMade += (_, move) => MovePieceSprites(move);
            _game.PiecePromoted += (_, pos) => UpdatePromotedSprite(pos);
            _game.PieceCaptured += (_, pos) => RemoveCapturedPiece(pos);
            _game.PlayerTransition += (_, player) => Log($"{player}'s turn");
            _game.GameEnded += (_, result) => EndGame(result);

            SelectedTile = null;
            _logPanel.Children.Clear();
            InitializePieces();
        }
    }

    private readonly Dictionary<Position, PieceSprite> _pieceSprites = new();

    private TileControl? _selectedTile;

    /// <summary>
    /// A tile that is currently selected by player.
    /// </summary>
    private TileControl? SelectedTile
    {
        get => _selectedTile;
        set
        {
            if (_selectedTile != null) _selectedTile.IsSelected = false;
            _selectedTile = value;
            if (_selectedTile != null) _selectedTile.IsSelected = true;
        }
    }

    public BoardView()
    {
        AvaloniaXamlLoader.Load(this);

        _boardGrid = this.FindControl<Grid>(nameof(BoardGrid))!;
        _boardCanvas = this.FindControl<Canvas>(nameof(BoardCanvas))!;
        _boardOverlayGrid = this.FindControl<Grid>(nameof(BoardOverlayGrid))!;
        _boardBackground = this.FindControl<Image>(nameof(BoardBackground))!;
        _logPanel = this.FindControl<StackPanel>(nameof(LogPanel))!;

        _tileSize = _boardGrid.Width / Game.BoardWidth;

        InitializeBoard();

        Game = GameFactory.Create();
    }

    /// <summary>
    /// Adds an entry to the log
    /// </summary>
    private void Log(string message) =>
        _logPanel.Children.Add(new TextBlock { Text = message });

    /// <summary>
    /// Fills the board with tiles, sets up the background 
    /// </summary>
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
                Width = _tileSize,
                Height = _tileSize,
                Color = color
            };

            tile.SetValue(Grid.ColumnProperty, column);
            tile.SetValue(Grid.RowProperty, row);

            if (color == Color.Black)
            {
                TileControl tileControl = new()
                {
                    Width = _tileSize,
                    Height = _tileSize,
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

    /// <summary>
    /// Removes all pieces from the board and adds them back in their initial positions.
    /// </summary>
    private void InitializePieces()
    {
        _pieceSprites.Clear();
        _boardCanvas.Children.Clear();

        foreach (var pieceOnBoard in Game.PieceMapping.Value)
        {
            var pieceSprite = new PieceSprite();
            var position = pieceOnBoard.Key;
            var pieceType = pieceOnBoard.Value;

            var x = position.Column * _tileSize;
            var y = position.Row * _tileSize;

            pieceSprite.Piece = pieceType;
            pieceSprite.SetValue(Canvas.LeftProperty, x);
            pieceSprite.SetValue(Canvas.BottomProperty, y);
            _pieceSprites.Add(position, pieceSprite);

            _boardCanvas.Children.Add(pieceSprite);
        }

        Log("Game is ready!");
    }

    /// <summary>
    /// Given a move, applies it to the board and animates the piece sprite
    /// </summary>
    private void MovePieceSprites(Move move)
    {
        if (_pieceSprites.TryGetValue(move.From, out var pieceSprite))
        {
            _pieceSprites.Remove(move.From);
            _pieceSprites.Add(move.To, pieceSprite);

            var x = move.To.Column * _tileSize;
            var y = move.To.Row * _tileSize;

            pieceSprite.SetValue(Canvas.LeftProperty, x);
            pieceSprite.SetValue(Canvas.BottomProperty, y);
        }

        SelectedTile = null;

        Log($"{move.Color} moved from {move.From.Index + 1} to {move.To.Index + 1}");
    }

    /// <summary>
    /// Updates the sprite of a piece that was promoted
    /// </summary>
    private void UpdatePromotedSprite(Position pos)
    {
        if (_pieceSprites.TryGetValue(pos, out var pieceSprite))
        {
            pieceSprite.Piece = Game.Board[pos.Index].Promote();
            Log($"Piece at {pos.Index + 1} was promoted to king!");
        }
    }

    /// <summary>
    /// Removes a captured piece from the board
    /// </summary>
    private void RemoveCapturedPiece(Position pos)
    {
        if (_pieceSprites.TryGetValue(pos, out var pieceSprite))
        {
            _pieceSprites.Remove(pos);
            pieceSprite.Piece = Piece.Empty;
        }

        Log($"Piece at {pos.Index + 1} was captured");
    }

    /// <summary>
    /// Called when player clicks a tile
    /// </summary>
    public void OnTilePressed(TileControl tile)
    {
        var pos = tile.Position!;

        if (Game.CurrentPlayer == Game.Board[pos.Index].GetColor() &&
            Game.MoveFinder.GetMoves().FirstOrDefault(move => move.From == pos) != null)
        {
            SelectedTile = tile;
        }
        else if (SelectedTile != null)
        {
            var move = Game.MoveFinder.GetMoves().FirstOrDefault(
                move => move.From == SelectedTile.Position! && move.To == pos);

            if (move != null) Game.MakeMove(move);
        }
    }

    /// <summary>
    /// Called when the game is finished
    /// </summary>
    private void EndGame(Game.GameResult result)
    {
        Log("Game finished!");
        Log(result switch
        {
            Game.GameResult.WhiteWins => "White wins!",
            Game.GameResult.BlackWins => "Black wins!",
            Game.GameResult.Draw => "Draw!",
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        });
    }

    /// <summary>
    /// Restarts the game
    /// </summary>
    private void NewGame(object sender, RoutedEventArgs e) =>
        Game = GameFactory.Create();
}
