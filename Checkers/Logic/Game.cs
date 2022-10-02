using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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

    private readonly List<Move> _movesLog = new();


    public Game(Piece[] initialBoard, Color firstPlayer)
    {
        if (initialBoard.Length != PlayableTiles)
            throw new ArgumentException("Board must have 32 tiles", nameof(initialBoard));

        CurrentPlayer = firstPlayer;
        _board = initialBoard;
        MoveFinder = new MoveFinder(CurrentPlayer, initialBoard);
    }

    public ImmutableArray<Piece> Board => _board.ToImmutableArray();
    public MoveFinder MoveFinder { get; private set; }
    public ImmutableArray<Move> Moves => MoveFinder.GetMoves().ToImmutableArray();
    public Color CurrentPlayer { get; private set; }
    public GameResult Result { get; private set; } = GameResult.None;
    public IReadOnlyList<Move> MovesLog => _movesLog;

    public IReadOnlyDictionary<Position, Piece> PieceMapping
    {
        get
        {
            var mapping = new Dictionary<Position, Piece>();
            for (var i = 0; i < PlayableTiles; i++)
                if (_board[i] != Piece.Empty)
                    mapping.Add(new Position(i), _board[i]);

            return mapping;
        }
    }

    public event EventHandler<Move> MoveMade = delegate { };
    public event EventHandler<Position> PieceCaptured = delegate { };
    public event EventHandler<Position> PiecePromoted = delegate { };
    public event EventHandler<GameResult> GameEnded = delegate { };

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
            _board[move.Jumped.Index] = Piece.Empty;
            PieceCaptured(this, move.Jumped);
        }

        TryPromote(move.To);
        MoveMade(this, move);

        var currentPlayerMoveFinder = new MoveFinder(CurrentPlayer, _board);
        if (move.Jumped != null && currentPlayerMoveFinder.GetForcedMoves().Count > 0)
            MoveFinder = currentPlayerMoveFinder;
        else
        {
            var opponentHasMoves = currentPlayerMoveFinder.GetMoves().Count > 0;

            CurrentPlayer = CurrentPlayer.Opposite();
            MoveFinder = new MoveFinder(CurrentPlayer, _board);

            var playerHasMoves = MoveFinder.GetMoves().Count > 0;

            if (!opponentHasMoves)
            {
                if (!playerHasMoves)
                    Result = GameResult.Draw;
                else
                    Result = CurrentPlayer == Color.White
                        ? GameResult.WhiteWins
                        : GameResult.BlackWins;

                GameEnded(this, Result);
            }
        }
    }
}
