using System;

namespace Checkers.Logic;

public enum Color : sbyte
{
    White = 1,
    Black = -1,
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
}
