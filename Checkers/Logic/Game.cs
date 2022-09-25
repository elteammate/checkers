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

    public void MakeMove(Move move)
    {
        throw new NotImplementedException();
    }
}
