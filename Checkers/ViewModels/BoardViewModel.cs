using System.Collections.Generic;
using System.Collections.ObjectModel;
using Checkers.Logic;

namespace Checkers.ViewModels;

public class BoardViewModel : ViewModelBase
{
    public Game Game => GameFactory.Create();
    public ObservableCollection<KeyValuePair<Position, Piece>> Pieces { get; } = new();

    public BoardViewModel()
    {
        for (var index = 0; index < Game.PlayableTiles; index++)
        {
            if (Game.Board[index] == Piece.Empty) continue;
            Pieces.Add(new KeyValuePair<Position, Piece>(new Position(index), Game.Board[index]));
        }
    }
}
