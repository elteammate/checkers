using System;

namespace Checkers.Logic;

public enum Color : sbyte
{
    White = 1,
    Black = -1
}

public static class ColorExtensions
{
    public static Color Opposite(this Color color) =>
        color == Color.White ? Color.Black : Color.White;
}

public enum Piece : sbyte
{
    Empty = 0,
    White = 1,
    Black = -1,
    WhiteKing = 2,
    BlackKing = -2
}

public static class PieceExtensions
{
    public static Piece Promote(this Piece piece)
    {
        return piece switch
        {
            Piece.White => Piece.WhiteKing,
            Piece.Black => Piece.BlackKing,
            _ => piece
        };
    }

    public static Color? GetColor(this Piece piece)
    {
        return piece switch
        {
            Piece.White => Color.White,
            Piece.Black => Color.Black,
            Piece.WhiteKing => Color.White,
            Piece.BlackKing => Color.Black,
            Piece.Empty => null,
            _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null)
        };
    }

    public static RelativePiece ToRelative(this Piece piece, Color color)
    {
        if (color == Color.White)
        {
            return piece switch
            {
                Piece.Empty => RelativePiece.Empty,
                Piece.White => RelativePiece.Friendly,
                Piece.Black => RelativePiece.Enemy,
                Piece.WhiteKing => RelativePiece.FriendlyKing,
                Piece.BlackKing => RelativePiece.EnemyKing,
                _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null)
            };
        }

        return piece switch
        {
            Piece.Empty => RelativePiece.Empty,
            Piece.White => RelativePiece.Enemy,
            Piece.Black => RelativePiece.Friendly,
            Piece.WhiteKing => RelativePiece.EnemyKing,
            Piece.BlackKing => RelativePiece.FriendlyKing,
            _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null)
        };
    }
}

public enum RelativePiece : sbyte
{
    Empty = 0,
    Friendly = 1,
    Enemy = -1,
    FriendlyKing = 2,
    EnemyKing = -2
}

public static class RelativePieceExtensions
{
    public static Piece ToAbsolute(this RelativePiece piece, Color color)
    {
        if (color == Color.White)
        {
            return piece switch
            {
                RelativePiece.Empty => Piece.Empty,
                RelativePiece.Friendly => Piece.White,
                RelativePiece.Enemy => Piece.Black,
                RelativePiece.FriendlyKing => Piece.WhiteKing,
                RelativePiece.EnemyKing => Piece.BlackKing,
                _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null)
            };
        }

        return piece switch
        {
            RelativePiece.Empty => Piece.Empty,
            RelativePiece.Friendly => Piece.Black,
            RelativePiece.Enemy => Piece.White,
            RelativePiece.FriendlyKing => Piece.BlackKing,
            RelativePiece.EnemyKing => Piece.WhiteKing,
            _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null)
        };
    }
}
