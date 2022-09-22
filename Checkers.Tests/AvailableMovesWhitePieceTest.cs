using Checkers.Logic;
using NUnit.Framework;

namespace Checkers.Tests;

public class AvailableMovesTest
{
    [Test]
    public void WhitePiece_EmptyTile_ThrowsIllegalArgument()
    {
        var game = GameFactory.Create();

        Assert.Throws<ArgumentException>(
            () => game.GetAllowedMoves(new Position(3, 1), false));
        Assert.Throws<ArgumentException>(
            () => game.GetAllowedMoves(new Position(4, 4), true));
        Assert.Throws<ArgumentException>(
            () => game.GetAllowedMoves(new Position(3, 7), false));
    }

    [Test]
    public void WhitePiece_WrongColor_ThrowsIllegalArgument()
    {
        var game = GameFactory.Create();

        Assert.Throws<ArgumentException>(
            () => game.GetAllowedMoves(new Position(7, 1), false));
        Assert.Throws<ArgumentException>(
            () => game.GetAllowedMoves(new Position(5, 7), true));
        Assert.Throws<ArgumentException>(
            () => game.GetAllowedMoves(new Position(6, 4), false));
    }

    [Test]
    public void WhitePiece_Blocked_ReturnsEmptyList()
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

        Assert.IsEmpty(game.GetAllowedMoves(new Position(7, 3), false));
        Assert.IsEmpty(game.GetAllowedMoves(new Position(4, 0), false));
        Assert.IsEmpty(game.GetAllowedMoves(new Position(1, 7), false));

        Assert.IsEmpty(game.GetAllowedMoves(new Position(7, 3), true));
        Assert.IsEmpty(game.GetAllowedMoves(new Position(4, 0), true));
        Assert.IsEmpty(game.GetAllowedMoves(new Position(1, 7), true));
    }

    [Test]
    public void WhitePieceForced_NoMove_ReturnsEmptyList()
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

        Assert.IsEmpty(game.GetAllowedMoves(new Position(5, 2), true));
        Assert.IsEmpty(game.GetAllowedMoves(new Position(4, 4), true));
        Assert.IsEmpty(game.GetAllowedMoves(new Position(4, 6), true));
        Assert.IsEmpty(game.GetAllowedMoves(new Position(3, 6), true));
    }

    [Test]
    public void WhitePiece_OneMoveAvailable_ReturnsCorrectList()
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

        var moves = game.GetAllowedMoves(new Position(4, 0), false);
        Assert.AreEqual(1, moves.Count);
        Assert.AreEqual(new Position(5, 1), moves[0]);

        moves = game.GetAllowedMoves(new Position(3, 5), false);
        Assert.AreEqual(1, moves.Count);
        Assert.AreEqual(new Position(4, 6), moves[0]);

        moves = game.GetAllowedMoves(new Position(1, 7), false);
        Assert.AreEqual(1, moves.Count);
        Assert.AreEqual(new Position(2, 6), moves[0]);
    }

    [Test]
    public void WhitePiece_MultipleMovesAvailable_ReturnsCorrectList()
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

        var moves = game.GetAllowedMoves(new Position(4, 4), false);
        Assert.AreEqual(2, moves.Count);
        Assert.Contains(new Position(5, 3), moves);
        Assert.Contains(new Position(5, 5), moves);

        moves = game.GetAllowedMoves(new Position(1, 3), false);
        Assert.AreEqual(2, moves.Count);
        Assert.Contains(new Position(2, 2), moves);
        Assert.Contains(new Position(2, 4), moves);
    }

    [Test]
    public void WhitePiece_Forced_ReturnsCorrectList()
    {
        var game = GameFactory.Create(
            Color.White,
            "/ / / / ",
            "b/b/ / /",
            "/w/ /b/ ",
            " / /w/ /",
            "/ / / / ",
            " /b/w/ /",
            "/ /w/b/ ",
            " / / /w/");

        var moves = game.GetAllowedMoves(new Position(4, 4), true);
        Assert.AreEqual(1, moves.Count);
        Assert.AreEqual(new Position(6, 6), moves[0]);

        moves = game.GetAllowedMoves(new Position(1, 3), true);
        Assert.AreEqual(1, moves.Count);
        Assert.AreEqual(new Position(3, 1), moves[0]);

        moves = game.GetAllowedMoves(new Position(5, 2), true);
        Assert.AreEqual(2, moves.Count);
        Assert.Contains(new Position(7, 0), moves);
        Assert.Contains(new Position(7, 4), moves);

        Assert.IsEmpty(game.GetAllowedMoves(new Position(0, 6), true));
    }

    [Test]
    public void SingleKing_NotForced_ReturnsCorrectList()
    {
        var game = GameFactory.Create(
            Color.White,
            "/ / /b/ ",
            " / / / /",
            "/w/ / / ",
            " / / / /",
            "/ /W/ / ",
            " / /w/ /",
            "/ / / / ",
            " / / / /");

        var moves = game.GetAllowedMoves(new Position(3, 3), false);
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
    public void SingleKing_Forced_ReturnsCorrectList()
    {
        var game = GameFactory.Create(
            Color.White,
            "/ / /b/ ",
            " / / /b/",
            "/b/ / / ",
            " / / / /",
            "/ /W/ / ",
            " / /B/ /",
            "/b/ / / ",
            "w/ / / /");

        var moves = game.GetAllowedMoves(new Position(3, 3), true);
        Assert.AreEqual(2, moves.Count);
        Assert.Contains(new Position(6, 0), moves);
        Assert.Contains(new Position(1, 5), moves);
    }
}
