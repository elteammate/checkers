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

        Board = new RelativePiece[Checkers.PlayableTiles];
        for (var index = 0; index < Checkers.PlayableTiles; index++)
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

    private RelativePosition Transform(Position p)
    {
        return _currentPlayer == Color.White
            ? new RelativePosition(p.Index)
            : new RelativePosition(Checkers.PlayableTiles - p.Index);
    }

    private Position Transform(RelativePosition p)
    {
        return _currentPlayer == Color.White
            ? new Position(p.Index)
            : new Position(Checkers.PlayableTiles - p.Index);
    }

    private Move GetMove(RelativePosition From, RelativePosition To, RelativePosition? Jumped)
    {
        var from = Transform(From);
        var to = Transform(To);
        var jumped = Jumped == null ? null : Transform(Jumped);
        return new Move(_currentPlayer, from, to, jumped);
    }

    private static RelativePosition? TryGetPosition(int row, int col)
    {
        if (row is < 0 or >= Checkers.BoardSize || col is < 0 or >= Checkers.BoardSize)
            return null;

        return new RelativePosition(row, col);
    }

    private bool IsEmpty(RelativePosition? position) =>
        position != null && Board[position.Index] == RelativePiece.Empty;

    private bool IsFriend(RelativePosition? position) =>
        position != null &&
        Board[position.Index] is RelativePiece.Friendly or RelativePiece.FriendlyKing;

    private bool IsEnemy(RelativePosition? position) =>
        position != null &&
        Board[position.Index] is RelativePiece.Enemy or RelativePiece.EnemyKing;

    private List<Move> GetMovesFromNormalPiece(RelativePosition pos)
    {
        var optionLeft = TryGetPosition(pos.Row + 1, pos.Column - 1);
        var optionRight = TryGetPosition(pos.Row + 1, pos.Column + 1);

        var result = new List<Move>();
        if (IsEmpty(optionLeft))
            result.Add(GetMove(pos, optionLeft!, null));
        if (IsEmpty(optionRight))
            result.Add(GetMove(pos, optionRight!, null));
        return result;
    }

    private List<Move> GetForcedMovesFromNormalPiece(RelativePosition relativePosition)
    {
        var result = new List<Move>();

        var optionLeft = TryGetPosition(relativePosition.Row + 2, relativePosition.Column - 2);
        var jumpedLeft = TryGetPosition(relativePosition.Row + 1, relativePosition.Column - 1);
        if (IsEmpty(optionLeft) && IsEnemy(jumpedLeft))
            result.Add(GetMove(relativePosition, optionLeft!, jumpedLeft));

        var optionRight = TryGetPosition(relativePosition.Row + 2, relativePosition.Column + 2);
        var jumpedRight = TryGetPosition(relativePosition.Row + 1, relativePosition.Column + 1);
        if (IsEmpty(optionRight) && IsEnemy(jumpedRight))
            result.Add(GetMove(relativePosition, optionRight!, jumpedRight));

        var optionBackLeft = TryGetPosition(relativePosition.Row - 2, relativePosition.Column - 2);
        var jumpedBackLeft = TryGetPosition(relativePosition.Row - 1, relativePosition.Column - 1);
        if (IsEmpty(optionBackLeft) && IsEnemy(jumpedBackLeft))
            result.Add(GetMove(relativePosition, optionBackLeft!, jumpedBackLeft));

        var optionBackRight = TryGetPosition(relativePosition.Row - 2, relativePosition.Column + 2);
        var jumpedBackRight = TryGetPosition(relativePosition.Row - 1, relativePosition.Column + 1);
        if (IsEmpty(optionBackRight) && IsEnemy(jumpedBackRight))
            result.Add(GetMove(relativePosition, optionBackRight!, jumpedBackRight));


        return result;
    }

    private List<Move> GetMovesFromKingPiece(RelativePosition relativePosition)
    {
        var result = new List<Move>();

        bool AddToResult(int row, int col)
        {
            var pos = TryGetPosition(row, col);
            var isFree = IsEmpty(pos);
            if (isFree) result.Add(GetMove(relativePosition, pos!, null));
            return isFree;
        }

        void AddToResultWhileAble(int yDir, int xDir)
        {
            for (var i = 1;; i++)
            {
                if (!AddToResult(relativePosition.Row + yDir * i,
                        relativePosition.Column + xDir * i))
                    break;
            }
        }

        AddToResultWhileAble(1, 1);
        AddToResultWhileAble(1, -1);
        AddToResultWhileAble(-1, 1);
        AddToResultWhileAble(-1, -1);

        return result;
    }

    private List<Move> GetForcedMovesFromKingPiece(RelativePosition relativePosition)
    {
        var result = new List<Move>();

        void TryFindJump(int yDir, int xDir)
        {
            for (var i = 1;; i++)
            {
                var pos = TryGetPosition(
                    relativePosition.Row + yDir * i,
                    relativePosition.Column + xDir * i
                );

                if (pos == null || IsFriend(pos)) break;
                if (IsEmpty(pos)) continue;

                var jump = TryGetPosition(pos.Row + yDir, pos.Column + xDir);

                if (IsEnemy(pos) && IsEmpty(jump))
                    result.Add(GetMove(relativePosition, jump!, pos));
                break;
            }
        }

        TryFindJump(1, 1);
        TryFindJump(1, -1);
        TryFindJump(-1, 1);
        TryFindJump(-1, -1);

        return result;
    }

    public List<Move> GetMovesFrom(Position absolutePosition)
    {
        var position = Transform(absolutePosition);
        if (!IsFriend(position))
            throw new ArgumentException("Piece must be friendly", nameof(absolutePosition));

        return Board[position.Index] switch
        {
            RelativePiece.Friendly => GetMovesFromNormalPiece(position),
            RelativePiece.FriendlyKing => GetMovesFromKingPiece(position),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public List<Move> GetForcedMovesFrom(Position position)
    {
        var pos = Transform(position);
        if (!IsFriend(pos))
            throw new ArgumentException("Piece must be friendly", nameof(position));

        return Board[pos.Index] switch
        {
            RelativePiece.Friendly => GetForcedMovesFromNormalPiece(pos),
            RelativePiece.FriendlyKing => GetForcedMovesFromKingPiece(pos),
            _ => throw new ArgumentOutOfRangeException()
        };
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

    public List<Move> GetForcedMoves() => GetMoves(true);

    public List<Move> GetMoves()
    {
        var forced = GetMoves(true);
        return forced.Count > 0 ? forced : GetMoves(false);
    }
}
