using System;

namespace Checkers.Logic;

/// <summary>
///     This class represents a position on a checkers board.
///     The usual notation is used, with the origin at the top left.
///     The tile 0 is the top left corner and is occupied by the black
///     piece at the start of the game.
///     <code>
///  >      col 0  1  2  3  4  5  6  7
///  >   row   
///  >   7    __  0 __  1 __  2 __  3
///  >   6     4 __  5 __  6 __  7 __
///  >   5    __  8 __  9 __ 10 __ 11
///  >   4    12 __ 13 __ 14 __ 15 __
///  >   3    __ 16 __ 17 __ 18 __ 19
///  >   2    20 __ 21 __ 22 __ 23 __
///  >   1    __ 24 __ 25 __ 26 __ 27
///  >   0    28 __ 29 __ 30 __ 31 __
/// </code>
/// </summary>
public class Position
{
    public Position(int index)
    {
        if (index is < 0 or > 31)
            throw new ArgumentOutOfRangeException(nameof(index));
        Index = index;
    }

    public Position(int row, int column) : this((7 - row) * 4 + column / 2)
    {
        if (column is < 0 or > 7)
            throw new ArgumentOutOfRangeException(nameof(column));

        if (row is < 0 or > 7)
            throw new ArgumentOutOfRangeException(nameof(row));

        if ((row + column) % 2 != 0)
            throw new ArgumentException("Invalid position");
    }

    public int Row => Game.BoardHeight - 1 - Index / (Game.BoardWidth / 2);

    public int Column =>
        Index % (Game.BoardWidth / 2) * 2 + Row % 2;

    public int Index { get; }

    public RelativePosition ToRelative(Color player) => new(player, Index);

    public static bool operator ==(Position? left, Position? right) => left?.Index == right?.Index;
    public static bool operator !=(Position? left, Position? right) => left?.Index != right?.Index;

    public override bool Equals(object obj) => obj is Position position && Index == position.Index;
    public override int GetHashCode() => Index;

    public override string ToString() => $"({Row}, {Column})";
}

/// <summary>
///     This class is used by <see cref="MoveFinder" /> to
///     represent a position on the oriented board.
///     The position with index 0 is the top left corner of the board if
///     current player is white and the bottom right corner if current player
///     is black.
/// </summary>
public class RelativePosition
{
    private readonly Position _position;

    public RelativePosition(int index)
    {
        _position = new Position(index);
    }

    public RelativePosition(Color player, int index)
    {
        _position = new Position(player == Color.White ? index : Game.PlayableTiles - 1 - index);
    }

    public RelativePosition(int row, int column)
    {
        _position = new Position(row, column);
    }

    public RelativePosition(Color player, int row, int column)
    {
        if (player == Color.Black)
            _position = new Position(Game.BoardHeight - 1 - row, Game.BoardWidth - 1 - column);
        else
            _position = new Position(row, column);
    }

    public int Index => _position.Index;
    public int Row => _position.Row;
    public int Column => _position.Column;

    public Position ToAbsolute(Color color) =>
        new(color == Color.White
            ? _position.Index
            : Game.PlayableTiles - 1 - _position.Index);

    public override string ToString() => $"({Row}, {Column})";
}
