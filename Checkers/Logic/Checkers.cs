using System;
using System.Collections.Generic;

namespace Checkers.Logic;

public class Checkers
{
    public const int BoardSize = 8;
    public const int PlayableTiles = BoardSize * BoardSize / 2;

    public enum GameResult
    {
        None,
        WhiteWins,
        BlackWins,
        Draw
    }

    public record Move(Color Player, int From, int To);

    private Piece[] _board;
    public Color CurrentPlayer { get; private set; }
    public GameResult Result { get; private set; } = GameResult.None;
    private List<Move> _movesLog = new();
    public IReadOnlyList<Move> MovesLog => _movesLog;

    public Checkers(Piece[] initialBoard, Color firstPlayer)
    {
        if (initialBoard.Length != PlayableTiles)
            throw new ArgumentException("Board must have 32 tiles", nameof(initialBoard));

        CurrentPlayer = firstPlayer;
        _board = initialBoard;
    }

    public List<Move> GetAllowedMoves(Position position, bool forced)
    {
        throw new NotImplementedException();
    }

    public List<Move> GetAllowedMoves()
    {
        throw new NotImplementedException();
    }

    public void MakeMove(Move move)
    {
        throw new NotImplementedException();
    }
}
