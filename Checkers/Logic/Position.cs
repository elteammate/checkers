using System;

namespace Checkers.Logic;

/**
    This class represents a position on a checkers board.
    The usual notation is used, with the origin at the top left.
    <code>
      __  0 __  1 __  2 __  3
       4 __  5 __  6 __  7 __
      __  8 __  9 __ 10 __ 11
      12 __ 13 __ 14 __ 15 __
      __ 16 __ 17 __ 18 __ 19
      20 __ 21 __ 22 __ 23 __
      __ 24 __ 25 __ 26 __ 27
      28 __ 29 __ 30 __ 31 __
    </code>
    The tile 0 is the top left corner and is occupied by the black 
    piece at the start of the game.
 */
public record Position(int Index)
{
    public int Row => 7 - Index / 4;
    public int Column => Index % 4 * 2 + 1 - Index / 4 % 2;

    public Position(int row, int column) : this((7 - row) * 4 + column / 2)
    {
        if (row + column % 2 != 1)
            throw new ArgumentException("Invalid position");
    }
}
