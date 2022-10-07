using System;
using System.Collections.Generic;

namespace Checkers.Logic;

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
    public const int PlayableTiles = BoardHeight * BoardWidth / 2;

    private readonly Piece[] _board;
    public IReadOnlyList<Piece> Board => _board;
    public readonly Cached<IReadOnlyDictionary<Position, Piece>> PieceMapping;

    public MoveFinder MoveFinder { get; private set; }

    public Game(Piece[] initialBoard, Color firstPlayer)
    {
        if (initialBoard.Length != PlayableTiles)
            throw new ArgumentException("Board must have 32 tiles", nameof(initialBoard));

        CurrentPlayer = firstPlayer;
        _board = initialBoard;
        MoveFinder = new MoveFinder(CurrentPlayer, initialBoard);

        PieceMapping = new Cached<IReadOnlyDictionary<Position, Piece>>(() =>
        {
            var mapping = new Dictionary<Position, Piece>();
            for (var i = 0; i < PlayableTiles; i++)
                if (_board[i] != Piece.Empty)
                    mapping.Add(new Position(i), _board[i]);

            return mapping;
        });
    }

    public Color CurrentPlayer { get; private set; }

    public GameResult Result { get; private set; } = GameResult.None;


    public event EventHandler<Move> MoveMade = delegate { };
    public event EventHandler<Position> PieceCaptured = delegate { };
    public event EventHandler<Position> PiecePromoted = delegate { };
    public event EventHandler<Color> PlayerTransition = delegate { };
    public event EventHandler<GameResult> GameEnded = delegate { };
    public event EventHandler<Move> MoveFinished = delegate { };

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
