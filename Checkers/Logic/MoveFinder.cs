using System;
using System.Collections.Generic;

namespace Checkers.Logic;

public class MoveFinder
{
    public enum RelativePiece : sbyte
    {
        Empty = 0,
        Friendly = 1,
        Enemy = -1,
        FriendlyKing = 2,
        EnemyKing = -2
    }

    public readonly RelativePiece[] Board;
    private readonly Color _currentPlayer;

    public MoveFinder(Color currentPlayer, Piece[] board)
    {
        _currentPlayer = currentPlayer;

        Board = new RelativePiece[board.Length];
        for (var index = 0; index < Checkers.BoardSize; index++)
        {
            var piece = currentPlayer switch
            {
                Color.White => board[index],
                Color.Black => board[Checkers.PlayableTiles - 1 - index],
                _ => throw new ArgumentOutOfRangeException(nameof(currentPlayer), currentPlayer,
                    null)
            };

            if (currentPlayer == Color.White)
                Board[index] = piece switch
                {
                    Piece.Empty => RelativePiece.Empty,
                    Piece.White => RelativePiece.Friendly,
                    Piece.WhiteKing => RelativePiece.FriendlyKing,
                    Piece.Black => RelativePiece.Enemy,
                    Piece.BlackKing => RelativePiece.EnemyKing,
                    _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null)
                };
            else
                Board[index] = piece switch
                {
                    Piece.Empty => RelativePiece.Empty,
                    Piece.White => RelativePiece.Enemy,
                    Piece.WhiteKing => RelativePiece.EnemyKing,
                    Piece.Black => RelativePiece.Friendly,
                    Piece.BlackKing => RelativePiece.FriendlyKing,
                    _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null)
                };
        }
    }

    private static Position ToAbsolute(Position p) => new(Checkers.PlayableTiles - 1 - p.Index);

    private Move GetMove(Position relativeFrom, Position relativeTo, Position relativeJumped)
    {
        var from = ToAbsolute(relativeFrom);
        var to = ToAbsolute(relativeTo);
        var jumped = ToAbsolute(relativeJumped);
        return new Move(_currentPlayer, from, to, jumped);
    }

    public List<Move> GetMovesFrom(Position position)
    {
        throw new NotImplementedException();
    }

    public List<Move> GetForcedMovesFrom(Position position)
    {
        throw new NotImplementedException();
    }

    private List<Move> GetMoves(bool forced)
    {
        var moves = new List<Move>();
        for (var index = 0; index < Checkers.PlayableTiles; index++)
        {
            var position = new Position(index);
            var piece = Board[index];

            switch (piece)
            {
                case RelativePiece.Empty:
                case RelativePiece.Enemy:
                case RelativePiece.EnemyKing:
                    continue;
                case RelativePiece.Friendly:
                case RelativePiece.FriendlyKing:
                    moves.AddRange(forced ? GetMovesFrom(position) : GetForcedMovesFrom(position));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return moves;
    }

    public List<Move> GetForceMoves() => GetMoves(true);

    public List<Move> GetMoves()
    {
        var forced = GetMoves(true);
        return forced.Count > 0 ? forced : GetMoves(false);
    }
}
