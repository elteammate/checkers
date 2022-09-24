using Checkers.Logic;
using NUnit.Framework;

namespace Checkers.Tests;

public class AvailableMovesTest
{
    [Test]
    public void MoveFinderOnePiece_EmptyTile_ThrowsIllegalArgument()
    {
        var game = GameFactory.Create();

        Assert.Throws<ArgumentException>(
            () => game.MoveFinder.GetMovesFrom(new Position(3, 1)));
        Assert.Throws<ArgumentException>(
            () => game.MoveFinder.GetMovesFrom(new Position(4, 4)));
        Assert.Throws<ArgumentException>(
            () => game.MoveFinder.GetMovesFrom(new Position(3, 7)));

        Assert.Throws<ArgumentException>(
            () => game.MoveFinder.GetForcedMovesFrom(new Position(3, 1)));
        Assert.Throws<ArgumentException>(
            () => game.MoveFinder.GetForcedMovesFrom(new Position(4, 4)));
        Assert.Throws<ArgumentException>(
            () => game.MoveFinder.GetForcedMovesFrom(new Position(3, 7)));
    }

    [Test]
    public void MoveFinderOnePiece_WrongColor_ThrowsIllegalArgument()
    {
        var game = GameFactory.Create();

        Assert.Throws<ArgumentException>(
            () => game.MoveFinder.GetMovesFrom(new Position(7, 1)));
        Assert.Throws<ArgumentException>(
            () => game.MoveFinder.GetMovesFrom(new Position(5, 7)));
        Assert.Throws<ArgumentException>(
            () => game.MoveFinder.GetMovesFrom(new Position(6, 4)));

        Assert.Throws<ArgumentException>(
            () => game.MoveFinder.GetForcedMovesFrom(new Position(7, 1)));
        Assert.Throws<ArgumentException>(
            () => game.MoveFinder.GetForcedMovesFrom(new Position(5, 7)));
        Assert.Throws<ArgumentException>(
            () => game.MoveFinder.GetForcedMovesFrom(new Position(6, 4)));
    }

    [Test]
    public void MoveFinderOnePiece_Blocked_ReturnsEmptyList()
    {
        var game = GameFactory.Create(
            Color.White,
            "/ /w/ / ",
            " / / / /",
            "/w/ / / ",
            "w/ /w/w/",
            "/ / /w/ ",
            " / / /w/",
            "/ / / /w",
            " / / / /");

        Assert.IsEmpty(game.MoveFinder.GetMovesFrom(new Position(7, 3)));
        Assert.IsEmpty(game.MoveFinder.GetMovesFrom(new Position(4, 0)));
        Assert.IsEmpty(game.MoveFinder.GetMovesFrom(new Position(1, 7)));

        Assert.IsEmpty(game.MoveFinder.GetForcedMovesFrom(new Position(7, 3)));
        Assert.IsEmpty(game.MoveFinder.GetForcedMovesFrom(new Position(4, 0)));
        Assert.IsEmpty(game.MoveFinder.GetForcedMovesFrom(new Position(1, 7)));
    }

    [Test]
    public void MoveFinderOnePieceForced_NoMove_ReturnsEmptyList()
    {
        var game = GameFactory.Create(
            Color.White,
            "/ /w/ / ",
            " / / / /",
            "/w/ / / ",
            "w/ /w/w/",
            "/ / /w/ ",
            " / / /w/",
            "/ / / /w",
            " / / / /");

        Assert.IsEmpty(game.MoveFinder.GetForcedMovesFrom(new Position(7, 3)));
        Assert.IsEmpty(game.MoveFinder.GetForcedMovesFrom(new Position(4, 0)));
        Assert.IsEmpty(game.MoveFinder.GetForcedMovesFrom(new Position(3, 5)));
        Assert.IsEmpty(game.MoveFinder.GetForcedMovesFrom(new Position(1, 7)));
    }

    [Test]
    public void MoveFinderOnePiece_OneMoveAvailable_ReturnsCorrectList()
    {
        var game = GameFactory.Create(
            Color.White,
            "/ / / / ",
            " / / / /",
            "/ / / / ",
            "w/ /w/ /",
            "/ / /w/ ",
            " / / / /",
            "/ / / /w",
            " / / / /");

        var moves = game.MoveFinder.GetMovesFrom(new Position(4, 0))
            .Select(m => m.To).ToList();
        Assert.AreEqual(1, moves.Count);
        Assert.AreEqual(new Position(5, 1), moves[0]);

        moves = game.MoveFinder.GetMovesFrom(new Position(3, 5))
            .Select(m => m.To).ToList();
        Assert.AreEqual(1, moves.Count);
        Assert.AreEqual(new Position(4, 6), moves[0]);

        moves = game.MoveFinder.GetMovesFrom(new Position(1, 7))
            .Select(m => m.To).ToList();
        Assert.AreEqual(1, moves.Count);
        Assert.AreEqual(new Position(2, 6), moves[0]);
    }

    [Test]
    public void MoveFinderOnePiece_MultipleMovesAvailable_ReturnsCorrectList()
    {
        var game = GameFactory.Create(
            Color.White,
            "/ / / / ",
            " / / / /",
            "/ / / / ",
            " / /w/ /",
            "/ / / / ",
            " / / / /",
            "/ /w/ / ",
            " / / / /");

        var moves = game.MoveFinder.GetMovesFrom(new Position(4, 4))
            .Select(m => m.To).ToList();
        Assert.AreEqual(2, moves.Count);
        Assert.Contains(new Position(5, 3), moves);
        Assert.Contains(new Position(5, 5), moves);

        moves = game.MoveFinder.GetMovesFrom(new Position(1, 3))
            .Select(m => m.To).ToList();
        Assert.AreEqual(2, moves.Count);
        Assert.Contains(new Position(2, 2), moves);
        Assert.Contains(new Position(2, 4), moves);
    }

    [Test]
    public void MoveFinderOnePiece_ForcedForward_ReturnsCorrectList()
    {
        var game = GameFactory.Create(
            Color.White,
            "/ / /b/b",
            "b/b/ / /",
            "/w/ /b/ ",
            " / /w/ /",
            "/ / / / ",
            " /b/w/ /",
            "/ /w/b/ ",
            " / / /w/");

        var moves = game.MoveFinder.GetForcedMovesFrom(new Position(4, 4))
            .Select(m => m.To).ToList();
        Assert.AreEqual(1, moves.Count);
        Assert.Contains(new Position(6, 6), moves);

        moves = game.MoveFinder.GetForcedMovesFrom(new Position(1, 3))
            .Select(m => m.To).ToList();
        Assert.AreEqual(1, moves.Count);
        Assert.Contains(new Position(3, 1), moves);

        moves = game.MoveFinder.GetForcedMovesFrom(new Position(5, 1))
            .Select(m => m.To).ToList();
        Assert.AreEqual(1, moves.Count);
        Assert.Contains(new Position(7, 3), moves);

        Assert.IsEmpty(game.MoveFinder.GetForcedMovesFrom(new Position(0, 6)));
    }

    [Test]
    public void MoveFinderOnePiece_ForcedBackwards_ReturnsCorrectList()
    {
        var game = GameFactory.Create(
            Color.White,
            "/ / /w/ ",
            " /w/ /b/",
            "/b/b/ / ",
            " / / /b/",
            "/ / /w/ ",
            " / /b/b/",
            "/ /w/ /b",
            " /b/b/ /");

        var moves = game.MoveFinder.GetForcedMovesFrom(new Position(7, 5))
            .Select(m => m.To).ToList();
        Assert.AreEqual(1, moves.Count);
        Assert.Contains(new Position(5, 7), moves);

        moves = game.MoveFinder.GetForcedMovesFrom(new Position(6, 2))
            .Select(m => m.To).ToList();
        Assert.AreEqual(2, moves.Count);
        Assert.Contains(new Position(4, 0), moves);
        Assert.Contains(new Position(4, 4), moves);

        moves = game.MoveFinder.GetForcedMovesFrom(new Position(3, 5))
            .Select(m => m.To).ToList();
        Assert.AreEqual(1, moves.Count);
        Assert.Contains(new Position(5, 7), moves);

        Assert.IsEmpty(game.MoveFinder.GetForcedMovesFrom(new Position(1, 3)));
    }

    [Test]
    public void MoveFinderOneKing_NotForced_ReturnsCorrectList()
    {
        var game = GameFactory.Create(
            Color.White,
            "/ / / /b",
            " / / / /",
            "/w/ / / ",
            " / / / /",
            "/ /W/ / ",
            " / /w/ /",
            "/ / / / ",
            " / / / /");

        var moves = game.MoveFinder.GetMovesFrom(new Position(3, 3))
            .Select(m => m.To).ToList();
        Assert.AreEqual(7, moves.Count);
        Assert.Contains(new Position(5, 5), moves);
        Assert.Contains(new Position(6, 6), moves);
        Assert.Contains(new Position(4, 4), moves);
        Assert.Contains(new Position(2, 2), moves);
        Assert.Contains(new Position(1, 1), moves);
        Assert.Contains(new Position(0, 0), moves);
        Assert.Contains(new Position(4, 2), moves);
    }

    [Test]
    public void MoveFinderOneKing_Forced_ReturnsCorrectList()
    {
        var game = GameFactory.Create(
            Color.White,
            "/ / / /b",
            " / / /b/",
            "/b/ / / ",
            " / / / /",
            "/ /W/ / ",
            " / /B/ /",
            "/b/ / / ",
            "w/ / / /");

        var moves = game.MoveFinder.GetForcedMovesFrom(new Position(3, 3))
            .Select(m => m.To).ToList();
        Assert.AreEqual(2, moves.Count);
        Assert.Contains(new Position(6, 0), moves);
        Assert.Contains(new Position(1, 5), moves);
    }
}
