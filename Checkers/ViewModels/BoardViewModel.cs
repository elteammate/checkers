using System;
using System.Collections.Generic;
using Checkers.Logic;

namespace Checkers.ViewModels;

public class BoardViewModel : ViewModelBase
{
    public readonly Game Game = GameFactory.Create();
    public Dictionary<Position, Piece> Pieces { get; } = new();
    public List<Move> Moves { get; private set; } = new();

    public event EventHandler<Move> PieceMoved = delegate { };

    public BoardViewModel()
    {
        Update();
    }

    public void Move(Move move)
    {
        Game.MakeMove(move);
        Update();
        PieceMoved.Invoke(this, move);
    }

    private void Update()
    {
        Pieces.Clear();
        for (var index = 0; index < Game.PlayableTiles; index++)
        {
            if (Game.Board[index] == Piece.Empty) continue;
            Pieces.Add(new Position(index), Game.Board[index]);
        }

        Moves = Game.MoveFinder.GetMoves();
    }
}
