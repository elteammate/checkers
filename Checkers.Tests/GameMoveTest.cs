using Checkers.Logic;
using NUnit.Framework;

namespace Checkers.Tests;

public class GameMoveTest
{
    [Test]
    public void Game_TestMoveSimple_PerformsCorrectMove()
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
            " / / / /"
        );

        game.MakeMove(new Move(Color.White, new Position(5, 1), new Position(6, 0)));

        Assert.AreEqual(game.Board, GameFactory.Create(
            Color.Black,
            "/ /w/ / ",
            "w/ / / /",
            "/ / / / ",
            "w/ /w/w/",
            "/ / /w/ ",
            " / / /w/",
            "/ / / /w",
            " / / / /"
        ).Board);
    }

    [Test]
    public void Game_TestMoveJump_PerformsCorrectMove()
    {
        var game = GameFactory.Create(
            Color.Black,
            "/ / / / ",
            " /b/ /b/",
            "/ / / / ",
            " / / /b/",
            "/b/ / / ",
            " / /b/ /",
            "/b/w/ /w",
            " / / / /"
        );

        game.MakeMove(new Move(Color.White, new Position(5, 1), new Position(6, 0)));

        game.MakeMove(new Move(
            Color.White,
            new Position(1, 3),
            new Position(3, 5),
            new Position(2, 4)
        ));

        game.MakeMove(new Move(
            Color.White,
            new Position(3, 5),
            new Position(5, 7),
            new Position(4, 6)
        ));

        game.MakeMove(new Move(
            Color.White,
            new Position(5, 7),
            new Position(7, 5),
            new Position(6, 6)
        ));

        game.MakeMove(new Move(
            Color.White,
            new Position(7, 5),
            new Position(2, 0),
            new Position(3, 1)
        ));

        game.MakeMove(new Move(
            Color.White,
            new Position(2, 0),
            new Position(0, 2),
            new Position(1, 1)
        ));

        Assert.AreEqual(game.Board, GameFactory.Create(
            Color.Black,
            "/ / / / ",
            " /b/ / /",
            "/ / / / ",
            " / / / /",
            "/ / / / ",
            " / / / /",
            "/ / / /w",
            " /W/ / /"
        ).Board);
    }

    [Test]
    public void Game_TestMoveWithPromotion_PerformsCorrectMove()
    {
        var game = GameFactory.Create(
            Color.Black,
            "/ / / / ",
            " / / / /",
            "/ / / / ",
            " / / / /",
            "/ / / / ",
            " / / / /",
            "/ /b/ / ",
            " / / / /"
        );

        game.MakeMove(new Move(Color.Black, new Position(1, 3), new Position(0, 2)));

        Assert.AreEqual(game.Board, GameFactory.Create(
            Color.Black,
            "/ / / / ",
            " / / / /",
            "/ / / / ",
            " / / / /",
            "/ / / / ",
            " / / / /",
            "/ / / / ",
            " /B/ / /"
        ).Board);
    }
}
