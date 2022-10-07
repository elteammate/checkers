using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Checkers.Logic;
using Checkers.Logic.AI;
using DynamicData.Kernel;

namespace Checkers.Views;

public partial class BoardView : UserControl
{
    private readonly Image _boardBackground;
    private readonly Canvas _boardCanvas;
    private readonly Grid _boardGrid;
    private readonly Grid _boardOverlayGrid;
    private readonly Panel _logPanel;

    private readonly Dictionary<Position, PieceSprite> _pieceSprites = new();
    private readonly double _tileSize;

    private Game _game = null!;

    private TileControl? _selectedTile;

    private bool _whiteIsAi, _blackIsAi;

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
    ///     A game being played on the board.
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
            _game.MoveFinished += (_, _) => MakeAiMoveIfNeeded();

            SelectedTile = null;
            _logPanel.Children.Clear();
            InitializePieces();

            MakeAiMoveIfNeeded();
        }
    }

    /// <summary>
    ///     A tile that is currently selected by player.
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

    /// <summary>
    ///     Adds an entry to the log
    /// </summary>
    private void Log(string message) =>
        _logPanel.Children.Add(new TextBlock { Text = message });

    /// <summary>
    ///     Fills the board with tiles, sets up the background
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
    ///     Removes all pieces from the board and adds them back in their initial positions.
    /// </summary>
    private void InitializePieces()
    {
        _pieceSprites.Clear();
        _boardCanvas.Children.Clear();

        for (var index = 0; index < Game.PlayableTiles; index++)
        {
            if (Game.Board[index] == Piece.Empty) continue;

            var pieceSprite = new PieceSprite { TileSize = _tileSize };
            var position = new Position(index);
            var pieceType = Game.Board[index];

            pieceSprite.Piece = pieceType;
            pieceSprite.Position = position;
            _pieceSprites.Add(position, pieceSprite);

            _boardCanvas.Children.Add(pieceSprite);
        }

        Log("Game is ready!");
    }

    /// <summary>
    ///     Given a move, applies it to the board and animates the piece sprite
    /// </summary>
    private void MovePieceSprites(Move move)
    {
        if (_pieceSprites.TryGetValue(move.From, out var pieceSprite))
        {
            _pieceSprites.Remove(move.From);
            _pieceSprites.Add(move.To, pieceSprite);
            pieceSprite.Position = move.To;
        }

        SelectedTile = null;

        Log($"{move.Color} moved from {move.From.Index + 1} to {move.To.Index + 1}");
    }

    /// <summary>
    ///     Updates the sprite of a piece that was promoted
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
    ///     Removes a captured piece from the board
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
    ///     Called when player clicks a tile
    /// </summary>
    public void OnTilePressed(TileControl tile)
    {
        if ((_game.CurrentPlayer == Color.Black && _blackIsAi) ||
            (_game.CurrentPlayer == Color.White && _whiteIsAi))
            return;

        var pos = tile.Position;

        if (Game.CurrentPlayer == Game.Board[pos.Index].GetColor() &&
            Game.MoveFinder.GetMoves().FirstOrOptional(move => move.From == pos).HasValue)
            SelectedTile = tile;
        else if (SelectedTile != null)
        {
            var move = Game.MoveFinder.GetMoves().FirstOrOptional(
                move => move.From == SelectedTile.Position && move.To == pos);

            if (move.HasValue) Game.MakeMove(move.Value);
        }
    }

    /// <summary>
    ///     Called when the game is finished
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
    ///     Restarts the game
    /// </summary>
    private void NewGame(bool isWhiteAi, bool isBlackAi)
    {
        _whiteIsAi = isWhiteAi;
        _blackIsAi = isBlackAi;
        Game = GameFactory.Create();
    }

    private void MakeAiMoveIfNeeded()
    {
        if (!((_game.CurrentPlayer == Color.White && _whiteIsAi) ||
              (_game.CurrentPlayer == Color.Black && _blackIsAi))) return;

        Task.Run(() =>
        {
            var networkId = new Random().Next(AssetManager.NeuralNetworks.Value.Length);
            var network = AssetManager.NeuralNetworks.Value[networkId];
            var solver = new Solver(_game, network.GetEvaluator());
            var move = solver.FindBestMove();
            Dispatcher.UIThread.InvokeAsync(() => { _game.MakeMove(move); });
        });
    }

    private void NewGamePlayerVsPlayer_OnClick(object sender, RoutedEventArgs e) =>
        NewGame(false, false);

    private void NewGamePlayerVsAi_OnClick(object sender, RoutedEventArgs e) =>
        NewGame(false, true);

    private void NewGameAiVsPlayer_OnClick(object sender, RoutedEventArgs e) =>
        NewGame(true, false);

    private void NewGameAiVsAi_OnClick(object sender, RoutedEventArgs e) =>
        NewGame(true, true);
}
