using System;

namespace Checkers.Logic;

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
