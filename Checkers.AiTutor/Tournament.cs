using Checkers.Logic;
using Checkers.Logic.AI;

namespace Checkers.Tutor;

public class Tournament
{
    public static void Play()
    {
        var genWhite = 25;
        var genBlack = 0;

        var random = new Random();

        while (true)
        {
            var white = NeuralNetwork.Load($"data/history/gen-{genWhite}/{random.Next(30)}.json");
            var black = NeuralNetwork.Load($"data/history/gen-{genBlack}/{random.Next(30)}.json");
            var result = Evolution.Play(white, black);
            switch (result)
            {
                case Game.GameResult.Draw:
                case Game.GameResult.None:
                    Console.Write("D");
                    break;
                case Game.GameResult.WhiteWins:
                    Console.Write("W");
                    break;
                case Game.GameResult.BlackWins:
                    Console.Write("L");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
