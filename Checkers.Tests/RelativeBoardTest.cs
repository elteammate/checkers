using Checkers.Logic;
using NUnit.Framework;

namespace Checkers.Tests;

public class RelativeBoardTest
{
    [Test]
    public void MoveFinderBoard_WhenInitialized_ShouldHaveCorrectNumberOfPieces()
    {
        var board = GameFactory.Create().MoveFinder.RelativeBoard;
        var friendly = Array.FindAll(board, x => x == MoveFinder.RelativePiece.Friendly).Length;
        var enemy = Array.FindAll(board, x => x == MoveFinder.RelativePiece.Enemy).Length;
        var empty = Array.FindAll(board, x => x == MoveFinder.RelativePiece.Empty).Length;
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
            MoveFinder.RelativePiece.Friendly, MoveFinder.RelativePiece.EnemyKing,
            MoveFinder.RelativePiece.Friendly, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Enemy,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.EnemyKing,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.FriendlyKing,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.Enemy, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Friendly,
            MoveFinder.RelativePiece.EnemyKing, MoveFinder.RelativePiece.Friendly,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.EnemyKing, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.FriendlyKing, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Enemy,
            MoveFinder.RelativePiece.Enemy, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Friendly,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Empty,
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
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.Enemy, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Friendly,
            MoveFinder.RelativePiece.Friendly, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.EnemyKing,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.FriendlyKing,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.Enemy, MoveFinder.RelativePiece.FriendlyKing,
            MoveFinder.RelativePiece.Enemy, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Friendly,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.EnemyKing, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.FriendlyKing, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.Friendly, MoveFinder.RelativePiece.Empty,
            MoveFinder.RelativePiece.Empty, MoveFinder.RelativePiece.Enemy,
            MoveFinder.RelativePiece.FriendlyKing, MoveFinder.RelativePiece.Enemy
        }, board);
    }
}
