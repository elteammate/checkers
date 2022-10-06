using Checkers.Logic;
using NUnit.Framework;

namespace Checkers.Tests;

public class RelativeBoardTest
{
    [Test]
    public void MoveFinderBoard_WhenInitialized_ShouldHaveCorrectNumberOfPieces()
    {
        var board = GameFactory.Create().MoveFinder.RelativeBoard;
        var friendly = Array.FindAll(board, x => x == RelativePiece.Friendly).Length;
        var enemy = Array.FindAll(board, x => x == RelativePiece.Enemy).Length;
        var empty = Array.FindAll(board, x => x == RelativePiece.Empty).Length;
        Assert.AreEqual(12, friendly);
        Assert.AreEqual(12, enemy);
        Assert.AreEqual(8, empty);
    }

    [Test]
    public void MoveFinderBoard_TranslatesWhitePlayerBoard_HasCorrectRelativeBoard()
    {
        var board = GameFactory.Create(Color.White,
            "/w/B/w/ ",
            " /b/ /B/",
            "/ /W/ / ",
            "b/ / /w/",
            "/B/w/ / ",
            "B/ /W/ /",
            "/ /b/b/ ",
            " /w/ / /").MoveFinder.RelativeBoard;

        Assert.AreEqual(new[]
        {
            RelativePiece.Friendly, RelativePiece.EnemyKing,
            RelativePiece.Friendly, RelativePiece.Empty,
            RelativePiece.Empty, RelativePiece.Enemy,
            RelativePiece.Empty, RelativePiece.EnemyKing,
            RelativePiece.Empty, RelativePiece.FriendlyKing,
            RelativePiece.Empty, RelativePiece.Empty,
            RelativePiece.Enemy, RelativePiece.Empty,
            RelativePiece.Empty, RelativePiece.Friendly,
            RelativePiece.EnemyKing, RelativePiece.Friendly,
            RelativePiece.Empty, RelativePiece.Empty,
            RelativePiece.EnemyKing, RelativePiece.Empty,
            RelativePiece.FriendlyKing, RelativePiece.Empty,
            RelativePiece.Empty, RelativePiece.Enemy,
            RelativePiece.Enemy, RelativePiece.Empty,
            RelativePiece.Empty, RelativePiece.Friendly,
            RelativePiece.Empty, RelativePiece.Empty
        }, board);
    }

    [Test]
    public void MoveFinderBoard_TranslatesBlackPlayerBoard_HasCorrectRelativeBoard()
    {
        var board = GameFactory.Create(Color.Black,
            "/w/B/w/ ",
            " /b/ /B/",
            "/ /W/ / ",
            "b/ / /w/",
            "/B/w/ / ",
            "B/ /W/ /",
            "/ /b/b/ ",
            " /w/ / /").MoveFinder.RelativeBoard;

        Assert.AreEqual(new[]
        {
            RelativePiece.Empty, RelativePiece.Empty,
            RelativePiece.Enemy, RelativePiece.Empty,
            RelativePiece.Empty, RelativePiece.Friendly,
            RelativePiece.Friendly, RelativePiece.Empty,
            RelativePiece.Empty, RelativePiece.EnemyKing,
            RelativePiece.Empty, RelativePiece.FriendlyKing,
            RelativePiece.Empty, RelativePiece.Empty,
            RelativePiece.Enemy, RelativePiece.FriendlyKing,
            RelativePiece.Enemy, RelativePiece.Empty,
            RelativePiece.Empty, RelativePiece.Friendly,
            RelativePiece.Empty, RelativePiece.Empty,
            RelativePiece.EnemyKing, RelativePiece.Empty,
            RelativePiece.FriendlyKing, RelativePiece.Empty,
            RelativePiece.Friendly, RelativePiece.Empty,
            RelativePiece.Empty, RelativePiece.Enemy,
            RelativePiece.FriendlyKing, RelativePiece.Enemy
        }, board);
    }
}
