using System;
using System.Collections.Immutable;
using System.Linq;

namespace Checkers.Logic;

/// <summary>
///     A helper class for creation of <see cref="Game" /> class instances.
/// </summary>
public static class GameFactory
{
    private static ImmutableArray<Piece> InitialBoard => BoardFromNotation(
        "/b/b/b/b",
        "b/b/b/b/",
        "/b/b/b/b",
        " / / / /",
        "/ / / / ",
        "w/w/w/w/",
        "/w/w/w/w",
        "w/w/w/w/"
    ).ToImmutableArray();

    /// <summary>
    ///     Creates a new board with the initial setup.
    /// </summary>
    public static Game Create() => new(InitialBoard.ToArray(), Color.White);

    /// <summary>
    ///     Create a new board with the given setup.
    /// </summary>
    /// <param name="player">A player who is currently making a turn</param>
    /// <param name="boardNotation">
    ///     Strings with checkers board notation <see cref="BoardFromNotation" />
    /// </param>
    public static Game Create(Color player, params string[] boardNotation) =>
        new(BoardFromNotation(boardNotation), player);

    /// <summary>
    ///     Returns a pieces layout from the given board notation.
    /// </summary>
    /// <param name="boardNotation">
    ///     An array of 8 strings.
    ///     Each strings represents a row of the board.
    ///     Unplayable squares are represented by a '/' character.
    ///     Pieces are represented by 'b' and 'w' characters, 'B' and 'W' characters for kings.
    ///     Empty squares are represented by a space character.
    /// </param>
    private static Piece[] BoardFromNotation(params string[] boardNotation)
    {
        var board = new Piece[Game.PlayableTiles];

        if (boardNotation.Length != Game.BoardHeight)
            throw new ArgumentException($"Board must have {Game.BoardHeight} rows",
                nameof(boardNotation));

        for (var row = 0; row < Game.BoardHeight; row++)
        {
            var line = boardNotation[Game.BoardHeight - 1 - row];
            if (line.Length != Game.BoardWidth)
                throw new ArgumentException($"Row {row} must have {Game.BoardWidth} columns",
                    nameof(boardNotation));

            for (var column = 0; column < Game.BoardWidth; column++)
            {
                if ((row + column) % 2 != 0)
                {
                    if (line[column] != '/')
                        throw new ArgumentException($"Row {row} column {column} must be empty",
                            nameof(boardNotation));
                    continue;
                }

                board[new Position(row, column).Index] = line[column] switch
                {
                    ' ' => Piece.Empty,
                    'w' => Piece.White,
                    'b' => Piece.Black,
                    'W' => Piece.WhiteKing,
                    'B' => Piece.BlackKing,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        return board;
    }
}
