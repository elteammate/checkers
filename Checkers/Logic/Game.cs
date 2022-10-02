using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Checkers.Logic;

public class Game
{
    public const int BoardHeight = 8;
    public const int BoardWidth = 8;
    public const int PlayableTiles = BoardHeight * BoardWidth / 2;

    public enum GameResult
    {
        None,
        WhiteWins,
        BlackWins,
        Draw
    }

    private Piece[] _board;
    public ImmutableArray<Piece> Board => _board.ToImmutableArray();
    public MoveFinder MoveFinder { get; private set; }
    public Color CurrentPlayer { get; private set; }
    public GameResult Result { get; private set; } = GameResult.None;

    private List<Move> _movesLog = new();
    public IReadOnlyList<Move> MovesLog => _movesLog;

    public Game(Piece[] initialBoard, Color firstPlayer)
    {
        if (initialBoard.Length != PlayableTiles)
            throw new ArgumentException("Board must have 32 tiles", nameof(initialBoard));

        CurrentPlayer = firstPlayer;
        _board = initialBoard;
        MoveFinder = new MoveFinder(CurrentPlayer, initialBoard);
    }

    private void TryPromote(Position pos)
    {
        if (_board[pos.Index] is Piece.Empty or Piece.BlackKing or Piece.WhiteKing) return;

        if (pos.Row == BoardHeight - 1 && _board[pos.Index] is Piece.White)
        {
            _board[pos.Index] = Piece.WhiteKing;
        }
        else if (pos.Row == 0 && _board[pos.Index] is Piece.Black)
        {
            _board[pos.Index] = Piece.BlackKing;
        }
    }

    public void MakeMove(Move move)
    {
        var piece = _board[move.From.Index];
        _board[move.From.Index] = Piece.Empty;
        _board[move.To.Index] = piece;
        if (move.Jumped != null) _board[move.Jumped.Index] = Piece.Empty;

        TryPromote(move.To);

        var currentPlayerMoveFinder = new MoveFinder(CurrentPlayer, _board);
        if (move.Jumped != null && currentPlayerMoveFinder.GetForcedMoves().Count > 0)
        {
            MoveFinder = currentPlayerMoveFinder;
        }
        else
        {
            var opponentHasMoves = currentPlayerMoveFinder.GetMoves().Count > 0;

            CurrentPlayer = CurrentPlayer.Opposite();
            MoveFinder = new MoveFinder(CurrentPlayer, _board);

            var playerHasMoves = MoveFinder.GetMoves().Count > 0;

            if (!opponentHasMoves && !playerHasMoves)
                Result = GameResult.Draw;
            else if (!opponentHasMoves)
                Result = CurrentPlayer == Color.White ? GameResult.WhiteWins : GameResult.BlackWins;
            else if (!playerHasMoves)
                Result = CurrentPlayer == Color.White ? GameResult.BlackWins : GameResult.WhiteWins;
        }
    }
}
