using System;
using System.Collections.Generic;

namespace Checkers.Logic;

/// <summary>
/// This class is used to find available moves for a given board state.
///
/// It uses the concept of board 'relativity'.
/// If we transpose the board and replace all black pieces with white pieces,
/// and vice versa, we can use the same algorithm to find the available moves
/// for both players.
/// </summary>
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

    /// <summary>
    /// Stores the relative board state.
    /// </summary>
    public readonly RelativePiece[] RelativeBoard;

    private readonly Color _currentPlayer;

    public MoveFinder(Color currentPlayer, Piece[] board)
    {
        _currentPlayer = currentPlayer;

        RelativeBoard = new RelativePiece[Game.PlayableTiles];
        for (var index = 0; index < Game.PlayableTiles; index++)
        {
            var piece = currentPlayer switch
            {
                Color.White => board[index],
                Color.Black => board[Game.PlayableTiles - 1 - index],
                _ => throw new ArgumentOutOfRangeException(nameof(currentPlayer), currentPlayer,
                    null)
            };

            if (currentPlayer == Color.White)
                RelativeBoard[index] = piece switch
                {
                    Piece.Empty => RelativePiece.Empty,
                    Piece.White => RelativePiece.Friendly,
                    Piece.WhiteKing => RelativePiece.FriendlyKing,
                    Piece.Black => RelativePiece.Enemy,
                    Piece.BlackKing => RelativePiece.EnemyKing,
                    _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null)
                };
            else
                RelativeBoard[index] = piece switch
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

    private RelativePosition Transform(Position p) => p.ToRelative(_currentPlayer);

    private Position Transform(RelativePosition p) => p.ToAbsolute(_currentPlayer);

    /// <summary>
    ///  A helper method to get a move given relative start and end positions.
    /// </summary>
    private Move GetMove(
        RelativePosition relFrom,
        RelativePosition relTo,
        RelativePosition? relJumped
    )
    {
        var from = Transform(relFrom);
        var to = Transform(relTo);
        var jumped = relJumped == null ? null : Transform(relJumped);
        return new Move(_currentPlayer, from, to, jumped);
    }

    /// <summary>
    /// Returns a position if it is on the board, otherwise null.
    /// It does not check if the position is correct
    /// </summary>
    private static RelativePosition? TryGetPosition(int row, int col)
    {
        if (row is < 0 or >= Game.BoardHeight || col is < 0 or >= Game.BoardWidth)
            return null;

        return new RelativePosition(row, col);
    }

    private bool IsEmpty(RelativePosition? position) =>
        position != null && RelativeBoard[position.Index] == RelativePiece.Empty;

    private bool IsFriend(RelativePosition? position) =>
        position != null &&
        RelativeBoard[position.Index] is RelativePiece.Friendly or RelativePiece.FriendlyKing;

    private bool IsEnemy(RelativePosition? position) =>
        position != null &&
        RelativeBoard[position.Index] is RelativePiece.Enemy or RelativePiece.EnemyKing;

    /// <summary>
    /// Returns a list of available non-forced moves for a given piece.
    /// It assumes that the piece in the given position is not a king and friendly.
    /// </summary>
    private List<Move> GetNormalMovesOfPiece(RelativePosition pos)
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

    /// <summary>
    /// Returns a list of available forced moves for a given piece.
    /// It assumes that the piece in the given position is not a king and friendly.
    /// </summary>
    private List<Move> GetForcedMovesOfPiece(RelativePosition pos)
    {
        var result = new List<Move>();

        var optionLeft = TryGetPosition(pos.Row + 2, pos.Column - 2);
        var jumpedLeft = TryGetPosition(pos.Row + 1, pos.Column - 1);
        if (IsEmpty(optionLeft) && IsEnemy(jumpedLeft))
            result.Add(GetMove(pos, optionLeft!, jumpedLeft));

        var optionRight = TryGetPosition(pos.Row + 2, pos.Column + 2);
        var jumpedRight = TryGetPosition(pos.Row + 1, pos.Column + 1);
        if (IsEmpty(optionRight) && IsEnemy(jumpedRight))
            result.Add(GetMove(pos, optionRight!, jumpedRight));

        var optionBackLeft = TryGetPosition(pos.Row - 2, pos.Column - 2);
        var jumpedBackLeft = TryGetPosition(pos.Row - 1, pos.Column - 1);
        if (IsEmpty(optionBackLeft) && IsEnemy(jumpedBackLeft))
            result.Add(GetMove(pos, optionBackLeft!, jumpedBackLeft));

        var optionBackRight = TryGetPosition(pos.Row - 2, pos.Column + 2);
        var jumpedBackRight = TryGetPosition(pos.Row - 1, pos.Column + 1);
        if (IsEmpty(optionBackRight) && IsEnemy(jumpedBackRight))
            result.Add(GetMove(pos, optionBackRight!, jumpedBackRight));


        return result;
    }

    /// <summary>
    /// Returns a list of available non-forced moves for a given piece.
    /// It assumes that the piece in the given position is a king and friendly.
    /// </summary>
    private List<Move> GetNormalMovesOfKing(RelativePosition relativePosition)
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

    /// <summary>
    /// Returns a list of available forced moves for a given piece.
    /// It assumes that the piece in the given position is a king and friendly.
    /// </summary>
    private List<Move> GetForcedMovesOfKing(RelativePosition relativePosition)
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

    /// <summary>
    /// Given a position, returns a list of available non-forced moves from it.
    /// Works for both kings and normal pieces.
    /// If the piece in given position is not friendly, throws ArgumentException.
    /// </summary>
    public List<Move> GetMovesFrom(Position absolutePosition)
    {
        var position = Transform(absolutePosition);
        if (!IsFriend(position))
            throw new ArgumentException("Piece must be friendly", nameof(absolutePosition));

        return RelativeBoard[position.Index] switch
        {
            RelativePiece.Friendly => GetNormalMovesOfPiece(position),
            RelativePiece.FriendlyKing => GetNormalMovesOfKing(position),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Given a position, returns a list of available forced moves from it.
    /// Works for both kings and normal pieces.
    /// If the piece in given position is not friendly, throws ArgumentException.
    /// </summary>
    public List<Move> GetForcedMovesFrom(Position position)
    {
        var pos = Transform(position);
        if (!IsFriend(pos))
            throw new ArgumentException("Piece must be friendly", nameof(position));

        return RelativeBoard[pos.Index] switch
        {
            RelativePiece.Friendly => GetForcedMovesOfPiece(pos),
            RelativePiece.FriendlyKing => GetForcedMovesOfKing(pos),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Returns a list of all available moves.
    /// </summary>
    /// <param name="forced">Whether method should find non-forced or forced moves only</param>
    private List<Move> GetMoves(bool forced)
    {
        var moves = new List<Move>();
        for (var index = 0; index < Game.PlayableTiles; index++)
        {
            var relativePosition = new RelativePosition(_currentPlayer, index);
            var absolutePosition = new Position(index);
            var piece = RelativeBoard[relativePosition.Index];

            switch (piece)
            {
                case RelativePiece.Empty:
                case RelativePiece.Enemy:
                case RelativePiece.EnemyKing:
                    continue;
                case RelativePiece.Friendly:
                case RelativePiece.FriendlyKing:
                    moves.AddRange(forced
                        ? GetForcedMovesFrom(absolutePosition)
                        : GetMovesFrom(absolutePosition));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return moves;
    }

    /// <summary>
    /// Returns all available forced moves.
    /// </summary>
    public List<Move> GetForcedMoves() => GetMoves(true);

    /// <summary>
    /// Returns all available moves, both forced and non-forced.
    /// It also checks if there are any forced moves available.
    /// </summary>
    public List<Move> GetMoves()
    {
        var forced = GetMoves(true);
        return forced.Count > 0 ? forced : GetMoves(false);
    }
}
