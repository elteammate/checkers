using Checkers.Logic;

namespace Checkers.Tutor;

public class SampleGenerator
{
    public static void Generate()
    {
        for (var i = 0; i < 1000; i++) GenerateOne();
    }

    public static void GenerateOne()
    {
        var random = new Random();
        var game = GameFactory.Create();
        var moveNumber = 0;

        while (game.Result == Game.GameResult.None)
        {
            moveNumber++;
            var moves = game.MoveFinder.GetMoves();

            if (random.Next(100 / moveNumber) == 0)
            {
                var values = new double[32];
                var heuristic = 0.0;
                for (var i = 0; i < 32; i++)
                {
                    values[i] = game.Board[i] switch
                    {
                        Piece.White => 1,
                        Piece.WhiteKing => 2,
                        Piece.Black => -1,
                        Piece.BlackKing => -2,
                        _ => 0
                    };

                    heuristic += values[i];

                    if (game.Board[i] == Piece.White)
                        heuristic += 0.1 * new Position(i).Row;

                    if (game.Board[i] == Piece.Black)
                        heuristic -= 0.1 * (7 - new Position(i).Row);
                }

                heuristic = Math.Tanh(heuristic / 10);

                File.AppendAllText("data/samples.txt", $"{string.Join(' ', values)} {heuristic}\n");
            }

            var move = moves[random.Next(moves.Count)];
            game.MakeMove(move);
        }
    }
}
