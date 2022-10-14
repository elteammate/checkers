using System;
using System.Collections.Generic;

namespace Checkers.Logic;

/// <summary>
///     Represents a game of checkers.
/// </summary>
public class Game
{
    public enum GameResult
    {
        None,
        WhiteWins,
        BlackWins,
        Draw
    }

    public const int BoardHeight = 8;

    public const int BoardWidth = 8;

    // Only black tiles are playable.
    public const int PlayableTiles = BoardHeight * BoardWidth / 2;

    /// <summary>
    ///     Mutable board state containing pieces on 32 squares.
    /// </summary>
    private readonly Piece[] _board;

    /// <summary>
    ///     Creates a new game of checkers.
    ///     It's more convenient to use the <see cref="GameFactory.Create()" /> method.
    /// </summary>
    public Game(Piece[] initialBoard, Color firstPlayer)
    {
        if (initialBoard.Length != PlayableTiles)
            throw new ArgumentException("Board must have 32 tiles", nameof(initialBoard));

        CurrentPlayer = firstPlayer;
        _board = initialBoard;
        MoveFinder = new MoveFinder(CurrentPlayer, initialBoard);
    }

    /// <summary>
    ///     Public read-only accessor for the current board state
    /// </summary>
    public IReadOnlyList<Piece> Board => _board;

    /// <summary>
    ///     Immutable object used to find all possible moves for the current player.
    /// </summary>
    public MoveFinder MoveFinder { get; private set; }

    public Color CurrentPlayer { get; private set; }

    public GameResult Result { get; private set; } = GameResult.None;


    /// <summary>
    ///     This event is raised when the the move is made
    ///     before transitioning to the next player.
    /// </summary>
    public event EventHandler<Move> MoveMade = delegate { };

    /// <summary>
    ///     This event is raised when the piece is captured
    ///     before MoveMade event.
    /// </summary>
    public event EventHandler<Position> PieceCaptured = delegate { };

    /// <summary>
    ///     This event is raised when the piece is promoted
    ///     after MoveMade event.
    /// </summary>
    public event EventHandler<Position> PiecePromoted = delegate { };

    /// <summary>
    ///     The event is raised when next move will be made by different player
    ///     after all the board updates
    /// </summary>
    public event EventHandler<Color> PlayerTransition = delegate { };

    /// <summary>
    ///     This event is raised when the game is over
    /// </summary>
    public event EventHandler<GameResult> GameEnded = delegate { };

    /// <summary>
    ///     This event is raised when the move is finished and all the update events before are raised
    ///     It's guaranteed that the event is raised after all the board updates
    ///     and that it wil not be raised after GameEnded event
    /// </summary>
    public event EventHandler<Move> MoveFinished = delegate { };


    /// <summary>
    ///     Tries to promote the piece at the given position.
    ///     Ignores the request if the piece is not promotable.
    /// </summary>
    private void TryPromote(Position pos)
    {
        if (_board[pos.Index] is Piece.Empty or Piece.BlackKing or Piece.WhiteKing) return;

        if (pos.Row == BoardHeight - 1 && _board[pos.Index] is Piece.White)
        {
            _board[pos.Index] = Piece.WhiteKing;
            PiecePromoted(this, pos);
        }
        else if (pos.Row == 0 && _board[pos.Index] is Piece.Black)
        {
            _board[pos.Index] = Piece.BlackKing;
            PiecePromoted(this, pos);
        }
    }

    /// <summary>
    ///     Makes a move on the board.
    ///     This method assumes that the move is valid.
    ///     All the move logic is implemented here.
    /// </summary>
    public void MakeMove(Move move)
    {
        var piece = _board[move.From.Index];
        _board[move.From.Index] = Piece.Empty;
        _board[move.To.Index] = piece;

        if (move.Jumped != null)
        {
            _board[move.Jumped!.Value.Index] = Piece.Empty;
            PieceCaptured(this, move.Jumped.Value);
        }

        MoveMade(this, move);
        TryPromote(move.To);

        var currentPlayerMoveFinder =
            new MoveFinder(CurrentPlayer, _board, move.Jumped != null ? move.To : null);

        if (move.Jumped != null && currentPlayerMoveFinder.GetForcedMoves().Count > 0)
        {
            MoveFinder = currentPlayerMoveFinder;
            MoveFinished(this, move);
        }
        else
        {
            var opponentHasMoves = currentPlayerMoveFinder.GetMoves().Count > 0;

            CurrentPlayer = CurrentPlayer.Opposite();
            MoveFinder = new MoveFinder(CurrentPlayer, _board);

            var playerHasMoves = MoveFinder.GetMoves().Count > 0;

            if (!playerHasMoves)
            {
                if (!opponentHasMoves)
                    Result = GameResult.Draw;
                else
                    Result = CurrentPlayer == Color.Black
                        ? GameResult.WhiteWins
                        : GameResult.BlackWins;

                GameEnded(this, Result);
            }
            else
            {
                PlayerTransition(this, CurrentPlayer);
                MoveFinished(this, move);
            }
        }
    }
}
