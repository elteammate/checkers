using Checkers.Logic;
using Checkers.Logic.AI;

namespace Checkers.Tutor;

public class Tournament
{
    public static void Play()
    {
        var genWhite = 52;
        var genBlack = 0;

        var random = new Random();

        var wins = 0;
        var draws = 0;
        var losses = 0;

        while (true)
        {
            var white = NeuralNetwork.Load($"data/history/gen-{genWhite}/{random.Next(30)}.json");
            var black = NeuralNetwork.Load($"data/history/gen-{genBlack}/{random.Next(30)}.json");
            var result = Evolution.Play(white, black);

            switch (result)
            {
                case Game.GameResult.Draw:
                case Game.GameResult.None:
                    Console.WriteLine("D");
                    draws++;
                    break;
                case Game.GameResult.WhiteWins:
                    Console.WriteLine("W");
                    wins++;
                    break;
                case Game.GameResult.BlackWins:
                    Console.WriteLine("L");
                    losses++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Console.WriteLine($"Gen {genWhite} vs Gen {genBlack} - W:{wins} D:{draws} L:{losses}");
        }
    }
}
